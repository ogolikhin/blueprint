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
    public personaReference: IArtifactReference;
    public baseItemTypePredefined: ItemTypePredefined;
    public typePrefix: string;

    constructor(artifact: IStatefulArtifact, subartifact: IProcessShape, services: IStatefulArtifactServices) {
        super(artifact, subartifact, services);

        this.propertyValues = subartifact.propertyValues;
        this.personaReference = subartifact.personaReference;
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
        if (!this.isFullArtifactLoadedOrLoading()) {
            return this.loadWithNotify();      
        }
        if (this.loadPromise) {
            return this.loadPromise;
        }
        return this.services.$q.when(this);
    }

    protected isFullArtifactLoadedOrLoading(): boolean {
        // If process shape has never been saved/published, then don't load.
        return super.isFullArtifactLoadedOrLoading() || !Helper.hasArtifactEverBeenSavedOrPublished(this);
    }
}
