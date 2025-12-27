using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Numerics;

namespace UnturnedAssetSpecTests;

public partial class MathMatrixTests
{
    public const int ActiveTypeLen = 12;

    public const int InactiveTypeLen =
#if NET7_0_OR_GREATER
            7
#elif NET5_0_OR_GREATER
            5
#else
            2
#endif
        ;

    public static Type[] ActiveTypes = new Type[ActiveTypeLen]
    {
        typeof(string), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte), typeof(long), typeof(int),
        typeof(short), typeof(sbyte), typeof(float), typeof(double), typeof(decimal)
    };

    public static Type[] InactiveTypes = new Type[InactiveTypeLen]
    {
#if NET5_0_OR_GREATER
        typeof(nint), typeof(nuint),
        typeof(Half),
#if NET7_0_OR_GREATER
        typeof(Int128), typeof(UInt128),
#endif
#endif
        typeof(char), typeof(bool)
    };

    private static BigInteger ToBigInt(object? v)
    {
        return v switch
        {
            sbyte i => new BigInteger(i),
            byte i => new BigInteger(i),
            short i => new BigInteger(i),
            ushort i => new BigInteger(i),
            int i => new BigInteger(i),
            uint i => new BigInteger(i),
            long i => new BigInteger(i),
            ulong i => new BigInteger(i),
            float f => new BigInteger(f),
            double f => new BigInteger(f),
            decimal f => new BigInteger(f),
            string s => (s.Contains(".") ? new BigInteger(decimal.Parse(s)) : BigInteger.Parse(s)),
            null => throw new ArgumentNullException(nameof(v)),
            _ => throw new InvalidCastException($"Unable to convert {v.GetType()} to BigInteger.")
        };
    }

    private static decimal ToDecimal(object? v)
    {
        return v switch
        {
            sbyte i => new decimal(i),
            byte i => new decimal(i),
            short i => new decimal(i),
            ushort i => new decimal(i),
            int i => new decimal(i),
            uint i => new decimal(i),
            long i => new decimal(i),
            ulong i => new decimal(i),
            float f => new decimal(f),
            double f => new decimal(f),
            decimal f => f,
            string s => decimal.Parse(s),
            null => throw new ArgumentNullException(nameof(v)),
            _ => throw new InvalidCastException($"Unable to convert {v.GetType()} to decimal.")
        };
    }


    private static class GetTestValues<T>
    {
        public static readonly T[] Values;

        static GetTestValues()
        {
            if (typeof(T) == typeof(ulong))
                Values = (T[])(object)new ulong[] { 0ul, long.MaxValue, long.MaxValue + 1ul, ulong.MaxValue, 3ul, 6148914691236517205ul, 4ul, 4611686018427387904, 2305843009213693952, 7ul, 1317624576693539401 };
            else if (typeof(T) == typeof(uint))
                Values = (T[])(object)new uint[] { 0u, int.MaxValue, int.MaxValue + 1u, uint.MaxValue, 3u, 4, 536870912 };
            else if (typeof(T) == typeof(ushort))
                Values = (T[])(object)new ushort[] { 0, (ushort)short.MaxValue, short.MaxValue + 1, ushort.MaxValue, 3, 16384, 32768, 4, 2 };
            else if (typeof(T) == typeof(byte))
                Values = (T[])(object)new byte[] { 0, (byte)sbyte.MaxValue, sbyte.MaxValue + 1, byte.MaxValue, 3, 2, 32, 64, 4, 8, 128 };
            else if (typeof(T) == typeof(long))
                Values = (T[])(object)new long[] { 0, long.MinValue, long.MinValue + 1, long.MaxValue, int.MinValue, int.MaxValue, -3, 3, -4, 4, 6148914691236517205, 4611686018427387904, 2305843009213693952, 1317624576693539401, -7, 7, -1317624576693539401 };
            else if (typeof(T) == typeof(int))
                Values = (T[])(object)new int[] { 0, int.MinValue, int.MinValue + 1, int.MaxValue, -3, 3, -4, 4, 536870912, -536870912 };
            else if (typeof(T) == typeof(short))
                Values = (T[])(object)new short[] { 0, short.MinValue, short.MinValue + 1, short.MaxValue, -3, 3, 16384, 4, 2, -16384, -32768, -4, -2 };
            else if (typeof(T) == typeof(sbyte))
                Values = (T[])(object)new sbyte[] { 0, sbyte.MinValue, sbyte.MinValue + 1, sbyte.MaxValue, -3, 3, 32, 64, 4, 8, -32, -64, -4, 8, -128, -2, 2 };
            else if (typeof(T) == typeof(float))
                Values = (T[])(object)new float[] { 0, -3f, 3f };
            else if (typeof(T) == typeof(double))
                Values = (T[])(object)new double[] { 0, -3d, 3d };
            else if (typeof(T) == typeof(decimal))
                Values = (T[])(object)new decimal[] { 0, -3m, 3m };
            else if (typeof(T) == typeof(string))
                Values = (T[])(object)new string[]
                {
                    "0",
                    "9223372036854775807",
                    "-9223372036854775807",
                    "9223372036854775808",
                    "-9223372036854775808",
                    "18446744073709551615",
                    "2147483647",
                    "-2147483647",
                    "2147483648",
                    "-2147483648",
                    "32767",
                    "-32767",
                    "32768",
                    "65535",
                    "65536",
                    "-32768",
                    "127",
                    "-127",
                    "255",
                    "256",
                    "0.0",
                    "-3.0",
                    "3.0"
                };
            else
                throw new InvalidOperationException($"Type invalid: {typeof(T)}.");
        }
    }

    private abstract class BaseExecuteVisitor : IGenericVisitor
    {
        public int ExecutedCt;
        public object? Value;
        public abstract bool Execute();
        
        public abstract bool Execute(params object[] values);

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            ++ExecutedCt;
            Value = value;
        }
    }

}