import * as vscode from "vscode";


export function isDatFile(uri: vscode.Uri | undefined)
{
    if (!uri)
    {
        return false;
    }

    const path = uri.path.toLowerCase();

    if (path.endsWith(".txt"))
    {
        return path.endsWith("config_easy.txt") || uri.path.endsWith("config_normal.txt") || uri.path.endsWith("config_hard.txt") || uri.path.endsWith("config.txt");
    }

    return path.endsWith(".dat") || path.endsWith(".asset") || path.endsWith(".udat") || path.endsWith(".udatproj");
}