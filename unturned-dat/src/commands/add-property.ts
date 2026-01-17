import { commands, Position, Range, window } from 'vscode';
import { DiscoverAssetProperties } from '../jsonrpc/add-property';
import { getAssetPropertiesViewProvider, getClient, languageId } from '../extension';

export async function addProperty(propertyKey: string)
{
    const txt = window.activeTextEditor;
    if (!txt || txt.document.languageId !== languageId)
    {
        window.showErrorMessage("Unturned Data File not open.");
        return;
    }

    const response = await getClient().sendRequest(DiscoverAssetProperties, {
        document: txt.document.uri.toString(),
        key: propertyKey
    });

    if (response === undefined)
    {
        window.showErrorMessage("Unable to find a location for the property.");
        return;
    }

    let insertText = propertyKey;

    if (!response.isFlag)
    {
        insertText += " ";
    }

    const cursorOffset = insertText.length;
    for (let i = 0; i < response.insertLines; ++i)
    {
        insertText = "\n" + insertText;
    }

    window.showInformationMessage(`Pos: ${JSON.stringify(response.position)} (lines: ${response.insertLines}).`);
    await txt.edit(e => e
        .insert(response.position, insertText)
    );

    const cursorPosition = new Position(response.position.line + response.insertLines, response.position.character + cursorOffset);
    await commands.executeCommand("unturnedDataFile.cursorMoveTo", new Range(cursorPosition, cursorPosition));

    getAssetPropertiesViewProvider().refresh();
}