import {IIStatefulItem} from "../item";
import {Models} from "../../../main/models";

export interface IMetaData {
    getItemType(): Models.IItemType;

    getArtifactPropertyTypes(): Models.IPropertyType[];
    getSubArtifactPropertyTypes(): Models.IPropertyType[];

    getArtifactPropertyType(propertyTypeId?: number): Models.IPropertyType;
    getSubArtifactPropertyType(propertyTypeId?: number): Models.IPropertyType;
}

export class MetaData implements IMetaData {

    constructor(private item: IIStatefulItem) {
    }

    public getItemType(): Models.IItemType {
        return this.item.getServices().metaDataService.getArtifactItemType(this.item.projectId, this.item.itemTypeId);
    }


    public getArtifactPropertyTypes(): Models.IPropertyType[] {
        return this.item.getServices().metaDataService.getArtifactPropertyTypes(this.item.projectId, this.item.itemTypeId);
    }

    public getSubArtifactPropertyTypes(): Models.IPropertyType[] {
        return this.item.getServices().metaDataService.getSubArtifactPropertyTypes(this.item.projectId, this.item.itemTypeId);

    }

    public getArtifactPropertyType(propertyTypeId?: number): Models.IPropertyType[] {
        return this.getArtifactPropertyTypes().filter((it: Models.IPropertyType) => it.id === propertyTypeId);
    }

    public getSubArtifactPropertyType(propertyTypeId?: number): Models.IPropertyType[] {
        return this.getSubArtifactPropertyTypes().filter((it: Models.IPropertyType) => it.id === propertyTypeId);
    }


}
