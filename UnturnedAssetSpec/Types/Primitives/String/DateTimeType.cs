using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A UTC date-time.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 2025-11-15T21:30:35
/// </code>
/// </summary>
public sealed class DateTimeType : PrimitiveType<DateTime, DateTimeType>
{
    public const string TypeId = "DateTime";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_DateTime;

    public override int GetHashCode() => 1066824625;
}