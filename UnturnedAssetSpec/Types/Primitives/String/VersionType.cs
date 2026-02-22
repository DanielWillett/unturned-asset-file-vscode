using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A string parsed as a 2-4 digit version.
/// <para>Currently unused in Unturned</para>
/// <code>
/// Prop 3.26.1.0
/// </code>
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="int"/> Digits</c> - Required digit count. Defaults to <c>0</c> (meaning 2, 3, or 4 digits).</item>
///     <item><c><see cref="int"/> MaximumValue</c> - Maximum value of any one component.</item>
///     <item><c><see cref="Version"/> MaximumValues</c> - Maximum value of all components.</item>
///     <item><c><see cref="int"/> MinimumValue</c> - Minimum value of any one component.</item>
///     <item><c><see cref="Version"/> MinimumValues</c> - Minimum value of all components.</item>
///     <item><c><see cref="bool"/> StrictFormatting</c> - Any issue with the bounds or digit counts is considered a failure to parse.</item>
/// </list>
/// </para>
/// </summary>
/// <remarks>
/// Most Unturned versions will probably be packed in a uint32, so they will have a maximum value of 255:
/// <example>
/// <code>
/// {
///     "Type": "Version",
///     "Digits": 4,
///     "MaximumValue": 255,
///     "StrictFormatting": true
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class VersionType : PrimitiveType<Version, VersionType>, ITypeParser<Version>
{
    public const string TypeId = "Version";

    /// <summary>
    /// Instance of <see cref="VersionType"/> that only parses 4-digit versions that can be packed into a <see cref="uint"/> value.
    /// </summary>
    public static VersionType PackableInstance { get; } = new VersionType(4, maxComponent: byte.MaxValue, strictFormatting: true);

    private readonly int _digits;
    private readonly int _maxComponent;
    private readonly Version? _maxVersion;
    private readonly int _minComponent;
    private readonly Version? _minVersion;
    private readonly bool _strictFormatting;

    public override string Id => TypeId;

    public override string DisplayName => Resources.Type_Name_Version;

    public override ITypeParser<Version> Parser => this;

    public VersionType()
    {
        _maxComponent = int.MaxValue;
    }

    public VersionType(
        int digits,
        int maxComponent = int.MaxValue,
        Version? maxVersion = null,
        int minComponent = 0,
        Version? minVersion = null,
        bool strictFormatting = false
    ) : this()
    {
        if (digits is not (0 or 2 or 3 or 4))
        {
            throw new ArgumentOutOfRangeException(nameof(digits));
        }

        _digits = digits;
        _maxComponent = maxComponent;
        _maxVersion = maxVersion;
        _minComponent = minComponent;
        _minVersion = minVersion;
        _strictFormatting = strictFormatting;
    }

    public bool TryParse(ref TypeParserArgs<Version> args, in FileEvaluationContext ctx, out Optional<Version> value)
    {
        if (TypeParsers.TryApplyMissingValueBehavior(ref args, in ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.TryParseStringValueOnly(ref args, out IValueSourceNode? valueNode))
        {
            value = Optional<Version>.Null;
            return false;
        }

        if (!Version.TryParse(valueNode.Value, out Version? version))
        {
            args.DiagnosticSink?.UNT2004_Generic(ref args, valueNode.Value, this);
            return false;
        }

        bool failure = false;
        if (args.DiagnosticSink != null || _strictFormatting)
        {
            FileRange fullRange = valueNode.Range;
            if (version.Major > _maxComponent || (_maxVersion is not null && version.Major > _maxVersion.Major))
            {
                TryGetNumberPosition(0, valueNode.Value, fullRange, out FileRange errRange);
                args.DiagnosticSink?.UNT1031_2031_More(ref args, version.Major, 0, Math.Min(_maxComponent, _maxVersion?.Major ?? int.MaxValue), errRange, _strictFormatting);
                failure = true;
            }
            if (version.Major < _minComponent || (_minVersion is not null && _minVersion.Major != -1 && version.Major < _minVersion.Major))
            {
                TryGetNumberPosition(0, valueNode.Value, fullRange, out FileRange errRange);
                args.DiagnosticSink?.UNT1031_2031_Less(ref args, version.Major, 0, Math.Max(_minComponent, _minVersion?.Major ?? 0), errRange, _strictFormatting);
                failure = true;
            }

            if (version.Minor > _maxComponent || (_maxVersion is not null && version.Minor > _maxVersion.Minor))
            {
                TryGetNumberPosition(1, valueNode.Value, fullRange, out FileRange errRange);
                args.DiagnosticSink?.UNT1031_2031_More(ref args, version.Minor, 1, Math.Min(_maxComponent, _maxVersion?.Minor ?? int.MaxValue), errRange, _strictFormatting);
                failure = true;
            }
            if (version.Minor < _minComponent || (_minVersion is not null && _minVersion.Minor != -1 && version.Minor < _minVersion.Minor))
            {
                TryGetNumberPosition(1, valueNode.Value, fullRange, out FileRange errRange);
                args.DiagnosticSink?.UNT1031_2031_Less(ref args, version.Minor, 1, Math.Max(_minComponent, _minVersion?.Minor ?? 0), errRange, _strictFormatting);
                failure = true;
            }

            if (version.Build >= 0)
            {
                if (version.Build > _maxComponent || (_maxVersion is not null && _maxVersion.Build != -1 && version.Build > _maxVersion.Build))
                {
                    TryGetNumberPosition(2, valueNode.Value, fullRange, out FileRange errRange);
                    args.DiagnosticSink?.UNT1031_2031_More(ref args, version.Build, 2, _maxVersion is null || _maxVersion.Build == -1 ? _maxComponent : Math.Min(_maxComponent, _maxVersion.Build), errRange, _strictFormatting);
                    failure = true;
                }
                if (version.Build < _minComponent || (_minVersion is not null && _minVersion.Build != -1 && version.Build < _minVersion.Build))
                {
                    TryGetNumberPosition(2, valueNode.Value, fullRange, out FileRange errRange);
                    args.DiagnosticSink?.UNT1031_2031_Less(ref args, version.Build, 2, _minVersion is null || _minVersion.Build == -1 ? _minComponent : Math.Max(_minComponent, _minVersion.Build), errRange, _strictFormatting);
                    failure = true;
                }
            }

            if (version.Revision >= 0)
            {
                if (version.Revision > _maxComponent || (_maxVersion is not null && _maxVersion.Revision != -1 && version.Revision > _maxVersion.Revision))
                {
                    TryGetNumberPosition(3, valueNode.Value, fullRange, out FileRange errRange);
                    args.DiagnosticSink?.UNT1031_2031_More(ref args, version.Revision, 3, _maxVersion is null || _maxVersion.Revision == -1 ? _maxComponent : Math.Min(_maxComponent, _maxVersion.Revision), errRange, _strictFormatting);
                    failure = true;
                }
                if (version.Revision < _minComponent || (_minVersion is not null && _minVersion.Revision != -1 && version.Revision < _minVersion.Revision))
                {
                    TryGetNumberPosition(3, valueNode.Value, fullRange, out FileRange errRange);
                    args.DiagnosticSink?.UNT1031_2031_Less(ref args, version.Revision, 3, _minVersion is null || _minVersion.Revision == -1 ? _minComponent : Math.Max(_minComponent, _minVersion.Revision), errRange, _strictFormatting);
                    failure = true;
                }
            }

            switch (_digits)
            {
                case 2:
                    if (version.Build != -1)
                    {
                        // 1.1.1 or 1.1.1.1
                        TryGetNumberPosition(2, valueNode.Value, fullRange, out FileRange errRange, remainder: true);
                        args.DiagnosticSink?.UNT1031_2031_Digits(ref args, version.Revision != -1 ? 4 : 3, 2, errRange, _strictFormatting);
                        failure = true;
                    }

                    break;

                case 3:
                    if (version.Revision != -1)
                    {
                        // 1.1.1.1
                        TryGetNumberPosition(3, valueNode.Value, fullRange, out FileRange errRange, remainder: true);
                        args.DiagnosticSink?.UNT1031_2031_Digits(ref args, 4, 3, errRange, _strictFormatting);
                        failure = true;
                    }

                    if (version.Build != -1)
                    {
                        break;
                    }

                    // 1.1
                    args.DiagnosticSink?.UNT1031_2031_Digits(ref args, 2, 3, fullRange, _strictFormatting);
                    failure = true;
                    break;

                case 4:
                    if (version.Revision != -1)
                    {
                        break;
                    }

                    // 1.1.1 or 1.1
                    args.DiagnosticSink?.UNT1031_2031_Digits(ref args, version.Build != -1 ? 3 : 2, 4, fullRange, _strictFormatting);
                    failure = true;
                    break;
            }
        }

        value = version;
        return !failure || !_strictFormatting;
    }

    private static void TryGetNumberPosition(int presendence, string value, FileRange fullRange, out FileRange range, bool remainder = false)
    {
        Span<Range> ranges = stackalloc Range[4];
        int r = SpanExtensions.Split(value, ranges, '.');
        if (r <= presendence)
        {
            range = fullRange;
            return;
        }

        Range textRange = ranges[presendence];
        (int offset, int length) = textRange.GetOffsetAndLength(value.Length);

        range.Start = fullRange.Start;
        range.Start.Character += offset;
        if (!remainder)
        {
            // 1.2.3.4
            //     _  
            range.End.Character = range.Start.Character + length - 1;
            range.End.Line = range.Start.Line;
        }
        else
        {
            // 1.2.3.4
            //    ____
            range.End = fullRange.End;
            --range.Start.Character;
        }
    }

    #region JSON

    public bool TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement json,
        out Optional<Version> value,
        IType<Version> valueType,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        TypeConverterParseArgs<Version> args = new TypeConverterParseArgs<Version>(this);
        return TypeConverters.Version.TryReadJson(in json, out value, ref args);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, Version value, IType<Version> valueType, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    protected override IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        int minComp = 0, maxComp = int.MaxValue, digits = 0;
        Version? minVersion = null, maxVersion = null;
        bool strict = false;

        if (typeDefinition.TryGetProperty("Digits"u8, out JsonElement element)
            && element.ValueKind != JsonValueKind.Null)
        {
            digits = element.GetInt32();
            if (digits is not (0 or 2 or 3 or 4))
                throw new JsonException($"Error reading at '{context}'. Expected \"Digits\" to be 0, 2, 3, or 4.");
        }

        if (typeDefinition.TryGetProperty("MaximumValue"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            maxComp = element.GetInt32();
            if (maxComp < 0)
                throw new JsonException($"Error reading at '{context}'. Expected \"MaximumValue\" >= 0.");
        }

        if (typeDefinition.TryGetProperty("MaximumValues"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            if (!Version.TryParse(element.GetString(), out maxVersion))
                throw new JsonException($"Error reading at '{context}'. Invalid Version format for \"MaximumValues\".");
        }

        if (typeDefinition.TryGetProperty("MinimumValue"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            minComp = element.GetInt32();
            if (minComp < 0)
                throw new JsonException($"Error reading at '{context}'. Expected \"MinimumValue\" >= 0.");
        }

        if (typeDefinition.TryGetProperty("MinimumValues"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            if (!Version.TryParse(element.GetString(), out minVersion))
                throw new JsonException($"Error reading at '{context}'. Invalid Version format for \"MinimumValues\".");
        }

        if (typeDefinition.TryGetProperty("StrictFormatting"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            strict = element.GetBoolean();
        }

        if (digits == 4 && maxComp == byte.MaxValue && minComp == 0 && maxVersion == null && minVersion == null && strict)
        {
            return PackableInstance;
        }

        return digits != 0 || maxComp != int.MaxValue || minComp != 0 || maxVersion != null || minVersion != null || strict
            ? new VersionType(digits, maxComp, maxVersion, minComp, minVersion, strict)
            : Instance;
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (_digits == 0 && _maxComponent == int.MaxValue && _maxVersion == null && _minComponent == 0 && _minVersion == null && !_strictFormatting)
        {
            base.WriteToJson(writer, options);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
        if (_digits != 0)
            writer.WriteNumber("Digits"u8, _digits);
        if (_maxComponent != int.MaxValue)
            writer.WriteNumber("MaximumValue"u8, _maxComponent);
        if (_maxVersion != null)
            writer.WriteString("MaximumValues"u8, _maxVersion.ToString());
        if (_minComponent != 0)
            writer.WriteNumber("MinimumValue"u8, _minComponent);
        if (_minVersion != null)
            writer.WriteString("MinimumValues"u8, _minVersion.ToString());
        if (_strictFormatting)
            writer.WriteBoolean("StrictFormatting"u8, true);

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(VersionType other)
    {
        return other._digits == _digits
               && other._minComponent == _minComponent
               && other._maxComponent == _maxComponent
               && other._minVersion == _minVersion
               && other._maxVersion == _maxVersion
               && other._strictFormatting == _strictFormatting;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(333922896, _digits, _minComponent, _maxComponent, _minVersion, _maxVersion, _strictFormatting);
    }
}