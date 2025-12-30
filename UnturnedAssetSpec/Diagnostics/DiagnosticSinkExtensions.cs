using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.RegularExpressions;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

// ReSharper disable InconsistentNaming

namespace DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;

/// <summary>
/// Methods for reporting diagnostics using preset strings.
/// </summary>
public static class DiagnosticSinkExtensions
{
    // match any <[ ]br[ ][/][ ]>
    private static readonly Regex AnyLineBreakTagsMatcher =
        new Regex(@"\<\s*br\s*\/{0,1}\s*\>", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    // match any <[ ]br[ ]/[ ]>
    private static readonly Regex InvalidLineBreakTagsMatcher =
        new Regex(@"\<\s*br\s*\/\s*\>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static string NodePropertyName<TDiagnosticProvider>(ISourceNode node, ref TDiagnosticProvider provider) where TDiagnosticProvider : struct, IDiagnosticProvider
    {
        if (node is IDictionarySourceNode { IsRootNode: true })
        {
            return provider.Property?.Key ?? "?";
        }

        return node switch
        {
            IPropertySourceNode property
                => PropertyBreadcrumbs.FromNode(property).ToString(false, property.Key),

            IAnyValueSourceNode { Parent: IListSourceNode }
                => PropertyBreadcrumbs.FromNode(node).ToString(false),

            IAnyValueSourceNode { Parent: IPropertySourceNode prop }
                => PropertyBreadcrumbs.FromNode(prop).ToString(false, prop.Key),

            _
                => node.ToString()!
        };
    }
    
    extension(IDiagnosticSink diagnosticSink)
    {

        /// <summary>
        /// Reports a format string that isn't using one or more of the arguments. 
        /// </summary>
        public void UNT102<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            string formattingArg
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT102,
                Message = string.Format(DiagnosticResources.UNT102, formattingArg),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports the usage of a ContentReference when a MasterBundleReference could've been used instead. 
        /// </summary>
        public void UNT104<TDiagnosticProvider>(
            ref TDiagnosticProvider provider
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT104,
                Message = DiagnosticResources.UNT104,
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports the usage of an object for a bundle reference. 
        /// </summary>
        public void UNT108<TDiagnosticProvider>(
            ref TDiagnosticProvider provider
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT108,
                Message = DiagnosticResources.UNT108,
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a value provided for a flag property. 
        /// </summary>
        public void UNT1003<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IAnyValueSourceNode node
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            string message = node switch
            {
                IValueSourceNode v => string.Format(DiagnosticResources.UNT1003_Value, NodePropertyName(node, ref provider), v.Value),
                IListSourceNode    => string.Format(DiagnosticResources.UNT1003_List, NodePropertyName(node, ref provider)),
                _                  => string.Format(DiagnosticResources.UNT1003_Dictionary, NodePropertyName(node, ref provider))
            };

            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2003,
                Message = message,
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a warning when rich text is used in a string that doesn't support it. 
        /// </summary>
        public void CheckUNT1006<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IValueSourceNode node
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            if (!KnownTypeValueHelper.ContainsRichText(node.Value))
                return;

            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1006,
                Message = DiagnosticResources.UNT1006,
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a related property missing for a property. 
        /// </summary>
        public void UNT1007<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            ISourceNode node, string requiredPropertyName
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1007,
                Message = string.Format(DiagnosticResources.UNT1007, NodePropertyName(node, ref provider), requiredPropertyName),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a related property missing in a dictionary. 
        /// </summary>
        public void UNT1007_Modern<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IDictionarySourceNode dictionary, string requiredPropertyName
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1007,
                Message = string.Format(DiagnosticResources.UNT1007_Modern, NodePropertyName(dictionary, ref provider), requiredPropertyName),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports an unsupported TranslationReference. 
        /// </summary>
        public void UNT1018<TDiagnosticProvider>(
            ref TDiagnosticProvider provider
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1018,
                Message = DiagnosticResources.UNT1018_TranslationReference,
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Checks a node for unexpected new-line tags and reports them as warnings.
        /// </summary>
        public void CheckUNT1021<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IValueSourceNode node
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            foreach (Match match in AnyLineBreakTagsMatcher.Matches(node.Value))
            {
                FileRange range = node.Range;
                range.Start.Character += match.Index;
                if (node.IsQuoted)
                    ++range.Start.Character;
                range.End.Character = range.Start.Character + (match.Length - 1);
                diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1021,
                    Message = DiagnosticResources.UNT1021,
                    Range = range
                });
            }
        }

        /// <summary>
        /// Checks a node for invalid new-line tags and reports them as warnings, as well as adding suggestions to replace \n and \r\n with &lt;br&gt;.
        /// </summary>
        internal void CheckUNT1022_UNT106<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IValueSourceNode node
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            string str = node.Value;

            int crlfInd = -1;
            while (crlfInd + 1 < str.Length)
            {
                crlfInd = str.IndexOf('\n', crlfInd + 1);
                if (crlfInd < 0)
                    break;

                int startIndex = crlfInd > 0 && str[crlfInd - 1] == '\r' ? crlfInd - 1 : crlfInd;
                int len = crlfInd - startIndex + 1;

                FileRange range = node.Range;
                range.Start.Character += startIndex;
                if (node.IsQuoted)
                    ++range.Start.Character;
                range.End.Character = range.Start.Character + (len - 1);
                diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
                {
                    Range = range,
                    Diagnostic = DatDiagnostics.UNT106,
                    Message = DiagnosticResources.UNT106
                });
            }

            foreach (Match match in InvalidLineBreakTagsMatcher.Matches(str))
            {
                FileRange range = node.Range;
                range.Start.Character += match.Index;
                if (node.IsQuoted)
                    ++range.Start.Character;
                range.End.Character = range.Start.Character + (match.Length - 1);
                diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
                {
                    Range = range,
                    Diagnostic = DatDiagnostics.UNT1022,
                    Message = DiagnosticResources.UNT1022
                });
            }
        }

