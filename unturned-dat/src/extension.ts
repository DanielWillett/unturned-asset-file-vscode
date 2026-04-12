import { existsSync, accessSync, constants } from 'fs';
import { join } from 'path';
import { commands, Disposable, env, ExtensionContext, LogLevel, window, workspace, LogOutputChannel, ConfigurationTarget, MarkdownString } from 'vscode';
import { ExecutableOptions, LanguageClient, LanguageClientOptions, ServerOptions, State, TransportKind } from 'vscode-languageclient/node';

import { execFile, execFileSync, spawnSync } from 'child_process';

import { AssetPropertiesViewProvider } from './views/asset-properties';

// commands
import { addProperty } from './commands/add-property';
import { cursorMoveTo } from './commands/cursor-move-to';
import { refreshAssetProperties } from './commands/refresh-asset-properties';

// request handlers
import { GetDocumentText, handleGetDocumentText } from './jsonrpc/get-document-text';
import { Ready } from './jsonrpc/ready';
import { getEnvironmentData } from 'worker_threads';
import { Trace } from "vscode-jsonrpc";
import { RequestAdminPrivileges, SendAdminPrivilegesResponse } from "./jsonrpc/request-admin-privileges";


export const languageId = "unturned-dat";
export const configSection = "unturned-data-file-lsp";
export const fileMatcher = "{**/*.dat,**/*.asset,**/*.udatproj,**/Config_*Difficulty.txt,**/Config.txt}";
export const dotnetVersion = 10;

let client: LanguageClient | undefined;
let output: LogOutputChannel | undefined;
let isReady: boolean;

let registrations: Disposable[] = [];

let assetPropertiesViewProvider: AssetPropertiesViewProvider | undefined;

export function getClient(): LanguageClient
{
    if (!client)
    {
        throw new Error("Uninitialized.");
    }

    return client;
}

export function getOutputChannel(): LogOutputChannel
{
    if (!output)
    {
        throw new Error("Uninitialized.");
    }

    return output;
}

export function getIsReady(): boolean
{
    return isReady;
}

export function getAssetPropertiesViewProvider(): AssetPropertiesViewProvider
{
    if (assetPropertiesViewProvider === undefined)
    {
        throw new Error("Uninitialized.");
    }

    return assetPropertiesViewProvider;
}

function _tryAccessSync(path: string, mode: number)
{
    try
    {
        accessSync(path, mode);
        return true;
    }
    catch
    {
        return false;
    }
}

async function logAndError(msg: string) : Promise<void>
{
    output?.error(msg);
    await window.showErrorMessage(msg);
}

