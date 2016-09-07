import {IChangeSet, IChangeCollector, ChangeTypeEnum} from "../../models";

export class ChangeSetCollector implements IChangeCollector {
    private _collection: IChangeSet[];

    constructor() {
        this.reset();
    }

    public get collection(): IChangeSet[] {
        return this._collection || (this._collection = []);

    }

    public add(changeset: IChangeSet, oldValue?: any) {                 
        let init = this.collection.filter((it: IChangeSet) => 
            it.key === changeset.key && changeset.type === ChangeTypeEnum.Initial
        )[0];
        if (!init) {
            this.collection.push({
               type: ChangeTypeEnum.Initial,
               key: name,
               value: oldValue              
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
    }


    public reset(): IChangeSet[] {
        let initValues = this.collection.filter((it: IChangeSet) => 
            it.type === ChangeTypeEnum.Initial
        );
        
        this._collection = [];
        return initValues;
    }

}