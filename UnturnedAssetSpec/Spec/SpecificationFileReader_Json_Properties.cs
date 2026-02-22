using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
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
        if (!isImport && key[0] == '#')
        {
            // #This.Key = ""
            if (DataRefs.TryParseDataRef(key,
                    out ReadOnlySpan<char> dataRefRoot,
                    out bool isRootEscaped,
                    out ReadOnlySpan<char> dataRefProperty,
                    out ReadOnlySpan<char> _,
                    out ReadOnlySpan<char> _)
                && !isRootEscaped
                && dataRefRoot.Equals("This", StringComparison.OrdinalIgnoreCase)
                && dataRefProperty.Equals("Key", StringComparison.OrdinalIgnoreCase)
            )
            {
                key = string.Empty;
            }
        }

        DatProperty? overriding = null;
        if (!isImport && (!root.TryGetProperty("PreventOverride"u8, out element) || element.ValueKind != JsonValueKind.True))
        {
            // find property to override in parent types
            for (DatTypeWithProperties? parentType = owner.BaseType; parentType is T pType && overriding == null; parentType = parentType.BaseType)
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
                singleAlias = ReadAlias(in singleAliasElement, owner, property, key, -1);
            }

            ImmutableArray<DatPropertyKey>.Builder b = ImmutableArray.CreateBuilder<DatPropertyKey>(aliasCount + 1 + (singleAlias != null ? 1 : 0));
            b.Add(new DatPropertyKey(key, filter, keyCondition));

            for (int i = 0; i < aliasCount; ++i)
            {
                JsonElement aliasObj = element[i];
                b.Add(ReadAlias(in aliasObj, owner, property, key, i));
            }

            if (singleAlias != null)
                b.Add(singleAlias);

            property.Keys = b.MoveToImmutable();
        }
        else if (root.TryGetProperty("Alias"u8, out singleAliasElement) && singleAliasElement.ValueKind != JsonValueKind.Null)
        {
            DatPropertyKey singleAlias = ReadAlias(in singleAliasElement, owner, property, key, -1);
            property.Keys = ImmutableArray.Create(
                new DatPropertyKey(key, filter, keyCondition),
                singleAlias
            );
        }
        else if (filter != LegacyExpansionFilter.Either || keyCondition != null)
        {
            property.Keys = ImmutableArray.Create(new DatPropertyKey(key, filter, keyCondition));
        }
        else
        {
            property.Keys = ImmutableArray<DatPropertyKey>.Empty;
        }

        // FileCrossRef
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

        // ListReference
        if (root.TryGetProperty("ListReference"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            property.AvailableValuesTarget = Value.TryReadValueFromJson(in element, ValueReadOptions.AssumeProperty | ValueReadOptions.AllowExclamationSuffix, null, Database, property);
            if (property.AvailableValuesTarget == null)
            {
                throw new JsonException(
                    string.Format(Resources.JsonException_FailedToParseValue, "Property Reference", $"{owner.FullName}.{key}.FileCrossRef")
                );
            }

            property.AvailableValuesTargetIsRequired = element.ValueKind == JsonValueKind.String && element.GetString()!.EndsWith("!", StringComparison.Ordinal);
        }

        // SubtypeSwitch
        if (root.TryGetProperty("SubtypeSwitch"u8, out element) && element.ValueKind != JsonValueKind.String)
        {
            property.SubtypeSwitchPropertyName = element.GetString();
        }

        // Required
        if (root.TryGetProperty("Required"u8, out element) && element.ValueKind is not JsonValueKind.False and not JsonValueKind.Null)
        {
            property.Required = this.ReadValue(in element, BooleanType.Instance, property, $"{owner.FullName}.{key}.Required");
        }

        // AssetPosition
        if (root.TryGetProperty("AssetPosition"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            if (!Enum.TryParse(element.GetString(), out AssetDatPropertyPositionExpectation assetPosition))
            {
                throw new JsonException(
                    string.Format(Resources.JsonException_FailedToParseEnum, nameof(LegacyExpansionFilter), element.GetString(), $"{owner.FullName}.{key}.KeyLegacyExpansionFilter")
                );
            }

            property.AssetPosition = assetPosition;
        }

        // DefaultValue
        if (root.TryGetProperty("DefaultValue"u8, out element))
        {
            property.DefaultValue = this.ReadValue(in element, type, property, $"{owner.FullName}.{key}.DefaultValue");
        }
        else if (property.Type is FlagType)
        {
            property.DefaultValue = Value.False;
        }

        // IncludedDefaultValue
        if (root.TryGetProperty("IncludedDefaultValue"u8, out element))
        {
            property.IncludedDefaultValue = this.ReadValue(in element, type, property, $"{owner.FullName}.{key}.IncludedDefaultValue");
        }
        else if (property.Type is FlagType or BooleanOrFlagType)
        {
            property.IncludedDefaultValue = Value.True;
        }

        // Description
        if (root.TryGetProperty("Description"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            property.Description = this.ReadValue(in element, StringType.Instance, property, $"{owner.FullName}.{key}.Description");
        }

        // Markdown
        if (root.TryGetProperty("Markdown"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            property.MarkdownDescription = this.ReadValue(in element, StringType.Instance, property, $"{owner.FullName}.{key}.Markdown");
        }

        // Variable
        if (root.TryGetProperty("Variable"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            property.Variable = this.ReadValue(in element, StringType.Instance, property, $"{owner.FullName}.{key}.Variable");
        }

        // Version
        if (root.TryGetProperty("Version"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            property.Version = this.ReadValue(in element, VersionType.PackableInstance, property, $"{owner.FullName}.{key}.Version");
        }

        // Docs
        if (root.TryGetProperty("Docs"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            property.Docs = this.ReadValue(in element, StringType.Instance, property, $"{owner.FullName}.{key}.Docs");
        }

        // Minimum[Exclusive]
        if (root.TryGetProperty("Minimum"u8, out element))
        {
            property.Minimum = this.ReadValue(in element, StringType.Instance, property, $"{owner.FullName}.{key}.Minimum");
        }
        else if (root.TryGetProperty("MinimumExclusive"u8, out element))
        {
            property.Minimum = this.ReadValue(in element, StringType.Instance, property, $"{owner.FullName}.{key}.MinimumExclusive");
            property.MinimumIsExclusive = true;
        }

        // Maximum[Exclusive]
        if (root.TryGetProperty("Maximum"u8, out element))
        {
            property.Maximum = this.ReadValue(in element, StringType.Instance, property, $"{owner.FullName}.{key}.Maximum");
        }
        else if (root.TryGetProperty("MaximumExclusive"u8, out element))
        {
            property.Maximum = this.ReadValue(in element, StringType.Instance, property, $"{owner.FullName}.{key}.MaximumExclusive");
            property.MaximumIsExclusive = true;
        }

        // Except
        if (root.TryGetProperty("Except"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            int exceptCt = element.GetArrayLength();
            ImmutableArray<IValue>.Builder bldr = ImmutableArray.CreateBuilder<IValue>(exceptCt);
            for (int i = 0; i < exceptCt; ++i)
            {
                JsonElement except = element[i];
                bldr.Add(this.ReadValue(in except, type, property, $"{owner.FullName}.{key}.Except[{i}]"));
            }

            property.Exceptions = bldr.MoveToImmutableOrCopy();
        }

        // ExclusiveWith
        if (root.TryGetProperty("ExclusiveWith"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (!ExclusionCondition.TryReadFromJson(in element, property, Database, out IExclusionCondition? condition))
                {
                    throw new JsonException($"Failed to read exclusion condition from \"{owner.FullName}.{key}.ExclusiveWith\".");
                }

                property.ExclusionConditions = ImmutableArray.Create(condition);
            }
            else
            {
                int conditionCt = element.GetArrayLength();
                ImmutableArray<IExclusionCondition>.Builder bldr = ImmutableArray.CreateBuilder<IExclusionCondition>(conditionCt);
                for (int i = 0; i < conditionCt; ++i)
                {
                    JsonElement cond = element[i];
                    if (!ExclusionCondition.TryReadFromJson(in cond, property, Database, out IExclusionCondition? condition))
                    {
                        throw new JsonException($"Failed to read exclusion condition from \"{owner.FullName}.{key}.ExclusiveWith[{i}]\".");
                    }

                    bldr.Add(condition);
                }

                property.ExclusionConditions = bldr.MoveToImmutableOrCopy();
            }
        }

        // InclusiveWith
        if (root.TryGetProperty("InclusiveWith"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (!InclusionCondition.TryReadFromJson(in element, property, Database, out IInclusionCondition? condition))
                {
                    throw new JsonException($"Failed to read exclusion condition from \"{owner.FullName}.{key}.InclusiveWith\".");
                }

                property.InclusionConditions = ImmutableArray.Create(condition);
            }
            else
            {
                int conditionCt = element.GetArrayLength();
                ImmutableArray<IInclusionCondition>.Builder bldr = ImmutableArray.CreateBuilder<IInclusionCondition>(conditionCt);
                for (int i = 0; i < conditionCt; ++i)
                {
                    JsonElement cond = element[i];
                    if (!InclusionCondition.TryReadFromJson(in cond, property, Database, out IInclusionCondition? condition))
                    {
                        throw new JsonException($"Failed to read exclusion condition from \"{owner.FullName}.{key}.InclusiveWith[{i}]\".");
                    }

                    bldr.Add(condition);
                }

                property.InclusionConditions = bldr.MoveToImmutableOrCopy();
            }
        }

        // Deprecated
        if (root.TryGetProperty("Deprecated"u8, out element) && element.ValueKind is not JsonValueKind.False and not JsonValueKind.Null)
        {
            property.Deprecated = this.ReadValue(in element, BooleanType.Instance, property, $"{owner.FullName}.{key}.Deprecated");
        }

        // Experimental
        if (root.TryGetProperty("Experimental"u8, out element) && element.ValueKind is not JsonValueKind.False and not JsonValueKind.Null)
        {
            property.Experimental = this.ReadValue(in element, BooleanType.Instance, property, $"{owner.FullName}.{key}.Experimental");
        }

        properties.Add(property);
    }

    private DatPropertyKey ReadAlias(in JsonElement aliasObj, IDatSpecificationObject owner, DatProperty property, string key, int i)
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

        return new DatPropertyKey(alias, aliasFilter, aliasCondition);
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

        string? fileType = null;
        int fileTypeSeparator = type.IndexOf("::", StringComparison.Ordinal);
        if (fileTypeSeparator > 0 && fileTypeSeparator < type.Length - 2)
        {
            fileType = type.Substring(0, fileTypeSeparator);
            type = type.Substring(fileTypeSeparator + 2);
        }

        if (CommonTypes.TypeFactories.TryGetValue(type, out Func<ITypeFactory>? factoryGetter))
        {
            ITypeFactory factory = factoryGetter();
            IType newType = factory.CreateType(in root, type, this, property, key);
            return newType;
        }

        if (type.IndexOf(',') >= 0)
        {
            QualifiedType qualType = new QualifiedType(type, isCaseInsensitive: true);
            DatFileType? file = owner.Owner;
            if (fileType != null)
            {
                file = GetOrReadFileType(new QualifiedType(fileType, true));
            }

            for (; file != null; file = file.Parent)
            {
                if (qualType.Equals(file.TypeName))
                {
                    return file;
                }

                IType? newType = GetOrReadType(file, type);
                if (newType is DatType dt)
                {
                    return dt;
                }
            }

            //Type clrType = Type.GetType(type, throwOnError: false, ignoreCase: false)!;
            //if (clrType != null && typeof(ITypeFactory).IsAssignableFrom(clrType))
            //{
            //    ITypeFactory factory = (ITypeFactory?)Activator.CreateInstance(clrType, true)!;
            //    IType newType = factory.CreateType(in root, type, this, property, key);
            //    return newType;
            //}
        }
        
        throw new JsonException(string.Format(Resources.JsonException_PropertyTypeNotFound, type, $"{owner.FullName}.{key}.Type"));
    }

    public IType ReadType(in JsonElement root, DatProperty property, string context = "")
    {
        if (ReadTypeReference(in root, property.Owner, property, allowSwitch: false) is IType t)
            return t;

        throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, "Type", context.Length == 0 ? property.FullName : $"{property.FullName}.{context}"));
    }

    public IValue ReadValue<TDataRefReadContext>(
        in JsonElement root,
        IPropertyType valueType,
        IDatSpecificationObject readObject,
        ref TDataRefReadContext dataRefContext,
        string context = "",
        ValueReadOptions options = ValueReadOptions.Default
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        if (Value.TryReadValueFromJson(in root, options, valueType, Database, readObject, ref dataRefContext) is { } value)
        {
            return value;
        }

        throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, valueType, context.Length == 0 ? readObject.FullName : $"{readObject.FullName}.{context}"));
    }

    public IValue<TValue> ReadValue<TValue, TDataRefReadContext>(
        in JsonElement root,
        IType<TValue> valueType,
        IDatSpecificationObject readObject,
        ref TDataRefReadContext dataRefContext,
        string context = "",
        ValueReadOptions options = ValueReadOptions.Default
    ) where TValue : IEquatable<TValue>
      where TDataRefReadContext : IDataRefReadContext?
    {
        if (Value.TryReadValueFromJson(in root, options, valueType, Database, readObject, ref dataRefContext) is { } value)
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
            DataRefs.NilDataRefContext c;
            if (SwitchCase.TryReadSwitchCase(switchType, Database, owner, in caseObj, ref c) is not { } sw)
            {
                throw new JsonException(string.Format(Resources.JsonException_FailedToReadValue, "Type", $"{owner.FullName}.{key}.Type[{i}]"));
            }

            caseArrayBuilder.Add(sw);
        }

        return new TypeSwitch(switchType, caseArrayBuilder.MoveToImmutable());
    }
}