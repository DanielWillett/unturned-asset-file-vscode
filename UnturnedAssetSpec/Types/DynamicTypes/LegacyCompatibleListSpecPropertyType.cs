using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A list of objects than can be formatted in either the modern format or the legacy format.
/// Individual properties can specify the <see cref="SpecProperty.KeyLegacyExpansionFilter"/> for different keys depending on the format.
/// <para>Example: <c>DialogueAsset.Conditions</c></para>
/// <code>
/// // Modern
/// Props
/// [
///     {
///         Value 3
///         Mode Add
///     }
///     {
///         Value 4
///         Mode Subtract
///     }
/// ]
///
/// // Legacy
/// Props 2
/// Prop_0_Value 3
/// Prop_0_Mode Add
/// Prop_1_Value 4
/// Prop_1_Mode Subtract
/// 
/// </code>
/// <para>
/// Supports the <c>PluralBaseKey</c> additional property to override the singular property name. By default it just removes the 's' from the end of the property name.
/// </para>
/// <para>
/// Also supports the <c>MinimumCount</c> and <c>MaximumCount</c> properties for list element count limits.
/// </para>
/// <para>
/// The defining property can set the <see cref="LegacyExpansionFilter"/> for the key being used to filter by only Legacy or Modern (although only modern should just use <see cref="ListSpecPropertyType{TElementType}"/>).
/// </para>
/// </summary>
public sealed class LegacyCompatibleListSpecPropertyType<T> :
    BaseSpecPropertyType<LegacyCompatibleListSpecPropertyType<T>, EquatableArray<T>>,
    ISpecPropertyType<EquatableArray<T>>,
    IListTypeSpecPropertyType,
    IEquatable<LegacyCompatibleListSpecPropertyType<T>>
    where T : IEquatable<T>
{
    private readonly ISpecPropertyType<T> _specifiedBaseType;

    public override string Type => "LegacyCompatibleList";

    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    string IElementTypeSpecPropertyType.ElementType => _specifiedBaseType.Type;

    public override string DisplayName { get; }

    public bool AllowSingleModern { get; }
    public bool AllowSingleLegacy { get; }

    ISpecPropertyType IListTypeSpecPropertyType.GetInnerType()
    {
        return _specifiedBaseType;
    }

    public override int GetHashCode()
    {
        return 73 ^ _specifiedBaseType.GetHashCode();
    }

    public LegacyCompatibleListSpecPropertyType(ISpecPropertyType<T> type, bool allowSingleModern, bool allowSingleLegacy)
    {
        _specifiedBaseType = type;
        DisplayName = $"List of {type.DisplayName} (Legacy Compatible)";
        AllowSingleModern = allowSingleModern;
        AllowSingleLegacy = allowSingleLegacy;
    }

    public bool Equals(LegacyCompatibleListSpecPropertyType<T>? other) => other != null
                                                                       && other.AllowSingleModern == AllowSingleModern
                                                                       && other.AllowSingleLegacy == AllowSingleLegacy
                                                                       && other._specifiedBaseType.Equals(_specifiedBaseType);

    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out EquatableArray<T> value)
    {
        bool passed = true;

        SpecProperty? property = parse.EvaluationContext.Self;
        LegacyExpansionFilter filter = property?.GetCorrespondingFilter(in parse) ?? LegacyExpansionFilter.Either;

        bool allowSingle = filter switch
        {
            LegacyExpansionFilter.Legacy => AllowSingleLegacy,
            LegacyExpansionFilter.Modern => AllowSingleModern,
            _ => AllowSingleLegacy && AllowSingleLegacy == AllowSingleModern
        };

        if (parse.Node is not IListSourceNode listNode)
        {
            if (filter == LegacyExpansionFilter.Modern && !allowSingle)
            {
                if (parse.HasDiagnostics)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                        Message = string.Format(DiagnosticResources.UNT2013_Legacy, _specifiedBaseType.DisplayName),
                        Diagnostic = DatDiagnostics.UNT2013
                    });
                }

                value = default;
                return false;
            }

            if (filter == LegacyExpansionFilter.Modern
                || parse.Node is not IValueSourceNode stringNode
                || !KnownTypeValueHelper.TryParseUInt8(stringNode.Value, out byte conditionCount))
            {
                if (!allowSingle)
                {
                    return FailedToParse(in parse, out value, parse.Node);
                }

                if (!TryParseValue(in parse, out T? type, modern: filter == LegacyExpansionFilter.Modern))
                {
                    if (parse.HasDiagnostics)
                    {
                        parse.Log(new DatDiagnosticMessage
                        {
                            Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                            Message = string.Format(DiagnosticResources.UNT2004, parse.Node, _specifiedBaseType.DisplayName),
                            Diagnostic = DatDiagnostics.UNT2004
                        });
                    }

                    value = default;
                    return false;
                }

                value = new EquatableArray<T>(new T[] { type });
                return true;
            }

            if (conditionCount == 0)
            {
                value = EquatableArray<T>.Empty;
                return true;
            }

            T[] objects = new T[conditionCount];
            for (int i = 0; i < objects.Length; ++i)
            {
                if (!TryParseLegacyInstance(in parse, i, out objects[i]))
                {
                    passed = false;
                }
            }

            KnownTypeValueHelper.TryGetMinimaxCountWarning(objects.Length, in parse);

            if (!passed)
            {
                return FailedToParse(in parse, out value, stringNode);
            }

            value = new EquatableArray<T>(objects);
            return true;
        }

        if (filter == LegacyExpansionFilter.Legacy && !allowSingle)
        {
            if (!allowSingle)
            {
                if (parse.HasDiagnostics)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                        Message = string.Format(DiagnosticResources.UNT2013_Modern, _specifiedBaseType.DisplayName),
                        Diagnostic = DatDiagnostics.UNT2013
                    });
                }
                value = default;
                return false;
            }

            if (!TryParseValue(in parse, out T? type, false))
            {
                if (parse.HasDiagnostics)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                        Message = string.Format(DiagnosticResources.UNT2004, parse.Node, _specifiedBaseType.DisplayName),
                        Diagnostic = DatDiagnostics.UNT2004
                    });
                }

                value = default;
                return false;
            }

            value = new EquatableArray<T>(new T[] { type });
            return true;
        }

        ImmutableArray<ISourceNode> children = listNode.Children;

        KnownTypeValueHelper.TryGetMinimaxCountWarning(children.Length, in parse);

        if (children.Length == 0)
        {
            value = EquatableArray<T>.Empty;
            return true;
        }

        List<T> output = new List<T>(children.Length);
        foreach (ISourceNode node in children)
        {
            if (node is not IAnyValueSourceNode anyVal)
                continue;

            if (!TryParseObjectInstance(in parse, anyVal, out T instance))
            {
                passed = false;
                continue;
            }

            output.Add(instance);
        }

        if (!passed)
        {
            return FailedToParse(in parse, out value, listNode);
        }

        value = new EquatableArray<T>(output);
        return true;
    }

    private bool TryParseLegacyInstance(in SpecPropertyTypeParseContext parse, int index, out T instance)
    {
        instance = default!;
        
        if (parse.BaseKey == null || parse.Parent is not IDictionarySourceNode dictionary)
        {
            return MissingNode(in parse, out _);
        }

        // ..._Conditions -> ..._Condition_n

        string baseKey;
        if (parse.EvaluationContext.Self.TryGetAdditionalProperty(CustomSpecType.PluralBaseKeyProperty, out object? value)
            || _specifiedBaseType is IAdditionalPropertyProvider additionalPropertyProvider && additionalPropertyProvider.TryGetAdditionalProperty(CustomSpecType.PluralBaseKeyProperty, out value))
        {
            baseKey = value?.ToString() ?? string.Empty;
        }
        else if (parse.BaseKey.Length > 1 && parse.BaseKey[^1] == 's')
        {
            baseKey = parse.BaseKey.Substring(0, parse.BaseKey.Length - 1);
        }
        else
        {
            baseKey = parse.BaseKey;
        }

        baseKey = $"{baseKey}_{index.ToString(CultureInfo.InvariantCulture)}";

        SpecPropertyTypeParseContext context = new SpecPropertyTypeParseContext(parse.EvaluationContext, parse.Breadcrumbs, parse.Diagnostics)
        {
            Node = dictionary,
            BaseKey = baseKey,
            Parent = dictionary
        };

        if (!TryParseValue(in context, out T? v, false))
        {
            if (parse.HasDiagnostics)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                    Message = string.Format(DiagnosticResources.UNT2004, dictionary, _specifiedBaseType.DisplayName),
                    Diagnostic = DatDiagnostics.UNT2004
                });
            }

            return false;
        }

        instance = v;
        return v != null;
    }

    private bool TryParseValue(in SpecPropertyTypeParseContext context, [MaybeNullWhen(false)] out T instance, bool modern)
    {
        bool failed = false;
        if (_specifiedBaseType is CustomSpecType t && typeof(T) == typeof(CustomSpecTypeInstance))
        {
            if (!t.TryParseValue(in context, out CustomSpecTypeInstance? parsed, modern ? CustomSpecTypeParseOptions.Object : CustomSpecTypeParseOptions.Legacy))
            {
                failed = true;
                instance = default;
            }
            else
            {
                instance = Unsafe.As<CustomSpecTypeInstance, T>(ref parsed!);
            }
        }
        else
        {
            if (!_specifiedBaseType.TryParseValue(in context, out T? parsed))
            {
                failed = true;
                instance = default;
            }
            else
            {
                instance = parsed!;
            }
        }

        return !failed;
    }

    private bool TryParseObjectInstance(in SpecPropertyTypeParseContext parse, IAnyValueSourceNode node, [MaybeNullWhen(false)] out T instance)
    {
        instance = default!;
        
        if (node is not IDictionarySourceNode dictionary)
        {
            return MissingNode(in parse, out _);
        }

        SpecPropertyTypeParseContext context = new SpecPropertyTypeParseContext(parse.EvaluationContext, parse.Breadcrumbs, parse.Diagnostics)
        {
            Node = dictionary,
            BaseKey = string.Empty,
            Parent = dictionary.Parent
        };

        if (!TryParseValue(in context, out instance, modern: false))
        {
            if (parse.HasDiagnostics)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                    Message = string.Format(DiagnosticResources.UNT2004, dictionary, _specifiedBaseType.DisplayName),
                    Diagnostic = DatDiagnostics.UNT2004
                });
            }

            return false;
        }

        return true;
    }
}

