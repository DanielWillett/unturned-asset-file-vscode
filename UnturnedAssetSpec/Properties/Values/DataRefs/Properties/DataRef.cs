using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// A data-ref property which can be used with an index in square brackets.
/// </summary>
public interface IIndexableDataRef : IDataRefTarget
{
    /// <summary>
    /// The index given to the value.
    /// </summary>
    int Index { get; set; }
}

/// <summary>
/// A data-ref property which can be provided with arbitrary settings in curly brackets.
/// </summary>
public interface IPropertiesDataRef : IDataRefTarget
{
    /// <summary>
    /// Sets a property as provided by the parser.
    /// </summary>
    /// <param name="property">The name of the property.</param>
    /// <param name="value">The value given to the property.</param>
    void SetProperty(ReadOnlySpan<char> property, ReadOnlySpan<char> value);
    
    /// <summary>
    /// Enumerates through all relevant properties.
    /// </summary>
    IEnumerable<KeyValuePair<string, string>> EnumerateProperties();
}

/// <summary>
/// References a special property of a target, such as the file name or if the target is included or not.
/// </summary>
public abstract class DataRef : IDataRefTarget, IEquatable<DataRef>, IEquatable<ISpecDynamicValue>, IDataRefExpressionNode
{
    /// <summary>
    /// The target of this data-ref property.
    /// </summary>
    public IDataRefTarget Target { get; }

    /// <summary>
    /// The type of value this property supplies, if known.
    /// </summary>
    public ISpecPropertyType? ValueType { get; }

    /// <summary>
    /// The name of this property.
    /// </summary>
    public abstract string PropertyName { get; }

    protected DataRef(IDataRefTarget target, ISpecPropertyType? valueType)
    {
        Target = target;
        ValueType = valueType;
    }

    /// <summary>
    /// Gets a known data-ref target by it's namespace name, or <see langword="null"/> if it's not recognized.
    /// </summary>
    public static IDataRefTarget? FromName(ReadOnlySpan<char> name, string? optionalString = null)
    {
        if (name.IsEmpty)
            return null;

        if (name.Equals("Self".AsSpan(), StringComparison.Ordinal))
        {
            return SelfDataRef.Instance;
        }

        if (name.Equals("This".AsSpan(), StringComparison.Ordinal))
        {
            return ThisDataRef.Instance;
        }

        if (name.Equals("\\Self".AsSpan(), StringComparison.Ordinal))
        {
            return new PropertyDataRef("Self");
        }

        if (name.Equals("\\This".AsSpan(), StringComparison.Ordinal))
        {
            return new PropertyDataRef("This");
        }

        return new PropertyDataRef(optionalString ?? name.ToString());
    }

    /// <summary>
    /// Gets a known data-ref property by it's property name given it's <paramref name="target"/>, or <see langword="null"/> if it's not recognized or supported on this target.
    /// </summary>
    public static IDataRefTarget? FromName(ReadOnlySpan<char> name, IDataRefTarget target, string? optionalString = null)
    {
        if (name.Equals("Excluded".AsSpan(), StringComparison.Ordinal))
        {
            return new ExcludedDataRef(target);
        }

        if (name.Equals("Included".AsSpan(), StringComparison.Ordinal))
        {
            return new IncludedDataRef(target);
        }

        if (name.Equals("Key".AsSpan(), StringComparison.Ordinal))
        {
            return new KeyDataRef(target);
        }

        if (name.Equals("Value".AsSpan(), StringComparison.Ordinal))
        {
            return new ValueDataRef(target);
        }

        if (name.Equals("AssetName".AsSpan(), StringComparison.Ordinal))
        {
            return target is not ThisDataRef ? null : AssetNameDataRef.Instance;
        }

        if (name.Equals("Difficulty".AsSpan(), StringComparison.Ordinal))
        {
            return target is not ThisDataRef ? null : DifficultyDataRef.Instance;
        }

        if (name.Equals("TemplateGroups".AsSpan(), StringComparison.Ordinal))
        {
            return new TemplateGroupsDataRef(target);
        }

        if (name.Equals("IsLegacy".AsSpan(), StringComparison.Ordinal))
        {
            if (target is ThisDataRef or PropertyDataRef { Property.Context: >= SpecPropertyContext.CrossReferenceUnspecified and <= SpecPropertyContext.CrossReferenceLocalization })
            {
                return null;
            }

            return new IsLegacyDataRef(target);
        }

        if (name.Equals("ValueType".AsSpan(), StringComparison.Ordinal))
        {
            if (target is ThisDataRef or PropertyDataRef { Property.Context: >= SpecPropertyContext.CrossReferenceUnspecified and <= SpecPropertyContext.CrossReferenceLocalization })
            {
                return null;
            }

            return new ValueTypeDataRef(target);
        }

        return null;
    }

