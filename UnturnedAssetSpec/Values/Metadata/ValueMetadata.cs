using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS8500

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;


/// <summary>
/// Extension methods relating to <see cref="ValueMetadata{T}"/>.
/// </summary>
public static class ValueMetadataExtensions
{
    /// <summary>
    /// Parses a value and visits the metadata for it.
    /// </summary>
    /// <typeparam name="TVisitor">A visitor type to accept the metadata.</typeparam>
    /// <param name="value">The value to resolve.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="visitor">A visitor to accept the metadata.</param>
    /// <returns>Whether or not the visitor was invoked with a valid value.</returns>
    public static ValueMetadata<TValue> CreateMetadata<TValue>(
        this IType<TValue> type,
        Optional<TValue> value,
        IAnyValueSourceNode? valueNode,
        ref FileEvaluationContext ctx,
        out bool metadataIsPopulated
    ) where TValue : IEquatable<TValue>
    {
        ValueMetadata<TValue> metadata = new ValueMetadata<TValue>(type, value);
        metadataIsPopulated = type is IValueMetadataProviderType<TValue> provider
                              && provider.PopulateMetadata(valueNode, ref ctx, metadata);
        return metadata;
    }

    /// <summary>
    /// Parses a value and visits the metadata for it.
    /// </summary>
    /// <typeparam name="TVisitor">A visitor type to accept the metadata.</typeparam>
    /// <param name="value">The value to resolve.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="visitor">A visitor to accept the metadata.</param>
    /// <returns>Whether or not the visitor was invoked with a valid value.</returns>
    public static bool TryVisitMetadata<TVisitor>(
        this IValue value,
        ref TVisitor visitor,
        IAnyValueSourceNode? valueNode,
        ref FileEvaluationContext ctx,
        out bool metadataIsPopulated
    ) where TVisitor : IValueMetadataVisitor
#if NET9_0_OR_GREATER
          , allows ref struct
#endif
    {
        ValueParseVisitorForMetadata<TVisitor> metadataVisitor;
        metadataVisitor.Visited = false;
        metadataVisitor.GotMetadata = false;
        metadataVisitor.ValueNode = valueNode;
        unsafe
        {
            fixed (TVisitor* ptr = &visitor)
            fixed (FileEvaluationContext* ctxPtr = &ctx)
            {
                metadataVisitor.Visitor = ptr;
                metadataVisitor.Context = ctxPtr;
                value.VisitValue(ref metadataVisitor, ref ctx);
            }
        }

        metadataIsPopulated = metadataVisitor.GotMetadata;
        return metadataVisitor.Visited;
    }

    private unsafe struct ValueParseVisitorForMetadata<TVisitor> : IValueVisitor
        where TVisitor : IValueMetadataVisitor
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        public TVisitor* Visitor;
        public FileEvaluationContext* Context;
        public IAnyValueSourceNode? ValueNode;
        public bool Visited;
        public bool GotMetadata;

        public void Accept<TValue>(IType<TValue> type, Optional<TValue> value)
            where TValue : IEquatable<TValue>
        {
            Visited = true;
            ValueMetadata<TValue> metadata = type.CreateMetadata(
                value,
                ValueNode,
                ref Unsafe.AsRef<FileEvaluationContext>(Context),
                out GotMetadata
            );
            Visitor->Accept(metadata, GotMetadata);
        }
    }
}

public interface IValueMetadataVisitor
{
    void Accept<T>(ValueMetadata<T> metadata, bool metadataIsPopulated)
        where T : IEquatable<T>;
}

/// <summary>
/// Created by types that implement <see cref="IValueMetadataProviderType"/>.
/// </summary>
public class ValueMetadata<TValue>(IType<TValue> type, Optional<TValue> value)
    where TValue : IEquatable<TValue>
{
    public IType<TValue> Type { get; } = type;

    public Optional<TValue> Value { get; } = value;

    public string? DisplayName { get; set; }

    public QualifiedType DeclaringType { get; set; }

    public string? Variable { get; set; }

    public string? Description { get; set; }

    public Version? Version { get; set; }

    public string? Docs { get; set; }

    public string? LinkName { get; set; }

    public bool IsDeprecated { get; set; }

    public bool IsExperimental { get; set; }

    public QualifiedType CorrespondingType { get; set; }

    public void CopyDetailsFrom<T>(ValueMetadata<T> metadata2)
        where T : IEquatable<T>
    {
        DisplayName = metadata2.DisplayName;
        DeclaringType = metadata2.DeclaringType;
        Variable = metadata2.Variable;
        Description = metadata2.Description;
        Version = metadata2.Version;
        Docs = metadata2.Docs;
        LinkName = metadata2.LinkName;
        IsDeprecated = metadata2.IsDeprecated;
        IsExperimental = metadata2.IsExperimental;
        CorrespondingType = metadata2.CorrespondingType;
    }
}

#pragma warning restore CS8500