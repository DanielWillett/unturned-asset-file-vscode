using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public ref partial struct SourceNodeTokenizer
{
    // UNT1002
    private readonly void LogDiagnostic_StringQuotesNotTerminated(int endCharacter)
    {
        _diagnosticSink?.AcceptDiagnostic(new DatDiagnosticMessage
        {
            Diagnostic = DatDiagnostics.UNT1002,
            Message = DiagnosticResources.UNT1002,
            Range = new FileRange(_position, new FilePosition(_position.Line, endCharacter))
        });
    }

    // UNT1004
    private readonly void LogDiagnostic_UnexpectedEscapeSequence(int slashCharacter, char character)
    {
        if (_diagnosticSink == null)
            return;

        string message;
        FileRange range;
        if (character == '\0' || char.IsControl(character))
        {
            message = DiagnosticResources.UNT1004_Trailing;
            range = new FileRange(_position.Line, slashCharacter, _position.Line, slashCharacter);
        }
        else
        {
            range = new FileRange(_position.Line, slashCharacter, _position.Line, slashCharacter + 1);

            // ex: [Key \"Value], which doesn't get parsed correctly
            if (character == '\"' && slashCharacter == _position.Character)
            {
                message = DiagnosticResources.UNT1004_LeadingEscapedSlash;
            }
            else
            {
                message = string.Format(DiagnosticResources.UNT1004, new string(character, 1));
            }
        }

        _diagnosticSink.AcceptDiagnostic(new DatDiagnosticMessage
        {
            Diagnostic = DatDiagnostics.UNT1004,
            Message = message,
            Range = range
        });
    }

    // UNT1001
    private readonly void LogDiagnostic_ValueAndListOrDict()
    {
        _diagnosticSink?.AcceptDiagnostic(new DatDiagnosticMessage
        {
            Diagnostic = DatDiagnostics.UNT1001,
            Message = DiagnosticResources.UNT1001,
            Range = new FileRange(_position, _position)
        });
    }

    // UNT2001 / UNT2002
    private readonly void LogDiagnostic_ListOrDictMissingClosingBracket(FilePosition startPos, bool isList)
    {
        _diagnosticSink?.AcceptDiagnostic(new DatDiagnosticMessage
        {
            Diagnostic = isList ? DatDiagnostics.UNT2002 : DatDiagnostics.UNT2001,
            Message = isList ? DiagnosticResources.UNT2002 : DiagnosticResources.UNT2001,
            Range = new FileRange(startPos, startPos)
        });
    }

    // UNT1020
    private readonly void LogDiagnostic_MaximumDepth(FileRange skipRange)
    {
        _diagnosticSink?.AcceptDiagnostic(new DatDiagnosticMessage
        {
            Diagnostic = DatDiagnostics.UNT1020,
            Message = string.Format(DiagnosticResources.UNT1020, MaxDepth),
            Range = skipRange
        });
    }

    // UNT105
    private readonly void LogDiagnostic_UnnecessaryComma(FilePosition commaPos)
    {
        _diagnosticSink?.AcceptDiagnostic(new DatDiagnosticMessage
        {
            Diagnostic = DatDiagnostics.UNT105,
            Message = DiagnosticResources.UNT105,
            Range = new FileRange(commaPos, commaPos)
        });
    }
}
