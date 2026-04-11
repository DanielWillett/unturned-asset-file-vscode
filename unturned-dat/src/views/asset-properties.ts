import
{
    TreeDataProvider,
    Event,
    EventEmitter,
    window,
    CancellationToken,
    TreeItem,
    ProviderResult,
    TreeItemCollapsibleState,
    ThemeIcon,
    Range,
    MarkdownString,
    env
} from 'vscode';

import { getClient, getOutputChannel, getIsReady } from '../extension';
import { isDatFile } from '../util/path';

import { DiscoverAssetProperties } from '../jsonrpc/asset-property';
import { DiscoverBundleAssets } from '../jsonrpc/bundle-asset-info';
import { AssetProperty, TypeHierarchyElement } from '../spec/asset-property';
import { BundleAssetInfo } from "../spec/bundle-asset-info";

export class AssetPropertiesViewProvider implements TreeDataProvider<AssetPropertyViewItem>
{

    propertyValues: AssetPropertyViewItem[] = [];
    bundleChildren: BundleAssetInfo[] | null = null;
    bundleChildrenElements: AssetPropertyViewItem[] | null = null;
    bundleRequestVersion: number = 0;

    private _onDidChangeTreeData: EventEmitter<void | AssetPropertyViewItem | AssetPropertyViewItem[] | null | undefined>
        = new EventEmitter<void | AssetPropertyViewItem | AssetPropertyViewItem[] | null | undefined>();

    onDidChangeTreeData?: Event<void | AssetPropertyViewItem | AssetPropertyViewItem[] | null | undefined> | undefined
        = this._onDidChangeTreeData.event;

    async refresh(): Promise<boolean>
    {
        const txtDoc = window.activeTextEditor;

        if (txtDoc === undefined || !isDatFile(txtDoc.document.uri))
        {
            if (this.propertyValues.length === 0)
            {
                return false;
            }

            this.propertyValues = [];
        }
        else if (!getIsReady())
        {
            if (this.propertyValues.length === 0)
            {
                return false;
            }

            this.propertyValues = [];
        }
        else
        {
            const result = await getClient().sendRequest(DiscoverAssetProperties, { document: txtDoc.document.uri.toString() });
            //getOutputChannel().info(JSON.stringify(result, null, "  "));
            if (this.propertyValues.length === result.length && result.every((prop, i) => this.propertyValues[i].property === prop))
            {
                return false;
            }

            this.propertyValues = result.map(prop => AssetPropertyViewItem.createForAssetProperty(prop, null, -1));
            this.bundleChildrenElements = null;
        }

        this._onDidChangeTreeData.fire();
        return true;
    }


    getTreeItem(element: AssetPropertyViewItem): TreeItem | Thenable<TreeItem>
    {
        return element;
    }


    getChildren(element?: AssetPropertyViewItem | undefined): ProviderResult<AssetPropertyViewItem[]>
    {
        if (!element)
        {
            return this.propertyValues;
        }

        if (element.property?.isBundleHeader)
        {
            if (this.bundleChildrenElements)
                return this.bundleChildrenElements;

            return this.getBundleChildren(element);
        }

        return element.getChildren();
    }

    async getBundleChildren(parent: AssetPropertyViewItem): Promise<AssetPropertyViewItem[]>
    {
        const txtDoc = window.activeTextEditor;

        if (txtDoc === undefined || !isDatFile(txtDoc.document.uri) || !getIsReady())
        {
            this.bundleChildren = [];
            this.bundleChildrenElements = [];
            return this.bundleChildrenElements;
        }

        this.bundleRequestVersion += 1;
        const startVersion = this.bundleRequestVersion;
        const result = await getClient().sendRequest(DiscoverBundleAssets, { document: txtDoc.document.uri.toString(), path: undefined });

        const elements = AssetPropertiesViewProvider.createBundleChildren(parent, result);

        if (startVersion === this.bundleRequestVersion)
        {
            this.bundleChildren = result;
            this.bundleChildrenElements = elements;
        }

        return elements;
    }

    static createBundleChildren(parent: AssetPropertyViewItem, bundleChildren: BundleAssetInfo[]): AssetPropertyViewItem[]
    {
        const outputs: AssetPropertyViewItem[] = [];

        for (let i = 0; i < bundleChildren.length; ++i)
        {
            const bundleInfo = bundleChildren[i];
            outputs[i] = AssetPropertyViewItem.createForBundleObject(bundleInfo, parent);
        }

        return outputs;
    }

    getParent?(element: AssetPropertyViewItem): ProviderResult<AssetPropertyViewItem>
    {
        return element.parent;
    }


    resolveTreeItem?(item: TreeItem, element: AssetPropertyViewItem, token: CancellationToken): ProviderResult<TreeItem>
    {
        element.resolve();
        return item;
    }
}

class AssetPropertyViewItem extends TreeItem
{
    property: AssetProperty | undefined;
    bundleObject: BundleAssetInfo | undefined;
    children: AssetPropertyViewItem[] | null;
    parent: AssetPropertyViewItem | null;
    typeIndex: number;


    static createForAssetProperty(property: AssetProperty, parent: AssetPropertyViewItem | null, typeIndex: number): AssetPropertyViewItem
    {
        const name = typeIndex >= 0
            ? property.typeHierarchy![typeIndex].displayName
            : getName(property);

        const state = property.isBundleHeader || (typeIndex < 0 && (property.children || property.typeHierarchy))
            ? TreeItemCollapsibleState.Collapsed
            : TreeItemCollapsibleState.None;

        const icon = typeIndex >= 0 ? new ThemeIcon("type-hierarchy-super") : getValueIcon(property);

        const item = new AssetPropertyViewItem(name, state, icon, parent, typeIndex);
        item.property = property;
        return item;
    }

