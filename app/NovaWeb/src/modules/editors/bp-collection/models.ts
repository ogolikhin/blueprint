import {IArtifact} from "../../main/models/models";
import {Models} from "../../main";

export interface ICollection extends IArtifact {
    reviewName: string;
    isCreated: boolean;        
    artifacts: ICollectionArtifact[];    
}

export interface ICollectionArtifact {
    id: number;
    name: string;
    prefix: string;
    description: string;
    itemTypeId: number;    
    artifactPath: string[];
    itemTypePredefined: Models.ItemTypePredefined;   
}
