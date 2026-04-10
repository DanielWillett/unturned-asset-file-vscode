import { Range } from 'vscode';

export interface TypeHierarchyElement
{
    type: string,
    displayName: string
}

export interface AssetProperty
{
    key: string,
    range: Range | undefined,
    value: any,
    description: string | null | undefined,
    markdown: string | null | undefined;
    ordinal: number | undefined;
    children: AssetProperty[] | undefined;
    typeHierarchy: TypeHierarchyElement[] | undefined;
    bundlePath: string | null | undefined;
}