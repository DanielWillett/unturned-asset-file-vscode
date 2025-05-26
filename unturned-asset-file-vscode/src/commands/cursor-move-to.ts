import { Range, Selection, window } from 'vscode';

export async function cursorMoveTo(range: Range)
{
    const txt = window.activeTextEditor;
    if (!txt)
    {
        window.showErrorMessage("Text Document not open.");
        return;
    }

    txt.selections = [ new Selection(range.start, range.end) ];
    txt.revealRange(range);
    window.showTextDocument(txt.document, txt.viewColumn, false);
}