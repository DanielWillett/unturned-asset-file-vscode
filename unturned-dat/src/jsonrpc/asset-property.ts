import { AssetProperty } from '../spec/asset-property';
import { RequestType } from 'vscode-languageclient';

export const DiscoverAssetProperties = new RequestType<DiscoverAssetPropertiesParams, AssetProperty[], void>("unturnedDataFile/assetProperties");

export interface DiscoverAssetPropertiesParams
{
    readonly document: string;
}