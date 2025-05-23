using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class CommaDelimitedStringSpecPropertyType :
    BaseSpecPropertyType<string>,
    ISpecPropertyType<string>,
    ISecondPassSpecPropertyType,
    IEquatable<CommaDelimitedStringSpecPropertyType>,
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

        value = val == null ? SpecDynamicValue.Null : new SpecDynamicConcreteValue<string>(val, this);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode stringNode)
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
    public bool Equals(CommaDelimitedStringSpecPropertyType other) => other != null && InnerType.Equals(other.InnerType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType other) => other is CommaDelimitedStringSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<string> other) => other is CommaDelimitedStringSpecPropertyType t && Equals(t);

    private abstract class ParseHandler
    {
        public abstract void ProcessDiagnostics(in SpecPropertyTypeParseContext parse, AssetFileStringValueNode stringNode);
    }

    private class ParseHandler<TParseHandler> : ParseHandler where TParseHandler : IEquatable<TParseHandler>
    {
        private readonly ISpecPropertyType<TParseHandler> _parser;

        public ParseHandler(ISpecPropertyType<TParseHandler> parser)
        {
            _parser = parser;
        }

        /// <inheritdoc />
        public override void ProcessDiagnostics(in SpecPropertyTypeParseContext parse, AssetFileStringValueNode stringNode)
        {
            string values = stringNode.Value;
            int lastIndex = -1;
            FilePosition start = stringNode.Range.Start, end = stringNode.Range.End;
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

                AssetFileValueNode node = new AssetFileStringValueNode
                {
                    Value = values.Substring(startIndex, length),
                    Range = new FileRange(start.Line, start.Character + startIndex, end.Line, start.Character + startIndex + length),
                    StartIndex = stringNode.StartIndex + startIndex,
                    EndIndex = stringNode.StartIndex + startIndex + length,
                    IsQuoted = false,
                    Parent = stringNode
                };

                SpecPropertyTypeParseContext ctx = parse with
                {
                    Node = node
                };

                _parser.TryParseValue(in ctx, out _);

                lastIndex = commaIndex;
            }
        }
    }

    public void Dispose()
    {
        if (InnerType is ISecondPassSpecPropertyType and IDisposable disp)
            disp.Dispose();
    }
}