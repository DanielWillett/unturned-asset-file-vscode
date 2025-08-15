using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

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
        if (!_evalFactory.TryCreate(request.Position, request.TextDocument.Uri, out SpecPropertyTypeParseContext ctx) && ctx.Node == null)
        {
            return Task.FromResult<Hover?>(null);
        }

        Range range = ctx.Node!.Range.ToRange();

        string? desc;
        SpecProperty? prop = ctx.EvaluationContext.Self;
        if (prop == null)
        {
            desc = "Unknown property";
        }
        else if (prop.Description == null
                 || !prop.Description.TryEvaluateValue(in ctx.EvaluationContext, out desc, out bool isNull)
                 || isNull
                 || string.IsNullOrEmpty(desc)
                )
        {
            desc = prop.Key;
        }

        return Task.FromResult<Hover?>(new Hover
        {
            Range = range,
            Contents = new MarkedStringsOrMarkupContent(new MarkupContent
            {
                Kind = MarkupKind.PlainText,
                Value = desc
            })
        });
    }
}