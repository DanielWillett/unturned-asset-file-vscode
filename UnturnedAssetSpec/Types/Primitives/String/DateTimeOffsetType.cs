using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A date-time parsed as a <see cref="DateTimeOffset"/> with an optional time-zone (assumed UTC).
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 2025-11-15T21:30:35
/// Prop 2025-11-16T02:30:35-05:00
/// </code>
/// </summary>
public sealed class DateTimeOffsetType : PrimitiveType<DateTimeOffset, DateTimeOffsetType>
{
    public const string TypeId = "DateTimeOffset";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_DateTimeOffset;

    public override int GetHashCode() => 1886809399;
}