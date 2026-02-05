using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

namespace UnturnedAssetSpecTests.Parsers;

public class ParserTest<T>
    where T : IEquatable<T>
{
    private readonly string _file;
    private readonly ITypeParser<T> _parser;
    public ParserTest(string file, ITypeParser<T> parser)
    {
        _file = file;
        _parser = parser;
    }

    public void Execute()
    {

    }
}
