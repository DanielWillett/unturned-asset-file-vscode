import { Position } from 'vscode';
import { RequestType } from 'vscode-languageclient';

export const DiscoverAssetProperties = new RequestType<GetAssetPropertyAddLocationParams, GetAssetPropertyAddLocationResponse, void>("unturnedDataFile/getAddProperty");

export interface GetAssetPropertyAddLocationParams
{
    readonly document: string,
    readonly key: string;
}

export interface GetAssetPropertyAddLocationResponse
{
    readonly position: Position,
    readonly isFlag: boolean;
    readonly insertLines: number;
}