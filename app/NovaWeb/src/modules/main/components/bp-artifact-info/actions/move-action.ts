import {BPButtonAction, IDialogSettings, IDialogService, BPDropdownAction, BPDropdownItemAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager, IArtifactManager} from "../../../../managers";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {
    MoveArtifactPickerDialogController, 
    MoveArtifactResult, 
    MoveArtifactInsertMethod,
    IMoveArtifactPickerOptions
} from "../../../../main/components/dialogs/move-artifact/move-artifact";
import {Models, Enums} from "../../../../main/models";

export class MoveAction extends BPDropdownAction {
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
            (): boolean => true,
            "fonticon2-move",
            localization.get("App_Toolbar_Move"),
            undefined,
            new BPDropdownItemAction(
                localization.get("App_Toolbar_Move"),
                () => this.executeAction($q, artifact, localization, messageService, projectManager, dialogService),
                (): boolean => true,
            )
        );
    }

    private executeAction($q: ng.IQService, 
                artifact: IStatefulArtifact,
                localization: ILocalizationService,
                messageService: IMessageService,
                projectManager: IProjectManager,
                dialogService: IDialogService) {
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
                    let orderIndex: number = projectManager.calculateOrderIndex(insertMethod, result[0].artifacts[0]);

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
    }
}
