using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

partial class SpecificationFileReader
{
    private void ReadPropertyFirstPass<T>(
        in JsonElement root,
        int index,
        string propertyList,
        Func<T, ImmutableArray<DatProperty>> getPropertyListFromChild,
        ImmutableArray<DatProperty>.Builder properties,
        SpecPropertyContext context,
        T owner) where T : DatTypeWithProperties, IDatSpecificationObject
    {
        if (!root.TryGetProperty("Key"u8, out JsonElement element) || element.ValueKind != JsonValueKind.String)
        {
            throw new JsonException(string.Format(
                Resources.JsonException_PropertyKeyMissing,
                $"{owner.FullName}.{propertyList}[{index}]")
            );
        }

        string key = element.GetString()!;
        bool isImport = string.IsNullOrWhiteSpace(key);
        DatProperty? overriding = null;
        if (!isImport && (!root.TryGetProperty("PreventOverride"u8, out element) || element.ValueKind != JsonValueKind.True))
        {
            // find property to override in parent types
            for (DatTypeWithProperties? parentType = owner.BaseType; parentType is T pType && overriding != null; parentType = parentType.BaseType)
            {
                ImmutableArray<DatProperty> parentProperties = getPropertyListFromChild(pType);
                for (int i = 0; i < parentProperties.Length; ++i)
                {
                    DatProperty p = parentProperties[i];
                    if (string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase))
                    {
                        overriding = p;
                        break;
                    }
                }
            }

            bool hideInherited = root.TryGetProperty("HideInherited"u8, out element) && element.ValueKind == JsonValueKind.True;
            if (hideInherited || root.TryGetProperty("RequireOverride"u8, out element) && element.ValueKind == JsonValueKind.True)
            {
                if (overriding == null)
                {
                    throw new JsonException(string.Format(Resources.JsonException_OverriddenPropertyNotFound, key, owner.FullName));
                }

                if (hideInherited)
                {
                    properties.Add(DatProperty.Hide(overriding, owner, context));
                    return;
                }
            }
        }

        DatProperty property = DatProperty.Create(key, owner, element, context);

        // Type
        IPropertyType type;
        if (!root.TryGetProperty("Type"u8, out element)
            || element.ValueKind is not JsonValueKind.String and not JsonValueKind.Object and not JsonValueKind.Array)
        {
            if (overriding == null)
                throw new JsonException(string.Format(Resources.JsonException_PropertyTypeMissing, $"{owner.FullName}.{key}"));

            type = overriding.Type;
        }
        else
        {
            type = ReadTypeReference(in element, owner, property);
        }

        property.Type = type;

