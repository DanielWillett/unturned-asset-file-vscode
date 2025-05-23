import * as vscode from 'vscode'
import * as path from '../util/path'
import * as ext from '../extension'

import { AssetProperty } from '../data/asset-property'

export class AssetPropertiesViewProvider implements vscode.TreeDataProvider<AssetPropertyViewItem> {

    propertyValues: AssetPropertyViewItem[] = [];

    private _onDidChangeTreeData: vscode.EventEmitter<void | AssetPropertyViewItem | AssetPropertyViewItem[] | null | undefined>
        = new vscode.EventEmitter<void | AssetPropertyViewItem | AssetPropertyViewItem[] | null | undefined>();

    onDidChangeTreeData?: vscode.Event<void | AssetPropertyViewItem | AssetPropertyViewItem[] | null | undefined> | undefined
        = this._onDidChangeTreeData.event;

    async refresh(): Promise<boolean> {
        const txtDoc = vscode.window.activeTextEditor;

        if (txtDoc == undefined || !path.isDatFile(txtDoc.document.uri)) {
            if (this.propertyValues.length == 0)
                return false;

            this.propertyValues = [];
        }
        else {
            //await ext.getClient().sendRequest()
        }

        this._onDidChangeTreeData.fire();
        return true;
    }


    getTreeItem(element: AssetPropertyViewItem): vscode.TreeItem | Thenable<vscode.TreeItem> {
        return element;
    }


    getChildren(element?: AssetPropertyViewItem | undefined): vscode.ProviderResult<AssetPropertyViewItem[]> {
        return element == undefined ? this.propertyValues : [];
    }


    getParent?(element: AssetPropertyViewItem): vscode.ProviderResult<AssetPropertyViewItem> {
        return null;
    }


    resolveTreeItem?(item: vscode.TreeItem, element: AssetPropertyViewItem, token: vscode.CancellationToken): vscode.ProviderResult<vscode.TreeItem> {
        element.resolve();
        return item;
    }
}

class AssetPropertyViewItem extends vscode.TreeItem {

    property: AssetProperty;

    constructor(property: AssetProperty) {
        super(property.key, vscode.TreeItemCollapsibleState.None);
        this.property = property;
    }

    resolve(): void {
        this.tooltip = this.property.markdown ?? this.property.description ?? this.property.key;
    }
}