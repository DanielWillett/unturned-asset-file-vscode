using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

partial class SpecificationFileReader
{
    private void ReadPropertyFirstPass<T>(
        in JsonElement root,
        int index,
        string propertyList,
        Func<T, ImmutableArray<DatProperty>> getPropertyListFromChild,
        ImmutableArray<DatProperty>.Builder properties,
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
        if (!isImport)
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
                    properties.Add(DatProperty.Hide(overriding, owner));
                    return;
                }
            }
        }

        // Types
        PropertyTypeOrSwitch type = default;
        if (!root.TryGetProperty("Type"u8, out element)
            || element.ValueKind is not JsonValueKind.String and not JsonValueKind.Object and not JsonValueKind.Array)
        {
            if (overriding == null || overriding.Type.Type == null && overriding.Type.TypeSwitch == null)
                throw new JsonException(string.Format(Resources.JsonException_PropertyTypeMissing, $"{owner.FullName}.{key}"));

            type = overriding.Type;
        }
        else
        {
            type = ReadTypeReference(in element, owner, key);
        }

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

        DatProperty property = DatProperty.Create(key, type, owner, element);

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

        // todo: KeyCondition
        SpecDynamicSwitchCaseOrCondition keyCondition = default;

        if (root.TryGetProperty("Aliases"u8, out element) || element.ValueKind != JsonValueKind.Null)
        {
            int aliasCount = element.GetArrayLength();

            ImmutableArray<DatPropertyKey>.Builder b = ImmutableArray.CreateBuilder<DatPropertyKey>(aliasCount + 1);
            b.Add(new DatPropertyKey(key, filter, keyCondition, keyTemplateProcessor));

            for (int i = 0; i < aliasCount; ++i)
            {
                JsonElement aliasObj = element[i];
                string? alias = null;
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
                            if (!Enum.TryParse(aliasElement.GetString(), out filter))
                            {
                                throw new JsonException(
                                    string.Format(Resources.JsonException_FailedToParseEnum, nameof(LegacyExpansionFilter), aliasElement.GetString(), $"{owner.FullName}.{key}.Aliases[{i}].LegacyExpansionFilter")
                                );
                            }
                        }

                        // todo: Condition
                        
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

                b.Add(new DatPropertyKey(alias, aliasFilter, keyCondition, processor));
            }

            property.Keys = b.MoveToImmutableOrCopy();
        }
        else if (filter != LegacyExpansionFilter.Either)
        {
            ImmutableArray<DatPropertyKey>.Builder b = ImmutableArray.CreateBuilder<DatPropertyKey>(1);
            b.Add(new DatPropertyKey(key, filter, default, keyTemplateProcessor));
            property.Keys = b.MoveToImmutableOrCopy();
        }
        else
        {
            property.Keys = ImmutableArray<DatPropertyKey>.Empty;
        }

        property.IsTemplate = isTemplate;
        if (isTemplate && root.TryGetProperty("TemplateGroups"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            int templateCt = element.GetArrayLength();
            ImmutableArray<TemplateGroup>.Builder b = ImmutableArray.CreateBuilder<TemplateGroup>(templateCt + 1);

        }
        else
        {
            property.TemplateGroups = isTemplate ? ImmutableArray<TemplateGroup>.Empty : default;
        }
    }

    private PropertyTypeOrSwitch ReadTypeReference(in JsonElement root, IDatSpecificationObject owner, string key, bool allowSwitch = true)
    {
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
                throw new JsonException(string.Format(Resources.JsonException_PropertyTypeMissing, $"{owner.FullName}.{key}.Type"));
        }
        else
        {
            if (allowSwitch)
                return new PropertyTypeOrSwitch(ReadTypeSwitch(in root, owner, key));

            throw new JsonException(string.Format(Resources.JsonException_PropertyTypeMissing, $"{owner.FullName}.{key}.Type[ ... ]"));
        }

        if (type.IndexOf(',') >= 0)
        {
            QualifiedType qualType = new QualifiedType(type, isCaseInsensitive: true);
            for (DatFileType? file = owner.Owner; file != null; file = file.Parent)
            {
                if (qualType.Equals(file.TypeName))
                {
                    return new PropertyTypeOrSwitch(file);
                }

                if (file.Types.TryGetValue(qualType, out DatType? newType))
                {
                    return new PropertyTypeOrSwitch(newType);
                }
            }

            Type clrType = Type.GetType(type, throwOnError: true, ignoreCase: false)!;
            if (clrType != null && typeof(ITypeFactory).IsAssignableFrom(clrType))
            {
                ITypeFactory factory = (ITypeFactory?)Activator.CreateInstance(clrType, true)!;
                IType newType = factory.CreateType(in root, this, owner, key);
                return new PropertyTypeOrSwitch(newType);
            }
        }
        
        if (CommonTypes.TypeFactories.TryGetValue(type, out Func<ITypeFactory>? factoryGetter))
        {
            ITypeFactory factory = factoryGetter();
            IType newType = factory.CreateType(in root, this, owner, key);
            return new PropertyTypeOrSwitch(newType);
        }

        throw new JsonException(string.Format(Resources.JsonException_PropertyTypeNotFound, type, $"{owner.FullName}.{key}.Type"));
    }

    private static SpecDynamicSwitchValue ReadTypeSwitch(in JsonElement root, IDatSpecificationObject owner, string key)
    {
        int cases = root.GetArrayLength();
        throw new NotImplementedException();
    }

    public IType ReadType(in JsonElement root, IDatSpecificationObject readObject, string context = "")
    {
        if (ReadTypeReference(in root, readObject, context, allowSwitch: false).Type is IType t)
            return t;

        throw new JsonException(string.Format(Resources.JsonException_PropertyTypeNotFound, "", context.Length == 0 ? readObject.FullName : $"{readObject.FullName}.{context}"));
    }

    public IValue<T> ReadValue<T>(in JsonElement root, IType<T> valueType, IDatSpecificationObject readObject, string context = "") where T : IEquatable<T>
    {
        // todo: support switch, refs, etc
        if (valueType.Parser.TryReadValueFromJson(in root, out Optional<T> value, valueType))
        {
            return valueType.CreateValue(value);
        }

        throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, valueType.Id, context.Length == 0 ? readObject.FullName : $"{readObject.FullName}.{context}"));
    }
}