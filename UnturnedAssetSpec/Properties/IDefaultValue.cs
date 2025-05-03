using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public interface IDefaultValue
{
    IDefaultValue<TValue>? As<TValue>() where TValue : IEquatable<TValue?>;
}

public interface IDefaultValue<TValue> : IDefaultValue,
    IEquatable<IDefaultValue<TValue>>
    where TValue : IEquatable<TValue?>
{
    TValue? GetDefaultValue(AssetFileTree assetFile, AssetSpecDatabase database);
}