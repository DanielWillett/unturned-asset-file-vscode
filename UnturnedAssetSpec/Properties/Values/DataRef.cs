using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public interface IIndexableDataRef : IDataRefTarget
{
    int Index { get; set; }
}

public interface IPropertiesDataRef : IDataRefTarget
{
    void SetProperty(ReadOnlySpan<char> property, ReadOnlySpan<char> value);
    IEnumerable<KeyValuePair<string, string>> EnumerateProperties();
}

/// <summary>
/// References a special property of a target, such as the file name or if the target is included or not.
/// </summary>
public abstract class DataRef : IDataRefTarget, IEquatable<DataRef>, IEquatable<ISpecDynamicValue>
{
    public IDataRefTarget Target { get; }
    public ISpecPropertyType? ValueType { get; }

    public abstract string PropertyName { get; }

    protected DataRef(IDataRefTarget target, ISpecPropertyType? valueType)
    {
        Target = target;
        ValueType = valueType;
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
    string? IDataRefTarget.EvaluateKey(in FileEvaluationContext ctx) => Target.EvaluateKey(in ctx);
    ISpecDynamicValue? IDataRefTarget.EvaluateValue(in FileEvaluationContext ctx) => Target.EvaluateValue(in ctx);
    int IDataRefTarget.EvaluateKeyGroup(in FileEvaluationContext ctx, int index) => Target.EvaluateKeyGroup(in ctx, index);

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
            }

            bldr.Append('}');
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

    public override string ToString() => Target + "." + PropertyName;
}

public sealed class IncludedDataRef : DataRef, IEquatable<IncludedDataRef>
{
    public IncludedDataRef(IDataRefTarget target) : base(target, KnownTypes.Flag) { }

    public override string PropertyName => "Included";

    public override bool Equals(DataRef other) => other is IncludedDataRef b && Equals(b);

    public bool Equals(IncludedDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not bool v)
        {
            return condition.EvaluateNulls(false, true);
        }

        return condition.Evaluate(
            Target.EvaluateIsIncluded(false, in ctx),
            v,
            ctx.Information.Information
        );
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        if (typeof(TValue) != typeof(bool))
        {
            if (typeof(TValue) == typeof(string))
            {
                string vStr = Target.EvaluateIsIncluded(false, in ctx).ToString();
                value = Unsafe.As<string, TValue>(ref vStr);
                return true;
            }

            value = default!;
            return false;
        }

        bool v = Target.EvaluateIsIncluded(false, in ctx);
        value = Unsafe.As<bool, TValue>(ref v);
        return true;
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = Target.EvaluateIsIncluded(false, in ctx);
        return true;
    }
}

public sealed class ExcludedDataRef : DataRef, IEquatable<ExcludedDataRef>
{
    public ExcludedDataRef(IDataRefTarget target) : base(target, KnownTypes.Flag) { }

    public override string PropertyName => "Excluded";

    public override bool Equals(DataRef other) => other is ExcludedDataRef b && Equals(b);

    public bool Equals(ExcludedDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not bool v)
        {
            return condition.EvaluateNulls(false, true);
        }

        return condition.Evaluate(!Target.EvaluateIsIncluded(false, in ctx), v, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        if (typeof(TValue) != typeof(bool))
        {
            if (typeof(TValue) == typeof(string))
            {
                string vStr = (!Target.EvaluateIsIncluded(false, in ctx)).ToString();
                value = Unsafe.As<string, TValue>(ref vStr);
                return true;
            }

            value = default!;
            return false;
        }

        bool v = !Target.EvaluateIsIncluded(false, in ctx);
        value = Unsafe.As<bool, TValue>(ref v);
        return true;
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = !Target.EvaluateIsIncluded(false, in ctx);
        return true;
    }
}

public sealed class KeyDataRef : DataRef, IEquatable<KeyDataRef>
{
    public KeyDataRef(IDataRefTarget target) : base(target, KnownTypes.String) { }

    public override string PropertyName => "Key";

    public override bool Equals(DataRef other) => other is KeyDataRef b && Equals(b);

    public bool Equals(KeyDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not string v)
        {
            return condition.EvaluateNulls(false, true);
        }

        return condition.Evaluate(Target.EvaluateKey(in ctx), v, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        if (typeof(TValue) != typeof(string))
        {
            isNull = false;
            value = default!;
            return false;
        }

        string? v = Target.EvaluateKey(in ctx);
        value = Unsafe.As<string?, TValue>(ref v);
        isNull = v == null;
        return true;
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = Target.EvaluateKey(in ctx);
        return value != null;
    }
}

