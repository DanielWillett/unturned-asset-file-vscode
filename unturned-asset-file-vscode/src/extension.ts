import * as path from 'path';
import vscode from 'vscode';
import { Trace } from 'vscode-jsonrpc';
import * as fs from 'fs'

import { AssetPropertiesViewProvider } from './views/asset-properties';

import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind
} from 'vscode-languageclient/node';

let client: LanguageClient | undefined;

var textDocChangeEventSub: vscode.Disposable | undefined;

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

	const assetPropertiesViewProvider = new AssetPropertiesViewProvider();

	vscode.window.registerTreeDataProvider(
		'unturnedDataFile.assetProperties',
		assetPropertiesViewProvider
	);

	vscode.commands.registerCommand('unturnedDataFile.assetProperties.refreshAssetProperties', () => {
		assetPropertiesViewProvider.refresh();
	});

	textDocChangeEventSub = vscode.window.onDidChangeActiveTextEditor(function (event) {
		vscode.commands.executeCommand('unturnedDataFile.assetProperties.refreshAssetProperties');
	});

	if (client) {
		client.start();
	}
}

export async function deactivate(): Promise<void> {

	await vscode.window.showErrorMessage("Closing...");
	if (textDocChangeEventSub) {
		textDocChangeEventSub.dispose();
		textDocChangeEventSub = undefined;
	}

	if (client) {
		await client.stop();
		client = undefined;
	}
	await vscode.window.showErrorMessage("Done");
}
