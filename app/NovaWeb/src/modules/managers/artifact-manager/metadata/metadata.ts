import { Models } from "../../../main/models";
import { 
    IIStatefulItem
} from "../../models";

export interface IMetaData {

    getItemType(id: number, versionId?: number):ng.IPromise<Models.IItemType>;

}

export class MetaData implements IMetaData {

    constructor(private item: IIStatefulItem ) {
    }
        

    public getItemType(id: number, versionId?: number): ng.IPromise<Models.IItemType> {
        let deferred = this.item.getServices().getDeferred<Models.IItemType>();

        deferred.resolve(this.item.getServices().projectManager.getArtifactItemType(id));
        return deferred.promise;
    }
}

