import { Models, Enums } from "../../../main/models";
import { ArtifactState} from "../state";
import { ArtifactAttachments } from "../attachments";
import { CustomProperties } from "../properties";
import { ChangeSetCollector } from "../changeset";
import { ChangeTypeEnum, IChangeCollector, IChangeSet  } from "../../models";

import { IStatefulArtifact, 
         IArtifactStates, 
         IArtifactProperties, 
         IArtifactAttachments, 
         IArtifactManager, 
         IState,
         IStatefulArtifactServices,
         IIStatefulArtifact,
         ISubArtifactCollection,
         IStatefulSubArtifact,
         IArtifactAttachmentsResultSet
} from "../../models";


export class StatefulSubArtifactCollection implements ISubArtifactCollection {

    private artifact: IIStatefulArtifact;
    private subArtifactList: IStatefulSubArtifact[];
    private services: IStatefulArtifactServices;

    constructor(artifact: IIStatefulArtifact, services: IStatefulArtifactServices) {
        this.artifact = artifact;
        this.services = services;
        this.subArtifactList = [];
    }

    public list(): IStatefulSubArtifact[] {
        return this.subArtifactList;
    }

    public get(id: number): IStatefulSubArtifact {
        return this.subArtifactList.filter((subArtifact: IStatefulSubArtifact) => subArtifact.id === id)[0] || null;
    }
    
    public add(subArtifact: IStatefulSubArtifact): IStatefulSubArtifact {
        // TODO: fix initialization
        // subArtifact.initialize(this, this.services);
        const length = this.subArtifactList.push(subArtifact);
        return this.subArtifactList[length - 1];
    }

    public remove(id: number): IStatefulSubArtifact {
        let stateArtifact: IStatefulArtifact;
        this.subArtifactList = this.subArtifactList.filter((artifact: IStatefulArtifact) => {
            if (artifact.id === id) {
                stateArtifact = artifact;
                return false;
            }
            return true;
        });
        return stateArtifact;
    }

    public update(id: number) {
        // TODO: 
    }

}