export async function activate(context: ExtensionContext): Promise<void>
{
    let dllPath: string;
    const relativeFilePath = context.asAbsolutePath(join('..', 'LspServer', 'bin', 'Debug', 'net10.0'));

    output = window.createOutputChannel("unturned-dat", { log: true });
    registrations.push(output);

    output.info("Unturned Data File (Full) by DanielWillett loading...");
    output.info("Repository: https://github.com/DanielWillett/unturned-asset-file-vscode");
    output.info("Licensed under the GPL-3.0-or-later.");

    const isWindows = process.platform === "win32";

    let dotnetLoc = workspace.getConfiguration(configSection).get<string>("dotnetLocation");

    let skipLsp = false;
    let useShell = false;

    if (dotnetLoc)
    {
        if ((isWindows ? dotnetLoc.toLowerCase() : dotnetLoc) === "dotnet") {
            dotnetLoc = undefined;
            useShell = true;
        }
        else {
            if (isWindows && !dotnetLoc.toLowerCase().endsWith(".exe")) {
                dotnetLoc = dotnetLoc + "\\dotnet.exe";
            }
            if (!existsSync(dotnetLoc)) {
                await logAndError(`.NET runtime not found at "${dotnetLoc}". Double-check that the '${configSection}.dotnetLocation' setting points to an executable file.`);
                dotnetLoc = undefined;
                skipLsp = true;
            }
            else if (!_tryAccessSync(dotnetLoc, constants.X_OK)) {
                await logAndError(`.NET runtime at "${dotnetLoc}" does not have executable permissions.`);
                dotnetLoc = undefined;
                skipLsp = true;
            }
            else {
                const versionBuffer = execFileSync(dotnetLoc, ["--version"], { shell: false, timeout: 1000 });
                const version = versionBuffer.toString("ascii");
                const versionMajor = parseInt(version.split('.')[0]);
                if (!versionMajor || versionMajor < dotnetVersion) {
                    await logAndError(`Installed .NET version is "${version}", but needs to be at least "${dotnetVersion}.0". Install the latest .NET from: [https://dotnet.microsoft.com/en-us/download/dotnet](https://dotnet.microsoft.com/en-us/download/dotnet).`);
                    skipLsp = true;
                }
            }
        }
    }
    else if (!isWindows)
    {
        await logAndError(`.NET runtime executable not configured. Install the latest .NET from: [https://dotnet.microsoft.com/en-us/download/dotnet](https://dotnet.microsoft.com/en-us/download/dotnet) and configure the '${configSection}.dotnetLocation' setting with the 'dotnet' executable file.`);
        skipLsp = true;
    }

    const useExeFile = isWindows && !useShell && !dotnetLoc;

    if (useExeFile)
    {
        // run exe directly on windows, otherwise use the dotnet cli to run it
        dllPath = join(relativeFilePath, 'LspServer.exe');
    }
    else
    {
        dllPath = join(relativeFilePath, 'LspServer.dll');
    }

    if (!existsSync(dllPath))
    {
        await logAndError(`LSP executable not found at "${dllPath}".`);
        client = undefined;
    }
    else if (!_tryAccessSync(dllPath, constants.R_OK))
    {
        await logAndError(`LSP executable not accessible at "${dllPath}".`);
        client = undefined;
    }
    else if (!skipLsp)
    {
        const isDebug = process.env.UNTURNED_LSP_DEBUG === "1";
        const args: string[] = [ ];
        let command = dllPath;
        if (!useExeFile)
        {
            args.splice(0, 0, `"${dllPath}"`);
            if (useShell)
            {
                command = "dotnet";
            }
            else if (dotnetLoc!.indexOf(" ") >= 0)
            {
                command = `"${dotnetLoc}"`;
                useShell = true;
            }
            else
            {
                command = dotnetLoc!;
            }
        }
        else
        {
            useShell = dllPath.indexOf(" ") >= 0;
        }

        const options: ExecutableOptions = {
            env: isDebug
                ? {
                    "UNTURNED_LSP_DEBUG": "1"
                }
                : {},
            shell: useShell,
            cwd: relativeFilePath
        };

        output.info(`Launching LSP with command: '${command}', args: [ '${args.join("', '")}'' ]. Create shell? ${useShell}.`);

        const operation = { command: command, args: args, options: options };        
        const serverOptions: ServerOptions = { run: operation, debug: operation };
        const clientOptions: LanguageClientOptions = {
            documentSelector: [
                {
                    pattern: fileMatcher,
                    language: languageId
                }
            ],
            synchronize: {
                fileEvents: workspace.createFileSystemWatcher(fileMatcher)
            },
            stdioEncoding: 'utf8',
            outputChannel: output,
            markdown: {
                isTrusted: true,
                supportHtml: true
            },
            connectionOptions: {
                maxRestartCount: 0
            }
        };

        client = new LanguageClient(configSection, "Unturned Data File format LSP", serverOptions, clientOptions);
    }

    assetPropertiesViewProvider = new AssetPropertiesViewProvider();

    // tree providers

    if (!skipLsp)
    {
        registrations.push(window.registerTreeDataProvider(
            'unturnedDataFile.assetProperties',
            assetPropertiesViewProvider
        ));

        // commands
        registrations.push(commands.registerCommand('unturnedDataFile.assetProperties.refreshAssetProperties', refreshAssetProperties));
        registrations.push(commands.registerCommand('unturnedDataFile.cursorMoveTo', cursorMoveTo));
        registrations.push(commands.registerCommand("unturnedDataFile.addProperty", addProperty));

        // events
        registrations.push(window.onDidChangeActiveTextEditor(() => {
            return commands.executeCommand('unturnedDataFile.assetProperties.refreshAssetProperties');
        }));
    }

    if (client)
    {
        registrations.push(client.onDidChangeState(async event =>
        {

            if (event.newState !== State.Running)
            {
                return;
            }

            output?.info("Language server is running.");
        }));

        registrations.push(client.onNotification(Ready, async event =>
        {
            output?.info("Language server is ready.");
            isReady = true;
            await commands.executeCommand("unturnedDataFile.assetProperties.refreshAssetProperties");
        }));

        registrations.push(client.onNotification(RequestAdminPrivileges, async event =>
        {
            const result = await window.showInformationMessage(event.message, "Allow", "Not now", "Don't ask again");

            switch (result)
            {
                case "Allow":
                    client?.sendNotification(SendAdminPrivilegesResponse, { type: event.type, allowed: true });
                    return;

                case "Don't ask again":
                    const config = workspace.getConfiguration(configSection);
                    await config.update("registerDiskCleanupHandler", false, ConfigurationTarget.Global);
                    break;
            }

            client?.sendNotification(SendAdminPrivilegesResponse, { type: event.type, allowed: false });
        }));

        await client.start();

        registrations.push(client.onRequest(GetDocumentText, handleGetDocumentText));
    }
}

export async function deactivate(): Promise<void>
{
    output?.trace("Deactivating Unturned Data File (Full)...");

    if (client)
    {
        await client.stop();
        client = undefined;
    }

    registrations.forEach(f => f.dispose());
    registrations = [];

    if (assetPropertiesViewProvider)
    {
        assetPropertiesViewProvider = undefined;
    }

    isReady = false;
}