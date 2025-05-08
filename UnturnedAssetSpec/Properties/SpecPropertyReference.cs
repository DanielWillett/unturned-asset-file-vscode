using System;
using System.Collections.Generic;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public class SpecPropertyReference
{
    public string Variable { get; set; }
    public string? Owner { get; set; }
    public SpecPropertyContext Context { get; set; }


    
}