using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[SpecificationType(FactoryMethod = nameof(Create))]
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicMethods)]
#endif
public class ProjectFileType : DatFileType
{
    public const string TypeId = "DanielWillett.UnturnedDataFileLspServer.Data.Types.ProjectFileType, UnturnedAssetSpec";
    public const string GuidStyleTypeId = "DanielWillett.UnturnedDataFileLspServer.Data.Types.ProjectFile+GuidStyle, UnturnedAssetSpec";

    internal static GuidStyle[] StyleEnumMap =
    [
        GuidStyle.NormalLower,
        GuidStyle.NormalUpper,
        GuidStyle.DashesLower,
        GuidStyle.DashesUpper,
        GuidStyle.BracesLower,
        GuidStyle.BracesUpper,
        GuidStyle.ParenthesisLower,
        GuidStyle.ParenthesisUpper,
        GuidStyle.HexLower,
        GuidStyle.HexUpper
    ];

    internal ProjectFileType(IDatSpecificationReadContext context) : base(new QualifiedType(TypeId, true), null, default)
    {
        ImmutableArray<DatProperty>.Builder builder = ImmutableArray.CreateBuilder<DatProperty>();

        JsonElement none = default;
        DisplayNameIntl = Resources.Type_Name_ProjectFile;
        // todo FileSchema.Docs = ???;

        // todo: PathType instead of StringType
        DatProperty orderfile = DatProperty.Create("Orderfile", StringType.Instance, this, none, SpecPropertyContext.Property);
        orderfile.Description = Value.Create("Relative or absolute path to a file defining the order of properties for files in this project.", StringType.Instance);
        orderfile.DefaultValue = Value.Null(StringType.Instance);
        builder.Add(orderfile);

        DatEnumType guidStyleEnumType = CreateEnumType(
            new QualifiedType(GuidStyleTypeId, true),
            false,
            none,
            this,
            context
        );

        guidStyleEnumType.CaseSensitive = true;

        ImmutableArray<DatEnumValue>.Builder values = ImmutableArray.CreateBuilder<DatEnumValue>(10);

        {
            DatEnumValue tempValue = DatEnumValue.Create("n", 0, guidStyleEnumType, none);
            tempValue.Description = "32 lowercase digits: `0123456789abcdef0123456789abcdef`";
            values.Add(tempValue);

            tempValue = DatEnumValue.Create("N", 1, guidStyleEnumType, none);
            tempValue.Description = "32 uppercase digits: `0123456789ABCDEF0123456789ABCDEF`";
            values.Add(tempValue);

            tempValue = DatEnumValue.Create("d", 2, guidStyleEnumType, none);
            tempValue.Description = "32 lowercase digits separated by hyphens: `01234567-89ab-cdef-0123-456789abcdef`";
            values.Add(tempValue);

            tempValue = DatEnumValue.Create("D", 3, guidStyleEnumType, none);
            tempValue.Description = "32 uppercase digits separated by hyphens: `01234567-89AB-CDEF-0123-456789ABCDEF`";
            values.Add(tempValue);

            tempValue = DatEnumValue.Create("b", 4, guidStyleEnumType, none);
            tempValue.Description = "32 lowercase digits separated by hyphens, enclosed in braces: `{01234567-89ab-cdef-0123-456789abcdef}`";
            values.Add(tempValue);

            tempValue = DatEnumValue.Create("B", 5, guidStyleEnumType, none);
            tempValue.Description = "32 uppercase digits separated by hyphens, enclosed in braces: `{01234567-89AB-CDEF-0123-456789ABCDEF}`";
            values.Add(tempValue);

            tempValue = DatEnumValue.Create("p", 6, guidStyleEnumType, none);
            tempValue.Description = "32 lowercase digits separated by hyphens, enclosed in parentheses: `(01234567-89ab-cdef-0123-456789abcdef)`";
            values.Add(tempValue);

            tempValue = DatEnumValue.Create("P", 7, guidStyleEnumType, none);
            tempValue.Description = "32 uppercase digits separated by hyphens, enclosed in parentheses: `(01234567-89AB-CDEF-0123-456789ABCDEF)`";
            values.Add(tempValue);

            tempValue = DatEnumValue.Create("x", 8, guidStyleEnumType, none);
            tempValue.Description = "Four lowercase hexadecimal values enclosed in braces, where the fourth value is a subset of eight hexadecimal values that is also enclosed in braces: `{0x01234567,0x89ab,0xcdef,{0x01,0x23,0x45,0x67,0x89,0xab,0xcd,0xef}}`";
            values.Add(tempValue);

            tempValue = DatEnumValue.Create("X", 9, guidStyleEnumType, none);
            tempValue.Description = "Four uppercase hexadecimal values enclosed in braces, where the fourth value is a subset of eight hexadecimal values that is also enclosed in braces: `{0X01234567,0X89AB,0XCDEF,{0X01,0X23,0X45,0X67,0X89,0XAB,0XCD,0XEF}}`";
            values.Add(tempValue);
        }

        guidStyleEnumType.Values = values.MoveToImmutable();

        DatProperty guidStyle = DatProperty.Create("Guid_Style", guidStyleEnumType, this, none, SpecPropertyContext.Property);
        guidStyle.Description = Value.Create("The style to use for generated GUIDs.", StringType.Instance);
        guidStyle.DefaultValue = guidStyleEnumType.Values[0];
        builder.Add(guidStyle);

        Properties = builder.MoveToImmutableOrCopy();
    }

    /// <summary>
    /// Factory method for the <see cref="ProjectFileType"/> type.
    /// </summary>
    private static ProjectFileType Create(in SpecificationTypeFactoryArgs args)
    {
        return new ProjectFileType(args.Context);
    }

    /// <exception cref="InvalidEnumArgumentException"/>
    internal static string GuidToString(Guid guid, GuidStyle style)
    {
        return style switch
        {
            GuidStyle.NormalLower => guid.ToString("N"),
            GuidStyle.NormalUpper => guid.ToString("N").ToUpperInvariant(),
            GuidStyle.DashesLower => guid.ToString("D"),
            GuidStyle.DashesUpper => guid.ToString("D").ToUpperInvariant(),
            GuidStyle.BracesLower => guid.ToString("B"),
            GuidStyle.BracesUpper => guid.ToString("B").ToUpperInvariant(),
            GuidStyle.ParenthesisLower => guid.ToString("P"),
            GuidStyle.ParenthesisUpper => guid.ToString("P").ToUpperInvariant(),
            GuidStyle.HexLower => guid.ToString("X"),
            GuidStyle.HexUpper => guid.ToString("X").ToUpperInvariant(),
            _ => throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(GuidStyle))
        };
    }
}