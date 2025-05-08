using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;

public interface IAutoCompleteSpecPropertyType
{
    Task<AutoCompleteResult[]> GetAutoCompleteResults(AutoCompleteParameters parameters);
}