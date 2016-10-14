import { IIStatefulItem } from "../item";

export enum ChangeTypeEnum {
    Add = 0,
    Update = 1,
    Delete = 2
}

export interface IChangeSet {
    type: ChangeTypeEnum;
    key: string | number;
    value: any;
}
export interface IChangeCollector {
    add(changeset: IChangeSet);
    get(): IChangeSet[];
    reset();
}

export class ChangeSetCollector implements IChangeCollector {
    private _collection: IChangeSet[];

    constructor(private item: IIStatefulItem) {
        this.reset();
    }

    public get collection(): IChangeSet[] {
        return this._collection || (this._collection = []);

    }

    public add(changeset: IChangeSet) {                 
        let found = this.collection.filter((it: IChangeSet) => {
            return it.key === changeset.key && changeset.type === ChangeTypeEnum.Update && it.type === ChangeTypeEnum.Update;
        })[0];

        if (found) {
            found.value = changeset.value;
        } else {
            this.collection.push(changeset);
        }
        this.item.artifactState.dirty = true;
    }


    public reset() {
        this._collection = [];
    }

    public get(): IChangeSet[] {
        // filter out initials. process add/update/delete on individual model level (attachments, docrefs, properties, etc).
        let changes = this.collection.filter((changeSet) => !!changeSet);
        return changes;
    }
}