        // Template
        bool isTemplate = false;
        if (root.TryGetProperty("Template"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            isTemplate = element.GetBoolean();
        }

        TemplateProcessor keyTemplateProcessor = TemplateProcessor.None;
        if (isTemplate)
        {
            keyTemplateProcessor = TemplateProcessor.CreateForKey(ref key);
        }

        property.OverriddenProperty = overriding;

        LegacyExpansionFilter filter = LegacyExpansionFilter.Either;

        // Keys
        if (root.TryGetProperty("KeyLegacyExpansionFilter"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            if (!Enum.TryParse(element.GetString(), out filter))
            {
                throw new JsonException(
                    string.Format(Resources.JsonException_FailedToParseEnum, nameof(LegacyExpansionFilter), element.GetString(), $"{owner.FullName}.{key}.KeyLegacyExpansionFilter")
                );
            }
        }
        IValue<bool>? keyCondition = null;

        if (root.TryGetProperty("KeyCondition"u8, out element))
        {
            if (!Conditions.TryReadComplexOrBasicConditionFromJson(in element, Database, property, out keyCondition))
            {
                throw new JsonException(
                    string.Format(Resources.JsonException_FailedToParseValue, "complex/basic condition", $"{owner.FullName}.{key}.KeyCondition")
                );
            }
        }

        JsonElement singleAliasElement;
        if (root.TryGetProperty("Aliases"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            int aliasCount = element.GetArrayLength();
            DatPropertyKey? singleAlias = null;
            if (root.TryGetProperty("Alias"u8, out singleAliasElement) && singleAliasElement.ValueKind != JsonValueKind.Null)
            {
                singleAlias = ReadAlias(in singleAliasElement, isTemplate, owner, property, key, -1);
            }

            ImmutableArray<DatPropertyKey>.Builder b = ImmutableArray.CreateBuilder<DatPropertyKey>(aliasCount + 1 + (singleAlias != null ? 1 : 0));
            b.Add(new DatPropertyKey(key, filter, keyCondition, keyTemplateProcessor));

            for (int i = 0; i < aliasCount; ++i)
            {
                JsonElement aliasObj = element[i];
                b.Add(ReadAlias(in aliasObj, isTemplate, owner, property, key, i));
            }

            if (singleAlias != null)
                b.Add(singleAlias);

            property.Keys = b.MoveToImmutable();
        }
        else if (root.TryGetProperty("Alias"u8, out singleAliasElement) && singleAliasElement.ValueKind != JsonValueKind.Null)
        {
            DatPropertyKey singleAlias = ReadAlias(in singleAliasElement, isTemplate, owner, property, key, -1);
            property.Keys = ImmutableArray.Create(
                new DatPropertyKey(key, filter, keyCondition, keyTemplateProcessor),
                singleAlias
            );
        }
        else if (filter != LegacyExpansionFilter.Either || keyCondition != null)
        {
            property.Keys = ImmutableArray.Create(new DatPropertyKey(key, filter, keyCondition, keyTemplateProcessor));
        }
        else
        {
            property.Keys = ImmutableArray<DatPropertyKey>.Empty;
        }

        property.IsTemplate = isTemplate;
        if (isTemplate && root.TryGetProperty("TemplateGroups"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            int templateCt = element.GetArrayLength();
            ImmutableArray<TemplateGroup>.Builder b = ImmutableArray.CreateBuilder<TemplateGroup>(templateCt);

            for (int i = 0; i < templateCt; ++i)
            {
                JsonElement item = element[i];
                if (!TemplateGroup.TryReadFromJson(i, in item, out TemplateGroup? group))
                {
                    throw new JsonException(
                        string.Format(Resources.JsonException_FailedToParseValue, "Template Group", $"{owner.FullName}.{key}.TemplateGroups[{i}]")
                    );
                }

                b.Add(group);
            }

            b.Sort((a, b) => a.Group.CompareTo(b.Group));
            property.TemplateGroups = b.MoveToImmutable();
        }
        else
        {
            property.TemplateGroups = isTemplate ? ImmutableArray<TemplateGroup>.Empty : default;
        }

        if (root.TryGetProperty("FileCrossRef"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            property.CrossReferenceTarget = Value.TryReadValueFromJson(in element, ValueReadOptions.AssumeProperty, null, Database, property);
            if (property.CrossReferenceTarget == null)
            {
                throw new JsonException(
                    string.Format(Resources.JsonException_FailedToParseValue, "Property Reference", $"{owner.FullName}.{key}.FileCrossRef")
                );
            }
        }

        if (root.TryGetProperty("CountForTemplateGroup"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            string templateGroupId = element.GetString()!;
            if (string.IsNullOrEmpty(templateGroupId))
            {
                throw new JsonException(
                    string.Format(Resources.JsonException_FailedToParseValue, "Template Group Name", $"{owner.FullName}.{key}.CountForTemplateGroup")
                );
            }

            property.CountForTemplateGroup = templateGroupId;
        }

        if (root.TryGetProperty("ListReference"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            property.AvailableValuesTarget = Value.TryReadValueFromJson(in element, ValueReadOptions.AssumeProperty | ValueReadOptions.AllowExclamationSuffix, null, Database, property);
            if (property.AvailableValuesTarget == null)
            {
                throw new JsonException(
                    string.Format(Resources.JsonException_FailedToParseValue, "Property Reference", $"{owner.FullName}.{key}.FileCrossRef")
                );
            }

            property.AvailableValuesTargetIsRequired = element.ValueKind == JsonValueKind.String && element.GetString()!.EndsWith("!");
        }

        // todo
    }

    private DatPropertyKey ReadAlias(in JsonElement aliasObj, bool isTemplate, IDatSpecificationObject owner, DatProperty property, string key, int i)
    {
        string? alias = null;
        IValue<bool>? aliasCondition = null;
        LegacyExpansionFilter aliasFilter = LegacyExpansionFilter.Either;
        switch (aliasObj.ValueKind)
        {
            case JsonValueKind.String:
                alias = aliasObj.GetString()!;
                break;

            default:
                if (aliasObj.TryGetProperty("Alias"u8, out JsonElement aliasElement))
                {
                    alias = aliasElement.GetString();
                }

                if (aliasObj.TryGetProperty("LegacyExpansionFilter"u8, out aliasElement))
                {
                    if (!Enum.TryParse(aliasElement.GetString(), out aliasFilter))
                    {
                        throw new JsonException(
                            string.Format(
                                Resources.JsonException_FailedToParseEnum,
                                nameof(LegacyExpansionFilter),
                                aliasElement.GetString(),
                                i == -1 ? $"{owner.FullName}.{key}.Alias.LegacyExpansionFilter" : $"{owner.FullName}.{key}.Aliases[{i}].LegacyExpansionFilter")
                        );
                    }
                }

                if (aliasObj.TryGetProperty("Condition"u8, out aliasElement))
                {
                    if (!Conditions.TryReadComplexOrBasicConditionFromJson(in aliasElement, Database, property, out aliasCondition))
                    {
                        throw new JsonException(
                            string.Format(
                                Resources.JsonException_FailedToParseValue,
                                "Complex or Basic Condition",
                                i == -1 ? $"{owner.FullName}.{key}.Alias.Condition" : $"{owner.FullName}.{key}.Aliases[{i}].Condition")
                        );
                    }
                }
                break;
        }

        if (string.IsNullOrEmpty(alias))
        {
            throw new JsonException(
                string.Format(Resources.JsonException_PropertyKeyMissing, $"{owner.FullName}.{key}.Aliases[{i}]")
            );
        }

        TemplateProcessor processor = isTemplate
            ? TemplateProcessor.CreateForKey(ref alias)
            : TemplateProcessor.None;

        return new DatPropertyKey(alias, aliasFilter, aliasCondition, processor);
    }

    private IPropertyType ReadTypeReference(in JsonElement root, IDatSpecificationObject owner, DatProperty property, bool allowSwitch = true)
    {
        string key = property.Key;
        string? type;
        bool isObject = root.ValueKind == JsonValueKind.Object;
        if (isObject)
        {
            if (!root.TryGetProperty("Type"u8, out JsonElement element)
                || element.ValueKind is not JsonValueKind.String
                || string.IsNullOrWhiteSpace(type = element.GetString()))
            {
                throw new JsonException(string.Format(Resources.JsonException_PropertyTypeMissing, $"{owner.FullName}.{key}.Type{{ ... }}"));
            }
        }
        else if (root.ValueKind == JsonValueKind.String)
        {
            type = root.GetString();
            if (string.IsNullOrEmpty(type))
                throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, "Type", $"{owner.FullName}.{key}.Type"));
        }
        else
        {
            if (allowSwitch)
                return ReadTypeSwitch(in root, property, key);

            throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, "Type (No Switches)", $"{owner.FullName}.{key}.Type[ ... ]"));
        }

        if (type.IndexOf(',') >= 0)
        {
            QualifiedType qualType = new QualifiedType(type, isCaseInsensitive: true);
            for (DatFileType? file = owner.Owner; file != null; file = file.Parent)
            {
                if (qualType.Equals(file.TypeName))
                {
                    return file;
                }

                if (file.Types.TryGetValue(qualType, out DatType? newType))
                {
                    return newType;
                }
            }

            Type clrType = Type.GetType(type, throwOnError: true, ignoreCase: false)!;
            if (clrType != null && typeof(ITypeFactory).IsAssignableFrom(clrType))
            {
                ITypeFactory factory = (ITypeFactory?)Activator.CreateInstance(clrType, true)!;
                IType newType = factory.CreateType(in root, type, this, property, key);
                return newType;
            }
        }
        
        if (CommonTypes.TypeFactories.TryGetValue(type, out Func<ITypeFactory>? factoryGetter))
        {
            ITypeFactory factory = factoryGetter();
            IType newType = factory.CreateType(in root, type, this, property, key);
            return newType;
        }

        throw new JsonException(string.Format(Resources.JsonException_PropertyTypeNotFound, type, $"{owner.FullName}.{key}.Type"));
    }

    public IType ReadType(in JsonElement root, DatProperty property, string context = "")
    {
        if (ReadTypeReference(in root, property.Owner, property, allowSwitch: false) is IType t)
            return t;

        throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, "Type", context.Length == 0 ? property.FullName : $"{property.FullName}.{context}"));
    }

