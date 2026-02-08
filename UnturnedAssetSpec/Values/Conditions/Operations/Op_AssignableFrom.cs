using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class AssignableFrom : ConditionOperation<AssignableFrom>
{
    public override string Name => "assignable-from";
    public override string Symbol => "≼";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        result = false;
        if (!ConditionOperations.TryGetType(ref comparand, out QualifiedType comparandType, out bool comparandIsAlias)
            || !ConditionOperations.TryGetType(ref value, out QualifiedType valueType, out bool valueTypeIsAlias))
        {
            return false;
        }

        if (comparandIsAlias | valueTypeIsAlias)
        {
            return false;
        }

        if (valueType.Equals(comparandType))
        {
            result = true;
            return true;
        }

        return false;
    }

    protected override bool TryEvaluate<TValue, TComparand>(TValue value, TComparand comparand, in FileEvaluationContext ctx, out bool result)
    {
        result = false;
        if (!ConditionOperations.TryGetType(ref comparand, out QualifiedType comparandType, out bool comparandIsAlias)
            || !ConditionOperations.TryGetType(ref value, out QualifiedType valueType, out bool valueTypeIsAlias))
        {
            return false;
        }

        AssetInformation databaseInformation = ctx.Services.Database.Information;
        if (comparandIsAlias)
        {
            if (!databaseInformation.AssetAliases.TryGetValue(comparandType.Type, out comparandType))
                return false;
        }
        if (valueTypeIsAlias)
        {
            if (!databaseInformation.AssetAliases.TryGetValue(valueType.Type, out valueType))
                return false;
        }

        result = databaseInformation.IsAssignableFrom(valueType, comparandType);
        return true;
    }

    public override int GetHashCode() => 1537091716;
}