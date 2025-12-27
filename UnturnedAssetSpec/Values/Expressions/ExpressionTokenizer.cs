using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;


/// <summary>
/// Handles parsing expression strings into tokens.
/// </summary>
internal ref struct ExpressionTokenizer
{
    private readonly ReadOnlySpan<char> _expr;
    private int _index;
    private int _depth;

    public ExpressionToken Current;

    public ExpressionTokenizer(ReadOnlySpan<char> expr)
    {
        _expr = expr;
        Reset();
    }

    public void Reset()
    {
        _index = -1;
        _depth = 0;
    }

    public bool MoveNext()
    {
        if (_index + 1 >= _expr.Length)
        {
            if (_depth > 0)
                throw new FormatException(Resources.FormatException_Expression_MissingEndingParenthesis);
            return _expr.IsEmpty
                ? throw new FormatException(Resources.FormatException_Expression_ExpectedFunctionName)
                : false;
        }

        ++_index;

        ReadOnlySpan<char> remaining = _expr.Slice(_index);

        switch (Current.Type)
        {
            case ExpressionTokenType.OpenParams:
            case ExpressionTokenType.ArgumentSeparator:
                bool hadEscapeSequences;
                if (remaining.IsEmpty)
                {
                    throw new FormatException(Resources.FormatException_Expression_ExpectedNextArgument);
                }

                // parenthesized value
                ExpressionValueType valueType = ExpressionValueType.Value;
                int valueStartIndex;
                int valueLength;
                if (remaining[0] is '(' or '@' or '=' or '#' or '%')
                {
                    int pInd = -1;
                    if (remaining[0] == '(')
                    {
                        pInd = 0;
                    }
                    else if (remaining.Length > 1)
                    {
                        valueType = remaining[0] switch
                        {
                            '@' => ExpressionValueType.PropertyRef,
                            '#' => ExpressionValueType.DataRef,
                            '=' => ExpressionValueType.Expression,
                            _ => 0
                        };
                        if (remaining[1] == '(')
                        {
                            pInd = 1;
                        }
                    }

                    if (pInd >= 0 && remaining.Length > pInd)
                    {
#if NET7_0_OR_GREATER
                        ReadOnlySpan<char> valueEndIdentifiers = [ '(', ')', '\\' ];
#else
                        ReadOnlySpan<char> valueEndIdentifiers = stackalloc char[] { '(', ')', '\\' };
#endif
                        valueStartIndex = pInd + 1;
                        int valueEndIndex = StringHelper.NextUnescapedIndexOf(remaining.Slice(valueStartIndex), valueEndIdentifiers, out hadEscapeSequences, useDepth: true);
                        if (valueEndIndex < 0)
                        {
                            throw new FormatException(Resources.FormatException_Expression_ExpectedParenthesizedValueEnd);
                        }

                        valueLength = valueEndIndex;

                        valueEndIndex += valueStartIndex;
                        if (valueEndIndex + 1 >= remaining.Length || remaining[valueEndIndex + 1] is not ' ' and not ')')
                        {
                            throw new FormatException(Resources.FormatException_Expression_ExpectedParenthesizedValueEnd);
                        }
                    }
                    else
                    {
#if NET7_0_OR_GREATER
                        ReadOnlySpan<char> valueEndIdentifiers = [ ' ', ')', '\\' ];
#else
                        ReadOnlySpan<char> valueEndIdentifiers = stackalloc char[] { ' ', ')', '\\' };
#endif
                        int valueEndIndex = StringHelper.NextUnescapedIndexOf(remaining.Slice(1), valueEndIdentifiers, out hadEscapeSequences, useDepth: false);
                        if (valueEndIndex < 0)
                        {
                            throw new FormatException(Resources.FormatException_Expression_ExpectedParenthesizedValueEnd);
                        }

                        valueLength = valueEndIndex;
                        valueStartIndex = 1;
                    }
                }
                else
                {
#if NET7_0_OR_GREATER
                    ReadOnlySpan<char> valueEndIdentifiers = [ ' ', ')', '\\' ];
#else
                    ReadOnlySpan<char> valueEndIdentifiers = stackalloc char[] { ' ', ')', '\\' };
#endif
                    int valueEndIndex = StringHelper.NextUnescapedIndexOf(remaining, valueEndIdentifiers, out hadEscapeSequences, useDepth: false);
                    if (valueEndIndex < 0)
                    {
                        throw new FormatException(Resources.FormatException_Expression_ExpectedParenthesizedValueEnd);
                    }

                    valueStartIndex = 0;
                    valueLength = valueEndIndex;
                }
                
                if (valueType == ExpressionValueType.Expression)
                {
                    Current.Type = ExpressionTokenType.Unknown;
                    remaining = remaining.Slice(valueStartIndex);
                    _index += valueStartIndex;
                    goto default;
                }

                ReadOnlySpan<char> value = remaining.Slice(valueStartIndex, valueLength);

                if (hadEscapeSequences)
                {
                    Current.SetContent(ExpressionTokenType.Value, StringHelper.Unescape(value));
                }
                else
                {
                    Current.SetContent(ExpressionTokenType.Value, value);
                }

                Current.ValueType = valueType;
                _index += valueStartIndex + valueLength - 1;
                return true;

            default:

#if NET7_0_OR_GREATER
                ReadOnlySpan<char> funcNameEndIdentifiers = [ '(', ' ', ')', '\\' ];
#else
                ReadOnlySpan<char> funcNameEndIdentifiers = stackalloc char[] { '(', ' ', ')', '\\' };
#endif

                int parenIndex = StringHelper.NextUnescapedIndexOf(remaining, funcNameEndIdentifiers, out hadEscapeSequences, useDepth: true);
                if (parenIndex == -1 && _depth > 0)
                    throw new FormatException(Resources.FormatException_Expression_ExpectedFunctionStart);

                ReadOnlySpan<char> function = parenIndex == -1 ? remaining : remaining.Slice(0, parenIndex);

                if (hadEscapeSequences)
                {
                    Current.SetContent(ExpressionTokenType.FunctionName, StringHelper.Unescape(function));
                }
                else
                {
                    Current.SetContent(ExpressionTokenType.FunctionName, function);
                }

                _index = (parenIndex == -1 ? _expr.Length : parenIndex + _index) - 1;
                return true;

            case ExpressionTokenType.FunctionName:
                // constant function (0-arg)
                if (remaining.IsEmpty)
                {
                    if (_depth > 0)
                        throw new FormatException(Resources.FormatException_Expression_ExpectedFunctionStart);

                    return false;
                }

                if (_depth > 0)
                {
                    switch (remaining[0])
                    {
                        case ' ':
                            Current.SetContent(ExpressionTokenType.ArgumentSeparator, remaining.Slice(0, 1));
                            return true;

                        case ')':
                            Current.SetContent(ExpressionTokenType.CloseParams, remaining.Slice(0, 1));
                            --_depth;
                            return true;
                    }
                }

                if (remaining[0] != '(')
                    throw new FormatException(Resources.FormatException_Expression_ExpectedFunctionStart);

                Current.SetContent(ExpressionTokenType.OpenParams, remaining.Slice(0, 1));
                ++_depth;
                return true;
            
            case ExpressionTokenType.CloseParams:
            case ExpressionTokenType.Value:
                if (_depth <= 0)
                    return false;

                if (remaining.IsEmpty || remaining[0] is not ' ' and not ')')
                {
                    throw new FormatException(Resources.FormatException_Expression_ExpectedNextArgument);
                }

                switch (remaining[0])
                {
                    case ' ':
                        Current.SetContent(ExpressionTokenType.ArgumentSeparator, remaining.Slice(0, 1));
                        return true;

                    case ')':
                        Current.SetContent(ExpressionTokenType.CloseParams, remaining.Slice(0, 1));
                        --_depth;
                        return true;
                }

                return false;
        }
    }

    public void Dispose() { }
}

