
using System.Runtime.CompilerServices;
using System.Text;
using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
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
        if (!_evalFactory.TryCreate(
                request.Position,
                request.TextDocument.Uri,
                out FileEvaluationContext ctx,
                out DatProperty? property,
                out IPropertySourceNode? propertyNode,
                out ISourceNode? hoverNode
            ) && hoverNode == null)
        {
            return Task.FromResult<Hover?>(null);
        }

        HoverMarkdownBuilder builder = new HoverMarkdownBuilder(new StringBuilder(128), propertyNode);

        FileRange range = hoverNode.Range;

        if (property == null)
        {
            builder.UnknownProperty(ref ctx);
        }
        else if (!property.Type.TryEvaluateType(out IType? type, ref ctx))
        {
            builder.UnknownValue(property, type, ref ctx);
        }
        else
        {
            scoped MetadataVisitor metadataVisitor;
            metadataVisitor.Context = ref ctx;
            metadataVisitor.Builder = ref builder;
            metadataVisitor.Property = property;
            metadataVisitor.IsOnValue = hoverNode is IAnyValueSourceNode;
            metadataVisitor.Appended = false;
            metadataVisitor.ValueNode = propertyNode?.Value;

            property.VisitValue(
                ref metadataVisitor,
                ref ctx,
                missingValueBahvior: TypeParserMissingValueBehavior.FallbackToDefaultValue
            );

            if (!metadataVisitor.Appended)
            {
                type.Visit(ref metadataVisitor);
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

    private ref struct MetadataVisitor : IValueVisitor, ITypeVisitor
    {
        public ref HoverMarkdownBuilder Builder;
        public DatProperty Property;
        public ref FileEvaluationContext Context;
        public IAnyValueSourceNode? ValueNode;
        public bool Appended;
        public bool IsOnValue;

        public void Accept<TValue>(IType<TValue> type, Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            Appended = true;

            if (IsOnValue)
            {
                ValueMetadata<TValue> metadata = type.CreateMetadata(
                    value,
                    ValueNode,
                    ref Context,
                    out bool metadataIsPopulated
                );
                if (metadataIsPopulated)
                {
                    Builder.Value(Property, metadata, ref Context);
                    return;
                }
            }

            Builder.Property(Property, ref Context, type, value, true);
        }

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            // fallback
            Builder.Property(Property, ref Context, type, Optional<TValue>.Null, false);
            Appended = true;
        }
    }
}

