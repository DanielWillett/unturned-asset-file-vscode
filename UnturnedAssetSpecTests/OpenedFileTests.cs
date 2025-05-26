using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using System.Text;
// ReSharper disable HeapView.CanAvoidClosure

namespace UnturnedAssetSpecTests;

public class OpenedFileTests
{
#nullable disable
    
    private ILogger _logger;
    private ILoggerFactory _loggerFactory;

    [SetUp]
    public void SetUp()
    {
        _loggerFactory = LoggerFactory.Create(l => l.AddSimpleConsole());
        _logger = _loggerFactory.CreateLogger<OpenedFileTests>();
    }

    [TearDown]
    public void TearDown()
    {
        _loggerFactory.Dispose();
    }

#nullable restore

    [Test]
    public void ValidIndex([Values(true, false)] bool trailingNewLine, [Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(trailingNewLine, newLine), _logger);

        runner.AssertFileHasValidIndex();
    }

    [Test]
    public void GetPosition([Values(true, false)] bool trailingNewLine, [Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(trailingNewLine, newLine), _logger);

        // start of document
        FilePosition pos = runner.GetPosition(0, clampCharacter: false);

        Assert.That(pos, Is.EqualTo(new FilePosition(1, 1)));

        // first line
        pos = runner.GetPosition(1, clampCharacter: false);

        Assert.That(pos, Is.EqualTo(new FilePosition(1, 2)));

        for (int i = 0; i < newLine.Length; ++i)
        {
            // first line newline
            pos = runner.GetPosition(37 + i, clampCharacter: true);

            Assert.That(pos, Is.EqualTo(new FilePosition(1, 38)));
        }

        // next line start
        pos = runner.GetPosition(37 + newLine.Length);

        Assert.That(pos, Is.EqualTo(new FilePosition(2, 1)));
    }


    [Test]
    public void UpdateText([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            file.InsertText((2, 1), $"Test Line{newLine}")
                .InsertText((2, 1), "Test ")
                .RemoveText(((2, 1), (2, 14)))
                .ReplaceText(((1, 6), (1, 37)), "00000000000000000000000000000000")
                .ReplaceText(((1, 6), (1, 37)), "1111111111111111")
                .ReplaceText(((1, 6), (1, 21)), "2222222222222222222222222222222222222222222222222222222222222222")
                ;
        });

        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void StitchOneLine([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // remove "Arrest" from second line
            file.RemoveText(((2, 6), (2, 12)));
        });

