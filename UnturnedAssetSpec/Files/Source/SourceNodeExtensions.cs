using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public static class SourceNodeExtensions
{
    //extension(SourceNodeType type)
    //{
    //    public bool IsAcceptablePropertyValue
    //    {
    //        get
    //        {
    //            return type
    //                is SourceNodeType.Value      or SourceNodeType.ValueWithComment
    //                or SourceNodeType.Dictionary or SourceNodeType.DictionaryWithComment
    //                or SourceNodeType.List       or SourceNodeType.ListWithComment;
    //        }
    //    }
    //
    //    public bool IsAcceptableListValue
    //    {
    //        get
    //        {
    //            return type
    //                is SourceNodeType.Value      or SourceNodeType.ValueWithComment
    //                or SourceNodeType.Dictionary or SourceNodeType.DictionaryWithComment
    //                or SourceNodeType.List       or SourceNodeType.ListWithComment
    //                or SourceNodeType.Comment
    //                or SourceNodeType.Whitespace;
    //        }
    //    }
    //
    //    public bool IsAcceptableDictionaryValue
    //    {
    //        get
    //        {
    //            return type
    //                is SourceNodeType.Property or SourceNodeType.PropertyWithComment
    //                or SourceNodeType.Comment
    //                or SourceNodeType.Whitespace;
    //        }
    //    }
    //
    //    public bool IsProperty
    //    {
    //        get
    //        {
    //            return type is SourceNodeType.Property or SourceNodeType.PropertyWithComment;
    //        }
    //    }
    //
    //    public bool IsValue
    //    {
    //        get
    //        {
    //            return type is SourceNodeType.Value or SourceNodeType.ValueWithComment;
    //        }
    //    }
    //
    //    public bool IsList
    //    {
    //        get
    //        {
    //            return type is SourceNodeType.List or SourceNodeType.ListWithComment;
    //        }
    //    }
    //
    //    public bool IsDictionary
    //    {
    //        get
    //        {
    //            return type is SourceNodeType.Dictionary or SourceNodeType.DictionaryWithComment;
    //        }
    //    }
    //
    //    public bool IsMetadata
    //    {
    //        get
    //        {
    //            return type is SourceNodeType.Comment or SourceNodeType.Whitespace;
    //        }
    //    }
    //}

    extension(IDictionarySourceNode node)
    {
        /// <summary>
        /// Whether or not this node is the root dictionary in the file.
        /// </summary>
        public bool IsRootNode => ReferenceEquals(node.Parent, node);

        /// <summary>
        /// Try to get a property's value by name.
        /// </summary>
        public bool TryGetPropertyValue(string propertyName, [MaybeNullWhen(false)] out IAnyValueSourceNode value)
        {
            if (!node.TryGetProperty(propertyName, out IPropertySourceNode? property) || !property.HasValue)
            {
                value = null;
                return false;
            }

            value = property.Value;
            return value != null;
        }

        /// <summary>
        /// Try to get a property's string value by name.
        /// </summary>
        public bool TryGetPropertyValue(string propertyName, [MaybeNullWhen(false)] out IValueSourceNode value)
        {
            return node.TryGetPropertyValue(propertyName, out IAnyValueSourceNode? anyValue) & ((value = anyValue as IValueSourceNode) != null);
        }

        /// <summary>
        /// Try to get a property's dictionary value by name.
        /// </summary>
        public bool TryGetPropertyValue(string propertyName, [MaybeNullWhen(false)] out IDictionarySourceNode value)
        {
            return node.TryGetPropertyValue(propertyName, out IAnyValueSourceNode? anyValue) & ((value = anyValue as IDictionarySourceNode) != null);
        }

        /// <summary>
        /// Try to get a property's list value by name.
        /// </summary>
        public bool TryGetPropertyValue(string propertyName, [MaybeNullWhen(false)] out IListSourceNode value)
        {
            return node.TryGetPropertyValue(propertyName, out IAnyValueSourceNode? anyValue) & ((value = anyValue as IListSourceNode) != null);
        }

        /// <summary>
        /// Try to get a property by it's key or alias.
        /// </summary>
        public bool TryGetProperty(SpecProperty property, [MaybeNullWhen(false)] out IPropertySourceNode propertyNode)
        {
            if (property.IsTemplate || property.Key.Length == 0)
            {
                propertyNode = null;
                return false;
            }

            if (node.TryGetProperty(property.Key, out propertyNode))
                return true;

            foreach (Alias alias in property.Aliases)
            {
                if (node.TryGetProperty(alias.Value, out propertyNode))
                    return true;
            }

            return false;
        }
    }

    extension(IAssetSourceFile root)
    {
        /// <summary>
        /// Either the data in the 'Asset' dictionary, or the root dictionary if not present.
        /// </summary>
        public IDictionarySourceNode AssetData => root.GetAssetDataDictionary() ?? root;

        /// <summary>
        /// The data in the 'Metadata' dictionary.
        /// </summary>
        public IDictionarySourceNode? GetMetadataDictionary()
        {
            if (!root.TryGetProperty("Metadata", out IPropertySourceNode? property))
            {
                return null;
            }

            return property.Value as IDictionarySourceNode;
        }

        /// <summary>
        /// The data in the 'Asset' dictionary.
        /// </summary>
        public IDictionarySourceNode? GetAssetDataDictionary()
        {
            if (!root.TryGetProperty("Asset", out IPropertySourceNode? property))
            {
                return null;
            }

            return property.Value as IDictionarySourceNode;
        }
    }

    extension(ISourceFile root)
    {
        /// <summary>
        /// Gets a property of this asset from the root level of the asset data. This properly handles 'Metadata' properties.
        /// </summary>
        /// <remarks>Also will look for localization properties in the corresponding localization files, and vice versa with asset properties.</remarks>
        public bool TryResolveProperty(SpecProperty property, [MaybeNullWhen(false)] out IPropertySourceNode propertyNode)
        {
            if (root is ILocalizationSourceFile local && property.Context != SpecPropertyContext.Localization)
                root = local.Asset;

            if (root is not IAssetSourceFile asset)
                return root.TryGetProperty(property, out propertyNode);

            if (property.Context == SpecPropertyContext.Localization)
            {
                foreach (ILocalizationSourceFile localFile in asset.Localization)
                {
                    if (localFile.TryResolveProperty(property, out propertyNode))
                        return true;
                }

                propertyNode = null;
                return false;
            }

            if (!property.CanBeInMetadata)
                return asset.AssetData.TryGetProperty(property, out propertyNode);

            IDictionarySourceNode? assetData = asset.GetAssetDataDictionary();
            IDictionarySourceNode? metadata = asset.GetMetadataDictionary();
            bool hasValue;
            if (metadata != null || assetData == null)
            {
                IDictionarySourceNode dict = metadata ?? root;

                if (!dict.TryGetProperty(property, out propertyNode) && dict != root)
                {
                    hasValue = root.TryGetProperty(property, out propertyNode);
                }
                else hasValue = true;
            }
            else if (!property.Key.Equals("GUID", StringComparison.OrdinalIgnoreCase))
            {
                hasValue = assetData.TryGetProperty(property, out propertyNode);
            }
            else
            {
                propertyNode = null;
                hasValue = false;
            }

            return hasValue;
        }
    }

    extension(IAnyChildrenSourceNode root)
    {
        /// <summary>
        /// Gets the best node overlapping the given index.
        /// </summary>
        public ISourceNode? GetNodeFromIndex(int characterIndex, bool ignoreMetadata = true)
        {
            GetNodeFromIndexVisitor visitor = new GetNodeFromIndexVisitor(characterIndex, ignoreMetadata);
            CancellationTokenSource src = new CancellationTokenSource();
            visitor.Token = src.Token;
            try
            {
                root.Visit(ref visitor);
            }
            catch (BreakException)
            {
                return visitor.BestMatch;
            }
            finally
            {
                src.Dispose();
            }

            return null;
        }

        /// <summary>
        /// Gets the best node overlapping the given position.
        /// </summary>
        public ISourceNode? GetNodeFromPosition(FilePosition position, bool ignoreMetadata = true)
        {
            GetNodeFromPositionVisitor visitor = new GetNodeFromPositionVisitor(position, ignoreMetadata);
            CancellationTokenSource src = new CancellationTokenSource();
            visitor.Token = src.Token;
            try
            {
                root.Visit(ref visitor);
            }
            catch (BreakException) { }
            finally
            {
                src.Dispose();
            }

            return visitor.BestMatch;
        }
    }

    private class GetNodeFromIndexVisitor(int index, bool ignoreMetadata) : OrderedNodeVisitor
    {
        internal ISourceNode? BestMatch;

        protected override bool IgnoreMetadata => ignoreMetadata;

        protected override void AcceptNode(ISourceNode node)
        {
            if (node.FirstCharacterIndex >= index && node.LastCharacterIndex <= index + 1)
            {
                if (BestMatch == null || BestMatch.FirstCharacterIndex <= node.FirstCharacterIndex)
                    BestMatch = node;
                return;
            }

            if (BestMatch != null)
            {
                throw new BreakException();
            }
        }
    }

    private class GetNodeFromPositionVisitor(FilePosition position, bool ignoreMetadata) : OrderedNodeVisitor
    {
        internal ISourceNode? BestMatch;

        protected override bool IgnoreMetadata => ignoreMetadata;

        protected override void AcceptNode(ISourceNode node)
        {
            if (node.Range.Contains(position) || (node.Range.End.Line == position.Line && position.Character == node.Range.End.Character + 1))
            {
                if (BestMatch == null || BestMatch.FirstCharacterIndex <= node.FirstCharacterIndex)
                    BestMatch = node;
                return;
            }

            if (BestMatch != null && BestMatch.Range.End.Line < node.Range.Start.Line)
            {
                throw new BreakException();
            }
        }
    }

    private sealed class BreakException : Exception;
}
