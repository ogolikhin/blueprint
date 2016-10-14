import {
    IProcessShape,
    ItemTypePredefined,
    IHashMapOfPropertyValues,
    IArtifactReference
} from "./models/process-models";

import {StatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact";
import {IStatefulArtifact} from "../../managers/artifact-manager/artifact";
import {IStatefulArtifactServices} from "../../managers/artifact-manager/services";

export class StatefulProcessSubArtifact extends StatefulSubArtifact implements IProcessShape {

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
}