internal sealed class UnresolvedLegacyCompatibleListSpecPropertyType :
    IEquatable<UnresolvedLegacyCompatibleListSpecPropertyType?>,
    ISecondPassSpecPropertyType,
    IListTypeSpecPropertyType,
    IDisposable
{
    public ISecondPassSpecPropertyType InnerType { get; }
    public bool AllowSingleModern { get; }
    public bool AllowSingleLegacy { get; }
    ISpecPropertyType? IListTypeSpecPropertyType.GetInnerType() => InnerType;
    string IElementTypeSpecPropertyType.ElementType => InnerType.Type;

    public string DisplayName => "LegacyCompatibleList";
    public string Type => "LegacyCompatibleList";
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;
    public Type ValueType => typeof(EquatableArray<CustomSpecTypeInstance>);

    public UnresolvedLegacyCompatibleListSpecPropertyType(ISecondPassSpecPropertyType innerType, bool allowSingleModern = false, bool allowSingleLegacy = false)
    {
        InnerType = innerType ?? throw new ArgumentNullException(nameof(innerType));
        AllowSingleModern = allowSingleModern;
        AllowSingleLegacy = allowSingleLegacy;
    }

    public bool Equals(UnresolvedLegacyCompatibleListSpecPropertyType? other) =>
        other != null
        && other.AllowSingleModern == AllowSingleModern
        && other.AllowSingleLegacy == AllowSingleLegacy
        && InnerType.Equals(other.InnerType);

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
        return KnownTypes.LegacyCompatibleList(InnerType.Transform(property, database, assetFile), AllowSingleModern, AllowSingleLegacy);
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) { }
}