import { Range } from 'vscode';

export interface AssetProperty
{

    key: string,
    range: Range | undefined,
    value: any,
    description: string | null,
    markdown: string | null;
}