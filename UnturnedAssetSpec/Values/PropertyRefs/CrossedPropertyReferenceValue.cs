using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed reference to a property within a different file than the referencing property.
/// </summary>
public class CrossedPropertyReferenceValue : ICrossedPropertyReference
{
    private PropertyReference _propertyReference;

    /// <inheritdoc />
    public ref readonly PropertyReference Reference => ref _propertyReference;

    DatProperty IPropertyReferenceValue.Property => throw new NotSupportedException("Not supported on cross-reference property references.");

    /// <inheritdoc />
    public DatProperty Owner { get; }

    public CrossedPropertyReferenceValue(in PropertyReference pref, DatProperty owner)
    {
        Owner = owner;
        _propertyReference = pref;
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        _propertyReference.WriteToJson(writer);
    }

    public bool VisitValue<TVisitor>(ref TVisitor visitor, ref FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (!_propertyReference.TryGetProperty(Owner, ref ctx, out DatProperty? property))
        {
            return false;
        }

        return property.VisitValue(
            ref visitor,
            ref ctx,
            _propertyReference.Breadcrumbs,
            missingValueBahvior: TypeParserMissingValueBehavior.FallbackToDefaultValue
        );
    }

    /// <inheritdoc />
    public virtual bool Equals(IValue? other)
    {
        return other is CrossedPropertyReferenceValue r && r._propertyReference.Equals(_propertyReference);
    }

    /// <inheritdoc />
    public bool Equals(IExpressionNode? other)
    {
        return other is CrossedPropertyReferenceValue r && r._propertyReference.Equals(_propertyReference);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is CrossedPropertyReferenceValue r && Equals((IValue?)r);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1332296112, _propertyReference);
    }

    bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor) => false;
    bool IValue.IsNull => false;

    /// <inheritdoc />
    public bool TryResolveReference(
        ref FileEvaluationContext oldContext,
        [UnscopedRef] out FileEvaluationContext newContext,
        [NotNullWhen(true)] out DatProperty? property
    )
    {
        if (!_propertyReference.TryGetCrossReferencedTarget(Owner, ref oldContext, oldContext.Services.Database, out DiscoveredDatFile? discoveredFile)
            || !oldContext.Services.Database.FileTypes.TryGetValue(discoveredFile.Type, out DatFileType? fileType)
            || !_propertyReference.TryGetProperty(Owner, ref oldContext, out property, fileType)
            || oldContext.Services.Workspace.TemporarilyGetOrLoadFile(discoveredFile.FilePath) is not { } openedFile)
        {
            newContext = default;
            property = null;
            return false;
        }

        ISourceFile file = openedFile.SourceFile;
        newContext = new FileEvaluationContext(oldContext.Services, file);
        try
        {
            newContext.RootBreadcrumbs = _propertyReference.Breadcrumbs;
            newContext.CachedProperty = property;

            if (file is IAssetSourceFile assetFile)
            {
                if (!assetFile.TryGetProperty(property, ref newContext, out IPropertySourceNode? propertyNode))
                {
                    DisposeContext(ref newContext);
                    return false;
                }

                newContext.RootPosition = propertyNode.GetRootPosition();
                newContext.CachedPropertyNode = propertyNode;
            }
        }
        catch
        {
            DisposeContext(ref newContext);
            throw;
        }

        return true;
    }

   

    /// <inheritdoc />
    public void DisposeContext(ref FileEvaluationContext newContext)
    {
        if (newContext.File.WorkspaceFile is IDisposable disp)
        {
            disp.Dispose();
        }
    }
    IPropertyReferenceValue IPropertyReferenceExpressionNode.Value => this;
}

/// <summary>
/// A strongly-typed reference to a property within a different file than the referencing property.
/// </summary>
/// <typeparam name="TReferencedValue">The type of value being referenced.</typeparam>
public class CrossedPropertyReferenceValue<TReferencedValue> : CrossedPropertyReferenceValue, IPropertyReferenceValue<TReferencedValue>
    where TReferencedValue : IEquatable<TReferencedValue>
{
    /// <inheritdoc />
    public IType<TReferencedValue> Type { get; }

    public CrossedPropertyReferenceValue(in PropertyReference pref, DatProperty owner, IType<TReferencedValue> type)
        : base(in pref, owner)
    {
        Type = type;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TReferencedValue> value, ref FileEvaluationContext ctx)
    {
        ValueVisitor visitor;
        visitor.Value = Optional<TReferencedValue>.Null;
        visitor.Success = false;

        VisitValue(ref visitor, ref ctx);
        if (visitor.Success)
        {
            value = visitor.Value;
            return true;
        }

        value = Optional<TReferencedValue>.Null;
        return false;
    }

    /// <inheritdoc />
    public override bool Equals(IValue? other)
    {
        return other is CrossedPropertyReferenceValue<TReferencedValue> v && Type.Equals(v.Type) && base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Type);
    }

    bool IValue<TReferencedValue>.TryGetConcreteValue(out Optional<TReferencedValue> value)
    {
        value = Optional<TReferencedValue>.Null;
        return false;
    }

    private struct ValueVisitor : IValueVisitor
    {
        public Optional<TReferencedValue> Value;
        public bool Success;

        /// <inheritdoc />
        public void Accept<TValue>(IType<TValue> type, Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            if (typeof(TValue) == typeof(TReferencedValue))
            {
                Value = Unsafe.As<Optional<TValue>, Optional<TReferencedValue>>(ref value);
                Success = true;
                return;
            }

            ConvertVisitor<TReferencedValue> converter = default;
            converter.IsNull = !value.HasValue;

            converter.Accept(value.Value);
            if (!converter.WasSuccessful)
                return;

            Value = new Optional<TReferencedValue>(converter.Result);
            Success = true;
        }
    }
}