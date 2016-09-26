 import {IIStatefulItem} from "../../models";


export enum ChangeTypeEnum {
    Add = 0,
    Update = 1,
    Delete = 2,
    Initial = 3
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
        this.item.artifactState.set({dirty: true});
    }


    public reset(): IChangeSet[] {
        let initValues = this.collection.filter((it: IChangeSet) => 
            it.type === ChangeTypeEnum.Initial
        );
        
        this._collection = [];
        return initValues;
    }

    public get(): IChangeSet[] {
        // filter out initials. process add/update/delete on individual model level (attachments, docrefs, properties, etc).
        let changes = this.collection.map((changeSet: IChangeSet) => changeSet.type !== ChangeTypeEnum.Initial ? changeSet : null).filter((changeSet) => !!changeSet)
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