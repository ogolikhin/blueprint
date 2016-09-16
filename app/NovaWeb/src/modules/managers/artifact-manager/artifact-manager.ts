import { ISelectionManager,  ISelection,  SelectionSource } from "../selection-manager/selection-manager";
import { IMetaDataService } from "./metadata";

import { IStatefulArtifact } from "../models";

export { ISelectionManager, ISelection,  SelectionSource }

export interface IArtifactManager {
    selection: ISelectionManager;
    list(): IStatefulArtifact[];
    add(artifact: IStatefulArtifact);
    get(id: number): IStatefulArtifact;
    remove(id: number): IStatefulArtifact;
    removeAll(projectId: number);

    save(): void;
    publish(): void;
    refresh(): void;
}

export class ArtifactManager  implements IArtifactManager {

    public static $inject = [ "selectionManager", "metadataService" ];

    private artifactList: IStatefulArtifact[];

    constructor(private selectionService: ISelectionManager, private metadataService: IMetaDataService) {
        this.artifactList = [];
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

    public removeAll(projectId) {
        
        this.artifactList = this.artifactList.filter((it: IStatefulArtifact) => {
            if (it.projectId !== projectId) {
                this.metadataService.remove(it.projectId);
                return false;
            }
            return true;
        });
        
    }

    //TODO: moved from bp-artifactinfo 
    // public saveChanges() {
    public save() {
        throw new Error("Not implemented yet");
    //     let overlayId: number = this.loadingOverlayService.beginLoading();
    //     try {
    //         let state: ItemState = this.stateManager.getState(this._artifactId);
    //         let artifactDelta: Models.IArtifact = state.generateArtifactDelta();
    //         this.artifactService.updateArtifact(artifactDelta)
    //             .then((artifact: Models.IArtifact) => {
    //                     let oldArtifact = state.getArtifact();
    //                     if (artifact.version) {
    //                         state.updateArtifactVersion(artifact.version);
    //                     }
    //                     if (artifact.lastSavedOn) {
    //                         state.updateArtifactSavedTime(artifact.lastSavedOn);
    //                     }
    //                     this.messageService.addMessage(new Message(MessageType.Info, this.localization.get("App_Save_Artifact_Error_200")));
    //                     state.finishSave();
    //                     this.isChanged = false;
    //                     this.projectManager.updateArtifactName(state.getArtifact());
    //                 }, (error) => {
    //                     let message: string;
    //                     if (error) {
    //                         if (error.statusCode === 400) {
    //                             if (error.errorCode === 114) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_400_114");
    //                             } else {
    //                                 message = this.localization.get("App_Save_Artifact_Error_400") + error.message;
    //                             }
    //                         } else if (error.statusCode === 404) {
    //                             message = this.localization.get("App_Save_Artifact_Error_404");
    //                         } else if (error.statusCode === 409) {
    //                             if (error.errorCode === 116) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_116");
    //                             } else if (error.errorCode === 117) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_117");
    //                             } else if (error.errorCode === 111) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_111");
    //                             } else if (error.errorCode === 115) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_115");
    //                             } else {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409");
    //                             }

    //                         } else {
    //                             message = this.localization.get("App_Save_Artifact_Error_Other") + error.statusCode;
    //                         }
    //                     }
    //                     this.messageService.addError(message);
    //                 }
    //             ).finally(() => this.loadingOverlayService.endLoading(overlayId));
    //     } catch (Error) {
    //         this.messageService.addError(this.localization.get(Error));
    //         this.loadingOverlayService.endLoading(overlayId);
    //     }
    
    }

    // TODO: 
    public publish() {
        throw new Error("Not implemented yet");
    }

    // TODO: 
    public refresh() {
        throw new Error("Not implemented yet");
    }

    
}
