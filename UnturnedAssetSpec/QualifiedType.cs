using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using System;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// Comparable assembly-qualified type with proper equality checking.
/// </summary>
/// <remarks>Types are case-sensitive.</remarks>
[JsonConverter(typeof(QualifiedTypeConverter))]
public readonly struct QualifiedType : IEquatable<QualifiedType>, IEquatable<string>, IComparable<QualifiedType>, IComparable
{
    private readonly bool _isCaseInsensitive;

    public static readonly QualifiedType None = default;
    public static readonly QualifiedType AssetBaseType = new QualifiedType(TypeHierarchy.AssetBaseType);
    public static readonly QualifiedType UseableBaseType = new QualifiedType(TypeHierarchy.UseableBaseType);

#nullable disable
    /// <summary>
    /// Underlying string storing the type.
    /// </summary>
    public string Type { get; }
#nullable restore

    /// <summary>
    /// If this qualified type is <see langword="default"/>.
    /// </summary>
    public bool IsNull => Type is null;

    /// <summary>
    /// If this type name is already in the normalized form.
    /// </summary>
    public bool IsNormalized
    {
        get
        {
            if (Type == null || !ExtractParts(Type.AsSpan(), out ReadOnlySpan<char> typeName, out ReadOnlySpan<char> assemblyName))
                return false;

            int len = typeName.Length + assemblyName.Length + 2 /* ", " */;

            // already normalized type name
            return len == Type.Length;
        }
    }

    /// <summary>
    /// If this type should be case-insensitive.
    /// </summary>
    public bool IsCaseInsensitive => _isCaseInsensitive;

    /// <summary>
    /// The case-sensitive version of this type.
    /// </summary>
    public QualifiedType CaseSensitive => _isCaseInsensitive ? new QualifiedType(Type, false) : this;

    /// <summary>
    /// The case-sensitive version of this type.
    /// </summary>
    public QualifiedType CaseInsensitive => _isCaseInsensitive ? this : new QualifiedType(Type, true);

    /// <summary>
    /// Creates a normalized version of this type.
    /// </summary>
    public QualifiedType Normalized
    {
        get
        {
            if (Type == null || !ExtractParts(Type.AsSpan(), out ReadOnlySpan<char> typeName, out ReadOnlySpan<char> assemblyName))
                return this;

            int len = typeName.Length + assemblyName.Length + 2 /* ", " */;

            // already normalized type name
            if (len == Type.Length)
                return this;

            string str;
            if (len <= 512)
            {
                Span<char> ttlSpan = stackalloc char[len];
                typeName.CopyTo(ttlSpan);
                ttlSpan[typeName.Length] = ',';
                ttlSpan[typeName.Length + 1] = ' ';
                assemblyName.CopyTo(ttlSpan.Slice(typeName.Length + 2));
                str = ttlSpan.ToString();
            }
            else
            {
                str = typeName.ToString() + ", " + assemblyName.ToString();
            }

            return new QualifiedType(str);
        }
    }

    internal QualifiedType(string type, bool isCaseInsensitive)
    {
        if (!string.IsNullOrEmpty(type))
            Type = type;
        _isCaseInsensitive = isCaseInsensitive;
    }

    public QualifiedType(string? type)
    {
        if (!string.IsNullOrEmpty(type))
            Type = type!;
    }

    public string GetTypeName()
    {
        return ExtractTypeName(Type.AsSpan()).ToString();
    }

    public static string NormalizeType(string type)
    {
        return new QualifiedType(type).Normalized.Type;
    }

    /// <summary>
    /// Compare the strings of type <see cref="QualifiedType"/> types.
    /// </summary>
    public static bool TypesEqual(string? left, string? right, bool caseInsensitive = false)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return TypesEqual(left.AsSpan(), right.AsSpan(), caseInsensitive);
    }

    /// <summary>
    /// Compare the strings of type <see cref="QualifiedType"/> types.
    /// </summary>
    public static bool TypesEqual(ReadOnlySpan<char> left, ReadOnlySpan<char> right, bool caseInsensitive = false)
    {
        StringComparison c = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (left.Equals(right, c))
            return true;

        if (left.IsEmpty || right.IsEmpty)
            return false;

        if (!ExtractParts(left, out ReadOnlySpan<char> typeNameLeft, out ReadOnlySpan<char> assemblyNameLeft)
            || !ExtractParts(right, out ReadOnlySpan<char> typeNameRight, out ReadOnlySpan<char> assemblyNameRight))
        {
            return false;
        }

        return typeNameLeft.Equals(typeNameRight, c) && assemblyNameLeft.Equals(assemblyNameRight, c);
    }

    public static ReadOnlySpan<char> ExtractTypeName(ReadOnlySpan<char> assemblyQualifiedTypeName)
    {
        if (!ExtractParts(assemblyQualifiedTypeName, out ReadOnlySpan<char> fullTypeName, out _) && fullTypeName.IsEmpty)
            fullTypeName = assemblyQualifiedTypeName;

        for (int i = fullTypeName.Length - 2; i >= 0; --i)
        {
            char c = fullTypeName[i];
            if (c is '.' or '+' && !IsEscaped(fullTypeName, i))
            {
                return fullTypeName.Slice(i + 1);
            }
        }

        return fullTypeName;
    }

    /// <summary>
    /// Extract the type name and assembly name from an assembly-qualified full type name.
    /// </summary>
    public static bool ExtractParts(ReadOnlySpan<char> assemblyQualifiedTypeName, out ReadOnlySpan<char> fullTypeName, out ReadOnlySpan<char> assemblyName)
    {
        bool isLookingForAssemblyName = false;
        if (assemblyQualifiedTypeName.IndexOfAny('[', ']') != -1)
        {
            fullTypeName = default;
            assemblyName = default;

            int genericDepth = 0;
            int escapeDepth = 0;

            // more advanced check taking generic types into consideration
            // ex: System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Int32, mscorlib]], mscorlib
            for (int i = 0; i < assemblyQualifiedTypeName.Length; ++i)
            {
                char c = assemblyQualifiedTypeName[i];

                if (c is '[' or ']')
                {
                    if (escapeDepth > 0 && escapeDepth % 2 == 1)
                    {
                        escapeDepth = 0;
                        continue;
                    }

                    escapeDepth = 0;
                    if (!fullTypeName.IsEmpty)
                    {
                        fullTypeName = default;
                        return false;
                    }

                    if (c == '[')
                    {
                        ++genericDepth;
                        continue;
                    }

                    --genericDepth;
                    if (genericDepth < 0)
                    {
                        fullTypeName = default;
                        assemblyName = default;
                        return false;
                    }
                }

                if (c == '\\')
                {
                    ++escapeDepth;
                    continue;
                }

                if (genericDepth != 0 || c != ',')
                {
                    escapeDepth = 0;
                    continue;
                }

                if (escapeDepth > 0 && escapeDepth % 2 == 1)
                {
                    escapeDepth = 0;
                    continue;
                }

                escapeDepth = 0;

                if (isLookingForAssemblyName)
                {
                    assemblyName = assemblyName.Slice(0, i).TrimEnd();
                    return !assemblyName.IsEmpty;
                }

                fullTypeName = assemblyQualifiedTypeName.Slice(0, i).Trim();
                if (fullTypeName.IsEmpty)
                    return false;
                assemblyQualifiedTypeName = assemblyQualifiedTypeName.Slice(i + 1).Trim();
                assemblyName = assemblyQualifiedTypeName;
                isLookingForAssemblyName = true;
            }

            if (genericDepth > 0)
            {
                fullTypeName = default;
                assemblyName = default;
            }
        }
        else
        {
            fullTypeName = default;
            assemblyName = default;
            if (assemblyQualifiedTypeName.Length == 0)
                return false;

            int lastIndex = -1;
            while (true)
            {
                ++lastIndex;
                if (lastIndex >= assemblyQualifiedTypeName.Length)
                    break;
                int nextInd = assemblyQualifiedTypeName.Slice(lastIndex).IndexOf(',');
                if (nextInd <= 0)
                    break;

                nextInd += lastIndex;

                if (IsEscaped(assemblyQualifiedTypeName, nextInd))
                {
                    lastIndex = nextInd;
                    continue;
                }

                if (isLookingForAssemblyName)
                {
                    assemblyName = assemblyName.Slice(0, nextInd).TrimEnd();
                    return !assemblyName.IsEmpty;
                }

                fullTypeName = assemblyQualifiedTypeName.Slice(0, nextInd).Trim();
                if (fullTypeName.IsEmpty)
                    return false;
                assemblyQualifiedTypeName = assemblyQualifiedTypeName.Slice(nextInd + 1).Trim();
                assemblyName = assemblyQualifiedTypeName;
                isLookingForAssemblyName = true;
                lastIndex = 0;
            }
        }

        return !assemblyName.IsEmpty;
    }

    private static bool IsEscaped(ReadOnlySpan<char> text, int index)
    {
        if (index == 0)
            return false;

        int slashCt = 0;
        while (index > 0 && text[index - 1] == '\\')
        {
            ++slashCt;
            --index;
        }

        return slashCt % 2 == 1;
    }

    /// <inheritdoc />
    public bool Equals(QualifiedType other)
    {
        return TypesEqual(Type, other.Type, _isCaseInsensitive & other._isCaseInsensitive);
    }

    /// <inheritdoc />
    public bool Equals(string? other)
    {
        return TypesEqual(Type, other, _isCaseInsensitive);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            QualifiedType t => TypesEqual(Type, t.Type, _isCaseInsensitive & t._isCaseInsensitive),
            string s => TypesEqual(Type, s),
            _ => false
        };
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return (_isCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal).GetHashCode(Normalized.Type ?? string.Empty);
    }

    /// <inheritdoc />
    public int CompareTo(QualifiedType other)
    {
        bool ci = _isCaseInsensitive & other._isCaseInsensitive;
        return TypesEqual(Type, other.Type, ci)
            ? 0
            : string.Compare(Type, other.Type, ci ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Normalized.Type;
    }

    /// <inheritdoc />
    public int CompareTo(object? obj) => obj is QualifiedType t ? CompareTo(t) : 1;

    public static bool operator ==(QualifiedType left, QualifiedType right) => left.Equals(right);

    public static bool operator !=(QualifiedType left, QualifiedType right) => !left.Equals(right);

    public static implicit operator string(QualifiedType type) => type.Type;

    public static implicit operator QualifiedType(string? type) => new QualifiedType(type!);
    public static implicit operator QualifiedType(Type? type) => type == null ? None : new QualifiedType(type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
}