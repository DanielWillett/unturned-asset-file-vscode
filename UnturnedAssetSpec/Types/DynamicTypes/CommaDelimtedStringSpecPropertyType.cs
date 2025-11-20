using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A string containing multiple values separated by a comma.
/// <para>Example: <c>Asset.INPCCondition.UI_Requirements</c></para>
/// <code>
/// Prop 1,2,3,4,5
/// </code>
/// <para>
/// Also supports the <c>MinimumCount</c> and <c>MaximumCount</c> properties for list element count limits.
/// </para>
/// </summary>
/// <remarks>A primitive element type such as Int32 can be defined for diagnostics.</remarks>
public sealed class CommaDelimitedStringSpecPropertyType :
    BaseSpecPropertyType<string>,
    ISpecPropertyType<string>,
    ISecondPassSpecPropertyType,
    IElementTypeSpecPropertyType,
    IEquatable<CommaDelimitedStringSpecPropertyType?>,
    IDisposable
{
    private ParseHandler? _handler;

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName => "Comma-Delimited List";

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "CommaDelimitedString";

    /// <inheritdoc />
    public Type ValueType => typeof(string);

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    public ISpecPropertyType InnerType { get; private set; }

    string IElementTypeSpecPropertyType.ElementType => InnerType.Type;

    public override int GetHashCode()
    {
        // note: using string because InnerType could change in Transform()
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return 66 ^ (InnerType.Type?.GetHashCode() ?? 0);
    }

    public CommaDelimitedStringSpecPropertyType(ISpecPropertyType innerType)
    {
        InnerType = innerType;
    }

    /// <inheritdoc />
    public ISpecPropertyType Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile)
    {
        if (InnerType is ISecondPassSpecPropertyType unresolved)
        {
            InnerType = unresolved.Transform(property, database, assetFile);
            if (unresolved is IDisposable disp)
                disp.Dispose();
            _handler = null;
        }

        return this;
    }

    private bool TryCreateParser()
    {
        if (InnerType is ISecondPassSpecPropertyType)
            return false;

        Type type = typeof(ParseHandler<>).MakeGenericType(InnerType.ValueType);
        _handler = (ParseHandler)Activator.CreateInstance(type, InnerType);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out string? val))
        {
            value = null!;
            return false;
        }

        value = val == null ? SpecDynamicValue.Null : new SpecDynamicConcreteConvertibleValue<string>(val, this);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode stringNode)
        {
            return FailedToParse(in parse, out value);
        }

        string val = stringNode.Value;
        if (parse.HasDiagnostics)
        {
            if (_handler != null || TryCreateParser())
                _handler!.ProcessDiagnostics(in parse, stringNode);
        }

        value = val;
        return true;
    }

    /// <inheritdoc />
    public bool Equals(CommaDelimitedStringSpecPropertyType? other) => other != null && InnerType.Equals(other.InnerType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is CommaDelimitedStringSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<string>? other) => other is CommaDelimitedStringSpecPropertyType t && Equals(t);

    private abstract class ParseHandler
    {
        public abstract void ProcessDiagnostics(in SpecPropertyTypeParseContext parse, IValueSourceNode stringNode);
    }

    private class ParseHandler<TParseHandler> : ParseHandler where TParseHandler : IEquatable<TParseHandler>
    {
        private readonly ISpecPropertyType<TParseHandler> _parser;

        public ParseHandler(ISpecPropertyType<TParseHandler> parser)
        {
            _parser = parser;
        }

        /// <inheritdoc />
        public override void ProcessDiagnostics(in SpecPropertyTypeParseContext parse, IValueSourceNode stringNode)
        {
            string values = stringNode.Value;
            int lastIndex = -1;
            FilePosition start = stringNode.Range.Start, end = stringNode.Range.End;
            int index = 0;

            while (lastIndex + 1 < values.Length)
            {
                int commaIndex = values.IndexOf(',', lastIndex + 1);
                if (commaIndex == -1)
                    commaIndex = values.Length;
                if (commaIndex <= lastIndex + 1)
                    continue;

                int endIndex = commaIndex - 1;
                while (endIndex > 0 && char.IsWhiteSpace(values, endIndex))
                {
                    --endIndex;
                }

                int startIndex = lastIndex + 1;
                while (startIndex + 1 < values.Length
                       && (values[startIndex] == ',' || char.IsWhiteSpace(values, startIndex)))
                {
                    ++startIndex;
                }

                int length = endIndex - startIndex;

                if (length <= 0)
                    continue;

                AnySourceNodeProperties props = default;
                props.Index = index;
                ++index;

                props.FirstCharacterIndex = stringNode.FirstCharacterIndex + startIndex;
                props.LastCharacterIndex = props.FirstCharacterIndex + length;
                props.Range = new FileRange(start.Line, start.Character + startIndex, end.Line, start.Character + startIndex + length);
                props.ChildIndex = index;
                props.Depth = stringNode.Depth;

                ValueNode node = ValueNode.Create(values.Substring(startIndex, length), false, Comment.None, in props);
                node.SetParentInfo(stringNode.File, stringNode.Parent);

                SpecPropertyTypeParseContext ctx = parse with
                {
                    Node = node
                };

                _parser.TryParseValue(in ctx, out _);

                lastIndex = commaIndex;
            }

            KnownTypeValueHelper.TryGetMinimaxCountWarning(index, in parse);
        }
    }

    public void Dispose()
    {
        if (InnerType is ISecondPassSpecPropertyType and IDisposable disp)
            disp.Dispose();
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}