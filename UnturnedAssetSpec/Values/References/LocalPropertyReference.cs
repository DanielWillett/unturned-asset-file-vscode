using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// A resolved <see cref="IPropertyReferenceValue"/> that doesn't cross-reference.
/// </summary>
public class LocalPropertyReference : IPropertyReferenceValue
{
    private readonly PropertyReference _propertyReference;
    private readonly IAssetSpecDatabase _database;

    private DatProperty? _property;

    /// <inheritdoc />
    public DatProperty Property
    {
        get
        {
            if (_property == null && !TryCacheProperty())
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
        _database = database.ResolveFacade();
        _database.OnInitialize(_ =>
        {
            if (_property == null)
            {
                TryCacheProperty();
            }
            return Task.CompletedTask;
        });
    }

    [MemberNotNullWhen(true, nameof(_property))]
    private bool TryCacheProperty()
    {
        if (!_database.IsInitialized)
            return false;

        DatTypeWithProperties objectOwner = Owner.Owner;
        if (_propertyReference.TypeName != null)
        {

        }
        else
        {

        }

        return false;
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        _propertyReference.WriteToJson(writer);
    }

    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
    {
        ISourceFile file = ctx.SourceFile;
        //_propertyReference.ResolveInFile(file);
        // todo
        return false;
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

public class LocalPropertyReference<TValue> : LocalPropertyReference, IPropertyReferenceValue<TValue>
    where TValue : IEquatable<TValue>
{
    /// <inheritdoc />
    public IType<TValue> Type { get; }

    public LocalPropertyReference(in PropertyReference pref, DatProperty owner, IAssetSpecDatabase database, IType<TValue> type)
        : base(in pref, owner, database)
    {
        Type = type;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TValue> value, in FileEvaluationContext ctx)
    {
        value = Optional<TValue>.Null;
        return false;
    }

    /// <inheritdoc />
    public override bool Equals(IValue? other)
    {
        return other is LocalPropertyReference<TValue> v && Type.Equals(v.Type) && base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Type);
    }

    bool IValue<TValue>.TryGetConcreteValue(out Optional<TValue> value)
    {
        value = Optional<TValue>.Null;
        return false;
    }
}