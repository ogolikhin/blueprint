import { IMessageService } from "../../core/";
// import { Models } from "../../main/models";
// import { StatefulArtifactServices } from "./services";
import {
    IArtifactManager, 
    IStatefulArtifact, 
    // ISession, 
    // IStatefulArtifactServices, 
    // IArtifactAttachmentsService,
    // IArtifactService
 } from "../models";


export class ArtifactManager  implements IArtifactManager {

    public static $inject = [
        "messageService",
    ];

    private artifactList: IStatefulArtifact[];

    constructor(private messageService: IMessageService) {
        this.artifactList = [];
    }

    public get messages(): IMessageService {
        return this.messageService;
    }

    public list(): IStatefulArtifact[] {
        return this.artifactList;
    }

    public get(id: number): IStatefulArtifact {
        return this.artifactList.filter((artifact: IStatefulArtifact) => artifact.id === id)[0] || null;
    }
    
    public add(artifact: IStatefulArtifact) {
        this.artifactList.push(artifact);
    }

    public remove(id: number): IStatefulArtifact {
        let stateArtifact: IStatefulArtifact;
        this.artifactList = this.artifactList.filter((artifact: IStatefulArtifact) => {
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
