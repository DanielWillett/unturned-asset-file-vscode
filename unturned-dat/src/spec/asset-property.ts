import { Range } from 'vscode';

export interface AssetProperty
{
    key: string,
    range: Range | undefined,
    value: any,
    description: string | null | undefined,
    markdown: string | null | undefined;
    ordinal: number | undefined;
    children: AssetProperty[] | undefined;
}