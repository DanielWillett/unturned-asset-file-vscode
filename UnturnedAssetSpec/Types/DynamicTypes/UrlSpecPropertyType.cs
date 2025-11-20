using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Linq;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A URL to content hosted on the internet. Can optionally filter by MIME types by passing them as element types.
/// <para>Example: <c>ServerListCurationAsset.IconURL</c></para>
/// <code>
/// Prop https://smartlydressedgames.com/favicon.png
/// </code>
/// <para>
/// Also supports the <c>MinimumCount</c> and <c>MaximumCount</c> properties for character count limits.
/// </para>
/// </summary>
public sealed class UrlSpecPropertyType :
    BaseSpecPropertyType<string>,
    ISpecPropertyType<string>,
    IEquatable<UrlSpecPropertyType?>,
    ISpecialTypesSpecPropertyType
{
    public static readonly UrlSpecPropertyType Instance = new UrlSpecPropertyType(null, OneOrMore<string>.Null);

    public OneOrMore<string> MimeTypes { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "Url";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(string);

    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => MimeTypes!;

    public override int GetHashCode()
    {
        return 84 ^ MimeTypes.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public UrlSpecPropertyType(OneOrMore<string> mimeTypes)
    {
        MimeTypes = mimeTypes;
        DisplayName = GetDisplayName();
    }

    public UrlSpecPropertyType(string? elementType, OneOrMore<string> specialTypes)
    {
        if (!string.IsNullOrEmpty(elementType))
            specialTypes = specialTypes.Add(elementType!);
        
        MimeTypes = specialTypes;
        DisplayName = GetDisplayName();
    }

    private string GetDisplayName()
    {
        // todo: test
        if (MimeTypes.IsNull)
            return "URL";

        // URL (text/plain)
        if (MimeTypes.Length == 1)
            return "URL (" + MimeTypes[0] + ")";
        
        // URL (image/{png,jpg}, application/json)
        StringBuilder sb = new StringBuilder("URL (");
        bool first = true;
        foreach (IGrouping<string, string> grp in MimeTypes.GroupBy(x =>
                 {
                     int ind = x.IndexOf('/');
                     return ind < 0 || ind == x.Length - 1 ? x : x.Substring(0, ind);
                 }))
        {
            if (!first)
                sb.Append(", ");
            else
                first = false;

            int startIndex = sb.Length;
            sb.Append(grp.Key);
            sb.Append("/{");
            string? single = null;
            int ct = 0;
            foreach (string str in grp)
            {
                single = str;
                ++ct;

                int ind = str.IndexOf('/');
                if (ind < 0 || ind == str.Length - 1)
                    continue;

                int paramIndex = str.IndexOf(';');
                if (paramIndex < 0)
                    paramIndex = str.Length;

                if (ct != 1)
                    sb.Append(',');

                sb.Append(str.Substring(ind + 1, paramIndex - ind - 1));
            }

            if (ct == 1)
            {
                sb.Remove(startIndex, sb.Length - startIndex);
                sb.Append(single);
            }
            else
            {
                sb.Append('}');
            }
        }

        return sb.Append(')').ToString();
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
            return MissingNode(in parse, out value);

        if (parse.Node is not IValueSourceNode stringNode)
            return FailedToParse(in parse, out value);

        string val = stringNode.Value;
        if (parse.HasDiagnostics)
        {
            KnownTypeValueHelper.TryGetMinimaxCountWarning(val.Length, in parse);

            if (val.IndexOf('\\') >= 0)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = stringNode.Range,
                    Diagnostic = DatDiagnostics.UNT1010,
                    Message = DiagnosticResources.UNT1010
                });
            }
        }
        
        if (!IsValidWebUrl(val))
        {
            return FailedToParse(in parse, out value);
        }

        value = val;
        return true;
    }

    internal bool IsValidMimeType(string mime)
    {
        // todo
        return MimeTypes.IsNull || MimeTypes.Contains(mime, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidWebUrl(string str)
    {
        if (!Uri.TryCreate(str, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public bool Equals(UrlSpecPropertyType? other) => other != null && MimeTypes.Equals(other.MimeTypes, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is UrlSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<string>? other) => other is UrlSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}