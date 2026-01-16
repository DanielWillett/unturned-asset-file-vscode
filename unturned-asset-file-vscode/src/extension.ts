import { existsSync } from 'fs';
import { join } from 'path';
import { commands, Disposable, env, ExtensionContext, window, workspace } from 'vscode';
import { ExecutableOptions, LanguageClient, LanguageClientOptions, ServerOptions, State } from 'vscode-languageclient/node';

import { AssetPropertiesViewProvider } from './views/asset-properties';

// commands
import { addProperty } from './commands/add-property';
import { cursorMoveTo } from './commands/cursor-move-to';
import { refreshAssetProperties } from './commands/refresh-asset-properties';

// request handlers
import { GetDocumentText, handleGetDocumentText } from './jsonrpc/get-document-text';
import { getEnvironmentData } from 'worker_threads';


export const languageId = "unturned-dat";
export const fileMatcher = "{**/*.dat,**/*.asset,**/Config_*Difficulty.txt}";

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
    await window.showInformationMessage("Thank you for using the UnturnedDat extension.");

    // todo: package with extension
    const dllPath = context.asAbsolutePath(join('..', 'LspServer', 'bin', 'Debug', 'net9.0', 'LspServer.dll'));

    if (!existsSync(dllPath))
    {
        await window.showErrorMessage("LSP executible not found at \"" + dllPath + "\".");
        client = undefined;
    }
    else
    {
        const isDebug = process.env.UNTURNED_LSP_DEBUG === "1";
        const options: ExecutableOptions = {
            env: isDebug
                ? {
                    "UNTURNED_LSP_DEBUG": "1"
                }
                : {}
        };
        const args = [ dllPath, "--clientProcessId", process.pid.toString() ];
        const serverOptions: ServerOptions = {
            run: { command: 'dotnet', args: args, options: options },
            debug: { command: 'dotnet', args: args, options: options }
        };

        const clientOptions: LanguageClientOptions = {
            documentSelector: [
                {
                    pattern: fileMatcher,
                    language: languageId
                }
            ],
            synchronize: {
                configurationSection: 'unturned-data-file-lsp',
                fileEvents: workspace.createFileSystemWatcher(fileMatcher)
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