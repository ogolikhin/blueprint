import { Models } from "../../../main/models";
import { 
    IIStatefulItem
} from "../../models";

export interface IMetaData {
    getItemType(itemTypeId: number, versionId?: number): ng.IPromise<Models.IItemType>;
    getArtifactPropertyTypes(itemTypeId: number): Models.IPropertyType[];
    getSubArtifactPropertyTypes(itemTypeId: number): Models.IPropertyType[];
}

export class MetaData implements IMetaData {

    constructor(private item: IIStatefulItem ) {
        this.item.getServices().metaDataService.add(item.projectId);
    }
        
    public getItemType(itemTypeId: number, versionId?: number): ng.IPromise<Models.IItemType> {
        let deferred = this.item.getServices().getDeferred<Models.IItemType>();
        deferred.resolve(this.item.getServices().metaDataService.getArtifactItemType(this.item.projectId, itemTypeId));
        return deferred.promise;
    }

    public getArtifactPropertyTypes(itemTypeId: number): Models.IPropertyType[] {
        let properties: Models.IPropertyType[] = [];
//        let itemType: Models.IItemType = this.getArtifactType(_artifact, subArtifact, _project);
                
        
        // //create list of system properties
        // if (subArtifact) {
        //     properties = this.getSubArtifactSystemPropertyTypes(subArtifact);
        // } else {
        //     properties = this.getArtifactSystemPropertyTypes(_artifact, itemType, _project.meta);
        // }

        properties = this.item.getServices().metaDataService.getArtifactSystemPropertyTypes(itemTypeId, this.item.projectId);
        //add custom property types
        _project.meta.propertyTypes.forEach((it: Models.IPropertyType) => {
            if (itemType.customPropertyTypeIds.indexOf(it.id) >= 0) {
                properties.push(it);
            }
        });
        return properties;

    }



}
