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
    get(): IChangeSet[];
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
               key: changeset.key,
               value: initValue              
           } as IChangeSet);
        } 
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


    public reset(): IChangeSet[] {
        let initValues = this.collection.filter((it: IChangeSet) => 
            it.type === ChangeTypeEnum.Initial
        );
        
        this._collection = [];
        return initValues;
    }

    public get(): IChangeSet[] {
        //filter out "UPDATES"
        let changes = this.collection.filter((it: IChangeSet) => 
            it.type === ChangeTypeEnum.Update
        );
        //combine all "ADD"
        let changeset = {
               type: ChangeTypeEnum.Add,
               key: ChangeTypeEnum[ChangeTypeEnum.Add],
               value: this.collection.map((it: IChangeSet) => it.type === ChangeTypeEnum.Add ? it : null).filter((it) => !!it)
        };
        if (changeset.value.length) {
            changes.push(changeset);    
        }
        //combine all "DELETE"
        changeset = {
               type: ChangeTypeEnum.Delete,
               key: ChangeTypeEnum[ChangeTypeEnum.Delete],
               value: this.collection.map((it: IChangeSet) => it.type === ChangeTypeEnum.Delete ? it : null).filter((it) => !!it)
        };
        if (changeset.value.length) {
            changes.push(changeset);    
        }
        
        return changes;
    }



    // private apply(item: IIStatefulItem){
    //         let propertyTypeId: number;
    //         let propertyValue: Models.IPropertyValue;

    //         this.collection.forEach((it: IChangeSet) => {

    //         });
    //         switch (changeSet.lookup) {
    //             case Enums.PropertyLookupEnum.System:
    //                 if (changeSet.id in this.originItem) {
    //                     item[changeSet.id] = changeSet.value;
    //                 }
    //                 break;
    //             case Enums.PropertyLookupEnum.Custom:
    //                 propertyTypeId = changeSet.id as number;
    //                 propertyValue = (this._changedItem.customPropertyValues || []).filter((it: Models.IPropertyValue) => {
    //                     return it.propertyTypeId === propertyTypeId;
    //                 })[0];
    //                 if (propertyValue) {
    //                     item.customPropertyValues.push(propertyValue);
    //                 }
    //                 break;
    //             case Enums.PropertyLookupEnum.Special:
    //                 propertyTypeId = changeSet.id as number;
    //                 propertyValue = (this._changedItem.specificPropertyValues || []).filter((it: Models.IPropertyValue) => {
    //                     return it.propertyTypeId === propertyTypeId;
    //                 })[0];
    //                 if (propertyValue) {
    //                     item.customPropertyValues.push(propertyValue);
    //                 }
    //                 break;
    //         }
    //         return item;
    //     }

}