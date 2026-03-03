using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

internal static class BuiltinTypeRegistrar
{
    internal static void OnFinalizingTypes(IDatSpecificationReadContext context, ICollection<DatType> types)
    {
        types.Add(Orderfile.FromDatabase(context.Database));
        types.Add(new ProjectFileType(context));
    }
}
