using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Net;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// Represents a IPv4 host filter which can filter by IP using CIDR notation and optionally a port range.
/// </summary>
/// <remarks>Format: <c>A.B.C.D/S:0-65535</c>.</remarks>
// ReSharper disable once InconsistentNaming
public readonly struct IPv4Filter : IComparable<IPv4Filter>, IEquatable<IPv4Filter>
#if NET6_0_OR_GREATER
    , ISpanFormattable
#else
    , IFormattable
#endif
#if NET7_0_OR_GREATER
    , ISpanParsable<IPv4Filter>
#endif
{
    private readonly uint _packedIp;
    private readonly uint _portRange;
    private readonly byte _cidrNumber;

    private const uint DefaultPortRange = ushort.MaxValue;

    /// <summary>
    /// The filter <c>0.0.0.0/0</c>, matching all hosts.
    /// </summary>
    public static readonly IPv4Filter All = new IPv4Filter(0, DefaultPortRange, 0);

    /// <summary>
    /// The minimum port that can be matched. Note that this may be larger than <see cref="MaximumPort"/> if entered incorrectly.
    /// </summary>
    public ushort MinimumPort => unchecked( (ushort)(_portRange >> 16) );

    /// <summary>
    /// The maximum port that can be matched. Note that this may be smaller than <see cref="MinimumPort"/> if entered incorrectly.
    /// </summary>
    public ushort MaximumPort => unchecked( (ushort)_portRange );

    /// <summary>
    /// The CIDR-notation subnet mask of this filter.
    /// This is a number between 0 and 32, specifying the number of lower bits that are used for the host ID.
    /// The subnet mask in 32-bit format can be determined by constructing a 32-bit binary string using
    /// <c>(32 - <see cref="SubnetMask"/>)</c> 1's then <c><see cref="SubnetMask"/></c> 0's.
    /// </summary>
    public int SubnetMask => _cidrNumber;

    /// <summary>
    /// Gets a new IPv4 filter that only contains the IPv4 address of this filter.
    /// </summary>
    public IPv4Filter Address => new IPv4Filter(_packedIp, DefaultPortRange, 32);

    /// <summary>
    /// Create a filter which matches a single IPv4 host on any port, given the 4 octets of the IP address.
    /// </summary>
    /// <param name="a">The first octet of the IP address.</param>
    /// <param name="b">The second octet of the IP address.</param>
    /// <param name="c">The third octet of the IP address.</param>
    /// <param name="d">The fourth octet of the IP address.</param>
    public IPv4Filter(byte a, byte b, byte c, byte d)
    {
        _packedIp = ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | d;
        _cidrNumber = 32;
        _portRange = DefaultPortRange;
    }

    /// <summary>
    /// Create a filter which matches a range of IPv4 hosts, given the 4 octets of the IP address, a port range, and a CIDR subnet mask.
    /// </summary>
    /// <param name="a">The first octet of the IP address.</param>
    /// <param name="b">The second octet of the IP address.</param>
    /// <param name="c">The third octet of the IP address.</param>
    /// <param name="d">The fourth octet of the IP address.</param>
    /// <param name="minPort">The minimum allowed port, inclusively.</param>
    /// <param name="maxPort">The maximum allowed port, inclusively.</param>
    /// <param name="cidrNumber">The CIDR-notation subnet mask of this filter. See <seealso cref="SubnetMask"/> for more info.</param>
    public IPv4Filter(byte a, byte b, byte c, byte d, ushort minPort = 0, ushort maxPort = ushort.MaxValue, byte cidrNumber = 32)
    {
        if (cidrNumber > 32)
            throw new ArgumentOutOfRangeException(nameof(cidrNumber));

        _packedIp = ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | d;
        _portRange = maxPort | ((uint)minPort << 16);
        _cidrNumber = cidrNumber;
    }

    /// <summary>
    /// Create a filter which matches a single IPv4 host on any port, given the packed 32-bit IP address.
    /// </summary>
    /// <param name="packedIp">The IP address packed into a 32-bit integer, where the highest 8 bits are the first octet of the IP address.</param>
    public IPv4Filter(uint packedIp)
    {
        _packedIp = packedIp;
        _cidrNumber = 32;
        _portRange = DefaultPortRange;
    }

    /// <summary>
    /// Create a filter which matches a range of IPv4 hosts, given the 4 octets of the IP address, a port range, and a CIDR subnet mask.
    /// </summary>
    /// <param name="packedIp">The IP address packed into a 32-bit integer, where the highest 8 bits are the first octet of the IP address.</param>
    /// <param name="minPort">The minimum allowed port, inclusively.</param>
    /// <param name="maxPort">The maximum allowed port, inclusively.</param>
    /// <param name="cidrNumber">The CIDR-notation subnet mask of this filter. See <seealso cref="SubnetMask"/> for more info.</param>
    public IPv4Filter(uint packedIp, ushort minPort = 0, ushort maxPort = ushort.MaxValue, byte cidrNumber = 32)
    {
        if (cidrNumber > 32)
            throw new ArgumentOutOfRangeException(nameof(cidrNumber));

        _packedIp = packedIp;
        _portRange = maxPort | ((uint)minPort << 16);
        _cidrNumber = cidrNumber;
    }

    private IPv4Filter(uint packedIp, uint ports, byte cidrNumber)
    {
        _packedIp = packedIp;
        _portRange = ports;
        _cidrNumber = cidrNumber;
    }

    /// <summary>
    /// Gets a filter with the same address range but aligned to the correct network ID for the given subnet mask.
    /// <para>
    /// For example, if the address is 192.168.4.17 and the subnet mask is 255.255.255.0 (/24), the aligned address would be 192.168.4.0.
    /// </para>
    /// </summary>
    public IPv4Filter Aligned
    {
        get
        {
            uint subnet = GetPackedSubnetMask();
            return new IPv4Filter(_packedIp & subnet, _portRange, _cidrNumber);
        }
    }

    /// <summary>
    /// Gets a filter where the port ranges are ordered correctly.
    /// <para>
    /// For example, if the filter is 1.2.3.4:2-1, the port-ordered address would be 1.2.3.4:1-2.
    /// </para>
    /// </summary>
    public IPv4Filter PortOrdered
    {
        get
        {
            uint minPort = _portRange >> 16;
            uint maxPort = _portRange & 0xFFFF;
            return minPort <= maxPort ? this : new IPv4Filter(_packedIp, minPort | (maxPort << 16), _cidrNumber);
        }
    }

    /// <summary>
    /// Whether or not this filter's IP and subnet mask match <paramref name="filter"/>'s address and it's minimum port falls within the given range.
    /// </summary>
    public bool ContainsAddress(in IPv4Filter filter)
    {
        return ContainsAddress(filter._packedIp, unchecked( (ushort)(filter._portRange >> 16) ));
    }

    /// <summary>
    /// Whether or not this filter's IP and subnet mask match the given packed IP <paramref name="address"/>.
    /// </summary>
    public bool ContainsAddress(uint address)
    {
        uint mask = GetPackedSubnetMask();
        return (_packedIp & mask) == (address & mask);
    }

    /// <summary>
    /// Whether or not this filter's IP and subnet mask match the given packed IP <paramref name="address"/>.
    /// </summary>
    public bool ContainsAddress(uint address, ushort port)
    {
        if (!ContainsAddress(address))
            return false;

        uint minPort = _portRange >> 16;
        uint maxPort = _portRange & 0xFFFF;
        if (minPort > maxPort)
        {
            return port >= maxPort && port <= minPort;
        }

        return port >= minPort && port <= maxPort;
    }

    /// <summary>
    /// Gets the IP address packed into a 32-bit integer, where the highest 8 bits are the first octet of the IP address.
    /// </summary>
    public uint GetPackedIPAddress()
    {
        return _packedIp;
    }

    /// <summary>
    /// Gets the subnet mask packed into a 32-bit integer, where the highest 8 bits are the first octet of the IP address.
    /// </summary>
    public uint GetPackedSubnetMask()
    {
        if (_cidrNumber == 32)
            return uint.MaxValue;

        return ~(0xFFFFFFFFu >> _cidrNumber);
    }

    /// <summary>
    /// Gets the IP address portion of this IPv4 filter as an <see cref="IPAddress"/>.
    /// </summary>
    public IPAddress GetIPAddress()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        Span<byte> addressBytes = stackalloc byte[4];
#else
        byte[] addressBytes = new byte[4];
#endif
        addressBytes[0] = unchecked((byte)(_packedIp >> 24));
        addressBytes[1] = unchecked((byte)(_packedIp >> 16));
        addressBytes[2] = unchecked((byte)(_packedIp >> 8));
        addressBytes[3] = unchecked((byte)_packedIp);
        return new IPAddress(addressBytes);
    }

    /// <inheritdoc />
    public int CompareTo(IPv4Filter other)
    {
        if (_packedIp != other._packedIp)
            return _packedIp.CompareTo(other._packedIp);
        if (_cidrNumber != other._cidrNumber)
            return (32 - _cidrNumber).CompareTo(32 - other._cidrNumber);
        return _portRange.CompareTo(other._portRange);
    }

    /// <inheritdoc />
    public bool Equals(IPv4Filter other)
    {
        return _packedIp == other._packedIp && _portRange == other._portRange && _cidrNumber == other._cidrNumber;
    }

    public static bool operator ==(IPv4Filter left, IPv4Filter right) => left.Equals(right);
    public static bool operator !=(IPv4Filter left, IPv4Filter right) => !left.Equals(right);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is IPv4Filter f && Equals(f);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_packedIp, _portRange, _cidrNumber);

    /// <summary>
    /// Formats this <see cref="IPv4Filter"/> into a string.
    /// <para>
    /// Strings will be formatted in one of the following formats depending on the data:
    /// <list type="none">
    ///     <item><c>###.###.###.###/##:#####-#####</c></item>
    ///     <item><c>###.###.###.###:#####-#####</c></item>
    ///     <item><c>###.###.###.###/##:#####</c></item>
    ///     <item><c>###.###.###.###:#####</c></item>
    ///     <item><c>###.###.###.###/##</c></item>
    ///     <item><c>###.###.###.###</c></item>
    /// </list>
    /// </para>
    /// </summary>
    public override string ToString()
    {
        Span<char> data = stackalloc char[30];
        TryFormat(data, out int index);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return new string(data.Slice(0, index));
#else
        return data.Slice(0, index).ToString();
#endif
    }

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

