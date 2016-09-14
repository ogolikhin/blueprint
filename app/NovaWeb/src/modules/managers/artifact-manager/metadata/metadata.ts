import { Models } from "../../../main/models";
import { 
    IIStatefulItem
} from "../../models";

export interface IMetaData {
    getItemType(itemTypeId: number, versionId?: number): ng.IPromise<Models.IItemType>;
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
}
