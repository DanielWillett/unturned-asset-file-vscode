using System.Threading.Tasks;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;

/// <summary>
/// A property type which provides auto-complete results.
/// </summary>
public interface IAutoCompleteSpecPropertyType
{
    Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters, in FileEvaluationContext context);
}