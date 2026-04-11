import { BundleAssetInfo } from '../spec/bundle-asset-info';
import { RequestType } from 'vscode-languageclient';

export const DiscoverBundleAssets = new RequestType<DiscoverBundleAssetsParams, BundleAssetInfo[], void>("unturnedDataFile/assetBundleAssets");

export interface DiscoverBundleAssetsParams
{
    readonly document: string,
    readonly path: string | undefined;
}