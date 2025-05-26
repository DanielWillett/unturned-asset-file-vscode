using DanielWillett.UnturnedDataFileLspServer.NewtonsoftConverters;
using Newtonsoft.Json;
using OmniSharp.Extensions.LanguageServer.Protocol.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Protocol;
internal class UnturnedLspSerializer : LspSerializer
{
    protected override void AddOrReplaceConverters(ICollection<JsonConverter> converters)
    {
        base.AddOrReplaceConverters(converters);
        ReplaceConverter(converters, new GuidOrIdConverter());
        ReplaceConverter(converters, new BundleReferenceConverter());
        ReplaceConverter(converters, new QualifiedTypeConverter());
        ReplaceConverter(converters, new UnityEngineVersionConverter());
    }
}