    static createForBundleObject(object: BundleAssetInfo, parent: AssetPropertyViewItem): AssetPropertyViewItem
    {
        const name = object.key;

        const icon = new ThemeIcon(object.path ? "build" : "question");

        const item = new AssetPropertyViewItem(name, TreeItemCollapsibleState.None, icon, parent, -1);
        item.bundleObject = object;
        return item;
    }


    constructor(name: string, collapsableState: TreeItemCollapsibleState, iconPath: ThemeIcon, parent: AssetPropertyViewItem | null, typeIndex: number)
    {
        super(name, collapsableState);
        this.children = null;
        this.iconPath = iconPath;
        this.parent = parent;
        this.typeIndex = typeIndex;
    }

    getChildren(): AssetPropertyViewItem[]
    {
        if (this.typeIndex >= 0 || !this.property)
        {
            return [ ];
        }

        if (this.property.typeHierarchy)
        {
            this.children = [ ];
            for (let i = 0; i < this.property.typeHierarchy.length; ++i)
            {
                this.children.push(AssetPropertyViewItem.createForAssetProperty(this.property, this, i));
            }

            return this.children;
        }

        if (!this.property.children)
        {
            return [ ];
        }

        if (this.children)
        {
            return this.children;
        }

        this.children = this.property.children.map(prop => AssetPropertyViewItem.createForAssetProperty(prop, this, -1));
        return this.children;
    }

    resolve(): void
    {
        if (!this.property)
        {
            if (!this.bundleObject)
            {
                return;
            }

            if (this.bundleObject.markdown)
            {
                if (this.bundleObject.path)
                {
                    this.tooltip = new MarkdownString(this.bundleObject.markdown + "\r\n\\\r\n`" + this.bundleObject.path + "`");
                }
                else
                {
                    this.tooltip = new MarkdownString(this.bundleObject.markdown);
                }
            }
            if (this.bundleObject.path)
            {
                if (this.bundleObject.description)
                {
                    this.tooltip = new MarkdownString(this.bundleObject.description + "\r\n\\\r\n`" + this.bundleObject.path + "`");
                }
                else
                {
                    this.tooltip = new MarkdownString("`" + this.bundleObject.path + "`");
                }
            }
            else
            {
                this.tooltip = this.bundleObject.description ?? this.bundleObject.key;
            }

            return;
        }

        if (this.typeIndex >= 0)
        {
            this.tooltip = this.property.typeHierarchy![this.typeIndex].type;
            return;
        }

        try
        {
            if (this.property.markdown)
            {
                this.tooltip = new MarkdownString(this.property.markdown);
            }
            else
            {
                this.tooltip = this.property.description ?? this.property.key;
            }

            if (this.property.children)
            {
                this.command = undefined;
                return;
            }

            if (this.property.range?.start !== undefined)
            {
                this.command = {
                    command: "unturnedDataFile.cursorMoveTo",
                    title: `Select ${this.property.key}`,
                    arguments: [ new Range(this.property.range.start, this.property.range.start) ],
                    tooltip: `L${(this.property.range.start.line + 1).toLocaleString()} C${(this.property.range.start.character + 1).toLocaleString()}`
                };
            }
            else
            {
                this.command = {
                    command: "unturnedDataFile.addProperty",
                    title: `Add ${this.property.key}`,
                    arguments: [ this.property.key ],
                    tooltip: this.property.key
                };
            }
        }
        catch (x)
        {
            window.showErrorMessage(`Ex: ${x}`);
        }
    }
}

function getName(property: AssetProperty): string
{
    let key = property.key;
    if (property.ordinal)
    {
        key = `[${property.ordinal - 1}]`;
    }

    if (property.typeHierarchy && property.typeHierarchy.length > 0)
    {
        return `${key} = ${property.typeHierarchy[0].displayName}`;
    }

    if (property.value === null)
    {
        return `${key} = [no value]`;
    }

    switch (typeof (property.value))
    {
        case "undefined":
        case "function":
        case "symbol":
            return `${key}`;

        case "string":
            return `${key} = \"${property.value.toString()}\"`;

        case "bigint":
        case "number":
        case "boolean":
            return `${key} = ${property.value.toString()}`;

        default:
            return `${key} = ${JSON.stringify(property.value)}`;
    }
}

function getValueIcon(property: AssetProperty): ThemeIcon
{
    if (property.isBundleHeader)
    {
        return new ThemeIcon("list-selection");
    }
    if (!property.range)
    {
        return new ThemeIcon("add");
    }
    else if (property.typeHierarchy)
    {
        return new ThemeIcon("symbol-class");
    }
    else if (property.children)
    {
        if (property.children.length === 0 || property.children[0].ordinal)
        {
            return new ThemeIcon("symbol-array");
        }
        else
        {
            return new ThemeIcon("symbol-object");
        }
    }

    if (property.value === null)
    {
        return new ThemeIcon("symbol-null");
    }

    switch (typeof (property.value))
    {
        case "undefined":
        case "function":
        case "symbol":
            return new ThemeIcon("symbol-null");

        case "string":
            return new ThemeIcon("symbol-string");

        case "bigint":
        case "number":
            return new ThemeIcon("symbol-numeric");
        case "boolean":
            return new ThemeIcon("symbol-boolean");

        default:
            return new ThemeIcon("symbol-object");
    }
}