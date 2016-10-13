import {
    IProcessShape,
    ItemTypePredefined,
    IHashMapOfPropertyValues,
    IArtifactReference
} from "./models/process-models";

import { IStatefulSubArtifact, StatefulSubArtifact } from "../../managers/artifact-manager/sub-artifact";
import { IStatefulArtifact } from "../../managers/artifact-manager/artifact";
import { IStatefulArtifactServices } from "../../managers/artifact-manager/services";
import { ChangeTypeEnum, IChangeSet } from "../../managers/artifact-manager/changeset";
import { IStatefulProcessItem } from "./process-artifact";


export interface IStatefulProcessSubArtifact extends IStatefulProcessItem, IStatefulSubArtifact {

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
    
    public addChangeset(name: string, value: any) {
        const changeset = {
            type: ChangeTypeEnum.Update,
            key: name,
            value: value              
        } as IChangeSet;
        this.changesets.add(changeset);
        
        this.lock(); 
    }

    public get prefix(): string {
        return this.typePrefix;
    }

    public get predefinedType(): ItemTypePredefined {
        return this.baseItemTypePredefined;
    }
}
