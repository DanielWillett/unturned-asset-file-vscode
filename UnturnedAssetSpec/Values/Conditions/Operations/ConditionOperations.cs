using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#pragma warning disable IDE0130

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning restore IDE0130

/// <summary>
/// Manages implementations of <see cref="IConditionOperation"/>.
/// </summary>
public static class ConditionOperations
{
    private static readonly ConcurrentDictionary<string, OneOrMore<IConditionOperation>> OperationTable
        = new ConcurrentDictionary<string, OneOrMore<IConditionOperation>>(StringComparer.Ordinal);

    static ConditionOperations()
    {
        RegisterOperation(Operations.Equal.Instance);
        RegisterOperation(Operations.EqualCaseInsensitive.Instance);
        RegisterOperation(Operations.NotEqual.Instance);
        RegisterOperation(Operations.NotEqualCaseInsensitive.Instance);
        RegisterOperation(Operations.GreaterThan.Instance);
        RegisterOperation(Operations.GreaterThanCaseInsensitive.Instance);
        RegisterOperation(Operations.GreaterThanOrEqual.Instance);
        RegisterOperation(Operations.GreaterThanOrEqualCaseInsensitive.Instance);
        RegisterOperation(Operations.LessThan.Instance);
        RegisterOperation(Operations.LessThanCaseInsensitive.Instance);
        RegisterOperation(Operations.LessThanOrEqual.Instance);
        RegisterOperation(Operations.LessThanOrEqualCaseInsensitive.Instance);
        RegisterOperation(Operations.Contains.Instance);
        RegisterOperation(Operations.ContainsCaseInsensitive.Instance);
    }


    /// <summary>
    /// Enumerates through all active operations.
    /// </summary>
    /// <remarks>This requires copying data so should be avoided if possible.</remarks>
    public static IEnumerator<IConditionOperation> EnumerateOperations()
    {
        // ReSharper disable once NotDisposedResourceIsReturned
        return OperationTable.Values.Where(x => !x.IsNull).Select(x => x.Last()).GetEnumerator();
    }

    /// <summary>
    /// Adds a new operation to the operation registration list.
    /// </summary>
    public static void RegisterOperation(IConditionOperation operation)
    {
#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        OperationTable.AddOrUpdate(
            operation.Name,
            static (_, operation) => new OneOrMore<IConditionOperation>(operation),
            static (_, e, operation) => e.Add(operation),
            operation
        );
#else
        OperationTable.AddOrUpdate(
            operation.Name,
            _ => new OneOrMore<IConditionOperation>(operation),
            (_, e) => e.Add(operation)
        );
#endif
    }

    /// <summary>
    /// Removes an operation from the operation registration list.
    /// </summary>
    public static void DeregisterOperation(IConditionOperation operation)
    {
        string operationName = operation.Name;
        while (true)
        {
#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            OneOrMore<IConditionOperation> m = OperationTable.AddOrUpdate(
                operationName,
                static (_, _) => OneOrMore<IConditionOperation>.Null,
                static (_, e, operation) => e.Remove(operation),
                operation
            );
#else
            OneOrMore<IConditionOperation> m = OperationTable.AddOrUpdate(
                operationName,
                _ => OneOrMore<IConditionOperation>.Null,
                (_, e) => e.Remove(operation)
            );
#endif
            if (!m.IsNull || !OperationTable.TryRemove(operationName, out OneOrMore<IConditionOperation> functions) || functions.IsNull)
                break;

#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            OperationTable.AddOrUpdate(
                operationName,
                static (_, functions) => functions,
                static (_, e, functions) =>
                {
                    foreach (IConditionOperation f in functions)
                        e = e.Add(f);
                    return e;
                },
                functions
            );
#else
            OperationTable.AddOrUpdate(
                operationName,
                _ => functions,
                (_, e) =>
                {
                    foreach (IConditionOperation f in functions)
                        e = e.Add(f);
                    return e;
                }
            );
#endif
        }
    }

    /// <summary>
    /// Attempts to find the most recently added operation with the given name.
    /// </summary>
    public static bool TryGetOperation([NotNullWhen(true)] string? operationName, [NotNullWhen(true)] out IConditionOperation? operation)
    {
        if (operationName == null)
        {
            operation = null;
            return false;
        }

        if (OperationTable.TryGetValue(operationName, out OneOrMore<IConditionOperation> functions) && functions.Length > 0)
        {
            operation = functions[^1];
            return true;
        }

        operation = null;
        return false;
    }
}
