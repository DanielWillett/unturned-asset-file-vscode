using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests.Util;

public class LightweightBitTests
{
    [Test]
    public void LightweightBitArray_Empty()
    {
        LightweightBitArray array = new LightweightBitArray(0);

        Assert.That(array.Capacity, Is.EqualTo(LightweightBitArray.MinimumSize));
    }

    [Test]
    public void LightweightBitArray_One()
    {
        LightweightBitArray array = new LightweightBitArray(1);
        array[0] = true;

        Assert.That(array[0], Is.True);
        Assert.That(array.Capacity, Is.EqualTo(LightweightBitArray.MinimumSize));
    }

    [Test]
    public void LightweightBitArray_OtherAmounts([Values(60, 63, 64, 65, 122, 128, 65535)] int count)
    {
        LightweightBitArray array = new LightweightBitArray(count);

        for (int i = 0; i < count; ++i)
            array[i] = i % 2 == 0;

        for (int i = 0; i < count; ++i)
            Assert.That(array[i], Is.EqualTo(i % 2 == 0));

        if (count < LightweightBitArray.MinimumSize)
            Assert.That(array.Capacity, Is.EqualTo(LightweightBitArray.MinimumSize));
        else
            Assert.That(array.Capacity, Is.EqualTo(count));
    }

    [Test]
    public void LightweightBitStack_Empty()
    {
        LightweightBitStack stack = new LightweightBitStack();

        Assert.That(stack.Count, Is.Zero);
    }

    [Test]
    public void LightweightBitStack_One([Values(true, false)] bool value)
    {
        LightweightBitStack stack = new LightweightBitStack();

        Assert.That(stack.Count, Is.Zero);

        stack.Push(value);

        Assert.That(stack.Count, Is.EqualTo(1));

        Assert.That(stack.Pop(), Is.EqualTo(value));

        Assert.That(stack.Count, Is.Zero);
    }

    [Test]
    public void LightweightBitStack_OtherAmounts([Values(60, 63, 64, 65, 122, 128, 65535)] int count)
    {
        LightweightBitStack stack = new LightweightBitStack();

        Assert.That(stack.Count, Is.Zero);

        for (int i = 0; i < count; ++i)
            stack.Push(i % 2 == 0);

        Assert.That(stack.Count, Is.EqualTo(count));

        for (int i = count - 1; i >= 0; --i)
            Assert.That(stack.Pop(), Is.EqualTo(i % 2 == 0));

        Assert.That(stack.Count, Is.Zero);

        for (int i = 0; i < count; ++i)
            stack.Push(i % 2 == 0);

        Assert.That(stack.Count, Is.EqualTo(count));

        for (int i = count - 1; i >= 0; --i)
            Assert.That(stack.Pop(), Is.EqualTo(i % 2 == 0));

        Assert.That(stack.Count, Is.Zero);
    }
}