        /// <summary>
        /// Reports a list or dictionary that has too few entries. 
        /// </summary>
        public void UNT1024_Less<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IParentSourceNode parentNode, int minimum
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1024,
                Message = string.Format(DiagnosticResources.UNT1024_Less, NodePropertyName(parentNode, ref provider), minimum),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a list or dictionary that has too many entries. 
        /// </summary>
        public void UNT1024_More<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IParentSourceNode parentNode, int maximum
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1024,
                Message = string.Format(DiagnosticResources.UNT1024_More, NodePropertyName(parentNode, ref provider), maximum),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a string that has too few characters. 
        /// </summary>
        public void UNT1024_LessString<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IParentSourceNode parentNode, int minimum
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1024,
                Message = string.Format(DiagnosticResources.UNT1024_LessString, NodePropertyName(parentNode, ref provider), minimum),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a string that has too many characters. 
        /// </summary>
        public void UNT1024_MoreString<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IParentSourceNode parentNode, int maximum
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1024,
                Message = string.Format(DiagnosticResources.UNT1024_MoreString, NodePropertyName(parentNode, ref provider), maximum),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Checks the length of a list or dictionary and emits any necessary diagnostics.
        /// </summary>
        public void CheckUNT1024<TDiagnosticProvider>(
            int length,
            ref TDiagnosticProvider provider,
            IParentSourceNode parentNode,
            int minCount, int maxCount
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            if (length < minCount)
                diagnosticSink.UNT1024_Less(ref provider, parentNode, minCount);
            else if (length < maxCount)
                diagnosticSink.UNT1024_More(ref provider, parentNode, maxCount);
        }

        /// <summary>
        /// Checks the length of a string and emits any necessary diagnostics.
        /// </summary>
        public void CheckUNT1024_String<TDiagnosticProvider>(
            int length,
            ref TDiagnosticProvider provider,
            IParentSourceNode parentNode,
            int minCount, int maxCount
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            if (length < minCount)
                diagnosticSink.UNT1024_LessString(ref provider, parentNode, minCount);
            else if (length < maxCount)
                diagnosticSink.UNT1024_MoreString(ref provider, parentNode, maxCount);
        }

