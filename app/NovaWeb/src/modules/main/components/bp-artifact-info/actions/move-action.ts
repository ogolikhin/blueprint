import {BPButtonAction, IDialogSettings, IDialogService} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {
    MoveArtifactPickerDialogController, 
    MoveArtifactResult, 
    MoveArtifactInsertMethod,
    IMoveArtifactPickerOptions
} from "../../../../main/components/dialogs/move-artifact/move-artifact";
import {Models, Enums} from "../../../../main/models";

export class MoveAction extends BPButtonAction {
    constructor($q: ng.IQService, 
                artifact: IStatefulArtifact,
                localization: ILocalizationService,
                messageService: IMessageService,
                projectManager: IProjectManager,
                dialogService: IDialogService) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }
        super(
            (): void => {
                const dialogSettings = <IDialogSettings>{
                    okButton: localization.get("App_Button_Move"),
                    template: require("../../../../main/components/dialogs/move-artifact/move-artifact-dialog.html"),
                    controller: MoveArtifactPickerDialogController,
                    css: "nova-open-project",
                    header: localization.get("Move_Artifacts_Picker_Header")
                };

                const dialogData: IMoveArtifactPickerOptions = {
                    showSubArtifacts: false,
                    selectionMode: "single",
                    isOneProjectLevel: true,
                    currentArtifact: artifact 
                };

                dialogService.open(dialogSettings, dialogData).then((result: MoveArtifactResult[]) => {
                    if (result && result.length === 1) {
                        const artifacts: Models.IArtifact[] = result[0].artifacts;
                        if (artifacts && artifacts.length === 1) {
                            let insertMethod: MoveArtifactInsertMethod = result[0].insertMethod;
                            let orderIndex: number = this.calculateOrderIndex(insertMethod, result[0].artifacts[0], projectManager);

                            let lockSavePromise: ng.IPromise<any>;

                            if (!artifact.artifactState.dirty) {
                                //lock
                                lockSavePromise = artifact.lock();
                                if (!lockSavePromise) {
                                    lockSavePromise = $q.resolve();
                                }
                            } else if (artifact.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
                                //save
                                lockSavePromise = artifact.save();
                            } else {
                                //do nothing
                                lockSavePromise = $q.resolve();
                            }

                            lockSavePromise.then(() => {
                                artifact
                                .move(insertMethod === MoveArtifactInsertMethod.Selection ? artifacts[0].id : artifacts[0].parentId, orderIndex)
                                .then(() => {
                                    projectManager.refresh(artifact.projectId).then(() => {
                                        projectManager.triggerProjectCollectionRefresh();
                                    });
                                })
                                .catch((err) => {
                                    messageService.addError(err);
                                });
                            });
                        }
                    }
                });                
                
            },
            (): boolean => true,
            "fonticon2-move",
            localization.get("App_Toolbar_Discard")
        );
    }

    private calculateOrderIndex(insertMethod: MoveArtifactInsertMethod, selectedArtifact: Models.IArtifact, projectManager: IProjectManager) {
        let orderIndex: number;
        let siblings = _.sortBy(projectManager.getArtifactNode(selectedArtifact.parentId).children, (a) => a.model.orderIndex); 
        let index = siblings.findIndex((a) => a.model.id === selectedArtifact.id);
        
        if (index === 1 && insertMethod === MoveArtifactInsertMethod.Above) {  //first, because of collections
            orderIndex = selectedArtifact.orderIndex / 2;
        } else if (index === siblings.length - 1 && insertMethod === MoveArtifactInsertMethod.Below) { //last
            orderIndex = selectedArtifact.orderIndex + 10;
        } else {    //in between
            if (insertMethod === MoveArtifactInsertMethod.Above) {
                orderIndex = (siblings[index - 1].model.orderIndex + selectedArtifact.orderIndex) / 2;
            } else if (insertMethod === MoveArtifactInsertMethod.Below) {
                orderIndex = (siblings[index + 1].model.orderIndex + selectedArtifact.orderIndex) / 2;
            } else {
                //leave undefined
            }
        }
        return orderIndex;
    }
}