#if NET6_0_OR_GREATER
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (TryFormat(destination, out charsWritten))
            return true;

        charsWritten = 0;
        return false;
    }
#endif

    /// <summary>
    /// Formats this <see cref="IPv4Filter"/> into a character buffer.
    /// The maximum length string this method can produce is 30 characters.
    /// <para>
    /// Strings will be formatted in one of the following formats depending on the data:
    /// <list type="none">
    ///     <item><c>###.###.###.###/##:#####-#####</c></item>
    ///     <item><c>###.###.###.###:#####-#####</c></item>
    ///     <item><c>###.###.###.###/##:#####</c></item>
    ///     <item><c>###.###.###.###:#####</c></item>
    ///     <item><c>###.###.###.###/##</c></item>
    ///     <item><c>###.###.###.###</c></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <remarks>If this method returns <see langword="false"/>, <paramref name="size"/> will contain the exact number of characters needed to format this <see cref="IPv4Filter"/>.</remarks>
    /// <param name="data">The buffer to format data into.</param>
    /// <param name="size">The number of characters written to <paramref name="data"/>.
    /// If this method fails, this will be the size required to fit the data.</param>
    /// <returns>Whether or not <paramref name="data"/> had enough space to store this <see cref="IPv4Filter"/>.</returns>
    public bool TryFormat(Span<char> data, out int size)
    {
        byte a = unchecked((byte)(_packedIp >> 24));
        byte b = unchecked((byte)(_packedIp >> 16));
        byte c = unchecked((byte)(_packedIp >> 8));
        byte d = unchecked((byte)_packedIp);

        // max length is 30 characters, if less make sure theres enough room
        // 000.000.000.000/00:00000-00000
        if (data.Length < 30)
        {
            int requiredLength = 3
                                 + StringHelper.CountDigits(a)
                                 + StringHelper.CountDigits(b)
                                 + StringHelper.CountDigits(c)
                                 + StringHelper.CountDigits(d);

            if (_cidrNumber != 32)
            {
                requiredLength += 1 + StringHelper.CountDigits(_cidrNumber);
            }

            if (_portRange != DefaultPortRange)
            {
                ushort minPort = unchecked((ushort)(_portRange >> 16));
                ushort maxPort = unchecked((ushort)_portRange);
                requiredLength += 1 + StringHelper.CountDigits(minPort);
                if (minPort != maxPort)
                {
                    requiredLength += 1 + StringHelper.CountDigits(maxPort);
                }
            }

            if (data.Length < requiredLength)
            {
                size = requiredLength;
                return false;
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        a.TryFormat(data, out int cw, provider: CultureInfo.InvariantCulture);
        data[cw] = '.';
        int index = cw + 1;
        b.TryFormat(data[index..], out cw, provider: CultureInfo.InvariantCulture);
        index += cw;
        data[index] = '.';
        ++index;
        c.TryFormat(data[index..], out cw, provider: CultureInfo.InvariantCulture);
        index += cw;
        data[index] = '.';
        ++index;
        d.TryFormat(data[index..], out cw, provider: CultureInfo.InvariantCulture);
        index += cw;
        if (_cidrNumber != 32)
        {
            data[index] = '/';
            ++index;
            _cidrNumber.TryFormat(data[index..], out cw, provider: CultureInfo.InvariantCulture);
            index += cw;
        }
        if (_portRange != DefaultPortRange)
        {
            ushort minPort = unchecked((ushort)(_portRange >> 16));
            ushort maxPort = unchecked((ushort)_portRange);
            data[index] = ':';
            ++index;
            minPort.TryFormat(data[index..], out cw, provider: CultureInfo.InvariantCulture);
            index += cw;
            if (minPort != maxPort)
            {
                data[index] = '-';
                ++index;
                maxPort.TryFormat(data[index..], out cw, provider: CultureInfo.InvariantCulture);
                index += cw;
            }
        }
#else
        string aStr = a.ToString(CultureInfo.InvariantCulture);
        string bStr = b.ToString(CultureInfo.InvariantCulture);
        string cStr = c.ToString(CultureInfo.InvariantCulture);
        string dStr = d.ToString(CultureInfo.InvariantCulture);
        aStr.AsSpan().CopyTo(data);
        int index = aStr.Length;
        data[index] = '.';
        ++index;
        bStr.AsSpan().CopyTo(data[index..]);
        index += bStr.Length;
        data[index] = '.';
        ++index;
        cStr.AsSpan().CopyTo(data[index..]);
        index += cStr.Length;
        data[index] = '.';
        ++index;
        dStr.AsSpan().CopyTo(data[index..]);
        index += dStr.Length;
        if (_cidrNumber != 32)
        {
            data[index] = '/';
            ++index;
            string cidrStr = _cidrNumber.ToString(CultureInfo.InvariantCulture);
            cidrStr.AsSpan().CopyTo(data[index..]);
            index += cidrStr.Length;
        }
        if (_portRange != DefaultPortRange)
        {
            ushort minPort = unchecked((ushort)(_portRange >> 16));
            ushort maxPort = unchecked((ushort)_portRange);
            data[index] = ':';
            ++index;
            string minPortStr = minPort.ToString(CultureInfo.InvariantCulture);
            minPortStr.AsSpan().CopyTo(data[index..]);
            index += minPortStr.Length;
            if (minPort != maxPort)
            {
                data[index] = '-';
                ++index;
                string maxPortStr = maxPort.ToString(CultureInfo.InvariantCulture);
                maxPortStr.AsSpan().CopyTo(data[index..]);
                index += maxPortStr.Length;
            }
        }
#endif

        size = index;
        return true;
    }

    /// <summary>
    /// Parse a <see cref="IPv4Filter"/> from a string, throwing an exception in the case of a failure.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="FormatException"/>
    public static IPv4Filter Parse(string s)
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));

        return !TryParse(s.AsSpan(), out IPv4Filter filter)
            ? throw new FormatException(Properties.Resources.FormatException_IPv4Format_FailedToParse)
            : filter;
    }

    /// <summary>
    /// Parse a <see cref="IPv4Filter"/> from a string, throwing an exception in the case of a failure.
    /// </summary>
    /// <exception cref="FormatException"/>
    public static IPv4Filter Parse(ReadOnlySpan<char> s)
    {
        return !TryParse(s, out IPv4Filter filter)
            ? throw new FormatException(Properties.Resources.FormatException_IPv4Format_FailedToParse)
            : filter;
    }

    public static bool TryParse(ReadOnlySpan<char> s, out IPv4Filter result)
    {
        result = All;
        // min length: 0.0.0.0
        if (s.Length < 7)
            return false;

        int portSpecifierIndex = s.LastIndexOf(':');
        int cidrNumberSpecifierIndex = s.Slice(0, portSpecifierIndex < 0 ? s.Length : portSpecifierIndex).LastIndexOf('/');
        int portSeparatorIndex = portSpecifierIndex < 0 ? -1 : s.Slice(portSpecifierIndex + 1).IndexOf('-');
        
        if (portSeparatorIndex >= 0)
            portSeparatorIndex += portSpecifierIndex + 1;

        int dot1 = s.IndexOf('.');
        if (dot1 < 0)
            return false;
        int dot2 = s.Slice(dot1 + 1).IndexOf('.');
        if (dot2 < 0)
            return false;
        dot2 += dot1 + 1;
        int dot3 = s.Slice(dot2 + 1).IndexOf('.');
        if (dot3 < 0)
            return false;
        dot3 += dot2 + 1;
        if (dot3 == s.Length - 1)
            return false;

        if (cidrNumberSpecifierIndex >= 0 && dot3 > cidrNumberSpecifierIndex)
            return false;

        if (portSpecifierIndex >= 0 && dot3 > portSpecifierIndex)
            return false;

        int addrEndIndex = s.Length;
        if (cidrNumberSpecifierIndex >= 0)
            addrEndIndex = cidrNumberSpecifierIndex;
        else if (portSpecifierIndex >= 0)
            addrEndIndex = portSpecifierIndex;

        ReadOnlySpan<char> octA = s.Slice(0, dot1);
        ReadOnlySpan<char> octB = s.Slice(dot1 + 1, dot2 - dot1 - 1);
        ReadOnlySpan<char> octC = s.Slice(dot2 + 1, dot3 - dot2 - 1);
        ReadOnlySpan<char> octD = s.Slice(dot3 + 1, addrEndIndex - dot3 - 1);

        if (cidrNumberSpecifierIndex >= s.Length - 1)
            return false;

        int cidrEndIndex = s.Length;
        if (portSpecifierIndex >= 0)
            cidrEndIndex = portSpecifierIndex;

        ReadOnlySpan<char> cidrNumber = cidrNumberSpecifierIndex < 0
            ? ReadOnlySpan<char>.Empty
            : s.Slice(cidrNumberSpecifierIndex + 1, cidrEndIndex - cidrNumberSpecifierIndex - 1);

        if (portSpecifierIndex >= s.Length - 1)
            return false;

        int minPortEndIndex = s.Length;
        if (portSeparatorIndex >= 0)
            minPortEndIndex = portSeparatorIndex;

        ReadOnlySpan<char> minPort = portSpecifierIndex < 0
            ? ReadOnlySpan<char>.Empty
            : s.Slice(portSpecifierIndex + 1, minPortEndIndex - portSpecifierIndex - 1);

        if (portSeparatorIndex >= s.Length - 1)
            return false;

        ReadOnlySpan<char> maxPort = portSeparatorIndex < 0 ? ReadOnlySpan<char>.Empty : s.Slice(portSeparatorIndex + 1);

        byte cidrNumberValue = 32;
        ushort minPortValue = 0, maxPortValue = ushort.MaxValue;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (!byte.TryParse(octA, NumberStyles.Any, CultureInfo.InvariantCulture, out byte a)
            || !byte.TryParse(octB, NumberStyles.Any, CultureInfo.InvariantCulture, out byte b)
            || !byte.TryParse(octC, NumberStyles.Any, CultureInfo.InvariantCulture, out byte c)
            || !byte.TryParse(octD, NumberStyles.Any, CultureInfo.InvariantCulture, out byte d)
            )
#else
        if (!byte.TryParse(octA.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte a)
            || !byte.TryParse(octB.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte b)
            || !byte.TryParse(octC.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte c)
            || !byte.TryParse(octD.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte d)
           )
#endif
        {
            return false;
        }

        if (cidrNumberSpecifierIndex >= 0)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            if (!byte.TryParse(cidrNumber, NumberStyles.Any, CultureInfo.InvariantCulture, out cidrNumberValue)
#else
            if (!byte.TryParse(cidrNumber.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out cidrNumberValue)
#endif
                || cidrNumberValue > 32)
            {
                return false;
            }
        }

        if (portSpecifierIndex >= 0)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            if (!ushort.TryParse(minPort, NumberStyles.Any, CultureInfo.InvariantCulture, out minPortValue))
#else
            if (!ushort.TryParse(minPort.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out minPortValue))
#endif
            {
                return false;
            }
            if (portSeparatorIndex >= 0)
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                if (!ushort.TryParse(maxPort, NumberStyles.Any, CultureInfo.InvariantCulture, out maxPortValue))
#else
                if (!ushort.TryParse(maxPort.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out maxPortValue))
#endif
                {
                    return false;
                }
            }
            else
            {
                maxPortValue = minPortValue;
            }
        }

        result = new IPv4Filter(
            ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | d,
            maxPortValue | ((uint)minPortValue << 16),
            cidrNumberValue
        );
        return true;
    }

#if NET7_0_OR_GREATER
    static IPv4Filter IParsable<IPv4Filter>.Parse(string s, IFormatProvider? provider)
    {
        return Parse(s);
    }

    static bool IParsable<IPv4Filter>.TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out IPv4Filter result)
    {
        return TryParse(s.AsSpan(), out result);
    }

    static IPv4Filter ISpanParsable<IPv4Filter>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Parse(s);
    }

    static bool ISpanParsable<IPv4Filter>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out IPv4Filter result)
    {
        return TryParse(s, out result);
    }
#endif
}