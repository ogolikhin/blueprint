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
//import {INavigationService, INavigationParams} from "../../../../core/navigation/navigation.svc";
import {ISelectionManager} from "../../../../managers/selection-manager";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";

enum Action {
    Move, Copy
}

export class MoveCopyAction extends BPDropdownAction {
    private action: Action;

    constructor(private $q: ng.IQService, 
                private artifact: IStatefulArtifact,
                private localization: ILocalizationService,
                private messageService: IMessageService,
                private projectManager: IProjectManager,
                private dialogService: IDialogService,
                private selectionManager: ISelectionManager,
                private artifactManager: IArtifactManager,
                private loadingOverlayService: ILoadingOverlayService) {

        super(undefined, undefined, undefined, undefined,
            new BPDropdownItemAction(
                localization.get("App_Toolbar_Move"),
                () => this.executeMove(),
                (): boolean => true,
            ),
            new BPDropdownItemAction(
                localization.get("App_Toolbar_Copy"),
                () => this.executeCopy(),
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
        return this.localization.get("App_Toolbar_Move_Copy");
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

    private executeMove() {
        this.action = Action.Move;
        this.checkProjectLoaded();
    }

    private executeCopy() {
        this.action = Action.Copy;
        this.checkProjectLoaded();
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
        .catch((err) => this.messageService.addError(err))
        .then(() => {
            this.openMoveDialog();
        });
    }

    private openMoveDialog(): ng.IPromise<void> {
        //next - open the move to dialog
        const okButtonLabel = this.action === Action.Move ? "App_Button_Move" : (this.action === Action.Copy ? "App_Button_Copy" : undefined);
        const headerLabel = this.action === Action.Move ? "Move_Artifacts_Picker_Header" : 
            (this.action === Action.Copy ? "Copy_Artifacts_Picker_Header" : undefined);
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get(okButtonLabel),
            template: require("../../../../main/components/dialogs/move-artifact/move-artifact-dialog.html"),
            controller: MoveArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get(headerLabel)
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
                if (this.action === Action.Move) {
                    return this.prepareArtifactForMove(insertMethod, artifacts[0], orderIndex);
                } else if (this.action === Action.Copy) {
                    return this.copyArtifact(insertMethod, artifacts[0], orderIndex);
                } else {
                    return this.$q.reject("unknown action");  //to prevent mistakes when adding more actions
                }
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

    private copyArtifact(insertMethod: MoveArtifactInsertMethod, artifact: Models.IArtifact, orderIndex: number): ng.IPromise<void>  {
        //finally, move the artifact
        return this.artifact
        .copy(insertMethod === MoveArtifactInsertMethod.Inside ? artifact.id : artifact.parentId, orderIndex)
        .then((result: Models.ICopyResultSet) => {
            let selectionId = result && result.artifact ? result.artifact.id : null;
            //refresh project
            let refreshLoadingOverlayId = this.loadingOverlayService.beginLoading();
            this.projectManager.refresh(this.artifact.projectId, selectionId).then(() => {
                this.projectManager.triggerProjectCollectionRefresh();
                if (selectionId) {
                    this.selectionManager.setExplorerArtifact(this.artifactManager.get(selectionId));
                }
            }).finally(() => {
                this.loadingOverlayService.endLoading(refreshLoadingOverlayId);
            });
        });
    }
}
