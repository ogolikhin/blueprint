import { IStatefulArtifact, StatefulArtifact } from "../../managers/artifact-manager/artifact";
import { IArtifact } from "../../main/models/models";
import { ItemTypePredefined } from "../../main/models/enums";

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
    itemTypePredefined: ItemTypePredefined;   
}

export interface IStatefulCollectionArtifact extends IStatefulArtifact {
    rapidReviewCreated: boolean;
    reviewName: string;      
    artifacts: ICollectionArtifact[]; 
}

export class StatefulCollectionArtifact extends StatefulArtifact implements IStatefulCollectionArtifact {

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<IArtifact> {
        const url = `/svc/bpartifactstore/collection/${id}`;
        return this.services.artifactService.getArtifactModel<ICollection>(url, id, versionId);
    }

    public get rapidReviewCreated() {
        if (this.artifact) {
            return (<ICollection>this.artifact).isCreated;
        }
        return false;
    }

    public get reviewName() {
        if (this.artifact) {
            return (<ICollection>this.artifact).reviewName;
        }
        return undefined;
    }

    public get artifacts() {
        if (this.artifact) {
            return (<ICollection>this.artifact).artifacts;
        }
        return undefined;
    }
}
