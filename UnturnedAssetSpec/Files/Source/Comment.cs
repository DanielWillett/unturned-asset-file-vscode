using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Describes a comment.
/// </summary>
public readonly record struct Comment(CommentPrefix Prefix, string Content, CommentPosition Position)
{
    public static readonly Comment None = default;

    /// <summary>
    /// Additional properties can be defined at the top of the file.
    /// <para>
    /// They must be formatted as a property starting in this value (case-sensitive), followed by a colon, then an optional value.
    /// <code>
    /// // udat-type: SDG.Unturned.ConfigData, Assembly-CSharp
    /// // udat-prop:
    /// Key Value
    /// </code>
    /// </para>
    /// </summary>
    public const string AdditionalPropertyPrefix = "udat-";

    /// <summary>
    /// Case-sensitive <c>udat-type</c> additional property defining a type for the file if it can't be figured out automatically.
    /// For example, server curation files.
    /// </summary>
    /// <remarks>Must go at the top of the file in a comment.</remarks>
    public const string TypeAdditionalProperty = "type";

    /// <summary>
    /// Total length of this comment's prefix and content.
    /// </summary>
    public int Length => Prefix.Length + Content.Length;

    public static Comment AdditionalProperty(string propertyName, object? value)
    {
        if (value == null)
        {
            return new Comment(CommentPrefix.Default, AdditionalPropertyPrefix + propertyName + ":", CommentPosition.NewLine);
        }
        if (value is not string str)
            str = value.ToString();

        return new Comment(CommentPrefix.Default, AdditionalPropertyPrefix + propertyName + ": " + str, CommentPosition.NewLine);
    }

    public bool Equals(Comment comment)
    {
        return Position == comment.Position && Prefix.Equals(comment.Prefix) && string.Equals(Content, comment.Content, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return (Prefix.GetHashCode() ^ Content.GetHashCode()) + (int)Position * 397;
    }

    public bool TryWrite(Span<char> span, out int charsWritten)
    {
        if (span.Length < Prefix.Length + Content.Length)
        {
            charsWritten = 0;
            return false;
        }

        if (!Prefix.TryWrite(span, out charsWritten))
        {
            return false;
        }

        Content.AsSpan().CopyTo(span.Slice(charsWritten));
        charsWritten += Content.Length;
        return true;
    }

    public override string ToString()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return string.Create(Prefix.Length + Content.Length, this, (span, state) =>
        {
            state.TryWrite(span, out _);
        });
#else
        Span<char> chars = stackalloc char[Prefix.Length + Content.Length];
        TryWrite(chars, out _);
        return chars.ToString();
#endif
    }

    public bool TryParseAsAdditionalProperty(out KeyValuePair<string, object?> property)
    {
        property = default;
        if (!Content.StartsWith(AdditionalPropertyPrefix, StringComparison.Ordinal))
            return false;

        int index = Content.IndexOf(':');
        if (index <= 5 || index >= Content.Length)
            return false;

        int endPropIndex = index;
        while (endPropIndex >= 5 && char.IsWhiteSpace(Content[endPropIndex - 1]))
        {
            --endPropIndex;
        }

        if (endPropIndex <= 5 || endPropIndex >= Content.Length)
            return false;

        string key = Content.Substring(5, endPropIndex - 5);
        while (index < Content.Length - 1 && char.IsWhiteSpace(Content[index + 1]))
            ++index;

        if (index >= Content.Length - 1)
            property = new KeyValuePair<string, object?>(key, null);
        else
        {
            int lastIndex = Content.Length - 1;
            while (lastIndex > index && char.IsWhiteSpace(Content[lastIndex]))
                --lastIndex;

            string? value = lastIndex > index + 1 ? Content.Substring(index + 1, lastIndex - index) : null;
            property = new KeyValuePair<string, object?>(key, value);
        }

        return true;
    }
}

/// <summary>
/// Describes how to replicate a comment prefix.
/// </summary>
/// <param name="Slashes">The number of slashes present in the prefix ('// Comment' = 2).</param>
/// <param name="Spaces">The number of spaces present after the prefix ('// Comment' = 1).</param>
public readonly record struct CommentPrefix(int Slashes, int Spaces)
{
    // common values
    private const string _2_0 = "//";
    private const string _2_1 = "// ";
    private const string _2_2 = "//  ";
    private const string _1_0 = "/";
    private const string _1_1 = "/ ";
    private const string _1_2 = "/  ";

    /// <summary>
    /// The '// ' comment prefix.
    /// </summary>
    public static readonly CommentPrefix Default = new CommentPrefix(2, 1);

    /// <summary>
    /// Total length of the prefix as a string.
    /// </summary>
    public int Length => Slashes + Spaces;

    public bool TryWrite(Span<char> span, out int charsWritten)
    {
        int len = Slashes + Spaces;
        if (len > span.Length)
        {
            charsWritten = 0;
            return false;
        }

        span.Slice(0, Slashes).Fill('/');
        span.Slice(Slashes).Fill(' ');
        charsWritten = len;
        return true;
    }

    public override string ToString()
    {
        switch (Slashes)
        {
            case 1:
                switch (Spaces)
                {
                    case 0: return _1_0;
                    case 1: return _1_1;
                    case 2: return _1_2;
                }
                break;
            case 2:
                switch (Spaces)
                {
                    case 0: return _2_0;
                    case 1: return _2_1;
                    case 2: return _2_2;
                }
                break;
        }

        if (Spaces == 0)
            return new string('/', Slashes);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return string.Create(Slashes + Spaces, this, (span, state) =>
        {
            span.Slice(0, state.Slashes).Fill('/');
            span.Slice(state.Slashes).Fill(' ');
        });
#else
        Span<char> chars = stackalloc char[Slashes + Spaces];
        chars.Slice(0, Slashes).Fill('/');
        chars.Slice(Slashes).Fill(' ');
        return chars.ToString();
#endif
    }
}

/// <summary>
/// Indicates where relative to the content of the node the comment is at.
/// </summary>
public enum CommentPosition
{
    /// <summary>
    /// On it's own line.
    /// <code>
    /// // Comment
    /// Key Value
    /// </code>
    /// </summary>
    NewLine,

    /// <summary>
    /// At the end of the property. If the property is a list or dictionary this would be after the closing bracket.
    /// <code>
    /// Key
    /// {
    /// } // Comment
    /// </code>
    /// or
    /// <code>
    /// Key "Value" // Comment
    /// </code>
    /// </summary>
    EndOfLine,

    /// <summary>
    /// After the opening bracket. This is only valid for lists and dictionaries.
    /// <code>
    /// Key
    /// { // Comment
    /// }
    /// </code>
    /// </summary>
    AfterOpeningBracket
}