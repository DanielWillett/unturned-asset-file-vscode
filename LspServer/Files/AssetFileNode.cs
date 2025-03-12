using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace LspServer.Files;

public abstract class AssetFileNode
{
    protected const int IndentMultiplier = 4;

    public AssetFileNode? Parent { get; internal set; }
    public required int StartIndex { get; init; }
    public required int EndIndex { get; init; }
    public required Range Range { get; init; }

    public abstract override string ToString();
    public abstract void WriteTo(TextWriter writer, ref int indent, ref bool hasNewLine);
    
    protected void WriteIndentTo(TextWriter writer, int indent)
    {
        for (int i = indent * IndentMultiplier; i > 0; --i)
            writer.Write(' ');
    }
}

public class AssetFileKeyValuePairNode : AssetFileNode
{
    public required AssetFileValueNode? Value { get; init; }
    public required AssetFileKeyNode Key { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return Key.ToString();
        return Key + (Value is AssetFileBlockValueNode { Value: not null } ? string.Empty : " ") + Value;
    }

    /// <inheritdoc />
    public override void WriteTo(TextWriter writer, ref int indent, ref bool hasNewLine)
    {
        if (!hasNewLine)
        {
            hasNewLine = true;
            writer.WriteLine();
        }
        WriteIndentTo(writer, indent);
        Key.WriteTo(writer, ref indent, ref hasNewLine);
        if (Value == null)
            return;
        if (Value is AssetFileStringValueNode)
            writer.Write(' ');
        Value.WriteTo(writer, ref indent, ref hasNewLine);
    }
}

public class AssetFileKeyNode : AssetFileNode
{
    public required bool IsQuoted { get; init; }
    public required string Value { get; init; }

    /// <inheritdoc />
    public override string ToString() => IsQuoted ? "\"" + Value + "\"" : Value;

    /// <inheritdoc />
    public override void WriteTo(TextWriter writer, ref int indent, ref bool hasNewLine)
    {
        if (IsQuoted)
            writer.Write('\"');
        writer.Write(Value);
        if (IsQuoted)
            writer.Write('\"');
        hasNewLine = false;
    }
}

public abstract class AssetFileValueNode : AssetFileNode;

public abstract class AssetFileBlockValueNode : AssetFileValueNode
{
    /// <summary>
    /// The extra value that is skipped before a block. Ex: "Key Value \n {...".
    /// </summary>
    public AssetFileStringValueNode? Value { get; init; }

    /// <summary>
    /// If the block was properly closed.
    /// </summary>
    public required bool IsClosed { get; init; }
}

public class AssetFileDictionaryValueNode : AssetFileBlockValueNode
{
    public required bool IsRoot { get; init; }
    public required IList<AssetFileKeyValuePairNode> Pairs { get; init; }

    public bool ContainsKey(string key)
    {
        for (int i = 0; i < Pairs.Count; ++i)
        {
            AssetFileKeyValuePairNode pair = Pairs[i];
            if (pair.Key.Value.Equals(key, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out AssetFileValueNode value)
    {
        for (int i = 0; i < Pairs.Count; ++i)
        {
            AssetFileKeyValuePairNode pair = Pairs[i];
            if (!pair.Key.Value.Equals(key, StringComparison.OrdinalIgnoreCase))
                continue;
            value = pair.Value;
            return value != null;
        }

        value = null;
        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        string str = "{" + string.Join(", ", Pairs) + "}";
        if (Value != null)
            return Value + str;

        return str;
    }

    /// <inheritdoc />
    public override void WriteTo(TextWriter writer, ref int indent, ref bool hasNewLine)
    {
        if (!hasNewLine)
            writer.WriteLine();
        
        if (!IsRoot)
        {
            WriteIndentTo(writer, indent);
            writer.Write('{');
            hasNewLine = false;

            ++indent;
        }
        else
        {
            hasNewLine = true;
        }

        foreach (AssetFileKeyValuePairNode pair in Pairs)
        {
            pair.WriteTo(writer, ref indent, ref hasNewLine);
        }

        hasNewLine = false;

        if (IsRoot)
            return;

        --indent;
        writer.WriteLine();
        WriteIndentTo(writer, indent);
        writer.Write('}');
    }
}
public class AssetFileListValueNode : AssetFileBlockValueNode
{
    public required IList<AssetFileValueNode> Elements { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        string str = "[" + string.Join(", ", Elements) + "]";
        if (Value != null)
            return Value + str;

        return str;
    }

    /// <inheritdoc />
    public override void WriteTo(TextWriter writer, ref int indent, ref bool hasNewLine)
    {
        if (!hasNewLine)
            writer.WriteLine();
        
        WriteIndentTo(writer, indent);
        writer.Write('[');
        hasNewLine = false;

        ++indent;
        foreach (AssetFileValueNode element in Elements)
        {
            if (!hasNewLine && element is AssetFileStringValueNode)
            {
                hasNewLine = true;
                writer.WriteLine();
            }

            element.WriteTo(writer, ref indent, ref hasNewLine);
        }

        --indent;
        writer.WriteLine();
        WriteIndentTo(writer, indent);
        writer.Write(']');
        hasNewLine = false;
    }
}

public class AssetFileStringValueNode : AssetFileValueNode
{
    public required bool IsQuoted { get; init; }
    public required string Value { get; init; }

    /// <inheritdoc />
    public override string ToString() => IsQuoted ? "\"" + Value + "\"" : Value;

    /// <inheritdoc />
    public override void WriteTo(TextWriter writer, ref int indent, ref bool hasNewLine)
    {
        if (IsQuoted)
            writer.Write('\"');
        writer.Write(Value);
        if (IsQuoted)
            writer.Write('\"');
        hasNewLine = false;
    }
}