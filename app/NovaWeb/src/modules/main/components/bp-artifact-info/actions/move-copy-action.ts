import {IDialogSettings, IDialogService, BPDropdownAction, BPDropdownItemAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {
    MoveCopyArtifactPickerDialogController,
    MoveCopyArtifactResult,
    MoveCopyArtifactInsertMethod,
    IMoveCopyArtifactPickerOptions,
    MoveCopyActionType
} from "../../dialogs/move-copy-artifact/move-copy-artifact";
import {Models, Enums} from "../../../../main/models";
import {ItemTypePredefined} from "../../../models/enums";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {IMessageService} from "../../messages/message.svc";

export class MoveCopyAction extends BPDropdownAction {
    private actionType: MoveCopyActionType;

    constructor(private $q: ng.IQService,
                private $timeout: ng.ITimeoutService,
                private artifact: IStatefulArtifact,
                private localization: ILocalizationService,
                private messageService: IMessageService,
                private projectManager: IProjectManager,
                private dialogService: IDialogService,
                private navigationService: INavigationService,
                private loadingOverlayService: ILoadingOverlayService) {
        super();

        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!projectManager) {
            throw new Error("Project manager not provided or is null");
        }

        if (!dialogService) {
            throw new Error("Dialog service not provided or is null");
        }

        this.actions.push(
            new BPDropdownItemAction(
                this.localization.get("App_Toolbar_Move"),
                () => this.executeMove(),
                (): boolean => this.canExecuteMove(),
                "fonticon2-move"
            )
        );
        this.actions.push(
            new BPDropdownItemAction(
                this.localization.get("App_Toolbar_Copy"),
                () => this.executeCopy(),
                (): boolean => this.canExecuteCopy(),
                "fonticon2-copy"
            )
        );
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Move_Copy");
    }

    public get disabled(): boolean {
        return !this.canExecute();
    }

    private canExecuteMove(): boolean {
        return this.canExecute() && !this.artifact.artifactState.readonly;
    }

    private canExecuteCopy(): boolean {
        const invalidTypes = [
            ItemTypePredefined.BaselineFolder,
            ItemTypePredefined.ArtifactBaseline,
            ItemTypePredefined.ArtifactReviewPackage,
            ItemTypePredefined.CollectionFolder,
            ItemTypePredefined.ArtifactCollection
        ];

        return this.canExecute() && invalidTypes.indexOf(this.artifact.predefinedType) === -1;
    }

    private canExecute(): boolean {
        if (!this.artifact) {
            return false;
        }

        const invalidTypes = [
            ItemTypePredefined.Project,
            ItemTypePredefined.Collections,
            ItemTypePredefined.BaselinesAndReviews
        ];

        return !this.artifact.artifactState.historical && (invalidTypes.indexOf(this.artifact.itemTypeId) === -1);
    }

    public executeMove() {
        this.actionType = MoveCopyActionType.Move;
        this.loadProjectIfNeeded();
    }

    public executeCopy() {
        this.actionType = MoveCopyActionType.Copy;
        this.loadProjectIfNeeded();
    }

    private loadProjectIfNeeded() {
        //first, check if project is loaded, and if not - load it
        let loadProjectPromise: ng.IPromise<any>;
        if (!this.projectManager.getProject(this.artifact.projectId)) {
            loadProjectPromise = this.projectManager.add(this.artifact.projectId);
        } else {
            loadProjectPromise = this.$q.resolve();
        }

        loadProjectPromise
        .catch((err) => this.messageService.addError(err))
        .then(() => {
            this.openMoveCopyDialog();
        });
    }

    private openMoveCopyDialog(): ng.IPromise<void> {
        //next - open the move to dialog
        let okButtonLabel: string;
        let headerLabel: string;
        if (this.actionType === MoveCopyActionType.Move) {
            okButtonLabel = "App_Button_Move";
            headerLabel = "Move_Artifacts_Picker_Header";
        } else if (this.actionType === MoveCopyActionType.Copy) {
            okButtonLabel = "App_Button_Copy";
            headerLabel = "Copy_Artifacts_Picker_Header";
        }

        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get(okButtonLabel),
            template: require("../../../../main/components/dialogs/move-copy-artifact/move-copy-artifact-dialog.html"),
            controller: MoveCopyArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get(headerLabel)
        };

        const baselineAndReviewTypes = [
            ItemTypePredefined.BaselineFolder,
            ItemTypePredefined.ArtifactBaseline,
            ItemTypePredefined.ArtifactReviewPackage
        ];

        const collectionTypes = [
            ItemTypePredefined.CollectionFolder,
            ItemTypePredefined.ArtifactCollection
        ];

        const dialogData: IMoveCopyArtifactPickerOptions = {
            showProjects: false,
            showArtifacts: baselineAndReviewTypes.indexOf(this.artifact.predefinedType) === -1 &&
                collectionTypes.indexOf(this.artifact.predefinedType) === -1,
            showBaselinesAndReviews: baselineAndReviewTypes.indexOf(this.artifact.predefinedType) !== -1,
            showCollections: collectionTypes.indexOf(this.artifact.predefinedType) !== -1,
            selectionMode: "single",
            currentArtifact: this.artifact,
            actionType: this.actionType,
            selectableItemTypes: baselineAndReviewTypes.indexOf(this.artifact.predefinedType) !== -1 ? baselineAndReviewTypes :
                collectionTypes.indexOf(this.artifact.predefinedType) !== -1 ? collectionTypes : undefined
        };

        return this.dialogService.open(dialogSettings, dialogData).then((result: MoveCopyArtifactResult[]) => {
            if (result && result.length === 1) {
                return this.computeNewOrderIndex(result[0])
                .then((orderIndex: number) => {
                    if (this.actionType === MoveCopyActionType.Move) {
                        return this.prepareArtifactForMove(result[0].artifacts[0]).then(() => {
                            return this.moveArtifact(result[0].insertMethod, result[0].artifacts[0], orderIndex);
                        });
                    } else if (this.actionType === MoveCopyActionType.Copy) {
                        return this.copyArtifact(result[0].insertMethod, result[0].artifacts[0], orderIndex);
                    } else {
                        return this.$q.reject("unknown action");  //to prevent mistakes when adding more actions
                    }
                })
                .catch((err) => this.messageService.addError(err));
            }
        });
    }

    private computeNewOrderIndex(result: MoveCopyArtifactResult): ng.IPromise<number> {
        //next - compute new order index
        const artifacts: Models.IArtifact[] = result.artifacts;
        if (artifacts && artifacts.length === 1) {
            let insertMethod: MoveCopyArtifactInsertMethod = result.insertMethod;
            return this.projectManager.calculateOrderIndex(insertMethod, result.artifacts[0]);
        } else {
            return this.$q.reject("No artifact provided");
        }
    }

    private prepareArtifactForMove(artifact: Models.IArtifact): ng.IPromise<IStatefulArtifact>  {
        //lock and presave if needed
        if (!this.artifact.artifactState.dirty) {
            //lock
            return this.artifact.lock();
        }
        if (this.artifact.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
            //save
            return this.artifact.save();
        }
        //do nothing
        return this.$q.resolve(null);
    }

    private moveArtifact(insertMethod: MoveCopyArtifactInsertMethod, artifact: Models.IArtifact, orderIndex: number): ng.IPromise<void>  {
        //finally, move the artifact
        return this.artifact
        .move(insertMethod === MoveCopyArtifactInsertMethod.Inside ? artifact.id : artifact.parentId, orderIndex)
        .then(() => {
            //refresh project
            const refreshLoadingOverlayId = this.loadingOverlayService.beginLoading();
            this.projectManager.refresh(this.artifact.projectId).then(() => {
                this.projectManager.triggerProjectCollectionRefresh();
            }).finally(() => {
                this.loadingOverlayService.endLoading(refreshLoadingOverlayId);
            });
        });
    }

    private copyArtifact(insertMethod: MoveCopyArtifactInsertMethod, artifact: Models.IArtifact, orderIndex: number): ng.IPromise<void>  {
        //finally, move the artifact
        return this.artifact
        .copy(insertMethod === MoveCopyArtifactInsertMethod.Inside ? artifact.id : artifact.parentId, orderIndex)
        .then((result: Models.ICopyResultSet) => {
            let selectionId = result && result.artifact ? result.artifact.id : null;
            //refresh project
            const refreshLoadingOverlayId = this.loadingOverlayService.beginLoading();
            this.projectManager.refresh(this.artifact.projectId, selectionId).then(() => {
                this.projectManager.triggerProjectCollectionRefresh();
                if (selectionId) {
                    this.$timeout(() => {
                        this.navigationService.navigateTo({id: selectionId});
                    });
                }
            }).finally(() => {
                this.loadingOverlayService.endLoading(refreshLoadingOverlayId);
            });
        });
    }
}
