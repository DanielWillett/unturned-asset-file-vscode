using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

public class DatCustomType : DatTypeWithProperties
{
    /// <inheritdoc />
    public override DatSpecificationType Type => DatSpecificationType.Custom;

    /// <inheritdoc />
    private protected override string FullName => $"{Owner.TypeName.GetFullTypeName()}/{TypeName.GetFullTypeName()}";

    /// <inheritdoc />
    public override DatFileType Owner { get; }

    internal DatCustomType(QualifiedType type, DatTypeWithProperties? baseType, JsonElement element, DatFileType file) : base(type, baseType, element)
    {
        Owner = file;
    }
}