        /// <summary>
        /// Reports a <see langword="false"/> value provided for a flag property. 
        /// </summary>
        public void UNT2003<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IValueSourceNode node
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2003,
                Message = string.Format(DiagnosticResources.UNT2003, NodePropertyName(node, ref provider), node.Value),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a generic failed to parse message for a type. 
        /// </summary>
        public void UNT2004_Generic<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            string original, IType type
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004, original, type.DisplayName),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a failed to parse RegEx message for a type. 
        /// </summary>
        public void UNT2004_Regex<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            Exception ex, string original
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            string fmt = string.Format(DiagnosticResources.UNT2004_Regex, original);
            string message;
            if (!string.IsNullOrEmpty(ex.Message))
            {
                message = ex.Message[^1] == '.'
                    ? $"{fmt} {ex.Message}"
                    : $"{fmt} {ex.Message}.";
            }
            else
            {
                message = fmt;
            }

            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = message,
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a failed to parse where a boolean value was 'T', 'Y', 'F', or 'N'.
        /// The boolean parser only accepts lowercase values for one-letter repsonses. 
        /// </summary>
        public void UNT2004_BooleanSingleCharCapitalized<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            string original, IType type
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            string recommendation = original[0] switch
            {
                'T' => "t",
                'Y' => "y",
                'F' => "f",
                'N' => "n",
                var c => new string(char.ToLowerInvariant(c), 1)
            };

            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_BooleanSingleCharCapitalized, original, type.DisplayName, recommendation),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a string value when a list was expected. 
        /// </summary>
        public void UNT2004_ValueInsteadOfList<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IValueSourceNode value, IType type
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_ValueInsteadOfList, value.Value, type.DisplayName, NodePropertyName(value, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a string value when a dictionary was expected. 
        /// </summary>
        public void UNT2004_ValueInsteadOfDictionary<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IValueSourceNode value, IType type
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_ValueInsteadOfDictionary, value.Value, type.DisplayName, NodePropertyName(value, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a list when a string value was expected. 
        /// </summary>
        public void UNT2004_ListInsteadOfValue<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IListSourceNode value, IType type
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_ListInsteadOfValue, type.DisplayName, NodePropertyName(value, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a list when a string value was expected. 
        /// </summary>
        public void UNT2004_DictionaryInsteadOfValue<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IDictionarySourceNode value, IType type
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_DictionaryInsteadOfValue, type.DisplayName, NodePropertyName(value, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a list when a dictionary was expected. 
        /// </summary>
        public void UNT2004_ListInsteadOfDictionary<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IListSourceNode value, IType type
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_ListInsteadOfDictionary, type.DisplayName, NodePropertyName(value, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a dictionary when a list was expected. 
        /// </summary>
        public void UNT2004_DictionaryInsteadOfList<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IDictionarySourceNode value, IType type
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_DictionaryInsteadOfList, type.DisplayName, NodePropertyName(value, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a missing string value. 
        /// </summary>
        public void UNT2004_NoValue<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IParentSourceNode parentNode
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_NoValue, NodePropertyName(parentNode, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a missing list value. 
        /// </summary>
        public void UNT2004_NoList<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IParentSourceNode parentNode
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_NoList, NodePropertyName(parentNode, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a missing dictionary value. 
        /// </summary>
        public void UNT2004_NoDictionary<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IParentSourceNode parentNode
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_NoDictionary, NodePropertyName(parentNode, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }

        /// <summary>
        /// Reports a dictionary value given for a BundleReference type that only supports string values.
        /// </summary>
        public void UNT2004_BundleReferenceStringOnly<TDiagnosticProvider>(
            ref TDiagnosticProvider provider, IType type, IParentSourceNode parent
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_BundleReferenceStringOnly, type.DisplayName, NodePropertyName(parent, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }
        
        /// <summary>
        /// Reports a dictionary value given for an AssetReference type that only supports string values.
        /// </summary>
        public void UNT2004_AssetReferenceStringOnly<TDiagnosticProvider>(
            ref TDiagnosticProvider provider, IType type, IParentSourceNode parent
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(DiagnosticResources.UNT2004_AssetReferenceStringOnly, type.DisplayName, NodePropertyName(parent, ref provider)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }


        /// <summary>
        /// Reports a malformed format string. 
        /// </summary>
        public void UNT2012<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            string value,
            FormatException ex
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            string? message;
            if (!string.IsNullOrEmpty(ex.Message))
            {
                message = ex.Message[^1] == '.'
                    ? ex.Message
                    : $"{ex.Message}.";
            }
            else
            {
                message = null;
            }

            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2012,
                Message = message == null ? DiagnosticResources.UNT2012 : string.Format(DiagnosticResources.UNT2012_WithMessage, message),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }
    }
}

public interface IDiagnosticProvider
{
    DatProperty? Property { get; }
    FileRange GetRangeAndRegisterDiagnostic();
}