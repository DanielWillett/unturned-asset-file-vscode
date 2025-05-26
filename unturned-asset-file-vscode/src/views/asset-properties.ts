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

import { getClient } from '../extension';
import { isDatFile } from '../util/path';

import { AssetProperty, DiscoverAssetProperties } from '../data/asset-property';

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
        else
        {
            const result = await getClient().sendRequest(DiscoverAssetProperties, { document: txtDoc.document.uri.toString() });
            if (this.propertyValues.length === result.length && result.every((prop, i) => this.propertyValues[i].property === prop))
            {
                return false;
            }

            this.propertyValues = result.map(prop => new AssetPropertyViewItem(prop));
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
        return element === undefined ? this.propertyValues : [];
    }


    getParent?(element: AssetPropertyViewItem): ProviderResult<AssetPropertyViewItem>
    {
        return null;
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

    constructor(property: AssetProperty)
    {
        super(getName(property), TreeItemCollapsibleState.None);
        this.property = property;
        this.iconPath = new ThemeIcon(property.range === undefined ? "symbol-constant" : "symbol-property");
    }

    resolve(): void
    {
        try
        {
            this.tooltip = this.property.markdown ?? this.property.description ?? this.property.key;

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

    if (property.value === null)
    {
        return `${property.key} = [no value]`;
    }

    switch (typeof (property.value))
    {
        case "undefined":
        case "function":
        case "symbol":
            return `${property.key} = [no value]`;

        case "bigint":
        case "number":
        case "boolean":
        case "string":
            return `${property.key} = \"${property.value.toString()}\"`;

        default:
            return `${property.key} = ${JSON.stringify(property.value)}`;
    }
}