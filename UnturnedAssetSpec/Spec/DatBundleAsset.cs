using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// A Unity asset that can be defined in the bundle of a <see cref="DatAssetType"/> or within an instance of a custom type.
/// </summary>
public sealed class DatBundleAsset : DatProperty
{
    private int _maxTemplateCount = -1;

    /// <summary>
    /// Whether or not this property is a template property that uses <see cref="TemplateGroups"/>.
    /// </summary>
    /// <remarks>Corresponds to the <c>Template</c> property.</remarks>
    [MemberNotNullWhen(true, nameof(TemplateGroups))]
    public bool IsTemplate { get; internal set; }

    /// <summary>
    /// Whether or not every value in this template group should be unique.
    /// </summary>
    /// <remarks>Corresponds to the <c>TemplateGroupUniqueValue</c> property.</remarks>
    public bool TemplateGroupUniqueValue { get; internal set; }

    /// <summary>
    /// List of template grops, if any.
    /// </summary>
    /// <remarks>Corresponds to the <c>TemplateGroups</c> property.</remarks>
    public ImmutableArray<TemplateGroup> TemplateGroups { get; internal set; }

    /// <inheritdoc cref="DatProperty.Type"/>
    public new IBundleAssetType Type => (IBundleAssetType)base.Type;

    /// <inheritdoc />
    internal DatBundleAsset(string key, IBundleAssetType type, DatTypeWithProperties owner, JsonElement element)
        : base(key, owner, element, SpecPropertyContext.BundleAsset)
    {
        base.Type = type;
    }

    /// <summary>
    /// Creates a new bundle asset property.
    /// </summary>
    /// <param name="key">The name of the bundle asset in the bundle (ex. <c>Barricade</c>).</param>
    /// <param name="type">The type of Unity object to expect.</param>
    /// <param name="owner">The type that defines this bundle asset.</param>
    /// <param name="element">The JSON data behind this element.</param>
    /// <returns>The newly created bundle asset.</returns>
    /// <exception cref="ArgumentNullException">One of the required parameters is <see langword="null"/>.</exception>
    public static DatBundleAsset Create(string key, IBundleAssetType type, DatTypeWithProperties owner, JsonElement element)
    {
        return new DatBundleAsset(
            key ?? throw new ArgumentNullException(nameof(key)),
            type ?? throw new ArgumentNullException(nameof(type)),
            owner ?? throw new ArgumentNullException(nameof(owner)),
            element
        );
    }

    /// <inheritdoc cref="MatchesKey(string,ref FileEvaluationContext,bool,out DatProperty.KeyMatch)"/>
    public override bool MatchesKey(string candidateKey, ref FileEvaluationContext ctx, out KeyMatch match)
    {
        return MatchesKey(candidateKey, ref ctx, false, out match);
    }

    /// <summary>
    /// Checks whether a not an object name could refer to this bundle asset.
    /// </summary>
    /// <param name="candidateKey">The object name.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="isCaseInsensitive">Whether or not the comparison should ignore case.</param>
    /// <param name="match">Information about the key that was matched.</param>
    /// <returns>Whether or not a key was matched.</returns>
    public override bool MatchesKey(string candidateKey, ref FileEvaluationContext ctx, bool isCaseInsensitive, out KeyMatch match)
    {
        if (!IsTemplate || Keys.IsDefaultOrEmpty)
        {
            return base.MatchesKey(candidateKey, ref ctx, isCaseInsensitive, out match);
        }
        
        if (_maxTemplateCount < 0)
        {
            int max = 0;
            foreach (DatPropertyKey key in Keys)
            {
                if (key is DatTemplatePropertyKey templateKey)
                {
                    max = Math.Max(max, templateKey.TemplateProcessor.TemplateCount);
                }
            }

            _maxTemplateCount = max;
        }

        StringComparison comparison = isCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        LegacyExpansionFilter keyFilter = LegacyExpansionFilter.Either;
        bool hasKeyFilter = false;

        Span<int> indices = stackalloc int[_maxTemplateCount];

        for (int i = 0; i < Keys.Length; i++)
        {
            DatPropertyKey key = Keys[i];
            int templateMatch = 0;
            if (key is DatTemplatePropertyKey { TemplateProcessor.TemplateCount: > 0 } templateKey)
            {
                if (!templateKey.TemplateProcessor.TryParseKeyValues(candidateKey, indices, comparison))
                {
                    continue;
                }

                templateMatch = templateKey.TemplateProcessor.TemplateCount;
            }
            else if (!key.Key.Equals(candidateKey, comparison))
            {
                continue;
            }

            if (!hasKeyFilter)
            {
                keyFilter = ctx.GetKeyFilter();
                hasKeyFilter = true;
            }

            if (!SourceNodeExtensions.FilterMatches(key.Filter, keyFilter))
            {
                continue;
            }
            
            if (key.Condition != null && (!key.Condition.TryEvaluateValue(out Optional<bool> conditionValue, ref ctx) || !conditionValue.Value))
            {
                continue;
            }

            match = templateMatch > 0
                ? new KeyMatch(i, OneOrMoreExtensions.Create(indices.Slice(0, templateMatch)))
                : new KeyMatch(i);

            return true;
        }

        match = KeyMatch.None;
        return false;
    }
}