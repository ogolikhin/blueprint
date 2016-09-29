import {IBPTreeController, ITreeNode} from "./bp-tree";
export {ITreeNode}

export class BPTreeControllerMock implements IBPTreeController {
    private add(id: number) {
        return {
            id: id,
            name: `Artifact ${id}`,
            itemTypeId: 1,
        } as ITreeNode;

    }
    public _datasource: ITreeNode[] = [];

    public get isEmpty(): boolean {
        return !Boolean(this._datasource && this._datasource.length);
    }

    public selectNode(id: number) { }

    public reload(data?: any[], id?: number) {
        if (!data) {
            this._datasource = data;
            return;
        }
            
        for (let i = 0; i < 10; i++) {
            this._datasource.push(this.add(i));
        }
        if (id) {
            let node = this._datasource[id];
            node.children = [];
            for (let i = 100; i < 105; i++) {
                node.children.push(this.add(i));
            }
            node.hasChildren = true;
        }
    }

    public nodeExists(id: number): boolean { return false; } 

    public showLoading() { }

    public showNoRows() { }

    public hideOverlays() { }
}
