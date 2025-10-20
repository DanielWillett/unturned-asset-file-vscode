﻿using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class LegacyCompatibleListSpecPropertyType :
    BaseSpecPropertyType<EquatableArray<CustomSpecTypeInstance>>,
    ISpecPropertyType<EquatableArray<CustomSpecTypeInstance>>,
    IListTypeSpecPropertyType,
    IEquatable<LegacyCompatibleListSpecPropertyType>
{
    private readonly ISpecType _specifiedBaseType;

    public override string Type => "LegacyCompatibleList";
    public Type ValueType => typeof(EquatableArray<CustomSpecTypeInstance>);
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    string IElementTypeSpecPropertyType.ElementType => _specifiedBaseType.Type;

    public override string DisplayName { get; }

    ISpecPropertyType? IListTypeSpecPropertyType.GetInnerType(IAssetSpecDatabase database)
    {
        return _specifiedBaseType as ISpecPropertyType;
    }

    public override int GetHashCode()
    {
        return 73 ^ _specifiedBaseType.GetHashCode();
    }

    public LegacyCompatibleListSpecPropertyType(ISpecType type)
    {
        _specifiedBaseType = type;
        DisplayName = $"List of {type.DisplayName} (Legacy Compatible)";
    }

    public bool Equals(ISpecPropertyType? other) => other is LegacyCompatibleListSpecPropertyType;
    public bool Equals(ISpecPropertyType<EquatableArray<CustomSpecTypeInstance>>? other) => other is LegacyCompatibleListSpecPropertyType;
    public bool Equals(LegacyCompatibleListSpecPropertyType? other) => other != null;

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (TryParseValue(in parse, out EquatableArray<CustomSpecTypeInstance> array))
        {
            value = array.Array is not { Length: > 0 }
                ? SpecDynamicValue.Null
                : new SpecDynamicConcreteValue<EquatableArray<CustomSpecTypeInstance>>(array, this);
            return true;
        }

        value = null!;
        return false;
    }

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out EquatableArray<CustomSpecTypeInstance> value)
    {
        bool passed = true;

        if (_specifiedBaseType is not CustomSpecType customType)
        {
            if (parse.HasDiagnostics)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                    Message = string.Format(DiagnosticResources.UNT2005, _specifiedBaseType.Type),
                    Diagnostic = DatDiagnostics.UNT2005
                });
            }

            value = default;
            return false;
        }

        if (parse.Node is IValueSourceNode stringNode)
        {
            if (!KnownTypeValueHelper.TryParseUInt8(stringNode.Value, out byte conditionCount))
            {
                return FailedToParse(in parse, out value, stringNode);
            }

            if (conditionCount == 0)
            {
                value = EquatableArray<CustomSpecTypeInstance>.Empty;
                return true;
            }

            CustomSpecTypeInstance[] conditions = new CustomSpecTypeInstance[conditionCount];
            for (int i = 0; i < conditions.Length; ++i)
            {
                if (!TryParseLegacyInstance(in parse, i, out conditions[i], customType))
                {
                    passed = false;
                }
            }

            if (!passed)
            {
                return FailedToParse(in parse, out value, stringNode);
            }

            value = new EquatableArray<CustomSpecTypeInstance>(conditions);
            return true;
        }

        if (parse.Node is not IListSourceNode list)
        {
            return MissingNode(in parse, out value);
        }

        ImmutableArray<ISourceNode> children = list.Children;
        if (children.Length == 0)
        {
            value = EquatableArray<CustomSpecTypeInstance>.Empty;
            return true;
        }

        List<CustomSpecTypeInstance> output = new List<CustomSpecTypeInstance>(children.Length);
        foreach (ISourceNode node in children)
        {
            if (node is not IAnyValueSourceNode anyVal)
                continue;

            if (!TryParseObjectInstance(in parse, anyVal, out CustomSpecTypeInstance instance, customType))
            {
                passed = false;
                continue;
            }

            output.Add(instance);
        }

        if (!passed)
        {
            return FailedToParse(in parse, out value, list);
        }

        value = new EquatableArray<CustomSpecTypeInstance>(output);
        return true;
    }

    private bool TryParseLegacyInstance(in SpecPropertyTypeParseContext parse, int index, out CustomSpecTypeInstance instance, CustomSpecType customType)
    {
        instance = null!;
        
        if (parse.BaseKey == null || parse.Parent is not IDictionarySourceNode dictionary)
        {
            return MissingNode(in parse, out _);
        }

        // ..._Conditions -> ..._Condition_n

        string baseKey;
        if (customType.AdditionalProperties.TryGetValue(CustomSpecType.PluralBaseKeyProperty, out object? value))
        {
            baseKey = value?.ToString() ?? string.Empty;
        }
        else if (parse.BaseKey.Length > 1)
        {
            baseKey = parse.BaseKey.Substring(0, parse.BaseKey.Length - 1);
        }
        else
        {
            baseKey = parse.BaseKey;
        }

        baseKey = $"{baseKey}_{index.ToString(CultureInfo.InvariantCulture)}";

        SpecPropertyTypeParseContext context = new SpecPropertyTypeParseContext(parse.EvaluationContext, parse.Diagnostics)
        {
            Node = dictionary,
            BaseKey = baseKey,
            Parent = dictionary
        };

        if (!customType.TryParseValue(in context, out CustomSpecTypeInstance? type, CustomSpecTypeParseOptions.Legacy))
        {
            if (parse.HasDiagnostics)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                    Message = string.Format(DiagnosticResources.UNT2004, dictionary, customType.DisplayName),
                    Diagnostic = DatDiagnostics.UNT2004
                });
            }

            return false;
        }

        instance = type!;
        return instance != null;
    }

    private bool TryParseObjectInstance(in SpecPropertyTypeParseContext parse, IAnyValueSourceNode node, out CustomSpecTypeInstance instance, CustomSpecType customType)
    {
        instance = null!;
        
        if (node is not IDictionarySourceNode dictionary)
        {
            return MissingNode(in parse, out _);
        }

        SpecPropertyTypeParseContext context = new SpecPropertyTypeParseContext(parse.EvaluationContext, parse.Diagnostics)
        {
            Node = dictionary,
            BaseKey = string.Empty,
            Parent = dictionary.Parent
        };

        if (!customType.TryParseValue(in context, out CustomSpecTypeInstance? type, CustomSpecTypeParseOptions.Object))
        {
            if (parse.HasDiagnostics)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                    Message = string.Format(DiagnosticResources.UNT2004, dictionary, customType.DisplayName),
                    Diagnostic = DatDiagnostics.UNT2004
                });
            }

            return false;
        }

        instance = type!;
        return instance != null;
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}

