import { Models } from "../../../main/models";
import { 
    IIStatefulItem
} from "../../models";

export interface IMetaData {
    getItemType(): Models.IItemType;
    
    getArtifactPropertyType(propertyTypeId?: number): Models.IPropertyType;
    getArtifactPropertyTypes(): Models.IPropertyType[];
    getSubArtifactPropertyTypes(propertyTypeId?: number): Models.IPropertyType[];
}

export class MetaData implements IMetaData {

    constructor(private item: IIStatefulItem ) {
        if (item.projectId) {
            this.item.getServices().metaDataService.add(item.projectId);
        }
    }
        
    public getItemType(): Models.IItemType {
        return this.item.getServices().metaDataService.getArtifactItemType(this.item.projectId, this.item.itemTypeId);
    }

    public getArtifactPropertyType(propertyTypeId?: number): Models.IPropertyType[] {
        return this.getArtifactPropertyTypes().filter((it: Models.IPropertyType) => it.id === propertyTypeId);
    }

    public getArtifactPropertyTypes(propertyTypeId?: number): Models.IPropertyType[] {
        return  this.item.getServices().metaDataService.getArtifactPropertyTypes(this.item.projectId, this.item.itemTypeId);
    }
    
    public getSubArtifactPropertyTypes(subArtifactId: number): Models.IPropertyType[] {
        return this.item.getServices().metaDataService.getSubArtifactPropertyTypes( this.item.projectId, subArtifactId);

    }



}
