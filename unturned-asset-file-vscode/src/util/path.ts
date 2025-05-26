import * as vscode from "vscode";


export function isDatFile(uri: vscode.Uri | undefined)
{
    if (uri === undefined)
    {
        return false;
    }

    const path = uri.path.toLowerCase();
    return path.endsWith(".dat") || path.endsWith(".asset");
}