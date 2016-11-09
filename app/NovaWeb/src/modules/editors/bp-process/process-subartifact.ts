import {
    IProcessShape,
    ItemTypePredefined,
    IHashMapOfPropertyValues,
    IArtifactReference
} from "./models/process-models";

import { IStatefulSubArtifact, StatefulSubArtifact } from "../../managers/artifact-manager/sub-artifact";
import { IStatefulArtifact } from "../../managers/artifact-manager/artifact";
import { IStatefulArtifactServices } from "../../managers/artifact-manager/services";


export interface IStatefulProcessSubArtifact extends IStatefulSubArtifact {

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
}