    public virtual bool Equals(DataRef other)
    {
        if (other == null || !Target.Equals(other.Target))
            return false;

        if (ValueType == null)
            return other.ValueType == null;
        return other.ValueType != null && ValueType.Equals(other.ValueType);
    }

    public bool Equals(IDataRefTarget other) => other is DataRef r && Equals(r);

    public bool Equals(ISpecDynamicValue other) => other is DataRef r && Equals(r);

    public abstract bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition);
    public abstract bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull);
    public abstract bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value);

    bool IDataRefTarget.EvaluateIsIncluded(bool valueIncluded, in FileEvaluationContext ctx) => Target.EvaluateIsIncluded(valueIncluded, in ctx);
    bool IDataRefTarget.EvaluateIsLegacy(in FileEvaluationContext ctx) => Target.EvaluateIsLegacy(in ctx);
    string? IDataRefTarget.EvaluateKey(in FileEvaluationContext ctx) => Target.EvaluateKey(in ctx);
    ISpecDynamicValue? IDataRefTarget.EvaluateValue(in FileEvaluationContext ctx) => Target.EvaluateValue(in ctx);
    int IDataRefTarget.EvaluateTemplateGroup(in FileEvaluationContext ctx, int index) => Target.EvaluateTemplateGroup(in ctx, index);
    ValueTypeDataRefType IDataRefTarget.EvaluateValueType(in FileEvaluationContext ctx) => Target.EvaluateValueType(in ctx);

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        IIndexableDataRef? indexable = this as IIndexableDataRef;
        IPropertiesDataRef? properties = this as IPropertiesDataRef;

        if (indexable == null && properties == null)
        {
            string str = Target + "." + PropertyName;
            str = StringHelper.ContainsWhitespace(str) ? "#(" + str + ")" : ("#" + str);
            writer.WriteStringValue(str);
            return;
        }

        StringBuilder bldr = new StringBuilder(Target.ToString());
        bldr.Append('.')
            .Append(PropertyName);

        if (indexable != null)
        {
            bldr.Append('[')
                .Append(indexable.Index)
                .Append(']');
        }

        if (properties != null)
        {
            using IEnumerator<KeyValuePair<string, string>> props = properties.EnumerateProperties().GetEnumerator();
            if (props.MoveNext())
            {
                bldr.Append('{');
                bool needsComma = false;
                do
                {
                    KeyValuePair<string, string> property = props.Current;
                    if (needsComma)
                        bldr.Append(',');
                    else
                        needsComma = true;
                    bldr.Append(property.Key)
                        .Append('=')
                        .Append(property.Value);
                } while (props.MoveNext());

                bldr.Append('}');
            }
        }

        if (StringHelper.ContainsWhitespace(bldr))
        {
            bldr.Insert(0, "#(");
            bldr.Append(')');
        }
        else
        {
            bldr.Insert(0, '#');
        }

        writer.WriteStringValue(bldr.ToString());
    }

    bool IEquatable<IExpressionNode>.Equals(IExpressionNode? other)
    {
        return other is DataRef dr && Equals(dr);
    }

    public override string ToString() => Target + "." + PropertyName;

    DataRef IDataRefExpressionNode.DataRef => this;
}