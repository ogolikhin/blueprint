import { Models } from "../../../main/models";
import { 
    IIStatefulItem
} from "../../models";

export interface IMetaData {
    getItemType(): Models.IItemType;
    getArtifactPropertyTypes(): Models.IPropertyType[];
    getSubArtifactPropertyTypes(subArtifactId: number): Models.IPropertyType[];
}

export class MetaData implements IMetaData {

    constructor(private item: IIStatefulItem ) {
        this.item.getServices().metaDataService.add(item.projectId);
    }
        
    public getItemType(): Models.IItemType {
        return this.item.getServices().metaDataService.getArtifactItemType(this.item.projectId, this.item.itemTypeId);
    }

    public getArtifactPropertyTypes(): Models.IPropertyType[] {
        return this.item.getServices().metaDataService.getArtifactPropertyTypes(this.item.projectId, this.item.itemTypeId);

    }
    public getSubArtifactPropertyTypes(subArtifactId: number): Models.IPropertyType[] {
        return this.item.getServices().metaDataService.getSubArtifactPropertyTypes( this.item.projectId, subArtifactId);

    }



}