internal ref struct ExpressionToken
{
    public ExpressionTokenType Type;
    public ExpressionValueType ValueType;
    public ReadOnlySpan<char> Content;
    private string? _contentString;

    public void SetContent(ExpressionTokenType type, ReadOnlySpan<char> content)
    {
        Content = content;
        _contentString = null;
        Type = type;
        ValueType = ExpressionValueType.Expression;
    }

    public void SetContent(ExpressionTokenType type, string content)
    {
        Content = content;
        _contentString = content;
        Type = type;
        ValueType = ExpressionValueType.Expression;
    }

    public string GetContent()
    {
        return _contentString ??= Content.ToString();
    }
}

internal enum ExpressionValueType
{
    /// <summary>
    /// A literal value [%].
    /// </summary>
    Value,

    /// <summary>
    /// A reference to another property (@).
    /// </summary>
    PropertyRef,

    /// <summary>
    /// A 'data-ref' statement (#).
    /// </summary>
    DataRef,

    /// <summary>
    /// Another expression (=).
    /// </summary>
    Expression
}

internal enum ExpressionTokenType
{
    /// <summary>
    /// Token type of an uninitialized <see cref="ExpressionTokenizer"/>.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Name of the function.
    /// </summary>
    FunctionName,

    /// <summary>
    /// Token placed before any arguments in a function.
    /// </summary>
    OpenParams,

    /// <summary>
    /// Token placed after all arguments in a function.
    /// </summary>
    CloseParams,
    
    /// <summary>
    /// Token placed in-between arguments.
    /// </summary>
    ArgumentSeparator,
    
    /// <summary>
    /// Some other value surrounded in parenthesis.
    /// </summary>
    Value
}