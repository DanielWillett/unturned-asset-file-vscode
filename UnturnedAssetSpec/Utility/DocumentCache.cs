using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Stores a value as long as an integer value stays the same.
/// </summary>
/// <typeparam name="TValue">The type of value being stored.</typeparam>
/// <typeparam name="TState">An arbitrary state value passed to the value and version accessors.</typeparam>
// todo maybe remove this
internal struct DocumentCache<TValue, TState>
{
    internal readonly Func<TState, int> VersionGetter;
    internal readonly TState State;
    internal TValue? Value;
    internal int Version;

    public DocumentCache(TState state, Func<TState, int> versionGetter)
    {
        VersionGetter = versionGetter;
        Version = int.MinValue;
        State = state;
    }

    public void GetOrUpdateValue(out TValue value, Func<TState, TValue> valueGetter)
    {
        int state = VersionGetter(State);
        if (state == Version)
        {
            TValue? currentValue = Value;
            if (state == Version)
            {
                value = currentValue!;
                return;
            }
        }

        int v1 = state;
        Value = valueGetter(State);
        Version = v1;
        int v2 = VersionGetter(State);
        while (v1 >= v2)
        {
            v1 = VersionGetter(State);
            Value = valueGetter(State);
            Version = v1;
            v2 = VersionGetter(State);
        }

        value = Value;
    }

    public void UpdateValue(TValue value)
    {
        int v1, v2;
        do
        {
            v1 = VersionGetter(State);
            Value = value;
            Version = v1;
            v2 = VersionGetter(State);
        } while (v1 >= v2);
    }

    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        int state = VersionGetter(State);
        if (state == Version)
        {
            TValue? currentValue = Value;
            if (state == Version)
            {
                value = currentValue!;
                return true;
            }
        }

#if NET5_0_OR_GREATER
        Unsafe.SkipInit(out value);
#else
        value = default;
#endif
        return false;
    }
}