    public IValue ReadValue(in JsonElement root, IPropertyType valueType, DatProperty readObject, string context = "", ValueReadOptions options = ValueReadOptions.Default)
    {
        if (Value.TryReadValueFromJson(in root, options, valueType, Database, readObject) is { } value)
        {
            return value;
        }

        throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, valueType, context.Length == 0 ? readObject.FullName : $"{readObject.FullName}.{context}"));
    }

    private TypeSwitch ReadTypeSwitch(in JsonElement root, DatProperty owner, string key)
    {
        int cases = root.GetArrayLength();
        if (cases <= 0)
        {
            throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, "Type", $"{owner.FullName}.{key}.Type[]"));
        }

        JsonElement typeElement = default;
        IType<IType> switchType = (IType<IType>)TypeSwitch.ValueTypeFactory.CreateType(in typeElement, TypeSwitch.ValueTypeId, this, owner, key);

        ImmutableArray<ISwitchCase<IType>>.Builder caseArrayBuilder = ImmutableArray.CreateBuilder<ISwitchCase<IType>>(cases);

        for (int i = 0; i < cases; ++i)
        {
            JsonElement caseObj = root[i];
            if (SwitchCase.TryReadSwitchCase(switchType, Database, owner, in caseObj) is not { } sw)
            {
                throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, "Type", $"{owner.FullName}.{key}.Type[{i}]"));
            }

            caseArrayBuilder.Add(sw);
        }

        return new TypeSwitch(switchType, caseArrayBuilder.MoveToImmutable());
    }
}