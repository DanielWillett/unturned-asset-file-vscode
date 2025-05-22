using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.ComponentModel;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class BaseSpecPropertyType<TValue>
{
    public abstract string Type { get; }
    public abstract string DisplayName { get; }
    
#pragma warning disable CS0693
    public ISpecPropertyType<TValue>? As<TValue>() where TValue : IEquatable<TValue> => this as ISpecPropertyType<TValue>;
#pragma warning restore CS0693

    public override string ToString() => Type;

    protected bool MissingNode(in SpecPropertyTypeParseContext parse, out TValue? value)
    {
        if (parse.HasDiagnostics && parse.Parent is AssetFileKeyValuePairNode { Key: { } key })
        {
            DatDiagnosticMessage message = new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1005,
                Message = string.Format(DiagnosticResources.UNT1005, key.Value),
                Range = parse.Parent.Range
            };

            parse.Log(message);
        }

        value = default;
        return false;
    }

    protected bool MissingProperty(in SpecPropertyTypeParseContext parse, string property, out TValue? value)
    {
        if (parse.HasDiagnostics)
        {
            DatDiagnosticMessage message = new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1007,
                Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, property),
                Range = parse.Parent?.Range ?? parse.Node?.Range ?? default
            };

            parse.Log(message);
        }

        value = default;
        return false;
    }

    protected bool FailedToParse(in SpecPropertyTypeParseContext parse, out TValue? value, AssetFileNode? node = null)
    {
        node ??= parse.Node;
        if (parse.HasDiagnostics && node != null)
        {
            DatDiagnosticMessage message = new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(
                    DiagnosticResources.UNT2004,
                    parse.Node is AssetFileStringValueNode s ? s.Value : parse.Node.ToString(),
                    DisplayName
                ),
                Range = parse.Node.Range
            };

            parse.Log(message);
        }

        value = default;
        return false;
    }
}