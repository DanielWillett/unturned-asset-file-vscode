using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Utility;

internal static class JsonHelper
{
    public static bool TryCreateJValue<TValue>(TValue value, [NotNullWhen(true)] out JValue? json)
    {
        if (value == null)
        {
            json = JValue.CreateNull();
        }
        else if (typeof(TValue) == typeof(string))
        {
            json = new JValue(MathMatrix.As<TValue, string>(value));
        }
        else if (typeof(TValue) == typeof(int))
        {
            json = new JValue(MathMatrix.As<TValue, int>(value));
        }
        else if (typeof(TValue) == typeof(byte))
        {
            json = new JValue(MathMatrix.As<TValue, byte>(value));
        }
        else if (typeof(TValue) == typeof(bool))
        {
            json = new JValue(MathMatrix.As<TValue, bool>(value));
        }
        else if (typeof(TValue) == typeof(long))
        {
            json = new JValue(MathMatrix.As<TValue, long>(value));
        }
        else if (typeof(TValue) == typeof(float))
        {
            json = new JValue(MathMatrix.As<TValue, float>(value));
        }
        else if (typeof(TValue) == typeof(ulong))
        {
            json = new JValue(MathMatrix.As<TValue, ulong>(value));
        }
        else if (typeof(TValue) == typeof(short))
        {
            json = new JValue(MathMatrix.As<TValue, short>(value));
        }
        else if (typeof(TValue) == typeof(ushort))
        {
            json = new JValue(MathMatrix.As<TValue, ushort>(value));
        }
        else if (typeof(TValue) == typeof(double))
        {
            json = new JValue(MathMatrix.As<TValue, double>(value));
        }
        else if (typeof(TValue) == typeof(decimal))
        {
            json = new JValue(MathMatrix.As<TValue, decimal>(value));
        }
        else if (typeof(TValue) == typeof(sbyte))
        {
            json = new JValue(MathMatrix.As<TValue, sbyte>(value));
        }
        else if (typeof(TValue) == typeof(GuidOrId))
        {
            ref GuidOrId id = ref Unsafe.As<TValue, GuidOrId>(ref value);
            json = id.IsId ? new JValue(id.Id) : new JValue(id.Guid);
        }
        else if (typeof(TValue) == typeof(QualifiedType))
        {
            ref QualifiedType type = ref Unsafe.As<TValue, QualifiedType>(ref value);
            json = type.IsNull ? JValue.CreateNull() : new JValue(type.Normalized.Type);
        }
        else if (typeof(TValue) == typeof(Guid))
        {
            json = new JValue(MathMatrix.As<TValue, Guid>(value));
        }
        else if (typeof(TValue) == typeof(DateTime))
        {
            json = new JValue(MathMatrix.As<TValue, DateTime>(value));
        }
        else if (typeof(TValue) == typeof(DateTimeOffset))
        {
            json = new JValue(MathMatrix.As<TValue, DateTimeOffset>(value));
        }
        else if (typeof(TValue) == typeof(TimeSpan))
        {
            json = new JValue(MathMatrix.As<TValue, TimeSpan>(value));
        }
        else if (typeof(TValue) == typeof(char))
        {
            json = new JValue(MathMatrix.As<TValue, char>(value));
        }
        else
        {
            json = null;
            return false;
        }

        return true;
    }
}
