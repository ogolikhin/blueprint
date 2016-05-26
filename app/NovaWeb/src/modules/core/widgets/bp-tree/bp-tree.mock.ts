import {IBPTreeController, ITreeNode} from "./bp-tree"
export {IBPTreeController, ITreeNode}

export class BPTreeControllerMock implements IBPTreeController {
    private add(id: number) {
        return {
            id: id,
            name: `Artifact ${id}`,
            type: 1,
        } as ITreeNode;

    }
    public _datasource: ITreeNode[] = [];
    public addNode(data: any[], index?: number, propertyMap?: any) {
        for (let i = 0; i < 10; i++) {
            this._datasource.push(this.add(i));
        }
    }

    public addNodeChildren(id: number, data: any[], propertyMap?: any) {
        let node = this._datasource[0];
        node.children = [];
        for (let i = 100; i < 105; i++) {
            this._datasource.push(this.add(i));
        }
        node.hasChildren = true;
        node.loaded = true;
    }

    public removeNode(id: number) {
        this._datasource = this._datasource.filter(function (it) {
            return it.id !== id;
        });
    }

    public selectNode(id: number) { }

    public refresh() { }
}

