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
import {ItemTypePredefined} from "../../../../main/models/enums";

export class MoveAction extends BPDropdownAction {
    constructor(private $q: ng.IQService, 
                private artifact: IStatefulArtifact,
                private localization: ILocalizationService,
                private messageService: IMessageService,
                private projectManager: IProjectManager,
                private dialogService: IDialogService) {

        super(undefined, undefined, undefined, undefined,
            new BPDropdownItemAction(
                localization.get("App_Toolbar_Move"),
                () => this.execute(),
                (): boolean => true,
            )
        );
        
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!projectManager) {
            throw new Error("Project manager not provided or is null");
        }

        if (!dialogService) {
            throw new Error("Dialog service not provided or is null");
        }
    }

    
    public get icon(): string {
        return "fonticon2-move";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Move");
    }

    public get disabled(): boolean {
        return !this.canExecute();
    }

    public get execute()  {
        return this.moveArtifact;
    }

    private canExecute() {
        if (!this.artifact || !this.projectManager.getProject(this.artifact.projectId)) {
            return false;
        }

        const invalidTypes = [
            ItemTypePredefined.Project,
            ItemTypePredefined.Collections,
            ItemTypePredefined.CollectionFolder,
            ItemTypePredefined.ArtifactCollection
        ];

        if (invalidTypes.indexOf(this.artifact.predefinedType) >= 0) {
            return false;
        }

        if (this.artifact.artifactState.readonly) {
            return false;
        }

        return true;
    }

    private moveArtifact() {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Move"),
            template: require("../../../../main/components/dialogs/move-artifact/move-artifact-dialog.html"),
            controller: MoveArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get("Move_Artifacts_Picker_Header")
        };

        const dialogData: IMoveArtifactPickerOptions = {
            showSubArtifacts: false,
            selectionMode: "single",
            isOneProjectLevel: true,
            currentArtifact: this.artifact 
        };

        this.dialogService.open(dialogSettings, dialogData).then((result: MoveArtifactResult[]) => {
            if (result && result.length === 1) {
                const artifacts: Models.IArtifact[] = result[0].artifacts;
                if (artifacts && artifacts.length === 1) {
                    let insertMethod: MoveArtifactInsertMethod = result[0].insertMethod;
                    let orderIndex: number = this.projectManager.calculateOrderIndex(insertMethod, result[0].artifacts[0]);

                    let lockSavePromise: ng.IPromise<any>;

                    if (!this.artifact.artifactState.dirty) {
                        //lock
                        lockSavePromise = this.artifact.lock();
                        if (!lockSavePromise) {
                            lockSavePromise = this.$q.resolve();
                        }
                    } else if (this.artifact.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
                        //save
                        lockSavePromise = this.artifact.save();
                    } else {
                        //do nothing
                        lockSavePromise = this.$q.resolve();
                    }

                    lockSavePromise.then(() => {
                        this.artifact
                        .move(insertMethod === MoveArtifactInsertMethod.Selection ? artifacts[0].id : artifacts[0].parentId, orderIndex)
                        .then(() => {
                            this.projectManager.refresh(this.artifact.projectId).then(() => {
                                this.projectManager.triggerProjectCollectionRefresh();
                            });
                        })
                        .catch((err) => {
                            this.messageService.addError(err);
                        });
                    });
                }
            }
        }); 
    }
}
