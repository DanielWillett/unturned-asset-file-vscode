export interface BundleAssetInfo
{
    key: string,
    type: string,
    typeName: string,
    path: string | undefined,
    description: string | undefined,
    markdown: string | undefined,
    isComponent: boolean,
    hasChildren: boolean,
    isRequired: boolean,
    isUnknown: boolean;
}