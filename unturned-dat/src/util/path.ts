import * as vscode from "vscode";


export function isDatFile(uri: vscode.Uri | undefined)
{
    if (uri === undefined)
    {
        return false;
    }

    const path = uri.path.toLowerCase();

    if (path.endsWith(".txt"))
    {
        return uri.path.endsWith("Config_Easy.txt") || uri.path.endsWith("Config_Normal.txt") || uri.path.endsWith("Config_Hard.txt") || uri.path.endsWith("Config.txt");
    }

    return path.endsWith(".dat") || path.endsWith(".asset") || path.endsWith(".udat") || path.endsWith(".udatproj");
}