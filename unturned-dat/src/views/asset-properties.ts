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
    Range
} from 'vscode';

import { getClient, getOutputChannel, getIsReady } from '../extension';
import { isDatFile } from '../util/path';

import { DiscoverAssetProperties } from '../jsonrpc/asset-property';
import { AssetProperty, TypeHierarchyElement } from '../spec/asset-property';

export class AssetPropertiesViewProvider implements TreeDataProvider<AssetPropertyViewItem>
{

    propertyValues: AssetPropertyViewItem[] = [];

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

            this.propertyValues = result.map(prop => new AssetPropertyViewItem(prop, null, -1));
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

        return element.getChildren();
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
    property: AssetProperty;
    children: AssetPropertyViewItem[] | null;
    parent: AssetPropertyViewItem | null;
    typeIndex: number;

    constructor(property: AssetProperty, parent: AssetPropertyViewItem | null, typeIndex: number)
    {
        super(
            typeIndex >= 0 ? property.typeHierarchy![typeIndex].displayName : getName(property),
            typeIndex < 0 && (property.children || property.typeHierarchy) ? TreeItemCollapsibleState.Collapsed : TreeItemCollapsibleState.None
        );
        this.property = property;
        this.iconPath = typeIndex >= 0 ? new ThemeIcon("type-hierarchy-super") : getValueIcon(property);
        this.children = null;
        this.parent = parent;
        this.typeIndex = typeIndex;
    }

    getChildren(): AssetPropertyViewItem[]
    {
        if (this.typeIndex >= 0)
        {
            return [ ];
        }

        if (this.property.typeHierarchy)
        {
            this.children = [ ];
            for (let i = 0; i < this.property.typeHierarchy.length; ++i)
            {
                this.children.push(new AssetPropertyViewItem(this.property, this, i));
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

        this.children = this.property.children.map(prop => new AssetPropertyViewItem(prop, this, -1));
        return this.children;
    }

    resolve(): void
    {
        if (this.typeIndex >= 0)
        {
            this.tooltip = this.property.typeHierarchy![this.typeIndex].type;
            return;
        }

        try
        {
            this.tooltip = this.property.markdown ?? this.property.description ?? this.property.key;

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