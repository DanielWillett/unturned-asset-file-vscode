using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A time or time span.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 00:30:00
/// </code>
/// </summary>
public sealed class TimeSpanType : PrimitiveType<TimeSpan, TimeSpanType>
{
    public const string TypeId = "TimeSpan";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_TimeSpan;

    public override int GetHashCode() => 39252471;
}