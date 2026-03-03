using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

public class ProjectFile(string filePath)
{
    public string FilePath { get; set; } = filePath;

    public string? Orderfile { get; set; }

    public GuidStyle? GuidStyle { get; set; }

    public static bool TryReadFromFile(
        ref FileEvaluationContext ctx,
        [NotNullWhen(true)] out ProjectFile? file,
        IDiagnosticSink? diagnosticSink = null,
        IReferencedPropertySink? referencedPropertySink = null
    )
    {
        ProjectFile pj = new ProjectFile(ctx.File.WorkspaceFile.File);
        if (!pj.TryUpdateFromFile(ref ctx, diagnosticSink, referencedPropertySink))
        {
            file = null;
            return false;
        }

        file = pj;
        return true;
    }

    public bool TryUpdateFromFile(
        ref FileEvaluationContext ctx,
        IDiagnosticSink? diagnosticSink = null,
        IReferencedPropertySink? referencedPropertySink = null
    )
    {
        if (!ctx.Services.Database.FileTypes.TryGetValue(new QualifiedType(ProjectFileType.TypeId, true), out DatFileType? fileType))
        {
            ctx.Services.CreateLogger<ProjectFile>().LogWarning($"Database not yet initialized, or type {ProjectFileType.TypeId} isn't available.");
            return false;
        }

        foreach (DatProperty property in fileType.Properties)
        {
            if (property.TryGetValue(
                    ref ctx,
                    out IValue? value,
                    out _,
                    diagnosticSink,
                    referencedPropertySink,
                    TypeParserMissingValueBehavior.FallbackToDefaultValue
                ))
            {
                HandlePropertyValue(ref ctx, property, value);
            }
            else
            {
                ctx.Services.CreateLogger<ProjectFile>().LogWarning(
                    $"Failed to parse project file \"{ctx.File.WorkspaceFile.File}\" property {ctx.RootBreadcrumbs.ToString(false, property.Key)}."
                );
            }
        }

        return true;
    }

    private void HandlePropertyValue(ref FileEvaluationContext ctx, DatProperty property, IValue? value)
    {
        switch (property.Key)
        {
            case "Orderfile":
                if (value.TryGetValueAs(ref ctx, out string? orderfile) && !string.IsNullOrEmpty(orderfile))
                {
                    Orderfile = orderfile;
                }

                break;

            case "Guid_Style":
                if (value.TryGetValueAs(ref ctx, out DatEnumValue? guidStyleValue) && guidStyleValue != null)
                {
                    GuidStyle = ProjectFileType.StyleEnumMap[guidStyleValue.Index];
                }

                break;
        }
    }
}

public enum GuidStyle
{
    NormalLower,
    NormalUpper,
    DashesLower,
    DashesUpper,
    BracesLower,
    BracesUpper,
    ParenthesisLower,
    ParenthesisUpper,
    HexLower,
    HexUpper
}