using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class IsType : ConditionOperation<IsType>
{
    public override string Name => "is-type";
    public override string Symbol => ":";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        result = false;
        return false;
    }

    protected override bool TryEvaluate<TValue, TComparand>(TValue value, TComparand comparand, in FileEvaluationContext ctx, out bool result)
    {
        result = false;
        if (!ConditionOperations.TryGetType(ref comparand, out QualifiedType comparandType, out bool comparandIsAlias))
        {
            return false;
        }

        AssetInformation databaseInformation = ctx.Services.Database.Information;
        if (comparandIsAlias)
        {
            if (!databaseInformation.AssetAliases.TryGetValue(comparandType.Type, out comparandType))
                return false;
        }

        GuidOrId guidOrId;
        if (typeof(TValue) == typeof(Guid))
        {
            guidOrId = new GuidOrId(Unsafe.As<TValue, Guid>(ref value));
        }
        else if (typeof(TValue) == typeof(GuidOrId))
        {
            guidOrId = Unsafe.As<TValue, GuidOrId>(ref value);
        }
        else if (typeof(TValue) == typeof(string))
        {
            if (!GuidOrId.TryParse(Unsafe.As<TValue, string?>(ref value), out guidOrId))
            {
                return false;
            }
        }
        else if (typeof(TValue) == typeof(ushort))
        {
            guidOrId = new GuidOrId(Unsafe.As<TValue, ushort>(ref value));
        }
        else if (!ConvertVisitor<GuidOrId>.TryConvert(value, out guidOrId) || guidOrId.IsNull)
        {
            // result = false;
            // id rather this not fail if it can't be converted, just return false instead
            return true;
        }

        OneOrMore<DiscoveredDatFile> datFiles = ctx.Services.Installation.FindFile(guidOrId);
        if (datFiles.IsNull)
        {
            return true;
        }

        DiscoveredDatFile fileInfo = datFiles[0];
        result = databaseInformation.IsAssignableTo(fileInfo.Type, comparandType);
        return true;
    }

    public override int GetHashCode() => 549093573;
}