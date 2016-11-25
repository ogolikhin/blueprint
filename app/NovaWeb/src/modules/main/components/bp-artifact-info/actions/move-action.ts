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
import {Models, Enums, AdminStoreModels} from "../../../../main/models";
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
                (() => {
                    if (localization) {
                        return localization.get("App_Toolbar_Move");
                    } else {
                        throw new Error("Localization service not provided or is null");
                    }
                })(),
                () => this.execute(),
                (): boolean => this.canExecute(),
            )
        );
        
        if (!projectManager) {
            throw new Error("App_Error_No_Project_Manager");
        }

        if (!dialogService) {
            throw new Error("App_Error_No_Dialog_Service");
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

    public get execute(): () => void  {
        return this.checkProjectLoaded;
    }

    private canExecute(): boolean {
        if (!this.artifact) {
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

    private checkProjectLoaded() {
        //first, check if project is loaded, and if not - load it
        let loadProjectPromise: ng.IPromise<any>;
        if (!this.projectManager.getProject(this.artifact.projectId)) {
            loadProjectPromise = this.projectManager.load(this.artifact.projectId);
        } else {
            loadProjectPromise = this.$q.resolve();
        }

        loadProjectPromise
        .then(() => {
            this.openMoveDialog();
        }).catch((err) => {
            this.messageService.addError(err);
        });
    }

    private openMoveDialog(): ng.IPromise<void> {
        //next - open the move to dialog
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

        return this.dialogService.open(dialogSettings, dialogData).then((result: MoveArtifactResult[]) => {
            if (result && result.length === 1) {
                return this.computeNewOrderIndex(result[0]).catch((err) => this.messageService.addError(err));
            }
        }); 
    }

    private computeNewOrderIndex(result: MoveArtifactResult): ng.IPromise<void> {
        //next - compute new order index
        const artifacts: Models.IArtifact[] = result.artifacts;
        if (artifacts && artifacts.length === 1) {
            let insertMethod: MoveArtifactInsertMethod = result.insertMethod;
            return this.projectManager.calculateOrderIndex(insertMethod, result.artifacts[0]).then((orderIndex: number) => {
                return this.prepareArtifactForMove(insertMethod, artifacts[0], orderIndex);
            });
        }
    }

    private prepareArtifactForMove(insertMethod: MoveArtifactInsertMethod, artifact: Models.IArtifact, orderIndex: number): ng.IPromise<void>  {
        //lock and presave if needed
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

        return lockSavePromise.then(() => {
            return this.moveArtifact(insertMethod, artifact, orderIndex);
        });
    }

    private moveArtifact(insertMethod: MoveArtifactInsertMethod, artifact: Models.IArtifact, orderIndex: number): ng.IPromise<void>  {
        //finally, move the artifact
        return this.artifact
        .move(insertMethod === MoveArtifactInsertMethod.Inside ? artifact.id : artifact.parentId, orderIndex)
        .then(() => {
            //refresh project
            this.projectManager.refresh(this.artifact.projectId).then(() => {
                this.projectManager.triggerProjectCollectionRefresh();
            });
        });
    }
}
