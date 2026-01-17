using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Immutable;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Maps type IDs to basic factory functions.
/// </summary>
internal class TypeFactoryById : ITypeFactory
{
    private readonly ImmutableDictionary<string, Func<IType>>? _typeFactoriesSimple;
    private readonly ImmutableDictionary<string, Func<IDatSpecificationReadContext, DatProperty, string, IType>>? _typeFactoriesComplex;

    public TypeFactoryById(params (string, Func<IType>)[] typeTable)
    {
        if (typeTable == null)
            throw new ArgumentNullException(nameof(typeTable));

        ImmutableDictionary<string, Func<IType>>.Builder bldr = ImmutableDictionary.CreateBuilder<string, Func<IType>>();
        foreach ((string Key, Func<IType> Value) factory in typeTable)
        {
            bldr[factory.Key] = factory.Value;
        }

        _typeFactoriesSimple = bldr.ToImmutable();
    }

    public TypeFactoryById(params (string, Func<IDatSpecificationReadContext, DatProperty, string, IType>)[] typeTable)
    {
        if (typeTable == null)
            throw new ArgumentNullException(nameof(typeTable));

        ImmutableDictionary<string, Func<IDatSpecificationReadContext, DatProperty, string, IType>>.Builder bldr = ImmutableDictionary.CreateBuilder<string, Func<IDatSpecificationReadContext, DatProperty, string, IType>>();
        foreach ((string Key, Func<IDatSpecificationReadContext, DatProperty, string, IType> Value) factory in typeTable)
        {
            bldr[factory.Key] = factory.Value;
        }

        _typeFactoriesComplex = bldr.ToImmutable();
    }

    /// <inheritdoc />
    public IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context = "")
    {
        if (_typeFactoriesSimple == null)
        {
            return _typeFactoriesComplex![typeId](spec, owner, context);
        }

        return _typeFactoriesSimple[typeId]();
    }
}