internal sealed class UnresolvedLegacyCompatibleListSpecPropertyType :
    IEquatable<UnresolvedLegacyCompatibleListSpecPropertyType?>,
    ISecondPassSpecPropertyType,
    IListTypeSpecPropertyType,
    IDisposable
{
    public ISecondPassSpecPropertyType InnerType { get; }
    ISpecPropertyType IListTypeSpecPropertyType.GetInnerType(IAssetSpecDatabase database) => InnerType;
    string IElementTypeSpecPropertyType.ElementType => InnerType.Type;

    public string DisplayName => "LegacyCompatibleList";
    public string Type => "LegacyCompatibleList";
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;
    public Type ValueType => typeof(EquatableArray<CustomSpecTypeInstance>);

    public UnresolvedLegacyCompatibleListSpecPropertyType(ISecondPassSpecPropertyType innerType)
    {
        InnerType = innerType ?? throw new ArgumentNullException(nameof(innerType));
    }

    public bool Equals(UnresolvedLegacyCompatibleListSpecPropertyType? other) => other != null && InnerType.Equals(other.InnerType);
    public bool Equals(ISpecPropertyType? other) => other is UnresolvedLegacyCompatibleListSpecPropertyType l && Equals(l);
    public override bool Equals(object? obj) => obj is UnresolvedLegacyCompatibleListSpecPropertyType l && Equals(l);
    public override int GetHashCode() => InnerType.GetHashCode();
    public override string ToString() => $"Unresolved Legacy-Compatible List of {InnerType.Type}";
    public void Dispose()
    {
        if (InnerType is IDisposable d)
            d.Dispose();
    }

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        value = null!;
        return false;
    }

    public ISpecPropertyType Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile)
    {
        if (InnerType.Transform(property, database, assetFile) is not ISpecType type)
        {
            throw new Exception($"Type {InnerType} is not a custom type.");
        }

        return KnownTypes.LegacyCompatibleList(type);
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) { }
}