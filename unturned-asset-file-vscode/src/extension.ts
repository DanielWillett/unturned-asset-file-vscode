import { existsSync } from 'fs';
import { join } from 'path';
import { commands, Disposable, ExtensionContext, window, workspace } from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, State } from 'vscode-languageclient/node';

import { AssetPropertiesViewProvider } from './views/asset-properties';

// commands
import { addProperty } from './commands/add-property';
import { cursorMoveTo } from './commands/cursor-move-to';
import { refreshAssetProperties } from './commands/refresh-asset-properties';

// request handlers
import { GetDocumentText, handleGetDocumentText } from './jsonrpc/get-document-text';


export const languageId = "unturned-data-file";

let client: LanguageClient | undefined;

let registrations: Disposable[] = [];

let assetPropertiesViewProvider: AssetPropertiesViewProvider | undefined;

export function getClient(): LanguageClient
{
    if (client === undefined)
    {
        throw new Error("Uninitialized.");
    }

    return client;
}
export function getAssetPropertiesViewProvider(): AssetPropertiesViewProvider
{
    if (assetPropertiesViewProvider === undefined)
    {
        throw new Error("Uninitialized.");
    }

    return assetPropertiesViewProvider;
}

export async function activate(context: ExtensionContext): Promise<void>
{

    const dllPath = context.asAbsolutePath(join('..', 'LspServer', 'bin', 'Debug', 'net9.0', 'LspServer.dll'));

    if (!existsSync(dllPath))
    {
        await window.showErrorMessage("LSP executible not found at \"" + dllPath + "\".");
        client = undefined;
    }
    else
    {
        let serverOptions: ServerOptions = {
            run: { command: 'dotnet', args: [dllPath] },
            debug: { command: 'dotnet', args: [dllPath] }
        };

        let clientOptions: LanguageClientOptions = {
            documentSelector: [
                {
                    pattern: "**/*.{dat,asset}",
                    language: "unturned-data-file"
                }
            ],
            synchronize: {
                configurationSection: 'unturned-data-file-lsp',
                fileEvents: workspace.createFileSystemWatcher("**/*.{dat,asset}")
            }
        };

        client = new LanguageClient('unturned-data-file-lsp', "Unturned Data File format LSP", serverOptions, clientOptions);
    }

    assetPropertiesViewProvider = new AssetPropertiesViewProvider();

    // tree providers
    registrations.push(window.registerTreeDataProvider(
        'unturnedDataFile.assetProperties',
        assetPropertiesViewProvider
    ));

    // commands
    registrations.push(commands.registerCommand('unturnedDataFile.assetProperties.refreshAssetProperties', refreshAssetProperties));
    registrations.push(commands.registerCommand('unturnedDataFile.cursorMoveTo', cursorMoveTo));
    registrations.push(commands.registerCommand("unturnedDataFile.addProperty", addProperty));

    // events
    registrations.push(window.onDidChangeActiveTextEditor(() =>
    {
        return commands.executeCommand('unturnedDataFile.assetProperties.refreshAssetProperties');
    }));

    if (client)
    {
        registrations.push(client.onDidChangeState(async event =>
        {

            if (event.newState !== State.Running)
            {
                return;
            }

            await window.showInformationMessage("LSP initialized.");
            await commands.executeCommand("unturnedDataFile.assetProperties.refreshAssetProperties");
        }));

        client.start();

        registrations.push(client.onRequest(GetDocumentText, handleGetDocumentText));
    }
}

export async function deactivate(): Promise<void>
{

    console.log("Deactivating...");

    registrations.forEach(f => f.dispose());
    registrations = [];

    if (assetPropertiesViewProvider)
    {
        assetPropertiesViewProvider = undefined;
    }

    if (client)
    {
        await client.stop();
        client = undefined;
    }

    console.log("Done deactivating.");
}