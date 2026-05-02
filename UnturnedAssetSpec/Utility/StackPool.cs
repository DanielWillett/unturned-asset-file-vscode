using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Pool for stacks.
/// </summary>
internal static class StackPool<T>
{
    private static readonly ConcurrentBag<Stack<T>> Stacks = new ConcurrentBag<Stack<T>>();

    internal static Stack<T> Get(int capacity = 0)
    {
        if (Stacks.TryTake(out Stack<T>? stack))
        {
#if NET6_0_OR_GREATER
            if (capacity > 0)
            {
                stack.EnsureCapacity(capacity);
            }
#endif
            return stack;
        }

        stack = new Stack<T>(capacity);
        return stack;
    }

    internal static void Return(Stack<T> stack)
    {
        if (stack.Count > 0)
        {
            stack.Clear();
        }

        Stacks.Add(stack);
    }
}