        Assert.That(runner.GetFullText(), Does.Contain($"Type _End{newLine}"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void StitchOneLineFromBegin([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // remove "Type " from second line
            file.RemoveText(((2, 1), (2, 6)));
        });

        Assert.That(runner.GetFullText(), Does.Contain($"{newLine}Arrest_End{newLine}"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void StitchOneLineFromEnd([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // remove " Arrest_End" from second line
            file.RemoveText(((2, 5), (2, 16)));
        });

        Assert.That(runner.GetFullText(), Does.Contain($"{newLine}Type{newLine}"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void StitchTwoLines([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // set type to "Uncommon" by removing the following text: "Arrest_End\nRarity "
            file.RemoveText(((2, 6), (3, 8)));
        });

        Assert.That(runner.GetFullText(), Does.Contain($"Type Uncommon{newLine}"));
        Assert.That(runner.GetFullText(), Does.Contain($"Useable Arrest_End{newLine}"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void StitchTwoLinesEmptyStart([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // remove type by removing "\nType Arrest_End"
            file.RemoveText(((1, 38), (2, 16)));
        });

        Assert.That(runner.GetFullText(), Does.Not.Contain($"Type Arrest_End{newLine}"));
        Assert.That(runner.GetFullText(), Does.Contain($"GUID cb4352c6fcb044c2b58e6edfa5644904{newLine}Rarity Uncommon{newLine}"));

        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void StitchTwoLinesEmptyEnd([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // remove type by removing "Type Arrest_End\n"
            file.RemoveText(((2, 1), (2, 17)));
        });

        Assert.That(runner.GetFullText(), Does.Not.Contain("Type Arrest_End"));
        Assert.That(runner.GetFullText(), Does.Contain($"GUID cb4352c6fcb044c2b58e6edfa5644904{newLine}Rarity Uncommon{newLine}"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void RemoveOneEmptyLineByRemovingSameLineNewline([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // remove line 6 by removing "\n" from line 6 
            file.RemoveText(((6, 1), (7, 1)));
        });

        Assert.That(runner.GetFullText(), Does.Contain($"ID 1196{newLine}Size_X 1{newLine}"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void RemoveOneEmptyLineByRemovingPreviousLineNewline([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // remove line 6 by removing "\n" from line 6 
            file.RemoveText(((5, 8), (6, 1)));
        });

        Assert.That(runner.GetFullText(), Does.Contain($"ID 1196{newLine}Size_X 1{newLine}"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void StitchManyLines([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // Set GUID to 1196 by removing "cb4352c6fcb044c2b58e6edfa5644904\nType Arrest_End\nUseable Arrest_End\nID "
            file.RemoveText(((1, 6), (5, 4)));
        });

        Assert.That(runner.GetFullText(), Does.Contain($"GUID 1196{newLine}{newLine}Size_X 1"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void StitchManyLinesEmptyStart([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // Create Arrest_End flag by removing "Type Arrest_End\nRarity Uncommon"\nUseable "
            file.RemoveText(((2, 1), (4, 9)));
        });

        Assert.That(runner.GetFullText(), Does.Contain($"GUID cb4352c6fcb044c2b58e6edfa5644904{newLine}Arrest_End{newLine}ID 1196{newLine}"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void StitchManyLinesEmptyEnd([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // Set Type to 'ID 1196' by removing "Arrest_End\nRarity Uncommon"\nUseable Arrest_End\n"
            file.RemoveText(((2, 6), (4, 20)));
        });

        Assert.That(runner.GetFullText(), Does.Contain($"GUID cb4352c6fcb044c2b58e6edfa5644904{newLine}Type ID 1196{newLine}{newLine}"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void RemoveTwoEmptyLines([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // Remove bottom two empty lines
            file.RemoveText(((10, 1), (12, 1)));
        });

        Assert.That(runner.GetFullText(), Does.Not.Contain($"Size_Z 0.3{newLine}{newLine}{newLine}Blueprints 0"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void RemoveManyEmptyLines([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // Remove bottom three empty lines
            file.RemoveText(((13, 1), (16, 1)));
        });

        Assert.That(runner.GetFullText(), Does.Not.Contain($"Blueprints 0{newLine}{newLine}{newLine}{newLine}Actions 0"));
        
        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void InsertTextToOneLine([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // insert "_Item" after Arrest in the Type property
            file.InsertText((2, 12), "_Item");
        });

        Assert.That(runner.GetFullText(), Does.Contain($"Type Arrest_Item_End{newLine}"));

        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void InsertTextToOneLineAtBegin([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // insert "New_" before the Type property
            file.InsertText((2, 1), "New_");
        });

        Assert.That(runner.GetFullText(), Does.Contain($"{newLine}New_Type Arrest_End{newLine}"));

        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void InsertTextToOneLineAtEnd([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // insert "_Item" after Arrest_End in the Type property
            file.InsertText((2, 16), "_Item");
        });

        Assert.That(runner.GetFullText(), Does.Contain($"{newLine}Type Arrest_End_Item{newLine}"));

        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void InsertTwoLines([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // insert "Supply\nNotType " to change Type to Supply and make a new property: 'NotType Arrest_End'
            file.InsertText((2, 6), $"Supply{newLine}NotType ");
        });

        Assert.That(runner.GetFullText(), Does.Contain($"{newLine}Type Supply{newLine}NotType Arrest_End{newLine}"));

        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void InsertTwoLinesBeginEmpty([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // insert "NotType Supply\nBypass_ID_Limit" on line 6 to add a 'NotType Supply' property and a 'Bypass_ID_Limit' flag.
            file.InsertText((6, 1), $"NotType Supply{newLine}Bypass_ID_Limit");
        });

        Assert.That(runner.GetFullText(), Does.Contain($"{newLine}ID 1196{newLine}NotType Supply{newLine}Bypass_ID_Limit{newLine}"));

        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void InsertTwoLinesEndEmpty([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // add a 1 at the end of the ID and add a 'Bypass_ID_Limit' flag by adding "1\nBypass_ID_Limit" to line 5.
            file.InsertText((5, 8), $"1{newLine}Bypass_ID_Limit");
        });

        Assert.That(runner.GetFullText(), Does.Contain($"{newLine}ID 11961{newLine}Bypass_ID_Limit{newLine}{newLine}Size_X 1{newLine}"));

        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    [Test]
    public void InsertManyLines([Values("\n", "\r\n")] string newLine)
    {
        using OpenedFile runner = new OpenedFile(DocumentUri.File("C:\\test.dat"), WriteBasicDatFile(true, newLine), _logger);

        runner.UpdateText(file =>
        {
            // insert "Supply\nNotType Arrest_End\nName " to change Type to Supply and make new properties: 'NotType Gun', 'Test Property', and 'Name Arrest_End'
            file.InsertText((2, 6), $"Supply{newLine}NotType Gun{newLine}Test Property{newLine}Name ");
        });

        Assert.That(runner.GetFullText(), Does.Contain($"{newLine}Type Supply{newLine}NotType Gun{newLine}Test Property{newLine}Name Arrest_End{newLine}"));

        runner.AssertFileHasValidIndex();

        Console.WriteLine(runner.GetFullText());
    }

    private static string WriteBasicDatFile(bool trailingNewLine, string newLine)
    {
        StringBuilder bldr = new StringBuilder();

/* 1 */ bldr.Append("GUID cb4352c6fcb044c2b58e6edfa5644904").Append(newLine)
/* 2 */     .Append("Type Arrest_End").Append(newLine)
/* 3 */     .Append("Rarity Uncommon").Append(newLine)
/* 4 */     .Append("Useable Arrest_End").Append(newLine)
/* 5 */     .Append("ID 1196").Append(newLine)
/* 6 */     .Append("").Append(newLine)
/* 7 */     .Append("Size_X 1").Append(newLine)
/* 8 */     .Append("Size_Y 1").Append(newLine)
/* 9 */     .Append("Size_Z 0.3").Append(newLine)
/* 0 */     .Append("").Append(newLine)
/* 1 */     .Append("").Append(newLine)
/* 2 */     .Append("Blueprints 0").Append(newLine)
/* 3 */     .Append("").Append(newLine)
/* 4 */     .Append("").Append(newLine)
/* 5 */     .Append("").Append(newLine)
/* 6 */     .Append("Actions 0").Append(newLine);

        if (trailingNewLine)
            bldr.Append(newLine);

        return bldr.ToString();
    }
}
