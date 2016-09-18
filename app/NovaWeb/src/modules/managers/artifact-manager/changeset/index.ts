 import {IIStatefulItem} from "../../models";

export enum ChangeTypeEnum {
    Initial,
    Update,
    Add,
    Delete
}

export interface IChangeSet {
    type: ChangeTypeEnum;
    key: string | number;
    value: any;
}
export interface IChangeCollector {
    add(changeset: IChangeSet, old?: any);
    collection: IChangeSet[];
    reset(): IChangeSet[];
}

export class ChangeSetCollector implements IChangeCollector {
    private _collection: IChangeSet[];

    constructor(private item: IIStatefulItem) {
        this.reset();
    }

    public get collection(): IChangeSet[] {
        return this._collection || (this._collection = []);

    }

    public add(changeset: IChangeSet, initValue?: any) {                 
        let init = this.collection.filter((it: IChangeSet) => 
            it.key === changeset.key && changeset.type === ChangeTypeEnum.Initial
        )[0];
        if (!init) {
            this.collection.push({
               type: ChangeTypeEnum.Initial,
               key: name,
               value: initValue              
           } as IChangeSet);
        } 
        let found = this.collection.filter((it: IChangeSet) => {
            return it.key === changeset.key && changeset.type === ChangeTypeEnum.Update;
        })[0];

        if (found) {
            found.value = changeset.value;
        } else {
            this.collection.push(changeset);
        }
        this.item.artifactState.dirty = true;
    }


    public reset(): IChangeSet[] {
        let initValues = this.collection.filter((it: IChangeSet) => 
            it.type === ChangeTypeEnum.Initial
        );
        
        this._collection = [];
        return initValues;
    }

}