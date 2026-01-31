using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A CSteamID (64-bit Steam entity ID) parsed as a 64-bit number.
/// <para>Example: <c>ServerListCurationFile.Value</c></para>
/// <code>
/// Prop 76561198267927009
/// </code>
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="AccountType"/> AccountType</c> - Account type to verify against values.</item>
///     <item><c><see cref="AccountType"/>[] AccountTypes</c> - Account types to verify against values.</item>
/// </list>
/// </para>
/// </summary>
public sealed class Steam64IdType : BaseType<ulong, Steam64IdType>, ITypeParser<ulong>, ITypeFactory
{
    private static readonly Steam64IdType AnyInstance = new Steam64IdType(AccountTypes.Any);

    // copied from Steamworks

    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Used by Enum.Parse")]
    private enum AccountType
    {
        Invalid,
        Individual,
        Multiseat,
        GameServer,
        AnonGameServer,
        Pending,
        ContentServer,
        Clan,
        Chat,
        ConsoleUser,
        AnonUser,
        Max
    }

    [Flags]
    public enum AccountTypes
    {
        Any = Individual | Multiseat | GameServer | AnonGameServer | Pending | ContentServer | Clan | Chat | ConsoleUser | AnonUser,

        Individual = 1,
        Multiseat = 2,
        GameServer = 4,
        AnonGameServer = 8,
        Pending = 16,
        ContentServer = 32,
        Clan = 64,
        Chat = 128,
        ConsoleUser = 256,
        AnonUser = 512
    }

    private static readonly string?[] TypeDisplayNames =
    [
        null,
        Resources.Type_Name_Steam64ID_Individual,
        Resources.Type_Name_Steam64ID_Multiseat,
        Resources.Type_Name_Steam64ID_GameServer,
        Resources.Type_Name_Steam64ID_AnonGameServer,
        Resources.Type_Name_Steam64ID_Pending,
        Resources.Type_Name_Steam64ID_ContentServer,
        Resources.Type_Name_Steam64ID_Clan,
        Resources.Type_Name_Steam64ID_Chat,
        Resources.Type_Name_Steam64ID_ConsoleUser,
        Resources.Type_Name_Steam64ID_AnonUser
    ];


    public const string TypeId = "Steam64ID";

    public static ITypeFactory Factory => AnyInstance;

    public override string Id => TypeId;

    public override string DisplayName { get; }

    public override ITypeParser<ulong> Parser => this;

    public AccountTypes ValidAccountTypes { get; }

