using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed reference to a property within the same file as the referencing property.
/// </summary>
public class LocalPropertyReference : IPropertyReferenceValue
{
    private PropertyReference _propertyReference;
    private readonly IAssetSpecDatabase _database;

    private DatProperty? _property;

    /// <inheritdoc />
    public DatProperty Property
    {
        get
        {
            if (_property != null)
                return _property;
            
            if (_propertyReference.IsCrossReference || !TryCacheProperty(NullLoggerFactory.Instance, in FileEvaluationContext.None))
                throw new InvalidOperationException("Unable to cache property at this moment.");

            return _property;
        }
    }

    /// <inheritdoc />
    public DatProperty Owner { get; }

    public LocalPropertyReference(in PropertyReference pref, DatProperty owner, IAssetSpecDatabase database)
    {
        Owner = owner;
        _propertyReference = pref;
        _database = database;
        
        if (_propertyReference.IsCrossReference)
            return;

        _database.OnInitialize((_, loggerFactory) =>
        {
            if (_property != null)
                return Task.CompletedTask;

            if (!TryCacheProperty(loggerFactory, in FileEvaluationContext.None))
            {
                loggerFactory.CreateLogger<LocalPropertyReference>().LogError(
                    "Failed to resolve property reference \"{0}\" from property \"{1}\".", _propertyReference.ToString(), ((IDatSpecificationObject)Owner).FullName
                );
            }

            return Task.CompletedTask;
        });
    }

    [MemberNotNullWhen(true, nameof(_property))]
    private bool TryCacheProperty(ILoggerFactory loggerFactory, in FileEvaluationContext ctx)
    {
        if (!_database.IsInitialized)
            return false;

        return _propertyReference.TryGetProperty(Owner, in ctx, _database, loggerFactory, out _property);
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        _propertyReference.WriteToJson(writer);
    }

    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if ((_property == null || _propertyReference.IsCrossReference) && !TryCacheProperty(NullLoggerFactory.Instance, in ctx))
        {
            return false;
        }

        return _property.VisitValue(ref visitor, in ctx);
    }

    /// <inheritdoc />
    public virtual bool Equals(IValue? other)
    {
        return other is LocalPropertyReference r && r._propertyReference.Equals(_propertyReference);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is LocalPropertyReference r && Equals(r);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(2050227563, _propertyReference);
    }

    bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor) => false;
    bool IValue.IsNull => false;
}

/// <summary>
/// A strongly-typed reference to a property within the same file as the referencing property.
/// </summary>
/// <typeparam name="TReferencedValue">The type of value being referenced.</typeparam>
public class LocalPropertyReference<TReferencedValue> : LocalPropertyReference, IPropertyReferenceValue<TReferencedValue>
    where TReferencedValue : IEquatable<TReferencedValue>
{
    /// <inheritdoc />
    public IType<TReferencedValue> Type { get; }

    public LocalPropertyReference(in PropertyReference pref, DatProperty owner, IAssetSpecDatabase database, IType<TReferencedValue> type)
        : base(in pref, owner, database)
    {
        Type = type;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TReferencedValue> value, in FileEvaluationContext ctx)
    {
        ValueVisitor visitor;
        visitor.Value = Optional<TReferencedValue>.Null;
        visitor.Success = false;

        VisitValue(ref visitor, in ctx);
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
        return other is LocalPropertyReference<TReferencedValue> v && Type.Equals(v.Type) && base.Equals(other);
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
        public void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>
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