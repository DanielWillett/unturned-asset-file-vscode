using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// When instantiating <see cref="DatCustomType.StringDefaultValue"/>, the value of the string being parsed.
/// </summary>
public sealed class ValueDataRef : RootDataRef<string, ValueDataRef>
{
    /// <summary>
    /// Singleton instance of <see cref="ValueDataRef"/>.
    /// </summary>
    public static readonly ValueDataRef Instance = new ValueDataRef();

    static ValueDataRef() { }
    private ValueDataRef() { }

    /// <inheritdoc />
    public override string PropertyName => "Value";

    protected override bool IsPropertyNameKeyword => true;

    /// <inheritdoc />
    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out Optional<string> value)
    {
        // TODO
        value = Optional<string>.Null;
        return false;
    }

    /// <inheritdoc />
    protected override bool Equals(ValueDataRef other)
    {
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return 1624892613;
    }
}
