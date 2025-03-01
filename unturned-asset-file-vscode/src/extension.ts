import * as path from 'path';
import vscode from 'vscode';
import { Trace } from 'vscode-jsonrpc';

import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind
} from 'vscode-languageclient/node';

let client: LanguageClient;

export function activate(context: vscode.ExtensionContext) {

	// Use the console to output diagnostic information (console.log) and errors (console.error)
	// This line of code will only be executed once when your extension is activated
	console.log('Starting unturned-data-file-lsp...');
	

	// context.asAbsolutePath(path.join('etc'))

	let serverOptions : ServerOptions = {
		run: { command: 'dotnet', args: [ "A:\\repos\\UnturnedAssetFileLsp\\LspServer\\bin\\Debug\\net9.0\\LspServer.dll" ] },
		debug: { command: 'dotnet', args: [ "A:\\repos\\UnturnedAssetFileLsp\\LspServer\\bin\\Debug\\net9.0\\LspServer.dll" ] }
	};

	let clientOptions : LanguageClientOptions = {
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
	client.setTrace(Trace.Verbose);
	
	client.start();
}

export function deactivate() {
	if (client)
		return client.stop();
	else return undefined;
}
