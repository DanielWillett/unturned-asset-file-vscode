using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

[TestFixture]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_TrigFunc([Range(0, InactiveTypeLen - 1)] int typeNum, [Range(0, 5)] int trigFunc, [Values(true, false)] bool deg)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(InactiveTypes[typeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, trigFunc, deg)!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_TrigFunc([Range(0, ActiveTypeLen - 1)] int typeNum, [Range(0, 5)] int trigFunc, [Values(true, false)] bool deg)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(ActiveTypes[typeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, trigFunc, deg)!;

        Assert.That(testInstance.Execute(), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
    }

    [Test]
    [TestCase((byte)0, 0)]
    [TestCase((sbyte)0, 0)]
    [TestCase((ushort)0, 0)]
    [TestCase((short)0, 0)]
    [TestCase((uint)0, 0)]
    [TestCase(0, 0)]
    [TestCase("0", 0)]
    [TestCase((ulong)0, 0)]
    [TestCase((long)0, 0)]
    [TestCase(0f, 0)]
    [TestCase(0d, 0)]
    [TestCase((byte)90, 1)]
    [TestCase((sbyte)90, 1)]
    [TestCase((ushort)90, 1)]
    [TestCase((short)90, 1)]
    [TestCase((uint)90, 1)]
    [TestCase(90, 1)]
    [TestCase("90", 1)]
    [TestCase((ulong)90, 1)]
    [TestCase((long)90, 1)]
    [TestCase(90f, 1)]
    [TestCase(90d, 1)]
    public static void CheckValues_SinDeg(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 0, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_SinDeg_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => 90m
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 0, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(arg == 0m ? 0d : 1d).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, 1)]
    [TestCase((sbyte)0, 1)]
    [TestCase((ushort)0, 1)]
    [TestCase((short)0, 1)]
    [TestCase((uint)0, 1)]
    [TestCase(0, 1)]
    [TestCase("0", 1)]
    [TestCase((ulong)0, 1)]
    [TestCase((long)0, 1)]
    [TestCase(0f, 1)]
    [TestCase(0d, 1)]
    [TestCase((byte)90, 0)]
    [TestCase((sbyte)90, 0)]
    [TestCase((ushort)90, 0)]
    [TestCase((short)90, 0)]
    [TestCase((uint)90, 0)]
    [TestCase(90, 0)]
    [TestCase("90", 0)]
    [TestCase((ulong)90, 0)]
    [TestCase((long)90, 0)]
    [TestCase(90f, 0)]
    [TestCase(90d, 0)]
    public static void CheckValues_CosDeg(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 1, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_CosDeg_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => 90m
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 1, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(arg == 0m ? 1d : 0d).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, 0)]
    [TestCase((sbyte)0, 0)]
    [TestCase((ushort)0, 0)]
    [TestCase((short)0, 0)]
    [TestCase((uint)0, 0)]
    [TestCase(0, 0)]
    [TestCase("0", 0)]
    [TestCase((ulong)0, 0)]
    [TestCase((long)0, 0)]
    [TestCase(0f, 0)]
    [TestCase(0d, 0)]
    [TestCase((byte)45, 1)]
    [TestCase((sbyte)45, 1)]
    [TestCase((ushort)45, 1)]
    [TestCase((short)45, 1)]
    [TestCase((uint)45, 1)]
    [TestCase(45, 1)]
    [TestCase("45", 1)]
    [TestCase((ulong)45, 1)]
    [TestCase((long)45, 1)]
    [TestCase(45f, 1)]
    [TestCase(45d, 1)]
    [TestCase((byte)90, double.PositiveInfinity)]
    [TestCase((sbyte)90, double.PositiveInfinity)]
    [TestCase((ushort)90, double.PositiveInfinity)]
    [TestCase((short)90, double.PositiveInfinity)]
    [TestCase((uint)90, double.PositiveInfinity)]
    [TestCase(90, double.PositiveInfinity)]
    [TestCase("90", double.PositiveInfinity)]
    [TestCase((ulong)90, double.PositiveInfinity)]
    [TestCase((long)90, double.PositiveInfinity)]
    [TestCase(90f, float.PositiveInfinity)]
    [TestCase(90d, double.PositiveInfinity)]
    [TestCase((ushort)270, double.NegativeInfinity)]
    [TestCase((short)270, double.NegativeInfinity)]
    [TestCase((uint)270, double.NegativeInfinity)]
    [TestCase(270, double.NegativeInfinity)]
    [TestCase("270", double.NegativeInfinity)]
    [TestCase((ulong)270, double.NegativeInfinity)]
    [TestCase((long)270, double.NegativeInfinity)]
    [TestCase(270f, float.NegativeInfinity)]
    [TestCase(270d, double.NegativeInfinity)]
    [TestCase((sbyte)-90, double.NegativeInfinity)]
    [TestCase((short)-90, double.NegativeInfinity)]
    [TestCase(-90, double.NegativeInfinity)]
    [TestCase("-90", double.NegativeInfinity)]
    [TestCase((long)-90, double.NegativeInfinity)]
    [TestCase(-90f, float.NegativeInfinity)]
    [TestCase(-90d, double.NegativeInfinity)]
    [TestCase((short)-270, double.PositiveInfinity)]
    [TestCase(-270, double.PositiveInfinity)]
    [TestCase("-270", double.PositiveInfinity)]
    [TestCase((long)-270, double.PositiveInfinity)]
    [TestCase(-270f, float.PositiveInfinity)]
    [TestCase(-270d, double.PositiveInfinity)]
    public static void CheckValues_TanDeg(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 2, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_TanDeg_Decimal([Range(0, 5)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            1 => 45m,
            2 => 90m,
            3 => 270m,
            4 => -90m,
            _ => -270m
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 2, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(argType switch
        {
            0 => 0d,
            1 => 1d,
            3 or 4 => double.NegativeInfinity,
            _ => double.PositiveInfinity
        }).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, 0)]
    [TestCase((sbyte)0, 0)]
    [TestCase((ushort)0, 0)]
    [TestCase((short)0, 0)]
    [TestCase((uint)0, 0)]
    [TestCase(0, 0)]
    [TestCase("0", 0)]
    [TestCase((ulong)0, 0)]
    [TestCase((long)0, 0)]
    [TestCase(0f, 0)]
    [TestCase(0d, 0)]
    [TestCase("1.5707963267948966", 1)]
    [TestCase(MathF.PI / 2f, 1)]
    [TestCase(Math.PI / 2d, 1)]
    public static void CheckValues_SinRad(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 0, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_SinRad_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => new decimal(Math.PI / 2d)
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 0, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(argType == 0 ? 0d : 1d).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, 1)]
    [TestCase((sbyte)0, 1)]
    [TestCase((ushort)0, 1)]
    [TestCase((short)0, 1)]
    [TestCase((uint)0, 1)]
    [TestCase(0, 1)]
    [TestCase("0", 1)]
    [TestCase((ulong)0, 1)]
    [TestCase((long)0, 1)]
    [TestCase(0f, 1)]
    [TestCase(0d, 1)]
    [TestCase("1.5707963267948966", 0)]
    [TestCase(MathF.PI / 2f, 0)]
    [TestCase(Math.PI / 2d, 0)]
    public static void CheckValues_CosRad(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 1, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_CosRad_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => new decimal(Math.PI / 2d)
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 1, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(arg == 0m ? 1d : 0d).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, 0)]
    [TestCase((sbyte)0, 0)]
    [TestCase((ushort)0, 0)]
    [TestCase((short)0, 0)]
    [TestCase((uint)0, 0)]
    [TestCase(0, 0)]
    [TestCase("0", 0)]
    [TestCase((ulong)0, 0)]
    [TestCase((long)0, 0)]
    [TestCase(0f, 0)]
    [TestCase(0d, 0)]
    [TestCase("0.7853981633974483", 1)]
    [TestCase(MathF.PI / 4f, 1)]
    [TestCase(Math.PI / 4d, 1)]
    [TestCase("1.5707963267948966", double.PositiveInfinity)]
    [TestCase(MathF.PI / 2f, float.PositiveInfinity)]
    [TestCase(Math.PI / 2d, double.PositiveInfinity)]
    [TestCase("4.71238898038469", double.NegativeInfinity)]
    [TestCase(3f * (MathF.PI / 2f), float.NegativeInfinity)]
    [TestCase(3d * (Math.PI / 2d), double.NegativeInfinity)]
    [TestCase("-1.5707963267948966", double.NegativeInfinity)]
    [TestCase(MathF.PI / -2f, float.NegativeInfinity)]
    [TestCase(Math.PI / -2d, double.NegativeInfinity)]
    [TestCase("-4.71238898038469", double.PositiveInfinity)]
    [TestCase(-3f * (MathF.PI / 2f), float.PositiveInfinity)]
    [TestCase(-3d * (Math.PI / 2d), double.PositiveInfinity)] 
    public static void CheckValues_TanRad(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 2, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01m));
    }

    [Test]
    public static void CheckValues_TanRad_Decimal([Range(0, 5)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            1 => new decimal(Math.PI / 4d),
            2 => new decimal(Math.PI / 2d),
            3 => new decimal(3 * Math.PI / 2d),
            4 => new decimal(Math.PI / -2d),
            _ => new decimal(-3 * Math.PI / 2d)
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 2, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(argType switch
        {
            0 => 0d,
            1 => 1d,
            3 or 4 => double.NegativeInfinity,
            _ => double.PositiveInfinity
        }).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, 0)]
    [TestCase((sbyte)0, 0)]
    [TestCase((ushort)0, 0)]
    [TestCase((short)0, 0)]
    [TestCase((uint)0, 0)]
    [TestCase(0, 0)]
    [TestCase("0", 0)]
    [TestCase((ulong)0, 0)]
    [TestCase((long)0, 0)]
    [TestCase(0f, 0)]
    [TestCase(0d, 0)]
    [TestCase((byte)1, 90)]
    [TestCase((sbyte)1, 90)]
    [TestCase((ushort)1, 90)]
    [TestCase((short)1, 90)]
    [TestCase((uint)1, 90)]
    [TestCase(1, 90)]
    [TestCase("1", 90)]
    [TestCase((ulong)1, 90)]
    [TestCase((long)1, 90)]
    [TestCase(1f, 90)]
    [TestCase(1d, 90)]
    public static void CheckValues_AsinDeg(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 3, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_AsinDeg_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => 1m
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 3, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(arg == 0m ? 0d : 90d).Within(0.01d));
    }

    [Test]
    [TestCase((byte)1, 0)]
    [TestCase((sbyte)1, 0)]
    [TestCase((ushort)1, 0)]
    [TestCase((short)1, 0)]
    [TestCase((uint)1, 0)]
    [TestCase(1, 0)]
    [TestCase("1", 0)]
    [TestCase((ulong)1, 0)]
    [TestCase((long)1, 0)]
    [TestCase(1f, 0)]
    [TestCase(1d, 0)]
    [TestCase((byte)0, 90)]
    [TestCase((sbyte)0, 90)]
    [TestCase((ushort)0, 90)]
    [TestCase((short)0, 90)]
    [TestCase((uint)0, 90)]
    [TestCase(0, 90)]
    [TestCase("0", 90)]
    [TestCase((ulong)0, 90)]
    [TestCase((long)0, 90)]
    [TestCase(0f, 90)]
    [TestCase(0d, 90)]
    public static void CheckValues_AcosDeg(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 4, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_AcosDeg_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => 1m
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 4, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(arg == 0m ? 90d : 0d).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, 0)]
    [TestCase((sbyte)0, 0)]
    [TestCase((ushort)0, 0)]
    [TestCase((short)0, 0)]
    [TestCase((uint)0, 0)]
    [TestCase(0, 0)]
    [TestCase("0", 0)]
    [TestCase((ulong)0, 0)]
    [TestCase((long)0, 0)]
    [TestCase(0f, 0)]
    [TestCase(0d, 0)]
    [TestCase((byte)1, 45)]
    [TestCase((sbyte)1, 45)]
    [TestCase((ushort)1, 45)]
    [TestCase((short)1, 45)]
    [TestCase((uint)1, 45)]
    [TestCase(1, 45)]
    [TestCase("1", 45)]
    [TestCase((ulong)1, 45)]
    [TestCase((long)1, 45)]
    [TestCase(1f, 45)]
    [TestCase(1d, 45)]
    [TestCase("Infinity", 90)]
    [TestCase(float.PositiveInfinity, 90)]
    [TestCase(double.PositiveInfinity, 90)]
    [TestCase("-Infinity", -90)]
    [TestCase(float.NegativeInfinity, -90)]
    [TestCase(double.NegativeInfinity, -90)]
    public static void CheckValues_AtanDeg(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 5, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_AtanDeg_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => 1m
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 5, true)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(argType switch
        {
            0 => 0d,
            _ => 45d
        }).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, 0)]
    [TestCase((sbyte)0, 0)]
    [TestCase((ushort)0, 0)]
    [TestCase((short)0, 0)]
    [TestCase((uint)0, 0)]
    [TestCase(0, 0)]
    [TestCase("0", 0)]
    [TestCase((ulong)0, 0)]
    [TestCase((long)0, 0)]
    [TestCase(0f, 0)]
    [TestCase(0d, 0)]
    [TestCase((byte)1, Math.PI / 2)]
    [TestCase((sbyte)1, Math.PI / 2)]
    [TestCase((ushort)1, Math.PI / 2)]
    [TestCase((short)1, Math.PI / 2)]
    [TestCase((uint)1, Math.PI / 2)]
    [TestCase(1, Math.PI / 2)]
    [TestCase("1", Math.PI / 2)]
    [TestCase((ulong)1, Math.PI / 2)]
    [TestCase((long)1, Math.PI / 2)]
    [TestCase(1f, MathF.PI / 2f)]
    [TestCase(1d, Math.PI / 2)]
    public static void CheckValues_AsinRad(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 3, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_AsinRad_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => 1m
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 3, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(argType == 0 ? 0d : Math.PI / 2d).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, Math.PI / 2d)]
    [TestCase((sbyte)0, Math.PI / 2d)]
    [TestCase((ushort)0, Math.PI / 2d)]
    [TestCase((short)0, Math.PI / 2d)]
    [TestCase((uint)0, Math.PI / 2d)]
    [TestCase(0, Math.PI / 2d)]
    [TestCase("0", Math.PI / 2d)]
    [TestCase((ulong)0, Math.PI / 2d)]
    [TestCase((long)0, Math.PI / 2d)]
    [TestCase(0f, MathF.PI / 2f)]
    [TestCase(0d, Math.PI / 2d)]
    [TestCase((byte)1, 0)]
    [TestCase((sbyte)1, 0)]
    [TestCase((ushort)1, 0)]
    [TestCase((short)1, 0)]
    [TestCase((uint)1, 0)]
    [TestCase(1, 0)]
    [TestCase("1", 0)]
    [TestCase((ulong)1, 0)]
    [TestCase((long)1, 0)]
    [TestCase(1f, 0f)]
    [TestCase(1d, 0d)]
    public static void CheckValues_AcosRad(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 4, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01d));
    }

    [Test]
    public static void CheckValues_AcosRad_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => 1m
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 4, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(arg == 0m ? Math.PI / 2d : 0d).Within(0.01d));
    }

    [Test]
    [TestCase((byte)0, 0)]
    [TestCase((sbyte)0, 0)]
    [TestCase((ushort)0, 0)]
    [TestCase((short)0, 0)]
    [TestCase((uint)0, 0)]
    [TestCase(0, 0)]
    [TestCase("0", 0)]
    [TestCase((ulong)0, 0)]
    [TestCase((long)0, 0)]
    [TestCase(0f, 0)]
    [TestCase(0d, 0)]
    [TestCase((byte)1, Math.PI / 4d)]
    [TestCase((sbyte)1, Math.PI / 4d)]
    [TestCase((ushort)1, Math.PI / 4d)]
    [TestCase((short)1, Math.PI / 4d)]
    [TestCase((uint)1, Math.PI / 4d)]
    [TestCase(1, Math.PI / 4d)]
    [TestCase("1", Math.PI / 4d)]
    [TestCase((ulong)1, Math.PI / 4d)]
    [TestCase((long)1, Math.PI / 4d)]
    [TestCase(1f, MathF.PI / 4f)]
    [TestCase(1d, Math.PI / 4d)]
    [TestCase("Infinity", Math.PI / 2d)]
    [TestCase(float.PositiveInfinity, MathF.PI / 2f)]
    [TestCase(double.PositiveInfinity, Math.PI / 2d)]
    [TestCase("-Infinity", Math.PI / -2d)]
    [TestCase(float.NegativeInfinity, MathF.PI / -2f)]
    [TestCase(double.NegativeInfinity, Math.PI / -2d)]
    public static void CheckValues_AtanRad(object arg, object result)
    {
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 5, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.01m));
    }

    [Test]
    public static void CheckValues_AtanRad_Decimal([Range(0, 1)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            _ => 1m
        };
        Type testType = typeof(TrigFuncExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, 5, false)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(argType switch
        {
            0 => 0d,
            _ => Math.PI / 4d
        }).Within(0.01d));
    }

    private class TrigFuncExecuteVisitor<TIn>(int trigFunc, bool deg) : BaseExecuteVisitor
        where TIn : IEquatable<TIn>
    {
        public override bool Execute()
        {
            TrigFuncExecuteVisitor<TIn> visitor = this;
            return trigFunc switch
            {
                0 => deg
                    ? MathMatrix.SinDeg(default(TIn)!, ref visitor)
                    : MathMatrix.SinRad(default(TIn)!, ref visitor),
                1 => deg
                    ? MathMatrix.CosDeg(default(TIn)!, ref visitor)
                    : MathMatrix.CosRad(default(TIn)!, ref visitor),
                2 => deg
                    ? MathMatrix.TanDeg(default(TIn)!, ref visitor)
                    : MathMatrix.TanRad(default(TIn)!, ref visitor),
                3 => deg
                    ? MathMatrix.AsinDeg(default(TIn)!, ref visitor)
                    : MathMatrix.AsinRad(default(TIn)!, ref visitor),
                4 => deg
                    ? MathMatrix.AcosDeg(default(TIn)!, ref visitor)
                    : MathMatrix.AcosRad(default(TIn)!, ref visitor),
                _ => deg
                    ? MathMatrix.AtanDeg(default(TIn)!, ref visitor)
                    : MathMatrix.AtanRad(default(TIn)!, ref visitor)
            };
        }

        public override bool Execute(params object[] values)
        {
            TrigFuncExecuteVisitor<TIn> visitor = this;
            return trigFunc switch
            {
                0 => deg
                    ? MathMatrix.SinDeg((TIn)values[0], ref visitor)
                    : MathMatrix.SinRad((TIn)values[0], ref visitor),
                1 => deg
                    ? MathMatrix.CosDeg((TIn)values[0], ref visitor)
                    : MathMatrix.CosRad((TIn)values[0], ref visitor),
                2 => deg
                    ? MathMatrix.TanDeg((TIn)values[0], ref visitor)
                    : MathMatrix.TanRad((TIn)values[0], ref visitor),
                3 => deg
                    ? MathMatrix.AsinDeg((TIn)values[0], ref visitor)
                    : MathMatrix.AsinRad((TIn)values[0], ref visitor),
                4 => deg
                    ? MathMatrix.AcosDeg((TIn)values[0], ref visitor)
                    : MathMatrix.AcosRad((TIn)values[0], ref visitor),
                _ => deg
                    ? MathMatrix.AtanDeg((TIn)values[0], ref visitor)
                    : MathMatrix.AtanRad((TIn)values[0], ref visitor)
            };
        }
    }
}