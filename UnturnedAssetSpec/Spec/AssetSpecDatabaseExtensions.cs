using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Extensions for <see cref="IAssetSpecDatabase"/> implementations.
/// </summary>
public static class AssetSpecDatabaseExtensions
{
    /// <summary>
    /// Attempts to find a type by a type name with optional context. If the database isn't initialized, it will fallback to the active <see cref="IDatSpecificationReadContext"/>.
    /// </summary>
    /// <param name="database">The database to search in.</param>
    /// <param name="typeName">The name of the type.</param>
    /// <param name="type">The found type.</param>
    /// <param name="context">
    /// Object to search relative to. This can help resolve ambiguous searches.
    /// Built-in types (<seealso cref="SpecificationTypeAttribute"/>) will not be returned if a value is not given for this parameter.
    /// </param>
    /// <returns>Whether or not the type was found.</returns>
    public static bool TryFindType(this IAssetSpecDatabase database, QualifiedType typeName, [NotNullWhen(true)] out DatType? type, IDatSpecificationObject? context = null)
    {
        if (!database.IsInitialized)
        {
            if (context == null)
            {
                type = null;
                return false;
            }

            type = database.ReadContext.GetOrReadType(context, typeName) as DatType;
            return type != null;
        }

        if (!typeName.IsCaseInsensitive)
            typeName = typeName.CaseInsensitive;

        IDictionary<QualifiedType, DatType> allTypes = database.AllTypes;
        DatType? datType;
        if (allTypes != null)
        {
            if (!allTypes.TryGetValue(typeName, out datType))
            {
                type = null;
                return false;
            }

            if (context == null || datType is DatFileType || datType.Owner.Equals(context.Owner) || database.Information.IsAssignableTo(context.Owner.TypeName, datType.Owner.TypeName))
            {
                type = datType;
                return true;
            }
        }
        else
        {
            datType = null;
        }

        if (context != null)
        {
            for (DatFileType? file = context.Owner; file != null; file = file.Parent)
            {
                if (file.TypesBuilder != null)
                {
                    if (file.TypesBuilder.TryGetValue(typeName, out type))
                    {
                        return true;
                    }
                }
                else if (file.Types.TryGetValue(typeName, out type))
                {
                    return true;
                }
            }

            if (datType != null)
            {
                // if there are no types in the hierarchy matching this name return the unrelated one from the dictionary
                type = datType;
                return true;
            }

            Type? intlType = Type.GetType(typeName.Type, throwOnError: false, ignoreCase: true);
            if (intlType != null && CommonTypes.TryCreateBuiltInType(intlType, database.ReadContext, context, typeName.Type, out IType? t, throwExceptions: false, requireDatType: true))
            {
                type = (DatType)t;
                return true;
            }
        }

        type = null;
        return false;
    }
}
