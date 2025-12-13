using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable InconsistentNaming

namespace DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;

/// <summary>
/// Methods for reporting diagnostics using preset strings.
/// </summary>
public static class DiagnosticSinkExtensions
{
    private static string NodePropertyName(ISourceNode node)
    {
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
        /// Reports a value provided for a flag property. 
        /// </summary>
        public void UNT1003<TDiagnosticProvider>(
            ref TDiagnosticProvider provider,
            IAnyValueSourceNode node
        ) where TDiagnosticProvider : struct, IDiagnosticProvider
        {
            string message = node switch
            {
                IValueSourceNode v => string.Format(DiagnosticResources.UNT1003_Value, NodePropertyName(node), v.Value),
                IListSourceNode    => string.Format(DiagnosticResources.UNT1003_List, NodePropertyName(node)),
                _                  => string.Format(DiagnosticResources.UNT1003_Dictionary, NodePropertyName(node))
            };

            diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2003,
                Message = message,
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
                Message = string.Format(DiagnosticResources.UNT1007, NodePropertyName(node), requiredPropertyName),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
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
                Message = string.Format(DiagnosticResources.UNT2003, NodePropertyName(node), node.Value),
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
                Message = string.Format(DiagnosticResources.UNT2004_ValueInsteadOfList, value.Value, type.DisplayName, NodePropertyName(value)),
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
                Message = string.Format(DiagnosticResources.UNT2004_ValueInsteadOfDictionary, value.Value, type.DisplayName, NodePropertyName(value)),
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
                Message = string.Format(DiagnosticResources.UNT2004_ListInsteadOfValue, type.DisplayName, NodePropertyName(value)),
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
                Message = string.Format(DiagnosticResources.UNT2004_DictionaryInsteadOfValue, type.DisplayName, NodePropertyName(value)),
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
                Message = string.Format(DiagnosticResources.UNT2004_ListInsteadOfDictionary, type.DisplayName, NodePropertyName(value)),
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
                Message = string.Format(DiagnosticResources.UNT2004_DictionaryInsteadOfList, type.DisplayName, NodePropertyName(value)),
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
                Message = string.Format(DiagnosticResources.UNT2004_NoValue, NodePropertyName(parentNode)),
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
                Message = string.Format(DiagnosticResources.UNT2004_NoList, NodePropertyName(parentNode)),
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
                Message = string.Format(DiagnosticResources.UNT2004_NoDictionary, NodePropertyName(parentNode)),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
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
                Message = string.Format(DiagnosticResources.UNT1024_Less, NodePropertyName(parentNode), minimum),
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
                Message = string.Format(DiagnosticResources.UNT1024_More, NodePropertyName(parentNode), maximum),
                Range = provider.GetRangeAndRegisterDiagnostic()
            });
        }
    }
}

public interface IDiagnosticProvider
{
    FileRange GetRangeAndRegisterDiagnostic();
}