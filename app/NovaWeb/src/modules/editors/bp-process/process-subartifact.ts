import {
     IProcessShape,
     ItemTypePredefined, 
     IHashMapOfPropertyValues, 
     IArtifactReference
} from "./models/process-models";

import { StatefulSubArtifact } from "../../managers/artifact-manager/sub-artifact";
import { IStatefulArtifact } from "../../managers/artifact-manager/artifact";
import { IStatefulArtifactServices } from "../../managers/artifact-manager/services";

export class StatefulProcessSubArtifact extends StatefulSubArtifact  implements IProcessShape {
    
    public propertyValues: IHashMapOfPropertyValues;
    public associatedArtifact: IArtifactReference;

    constructor(artifact: IStatefulArtifact, subartifact: IProcessShape, services: IStatefulArtifactServices) {
        super(artifact, subartifact, services);
                
        this.propertyValues = subartifact.propertyValues;
        this.associatedArtifact = subartifact.associatedArtifact;
    }
    public get typePrefix(): string{
        return this.prefix;
    }    
    public get baseItemTypePredefined(): ItemTypePredefined{
        return this.predefinedType;
    }
}