public sealed class ValueDataRef : DataRef, IEquatable<ValueDataRef>
{
    public ValueDataRef(IDataRefTarget target) : base(target, KnownTypes.String) { }

    public override string PropertyName => "Value";

    public override bool Equals(DataRef other) => other is ValueDataRef b && Equals(b);

    public bool Equals(ValueDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        ISpecDynamicValue? val = Target.EvaluateValue(in ctx);
        if (val == null)
        {
            return condition.EvaluateNulls(true, condition.Comparand == null);
        }

        return val.EvaluateCondition(in ctx, in condition);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        return Target.TryEvaluateValue(in ctx, out value!, out isNull);
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        return Target.TryEvaluateValue(in ctx, out value);
    }
}

public sealed class KeyGroupsDataRef : DataRef, IEquatable<KeyGroupsDataRef>, IIndexableDataRef, IPropertiesDataRef
{
    public bool PreventSelfReference { get; set; }

    public int Index { get; set; }

    public KeyGroupsDataRef(IDataRefTarget target) : base(target, KnownTypes.String) { }

    public override string PropertyName => "KeyGroups";

    public override bool Equals(DataRef other) => other is KeyGroupsDataRef b && Equals(b);

    public bool Equals(KeyGroupsDataRef other) => base.Equals(other) && PreventSelfReference == other.PreventSelfReference && Index == other.Index;

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        int keyGroupValue = Target.EvaluateKeyGroup(in ctx, Index);
        if (condition.Comparand is not int expectedValue)
        {
            return condition.EvaluateNulls(keyGroupValue < 0, true);
        }

        if (keyGroupValue < 0)
        {
            return condition.EvaluateNulls(true, false);
        }

        return condition.Evaluate(keyGroupValue, expectedValue, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        int v = Target.EvaluateKeyGroup(in ctx, Index);
        if (v < 0)
        {
            value = default!;
            isNull = true;
            return true;
        }

        isNull = false;
        if (typeof(TValue) != typeof(int))
        {
            if (typeof(TValue) == typeof(string))
            {
                value = SpecDynamicEquationTreeValueHelpers.As<string, TValue>(v.ToString());
                return true;
            }

            value = default!;
            return false;
        }

        value = Unsafe.As<int, TValue>(ref v);
        return true;
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        int v = Target.EvaluateKeyGroup(in ctx, Index);
        if (v < 0)
        {
            value = null;
            return true;
        }

        value = v;
        return true;
    }

    public void SetProperty(ReadOnlySpan<char> property, ReadOnlySpan<char> value)
    {
        if (property.Equals(nameof(PreventSelfReference).AsSpan(), StringComparison.Ordinal))
        {
            PreventSelfReference = value.Equals("true".AsSpan(), StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public IEnumerable<KeyValuePair<string, string>> EnumerateProperties()
    {
        yield return new KeyValuePair<string, string>(nameof(PreventSelfReference), PreventSelfReference ? "true" : "false");
    }
}

public sealed class AssetNameDataRef : DataRef, IEquatable<AssetNameDataRef>
{
    public AssetNameDataRef(IDataRefTarget target) : base(target, KnownTypes.String)
    {
        if (target is not ThisDataRef)
            throw new ArgumentException("AssetName can only be used with #This.");
    }

    public override string PropertyName => "AssetName";

    public override bool Equals(DataRef other) => other is AssetNameDataRef b && Equals(b);

    public bool Equals(AssetNameDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        TryEvaluateValue(in ctx, out string? assetName, out bool isNull);

        if (condition.Comparand is not string str)
        {
            if (condition.Comparand == null)
                return condition.EvaluateNulls(isNull, true);
            str = condition.Comparand.ToString();
        }

        return isNull
            ? condition.EvaluateNulls(true, false)
            : condition.Evaluate(assetName, str, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        if (typeof(TValue) != typeof(string))
        {
            value = default!;
            return false;
        }

        try
        {
            string? name = ctx.OpenedFile.AssetName;
            if (string.IsNullOrEmpty(name))
            {
                name = null;
                isNull = true;
            }
            value = Unsafe.As<string?, TValue>(ref name);
            return true;
        }
        catch
        {
            value = default!;
            isNull = true;
            return true;
        }
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        try
        {
            string? name = ctx.OpenedFile.AssetName;
            value = string.IsNullOrEmpty(name) ? null : name;
            return true;
        }
        catch
        {
            value = null;
            return true;
        }
    }
}