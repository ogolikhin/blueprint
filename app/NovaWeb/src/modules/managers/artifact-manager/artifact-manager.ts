import { IMessageService } from "../../core/";
import { ISelectionManager,  ISelection,  SelectionSource } from "../selection-manager/selection-manager";

import {
    IStatefulArtifact, 
    // ISession, 
    // IStatefulArtifactServices, 
    // IArtifactAttachmentsService,
    // IArtifactService
 } from "../models";

export { ISelectionManager, ISelection,  SelectionSource }

export interface IArtifactManager {
    list(): IStatefulArtifact[];
    add(artifact: IStatefulArtifact);
    get(id: number): IStatefulArtifact;
    remove(id: number): IStatefulArtifact;
    messages: IMessageService;
    selection: ISelectionManager;
}

export class ArtifactManager  implements IArtifactManager {

    public static $inject = [
        "messageService",
        "selectionManager2"
    ];

    private artifactList: IStatefulArtifact[];

    constructor(private messageService: IMessageService, private selectionService: ISelectionManager) {
        this.artifactList = [];
    }

    public get messages(): IMessageService {
        return this.messageService;
    }
    public get selection(): ISelectionManager {
        return this.selectionService;
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
