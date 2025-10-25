using System.Text;
using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

internal class HoverHandler : IHoverHandler
{
    private readonly FileEvaluationContextFactory _evalFactory;

    /// <inheritdoc />
    HoverRegistrationOptions IRegistration<HoverRegistrationOptions, HoverCapability>.GetRegistrationOptions(
        HoverCapability capability, ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector
        };
    }

    public HoverHandler(FileEvaluationContextFactory evalFactory)
    {
        _evalFactory = evalFactory;
    }

    /// <inheritdoc />
    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        if (!_evalFactory.TryCreate(request.Position, request.TextDocument.Uri, out SpecPropertyTypeParseContext ctx, out ISourceNode? hoverNode) && hoverNode == null)
        {
            return Task.FromResult<Hover?>(null);
        }

        HoverMarkdownBuilder builder = new HoverMarkdownBuilder(new StringBuilder(128));

        FileRange range = hoverNode!.Range;

        SpecProperty? prop = ctx.EvaluationContext.Self;
        if (prop == null)
        {
            builder.UnknownProperty(ctx.BaseKey, ctx.Breadcrumbs);
        }
        else
        {
            ISpecPropertyType? type = prop.Type.GetType(in ctx.EvaluationContext);
            bool hasValue = ctx.EvaluationContext.TryGetValue(out ISpecDynamicValue? propValue);
            ValueHoverProviderResult? result = null;
            if (hasValue && propValue != null && hoverNode is IValueSourceNode && type is IValueHoverProviderSpecPropertyType valueHoverProvider)
                result = valueHoverProvider.GetDescription(in ctx, propValue);

            if (result?.DisplayName != null)
            {
                builder.Value(prop, result, in ctx);
            }
            else
            {
                builder.Property(prop, in ctx, type, propValue, hasValue);
            }
        }

        return Task.FromResult<Hover?>(new Hover
        {
            Range = range.ToRange(),
            Contents = new MarkedStringsOrMarkupContent(new MarkupContent
            {
                Kind = MarkupKind.Markdown,
                Value = builder.ToString()
            })
        });
    }
}

