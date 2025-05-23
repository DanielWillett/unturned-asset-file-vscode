import { Uri } from 'vscode';
import { RequestType } from 'vscode-languageclient';

export const DiscoverAssetProperties = new RequestType<DiscoverAssetPropertiesParams, AssetProperty[], void>('unturnedDataFile/assetProperties');

export interface DiscoverAssetPropertiesParams {
    readonly document: string
}

export interface AssetProperty {

    key: string,
    line: number | undefined,
    value: any,
    description: string | null,
    markdown: string | null
}