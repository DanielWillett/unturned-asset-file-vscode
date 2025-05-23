import * as path from 'path';
import vscode, { tasks } from 'vscode';
import { Trace } from 'vscode-jsonrpc';
import * as fs from 'fs'

import { AssetPropertiesViewProvider } from './views/asset-properties';

import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  State,
  TransportKind
} from 'vscode-languageclient/node';

let client: LanguageClient | undefined;

let eventSubs: vscode.Disposable[] = [];

let assetPropertiesViewProvider: AssetPropertiesViewProvider | undefined;

export function getClient(): LanguageClient {
	if (client == undefined) {
		throw new Error("Uninitialized.");
	}

	return client;
}

export async function activate(context: vscode.ExtensionContext): Promise<void> {

	const dllPath = context.asAbsolutePath(path.join('..', 'LspServer', 'bin', 'Debug', 'net9.0', 'LspServer.dll'));

	if (!fs.existsSync(dllPath)) {
		await vscode.window.showErrorMessage("LSP executible not found at \"" + dllPath + "\".");
		client = undefined;
	}
	else {
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
				fileEvents: vscode.workspace.createFileSystemWatcher("**/*.{dat,asset}")
			}
		}

		client = new LanguageClient('unturned-data-file-lsp', "Unturned Data File format LSP", serverOptions, clientOptions);
	}

	assetPropertiesViewProvider = new AssetPropertiesViewProvider();

	vscode.window.registerTreeDataProvider(
		'unturnedDataFile.assetProperties',
		assetPropertiesViewProvider
	);

	vscode.commands.registerCommand('unturnedDataFile.assetProperties.refreshAssetProperties', async () => {
		if (assetPropertiesViewProvider)
			await assetPropertiesViewProvider.refresh();
	});

	eventSubs.push(vscode.window.onDidChangeActiveTextEditor(() => {
		return vscode.commands.executeCommand('unturnedDataFile.assetProperties.refreshAssetProperties');
	}));

	if (client) {

		eventSubs.push(client.onDidChangeState(async event => {

			if (event.newState != State.Running)
				return;

			await vscode.window.showInformationMessage("LSP initialized.");

			if (assetPropertiesViewProvider && vscode.window.activeTextEditor != null)
				await assetPropertiesViewProvider.refresh();
		}));

		client.start();
	}
}

export async function deactivate(): Promise<void> {

	console.log("Deactivating...");

	eventSubs.forEach(f => f.dispose());
	eventSubs = [];

	if (assetPropertiesViewProvider) {
		assetPropertiesViewProvider = undefined;
	}

	if (client) {
		await client.stop();
		client = undefined;
	}

	console.log("Done deactivating.");
}
