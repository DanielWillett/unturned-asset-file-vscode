using System.Threading.Tasks;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;

public interface IAutoCompleteSpecPropertyType
{
    Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters, in FileEvaluationContext context);
}