public readonly struct HoverMarkdownBuilder
{
    private readonly StringBuilder _hov;

    public HoverMarkdownBuilder(StringBuilder hov)
    {
        _hov = hov;
    }

    public override string ToString()
    {
        return _hov.ToString();
    }

    public void UnknownProperty(string? propertyName, PropertyBreadcrumbs breadcrumbs)
    {
        _hov.Append(Properties.Resources.Hover_UnknownProperty)
            .Append(": '")
            .Append(breadcrumbs.ToString())
            .Append(propertyName)
            .Append('\'');
    }

    public void Value(SpecProperty prop, ValueHoverProviderResult result, in SpecPropertyTypeParseContext ctx)
    {
        _hov.Append("### ");
        if (result.IsDeprecated)
            _hov.Append("~~");
        _hov.Append(result.DisplayName);
        if (result.IsDeprecated)
            _hov.Append("~~");

        if (!result.CorrespondingType.IsNull)
        {
            _hov.Append(" → ").Append(QualifiedType.ExtractTypeName(result.CorrespondingType.Type));
        }

        if (!string.IsNullOrEmpty(result.Variable))
        {
            _hov.AppendLine().Append('`');
            if (!result.DeclaringType.IsNull)
            {
                _hov.Append(QualifiedType.ExtractTypeName(result.DeclaringType)).Append('.');
            }

            _hov.Append(result.Variable).Append('`');
        }
        else if (!result.DeclaringType.IsNull)
        {
            _hov.AppendLine().Append('`').Append(QualifiedType.ExtractTypeName(result.DeclaringType)).Append('`');
        }

        _hov.AppendLine().AppendLine().Append('-', 3).AppendLine().AppendLine();

        if (!string.IsNullOrEmpty(result.Description))
        {
            _hov.Append(result.Description).AppendLine().AppendLine()
                .Append('-', 3).AppendLine().AppendLine();
        }

        if (!string.IsNullOrEmpty(result.Docs))
        {
            _hov.Append("[").Append(result.LinkName ?? Properties.Resources.Hover_UnturnedDocumentationLinkName).Append("](")
                .Append(result.Docs).Append(')').AppendLine();

            if (prop.Version != null)
            {
                _hov.Append('\\').Append(Properties.Resources.Hover_AddedVersion).Append(" v").Append(prop.Version)
                    .AppendLine();
            }

            _hov.AppendLine()
                .Append('-', 3).AppendLine().AppendLine();
        }
        else if (prop.Version != null)
        {
            _hov.Append('\\').Append(Properties.Resources.Hover_AddedVersion).Append(" v").Append(prop.Version)
                .AppendLine().AppendLine().Append('-', 3).AppendLine().AppendLine();
        }
    }

    public void Property(SpecProperty prop, in SpecPropertyTypeParseContext ctx, ISpecPropertyType? type, ISpecDynamicValue? value, bool hasValue)
    {
        SpecProperty rootProperty = prop;
        while (rootProperty.Parent != null)
            rootProperty = rootProperty.Parent;

        _hov.Append("### ").Append(rootProperty.Owner.DisplayName).Append(" → ").Append(rootProperty.Key);
        if (prop.Variable != null
            && prop.Variable.TryEvaluateValue(in ctx.EvaluationContext, out string? variable, out bool isNull)
            && !isNull
            && !string.IsNullOrEmpty(variable))
        {
            ReadOnlySpan<char> typeName = QualifiedType.ExtractTypeName(rootProperty.Owner.Type.Type);
            _hov.AppendLine().Append('`').Append(typeName).Append('.').Append(variable).Append('`');
        }

        _hov.AppendLine().AppendLine().Append('-', 3).AppendLine().AppendLine();

        if (type != null)
        {
            _hov.Append("**").Append(type.DisplayName).Append("**").AppendLine().AppendLine()
                .Append('-', 3).AppendLine().AppendLine();
        }

        if (prop.Description != null
            && prop.Description.TryEvaluateValue(in ctx.EvaluationContext, out string? description, out isNull)
            && !isNull
            && !string.IsNullOrEmpty(description)
           )
        {
            _hov.Append(description).AppendLine().AppendLine()
                .Append('-', 3).AppendLine().AppendLine();
        }

        if (prop.Docs != null
            && prop.Docs.TryEvaluateValue(in ctx.EvaluationContext, out string? docsLink, out isNull)
            && !isNull
            && !string.IsNullOrEmpty(docsLink))
        {
            _hov.Append("[").Append(Properties.Resources.Hover_UnturnedDocumentationLinkName).Append("](")
                .Append(docsLink).Append(')').AppendLine();
            
            if (prop.Version != null)
            {
                _hov.Append('\\').Append(Properties.Resources.Hover_AddedVersion).Append(" v").Append(prop.Version)
                    .AppendLine();
            }

            _hov.AppendLine()
                .Append('-', 3).AppendLine().AppendLine();
        }
        else if (prop.Version != null)
        {
            _hov.Append('\\').Append(Properties.Resources.Hover_AddedVersion).Append(" v").Append(prop.Version)
                .AppendLine().AppendLine().Append('-', 3).AppendLine().AppendLine();
        }

        if (!hasValue)
        {
            _hov.Append("-# ").Append(Properties.Resources.Hover_InvalidValue);
            return;
        }

        if (value == null)
        {
            _hov.Append(Properties.Resources.Hover_ValueTitle).AppendLine(": **null**");
        }
        else if (value.ValueType is IStringParseableSpecPropertyType sp)
        {
            try
            {
                if (sp.ToString(value) is not { Length: > 0 } str)
                    _hov.Append(Properties.Resources.Hover_ValueTitle).Append(": **null**");
                else
                    _hov.Append(Properties.Resources.Hover_ValueTitle).Append(": `").Append(str).Append('`');
            }
            catch (InvalidCastException)
            {

            }
        }
        else
        {
            switch (ctx.Node)
            {
                case IListSourceNode list:
                    _hov.Append(Properties.Resources.Hover_ListTitle).Append(": [ n = ").Append(list.Count).Append(" ]");
                    break;
                case IDictionarySourceNode dict:
                    _hov.Append(Properties.Resources.Hover_DictionaryTitle).Append(": { n = ").Append(dict.Count).Append(" }");
                    break;
            }
        }
    }
}