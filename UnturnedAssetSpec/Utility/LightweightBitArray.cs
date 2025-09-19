using System.Collections;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Doesn't allocate anything for low array sizes (lte 64).
/// </summary>
internal struct LightweightBitArray
{
    public const int MinimumSize = 64;

    public BitArray? Bits;
    public long BitValue;

    public int Capacity => Bits?.Length ?? MinimumSize;

    public LightweightBitArray(int ct, bool defaultValue = false)
    {
        if (ct > MinimumSize)
        {
            Bits = new BitArray(ct);
        }

        if (defaultValue)
            SetAll(true);
    }

    public bool this[int index]
    {
        get
        {
            if (Bits != null)
                return Bits[index];

            return ((BitValue >>> index) & 1) == 1;
        }
        set
        {
            if (Bits != null)
            {
                Bits[index] = value;
                return;
            }

            long mask = (long)(1ul << index);
            if (value)
                BitValue |= mask;
            else
                BitValue &= ~mask;
        }
    }

    public void SetAll(bool value)
    {
        if (Bits == null)
        {
            BitValue = -1L;
        }
        else
        {
            Bits?.SetAll(value);
        }
    }
}

/// <summary>
/// Doesn't allocate anything for low stack sizes (lte 64).
/// </summary>
internal struct LightweightBitStack
{
    public const int MinimumSize = 64;

    public BitArray? Bits;
    public long BitValue;

    public int Count;
    
    public bool this[int index]
    {
        get
        {
            if (Bits != null)
                return Bits[index];

            return ((BitValue >>> index) & 1) == 1;
        }
        set
        {
            if (Bits != null)
            {
                Bits[index] = value;
                return;
            }

            long mask = (long)(1ul << index);
            if (value)
                BitValue |= mask;
            else
                BitValue &= ~mask;
        }
    }

    public void Push(bool value)
    {
        if (Count >= MinimumSize || Bits != null)
        {
            if (Bits == null)
            {
                Bits = new BitArray(Count + MinimumSize);
                for (int i = 0; i < MinimumSize; ++i)
                {
                    Bits[i] = ((BitValue >>> i) & 1) == 1;
                }
            }
            else if (Bits.Length <= Count)
            {
                BitArray newArray = new BitArray(Count + MinimumSize);
                for (int i = 0; i < Count; ++i)
                    newArray[i] = Bits[i];

                // this would be ideal but can't be done on arrays of different lengths
                // newArray.Or(Bits);
                Bits = newArray;
            }

            Bits[Count] = value;
        }
        else
        {
            long mask = (long)(1ul << Count);
            if (value)
                BitValue |= mask;
            else
                BitValue &= ~mask;
        }

        ++Count;
    }

    public bool Pop()
    {
        --Count;
        if (Bits == null)
        {
            return ((BitValue >>> Count) & 1) == 1;
        }

        return Bits[Count];
    }
}