    public Steam64IdType(AccountTypes types)
    {
        types &= AccountTypes.Any;
        if (types == 0)
            types = AccountTypes.Any;

        ValidAccountTypes = types;
        if (types == AccountTypes.Any)
        {
            ValidAccountTypes = AccountTypes.Any;
            DisplayName = string.Format(Resources.Type_Name_Steam64ID, Resources.Type_Name_Steam64ID_Any);
            return;
        }

        int matches = 0;
        AccountType firstMatch = 0, secondMatch = 0;
        for (AccountType i = AccountType.Individual; i < AccountType.Max; ++i)
        {
            AccountTypes mask = (AccountTypes)(1 << ((int)i - 1));
            if ((types & mask) == 0) continue;
            ++matches;
            if (matches == 1)
                firstMatch = i;
            else if (matches == 2)
                secondMatch = i;
        }

        switch (matches)
        {
            case 1:
                // a
                DisplayName = string.Format(Resources.Type_Name_Steam64ID, TypeDisplayNames[(int)firstMatch]);
                break;

            case 2:
                // a or b
                DisplayName = string.Format(
                    Resources.Type_Name_Steam64ID,
                    TypeDisplayNames[(int)firstMatch] + Resources.Type_Name_Steam64ID_Join + TypeDisplayNames[(int)secondMatch]
                );
                break;

            default:
                // a, b, or c
                StringBuilder sb = new StringBuilder(matches * 16);
                int matches2 = 0;
                for (AccountType i = AccountType.Individual; i < AccountType.Max; ++i)
                {
                    AccountTypes mask = (AccountTypes)(1 << ((int)i - 1));
                    if ((types & mask) == 0) continue;
                    ++matches2;
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                        if (matches == matches2)
                        {
                            sb.Append(Resources.Type_Name_Steam64ID_Join).Append(' ');
                        }
                    }

                    sb.Append(TypeDisplayNames[(int)i]);
                }

                DisplayName = string.Format(Resources.Type_Name_Steam64ID, sb);
                break;
        }
    }

    private static int GetPopCount(AccountTypes types)
    {
#if NET7_0_OR_GREATER
        return int.PopCount((int)(types & AccountTypes.Any));
#else
        int popct = 0;
        for (AccountType i = AccountType.Individual; i < AccountType.Max; ++i)
        {
            AccountTypes mask = (AccountTypes)(1 << ((int)i - 1));
            if ((types & mask) == 0) continue;
            ++popct;
        }

        return popct;
#endif
    }

    public bool TryParse(ref TypeParserArgs<ulong> args, in FileEvaluationContext ctx, out Optional<ulong> value)
    {
        if (!TypeParsers.UInt64.TryParse(ref args, in ctx, out value) || !value.HasValue)
        {
            value = Optional<ulong>.Null;
            return false;
        }

        CheckValidSteamId(ref args, value.Value);
        return true;
    }

    private void CheckValidSteamId(ref TypeParserArgs<ulong> args, ulong steam64)
    {
        if (!KnownTypeValueHelper.CSteamID_IsValid(steam64))
        {
            args.DiagnosticSink?.UNT1029(ref args, args.ReferenceNode, null);
            return;
        }

        AccountType type = (AccountType)KnownTypeValueHelper.CSteamID_GetEAccountType(steam64);
        AccountTypes mask = (AccountTypes)(1 << ((int)type - 1));

        if ((ValidAccountTypes & mask) != 0)
            return;

        args.DiagnosticSink?.UNT1029(ref args, args.ReferenceNode, type.ToString());
    }


    #region JSON

    public bool TryReadValueFromJson(in JsonElement json, out Optional<ulong> value, IType<ulong> valueType)
    {
        return TypeParsers.UInt64.TryReadValueFromJson(in json, out value, valueType);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, ulong value, IType<ulong> valueType, JsonSerializerOptions options)
    {
        TypeParsers.UInt64.WriteValueToJson(writer, value, valueType, options);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        if (typeDefinition.ValueKind != JsonValueKind.Object)
        {
            return AnyInstance;
        }

        AccountTypes types = 0;
        if (typeDefinition.TryGetProperty("AccountType"u8, out JsonElement element)
            && element.ValueKind != JsonValueKind.Null)
        {
            AccountType type = (AccountType)Enum.Parse(typeof(AccountType), element.GetString()!, ignoreCase: true);
            if (type is >= AccountType.Individual and < AccountType.Max)
                types |= (AccountTypes)(1 << ((int)type - 1));
        }
        
        if (typeDefinition.TryGetProperty("AccountTypes"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            foreach (JsonElement value in element.EnumerateArray())
            {
                if (value.ValueKind == JsonValueKind.Null)
                    continue;

                AccountType type = (AccountType)Enum.Parse(typeof(AccountType), value.GetString()!, ignoreCase: true);
                if (type is >= AccountType.Individual and < AccountType.Max)
                    types |= (AccountTypes)(1 << ((int)type - 1));
            }
        }

        if (types == AccountTypes.Any)
            return AnyInstance;

        return new Steam64IdType(types);
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        int ct = GetPopCount(ValidAccountTypes);

        if (ct == 0 || ValidAccountTypes == AccountTypes.Any)
        {
            writer.WriteStringValue(TypeId);
            return;
        }

        writer.WriteStartObject();
        
        WriteTypeName(writer);

        if (ct == 1)
        {
            writer.WritePropertyName("AccountType"u8);
        }
        else
        {
            writer.WriteStartArray("AccountTypes"u8);
        }

        bool wroteOne = false;
        for (AccountType i = AccountType.Individual; i < AccountType.Max; ++i)
        {
            AccountTypes mask = (AccountTypes)(1 << ((int)i - 1));
            if ((ValidAccountTypes & mask) == 0)
                continue;

            wroteOne = true;
            writer.WriteStringValue(i.ToString());
            if (ct == 1)
                break;
        }

        if (ct != 1)
        {
            writer.WriteEndArray();
        }
        else if (!wroteOne)
        {
            writer.WriteNullValue();
        }

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(Steam64IdType other)
    {
        return other.ValidAccountTypes == ValidAccountTypes;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(857338749, ValidAccountTypes);
    }
}