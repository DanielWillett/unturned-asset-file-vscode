using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types.DynamicTypes;

public sealed class TypeOrEnumSpecPropertyType :
    BaseSpecPropertyType<QualifiedType>,
    ISpecPropertyType<QualifiedType>,
    IElementTypeSpecPropertyType,
    IEquatable<TypeOrEnumSpecPropertyType>
{
    private AssetSpecDatabase? _cachedSpecDb;
    private ISpecType? _cachedType;

    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "TypeOrEnum";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    public TypeOrEnumSpecPropertyType(QualifiedType elementType)
    {
        ElementType = elementType;
        DisplayName = QualifiedType.ExtractParts(elementType.Type.AsSpan(), out ReadOnlySpan<char> c, out _)
            ? c.ToString()
            : elementType.Type;

        DisplayName = $"Type (or {DisplayName})";
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out QualifiedType value)
    {
        ISpecType? specType = null;

        if (_cachedSpecDb == parse.Database)
        {
            lock (this)
            {
                if (_cachedSpecDb == parse.Database)
                {
                    specType = _cachedType;
                }
            }
        }

        if (specType == null)
        {
            specType = parse.Database.FindType(ElementType.Type, parse.FileType);
            lock (this)
            {
                _cachedSpecDb = parse.Database;
                _cachedType = specType;
            }
        }

        if (specType is not EnumSpecType enumType)
        {
            if (parse.HasDiagnostics)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT2005,
                    Message = string.Format(DiagnosticResources.UNT2005, ElementType.Type),
                    Range = parse.Node?.Range ?? parse.Parent?.Range ?? default
                });
            }

            value = default;
            return false;
        }

        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode stringNode)
        {
            return FailedToParse(in parse, out value);
        }

        string val = stringNode.Value;

        if (val.IndexOf('.') >= 0)
        {
            EnumSpecTypeValue[] values = enumType.Values;
            bool found = false;
            for (int i = 0; i < values.Length; ++i)
            {
                ref EnumSpecTypeValue enumVal = ref values[i];
                if (enumVal.CorrespondingType.IsNull || !string.Equals(enumVal.Value, val, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                val = enumVal.CorrespondingType.Type;
                found = true;
                break;
            }

            if (!found && int.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out int index) && index >= 0 && index < values.Length)
            {
                string? type = values[index].CorrespondingType.Type;
                if (type != null)
                    val = type;
            }
        }

        return KnownTypeValueHelper.TryParseType(val, out value) || FailedToParse(in parse, out value);
    }

    /// <inheritdoc />
    public bool Equals(TypeOrEnumSpecPropertyType other) => ElementType.Equals(other.ElementType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType other) => other is TypeOrEnumSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<QualifiedType> other) => other is TypeOrEnumSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public ISpecPropertyType<TValue>? As<TValue>() where TValue : IEquatable<TValue> => this as ISpecPropertyType<TValue>;
}