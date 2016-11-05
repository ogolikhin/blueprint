import {
    IProcessShape,
    ItemTypePredefined,
    IHashMapOfPropertyValues,
    IArtifactReference
} from "./models/process-models";

import { IStatefulSubArtifact, StatefulSubArtifact } from "../../managers/artifact-manager/sub-artifact";
import { IStatefulArtifact } from "../../managers/artifact-manager/artifact";
import { IStatefulArtifactServices } from "../../managers/artifact-manager/services";
import { Helper } from "../../shared/utils/helper";


export interface IStatefulProcessSubArtifact extends IStatefulSubArtifact {
    loadProperties(): ng.IPromise<IStatefulSubArtifact>;
}
export class StatefulProcessSubArtifact extends StatefulSubArtifact  implements IStatefulProcessSubArtifact, IProcessShape {
    
    public propertyValues: IHashMapOfPropertyValues;
    public associatedArtifact: IArtifactReference;
    public baseItemTypePredefined: ItemTypePredefined;
    public typePrefix: string;

    constructor(artifact: IStatefulArtifact, subartifact: IProcessShape, services: IStatefulArtifactServices) {
        super(artifact, subartifact, services);

        this.propertyValues = subartifact.propertyValues;
        this.associatedArtifact = subartifact.associatedArtifact;
        this.baseItemTypePredefined = subartifact.baseItemTypePredefined;
        this.typePrefix = subartifact.typePrefix;
    }    

    public get prefix(): string {
        return this.typePrefix;
    }

    public get predefinedType(): ItemTypePredefined {
        return this.baseItemTypePredefined;
    }

    public loadProperties(): ng.IPromise<IStatefulSubArtifact> {      
        const deferred = this.services.getDeferred<IStatefulSubArtifact>();  
        if (!this.isFullArtifactLoadedOrLoading()) {
            this.loadPromise = this.load();
            this.loadPromise.then(() => {
                deferred.resolve(this);
            }).catch((error) => {
                this.error.onNext(error);
                deferred.reject(error);
            }).finally(() => {
                this.loadPromise = null;
            });
        } else {
            if (this.loadPromise) {
                return this.loadPromise;
            } else {
                deferred.resolve(this);
            }
        }
        return deferred.promise;
    }

    protected isFullArtifactLoadedOrLoading(): boolean {
        // If process shape has never been saved/published, then don't load.
        return super.isFullArtifactLoadedOrLoading() || !Helper.hasArtifactEverBeenSavedOrPublished(this);
    }
}