public readonly struct HoverMarkdownBuilder
{
    private readonly StringBuilder _hov;
    private readonly IPropertySourceNode? _node;

    public HoverMarkdownBuilder(StringBuilder hov, IPropertySourceNode? node)
    {
        _hov = hov;
        _node = node;
    }

    public override string ToString()
    {
        return _hov.ToString();
    }

    public void UnknownProperty(ref FileEvaluationContext ctx)
    {
        _hov.Append(Properties.Resources.Hover_UnknownProperty)
            .Append(": '")
            .Append(_node == null ? ctx.RootBreadcrumbs.ToString(true) : ctx.RootBreadcrumbs.ToString(false, _node.Key))
            .Append('\'');
    }

    public void UnknownValue(DatProperty prop, IType? type, ref FileEvaluationContext ctx, bool hasValue = false)
    {
        _hov.Append("### ").Append(prop.Owner.DisplayName).Append(" → ").Append(prop.Key);
        if (prop.Variable != null
            && prop.Variable.TryEvaluateValue(out Optional<string> variable, ref ctx)
            && variable.HasValue
            && !string.IsNullOrEmpty(variable.Value))
        {
            ReadOnlySpan<char> typeName = prop.Owner.TypeName.GetTypeName();
            _hov.AppendLine().Append('`').Append(typeName).Append('.').Append(variable.Value).Append('`');
        }

        _hov.AppendLine().AppendLine().Append('-', 3).AppendLine().AppendLine();

        if (type != null)
        {
            _hov.Append("**").Append(type.DisplayName).Append("**").AppendLine().AppendLine()
                .Append('-', 3).AppendLine().AppendLine();
        }

        if (prop.Description != null
            && prop.Description.TryEvaluateValue(out Optional<string> description, ref ctx)
            && description.HasValue
            && !string.IsNullOrEmpty(description.Value)
           )
        {
            _hov.Append(description.Value).AppendLine().AppendLine()
                .Append('-', 3).AppendLine().AppendLine();
        }

        if (prop.Docs != null
            && prop.Docs.TryEvaluateValue(out Optional<string> docsLink, ref ctx)
            && docsLink.HasValue
            && !string.IsNullOrEmpty(docsLink.Value))
        {
            _hov.Append("[").Append(Properties.Resources.Hover_UnturnedDocumentationLinkName).Append("](")
                .Append(docsLink.Value).Append(')').AppendLine();

            if (prop.Version != null && prop.Version.TryEvaluateValue(out Optional<Version> version, ref ctx) && version.Value is not null)
            {
                _hov.Append('\\').AppendLine().Append(Properties.Resources.Hover_AddedVersion).Append(" v").Append(version.Value)
                    .AppendLine();
            }

            _hov.AppendLine()
                .Append('-', 3).AppendLine().AppendLine();
        }
        else if (prop.Version != null && prop.Version.TryEvaluateValue(out Optional<Version> version, ref ctx) && version.Value is not null)
        {
            _hov.Append('\\').AppendLine().Append(Properties.Resources.Hover_AddedVersion).Append(" v").Append(version.Value)
                .AppendLine().AppendLine().Append('-', 3).AppendLine().AppendLine();
        }

        if (hasValue)
            return;
        _hov.Append("-# ").Append(Properties.Resources.Hover_InvalidValue);
    }

    public void Value<TValue>(DatProperty prop, ValueMetadata<TValue> result, ref FileEvaluationContext ctx)
        where TValue : IEquatable<TValue>
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

            if (prop.Version != null && prop.Version.TryEvaluateValue(out Optional<Version> version, ref ctx) && version.Value is not null)
            {
                _hov.Append('\\').AppendLine().Append(Properties.Resources.Hover_AddedVersion).Append(" v").Append(version.Value)
                    .AppendLine();
            }

            _hov.AppendLine()
                .Append('-', 3).AppendLine().AppendLine();
        }
        else if (prop.Version != null && prop.Version.TryEvaluateValue(out Optional<Version> version, ref ctx) && version.Value is not null)
        {
            _hov.Append('\\').AppendLine().Append(Properties.Resources.Hover_AddedVersion).Append(" v").Append(version.Value)
                .AppendLine().AppendLine().Append('-', 3).AppendLine().AppendLine();
        }
    }

    public void Property<TValue>(DatProperty prop, ref FileEvaluationContext ctx, IType? type, Optional<TValue> value, bool hasValue)
        where TValue : IEquatable<TValue>
    {
        UnknownValue(prop, type, ref ctx, hasValue);
        if (!hasValue)
            return;

        if (!value.HasValue)
        {
            _hov.Append(Properties.Resources.Hover_ValueTitle).Append(": **null**");
            return;
        }

        string? str;
        if (typeof(TValue) == typeof(string))
        {
            str = Unsafe.As<TValue, string>(ref Unsafe.AsRef(in value.Value));
        }
        else if (TypeConverters.TryGet<TValue>() is { } tc)
        {
            TypeConverterFormatArgs args = TypeConverterFormatArgs.Default;
            str = tc.Format(value.Value, ref args);
        }
        else
        {
            switch (_node?.Value)
            {
                case IListSourceNode list:
                    _hov.Append(Properties.Resources.Hover_ListTitle).Append(": [ n = ").Append(list.Count).Append(" ]");
                    break;
                case IDictionarySourceNode dict:
                    _hov.Append(Properties.Resources.Hover_DictionaryTitle).Append(": { n = ").Append(dict.Count).Append(" }");
                    break;
            }

            return;
        }

        // todo: better escaping
        if (str.IndexOf('\n') >= 0)
        {
            _hov.AppendLine(Properties.Resources.Hover_ValueTitle).AppendLine(str);
        }
        else
        {
            _hov.Append(Properties.Resources.Hover_ValueTitle).Append(": `").Append(str).Append('`');
        }
    }
}