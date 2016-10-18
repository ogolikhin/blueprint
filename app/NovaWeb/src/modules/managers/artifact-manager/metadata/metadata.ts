import {IIStatefulItem} from "../item";
import {Models} from "../../../main/models";

export interface IMetaData {
    getItemType(): ng.IPromise<Models.IItemType>;
    getItemTypeTemp(): Models.IItemType;

    getArtifactPropertyTypes(): ng.IPromise<Models.IPropertyType[]>;
    getSubArtifactPropertyTypes(): ng.IPromise<Models.IPropertyType[]>;
}

export class MetaData implements IMetaData {

    constructor(private item: IIStatefulItem) {
    }

    public getItemType(): ng.IPromise<Models.IItemType> {
        return this.item.getServices().metaDataService.getArtifactItemType(this.item.projectId, this.item.itemTypeId);
    }

    public getItemTypeTemp(): Models.IItemType {
        return this.item.getServices().metaDataService.getArtifactItemTypeTemp(this.item.projectId, this.item.itemTypeId);
    }

    public getArtifactPropertyTypes(): ng.IPromise<Models.IPropertyType[]> {
        return this.item.getServices().metaDataService.getArtifactPropertyTypes(this.item.projectId, this.item.itemTypeId);
    }

    public getSubArtifactPropertyTypes(): ng.IPromise<Models.IPropertyType[]> {
        return this.item.getServices().metaDataService.getSubArtifactPropertyTypes(this.item.projectId, this.item.itemTypeId);
    }

    // public getArtifactPropertyType(propertyTypeId?: number): Models.IPropertyType[] {
    //     return this.getArtifactPropertyTypes().filter((it: Models.IPropertyType) => it.id === propertyTypeId);
    // }

    // public getSubArtifactPropertyType(propertyTypeId?: number): Models.IPropertyType[] {
    //     return this.getSubArtifactPropertyTypes().filter((it: Models.IPropertyType) => it.id === propertyTypeId);
    // }


}
