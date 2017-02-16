import {ILoadingOverlayService} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../../../main/components/bp-artifact-picker";
import {Message, MessageType} from "../../../../../main/components/messages/message";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {ICreateArtifactService} from "../../../../../main/components/projectControls/create-artifact.svc";
import {ItemTypePredefined} from "../../../../../main/models/itemTypePredefined.enum";
import {IArtifact, IItem} from "../../../../../main/models/models";
import {IArtifactService, IStatefulArtifact, IStatefulArtifactFactory} from "../../../../../managers/artifact-manager/artifact";
import {ISelectionManager} from "../../../../../managers/selection-manager/selection-manager";
import {IDialogService, IDialogSettings} from "../../../../../shared";
import {ApplicationError, IApplicationError} from "../../../../../shell/error/applicationError";
import {ErrorCode} from "../../../../../shell/error/error-code";
import {ISession} from "../../../../../shell/login/session.svc";
import {ArtifactReference, IArtifactReference} from "../../../models/process-models";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";

export abstract class TaskModalController<T extends IModalDialogModel> extends BaseModalDialogController<T> {
    public includeArtifactName: string;
    public isIncludeResultsVisible: boolean;
    public isIncludeError: boolean;
    private everPublished: boolean = false;

    public abstract nameOnFocus();
    public abstract nameOnBlur();
    public abstract getActiveHeader(): string;
    public abstract getPersonaLabel(): string;

    protected abstract getAssociatedArtifact(): IArtifactReference;
    protected abstract setAssociatedArtifact(value: IArtifactReference);
    protected abstract getPersonaReference(): IArtifactReference;
    protected abstract setPersonaReference(value: IArtifactReference);
    protected abstract getDefaultPersonaReference(): IArtifactReference;
    protected abstract populateTaskChanges();
    protected abstract getModel(): IArtifact;
    protected abstract getNewArtifactName(): string;
    protected abstract getItemTypeId(): number;

    public static $inject = [
        "$scope",
        "$rootScope",
        "$timeout",
        "dialogService",
        "$q",
        "localization",
        "createArtifactService",
        "statefulArtifactFactory",
        "messageService",
        "artifactService",
        "loadingOverlayService",
        "session",
        "selectionManager"
    ];

    constructor(
        $scope: IModalScope,
        $rootScope: ng.IRootScopeService,
        $timeout: ng.ITimeoutService,
        private dialogService: IDialogService,
        protected $q: ng.IQService,
        protected localization: ILocalizationService,
        protected createArtifactService: ICreateArtifactService,
        protected statefulArtifactFactory: IStatefulArtifactFactory,
        protected messageService: IMessageService,
        protected artifactService: IArtifactService,
        protected loadingOverlayService: ILoadingOverlayService,
        private session: ISession,
        private selectionManager: ISelectionManager,
        $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
        dialogModel?: T
    ) {
        super($rootScope, $scope, $timeout, $uibModalInstance, dialogModel);

        this.nameOnBlur();

        if (this.getAssociatedArtifact()) {
            this.prepIncludeField();
        }

        this.isIncludeError = false;

        if (!this.isReadonly) {
            let getArtifactLoadingId = this.loadingOverlayService.beginLoading();

            this.artifactService.getArtifact(this.dialogModel.artifactId)
                .then((artifact) => {
                    this.everPublished = artifact.version > 0;
                })
                .finally(() => {
                    this.loadingOverlayService.endLoading(getArtifactLoadingId);
                });
        } else {
            this.everPublished = true;
        }
    }

    public get isReadonly(): boolean {
        return !this.dialogModel || this.dialogModel.isReadonly || this.dialogModel.isHistoricalVersion;
    }

    public showCreateNewArtifact(): boolean {
        return this.everPublished && !!this.getNewArtifactName();
    }

    public prepIncludeField(): void {
        this.isIncludeResultsVisible = true;
        this.includeArtifactName = this.formatIncludeLabel(this.getAssociatedArtifact());
    }

    public cleanIncludeField(): void {
        if (this.canCleanField()) {
            this.isIncludeResultsVisible = false;
            this.setAssociatedArtifact(null);
            this.refreshView();
        }
    }

    public cleanPersonaField(): void {
        if (this.canCleanField()) {
            this.setPersonaReference(null);
            this.refreshView();
        }
    }

    public formatIncludeLabel(model: IArtifactReference) {
        if (!model) {
            return "";
        }

        if (!model.typePrefix) {
            return `${model.name}`;
        }

        if (model.typePrefix === "<Inaccessible>") {
           return this.localization.get("ST_Inaccessible_Include_Artifact_Label");
        }

        return `${model.typePrefix}${model.id} - ${model.name}`;
    }

    public applyChanges(): ng.IPromise<void> {
        const savingDataOverlayId = this.loadingOverlayService.beginLoading();

        if (this.isReadonly) {
            return this.$q.reject(new Error("Changes cannot be made or saved as this is a read-only item"));
        }

        return this.applyAssociatedArtifactChanges()
            .finally(() => {
                this.populateTaskChanges();
                this.loadingOverlayService.endLoading(savingDataOverlayId);
            });
    }

    private applyAssociatedArtifactChanges(): ng.IPromise<void> {
        const associatedArtifact = this.getAssociatedArtifact();

        if (!associatedArtifact) {
            this.setAssociatedArtifact(null);
            return this.$q.resolve();
        }

        if (associatedArtifact.id < 0) {
            let newStatefulArtifact: IStatefulArtifact;
            let isLockedByOtheruser = false;

            return this.artifactService.getArtifact(this.dialogModel.artifactId)
            .then((artifact: IArtifact) => {
                if (artifact.lockedByUser &&
                    artifact.lockedByUser.id !== this.session.currentUser.id) {
                    this.selectionManager.getArtifact().refresh();
                    isLockedByOtheruser = true;
                    return this.$q.reject(new ApplicationError(this.localization.get("Artifact_Lock_AlreadyLocked")));
                }

                return this.createArtifactService.createNewArtifact(this.dialogModel.artifactId, null, false, associatedArtifact.name, this.getItemTypeId());
            })
            .then((newArtifact: IArtifact) => {
                return this.statefulArtifactFactory.createStatefulArtifactFromId(newArtifact.id);
            })
            .then((statefulArtifact: IStatefulArtifact) => {
                newStatefulArtifact = statefulArtifact;
                return newStatefulArtifact.publish();
            })
            .then(() => {
                this.setInclude(newStatefulArtifact);
            })
            .catch((error: any) => {
                if (!isLockedByOtheruser) {
                    this.onNewArtifactCreationError(error, newStatefulArtifact);
                    this.setAssociatedArtifact(null);
                }
                this.$q.reject(error);
            });
        }

        return this.$q.resolve();
    }

    private getIncludedArtifactTypes(): ItemTypePredefined[] {
        const selectableArtifactTypes: ItemTypePredefined[] = [
            ItemTypePredefined.BusinessProcess,
            ItemTypePredefined.Document,
            ItemTypePredefined.DomainDiagram,
            ItemTypePredefined.PrimitiveFolder,
            ItemTypePredefined.GenericDiagram,
            ItemTypePredefined.Glossary,
            ItemTypePredefined.Process,
            ItemTypePredefined.Storyboard,
            ItemTypePredefined.TextualRequirement,
            ItemTypePredefined.UIMockup,
            ItemTypePredefined.UseCase,
            ItemTypePredefined.UseCaseDiagram
        ];

        return selectableArtifactTypes;
    }

    public openIncludePicker() {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get("ST_Select_Include_Artifact_Label")
        };

        const subArtifact = this.getModel();

        const dialogOption: IArtifactPickerOptions = {
            selectableItemTypes: this.getIncludedArtifactTypes(),
            isItemSelectable: (item: IArtifact) => {
                        return item.id !== subArtifact.parentId &&
                                item.id !== subArtifact.id &&
                                item.id > 0 &&
                                !item.lockedByUser;
                    }
        };

        this.dialogService.open(dialogSettings, dialogOption).then((items: IItem[]) => {
            if (items.length === 1) {
                this.setInclude(items[0]);
            }
        });
    }

    public createNewArtifact(useModal: boolean): void {
        const newArtifactReference = <IArtifact>{
            id: -1,
            name: this.getNewArtifactName()
        };
        this.setInclude(newArtifactReference);
    }

    public openActorPicker() {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get("ST_Select_Include_Artifact_Label")
        };

        const dialogOption: IArtifactPickerOptions = {
            selectableItemTypes: [ItemTypePredefined.Actor],
            isItemSelectable: (item: IArtifact) => {
                        return item.id > 0 &&
                                !item.lockedByUser;
                    }
        };

        this.dialogService.open(dialogSettings, dialogOption).then((items: IItem[]) => {
            if (items.length === 1) {
                const artifactReference = new ArtifactReference();
                artifactReference.baseItemTypePredefined = items[0].predefinedType;
                artifactReference.id = items[0].id;
                artifactReference.name = items[0].name;
                artifactReference.typePrefix = items[0].prefix;
                this.postActorPickerAction(artifactReference);
            }
        });
    }

    private postIncludePickerAction = (artifactReference: ArtifactReference): void => {
        if (artifactReference.id === this.dialogModel.artifactId) {
            this.isIncludeError = true;
        } else {
            this.setAssociatedArtifact(artifactReference);
            this.prepIncludeField();
            this.isIncludeError = false;
        }
    }

    private postActorPickerAction = (artifactReference: ArtifactReference): void => {
        this.setPersonaReference(artifactReference);
    }

    private setInclude(item: IItem) {
        const artifactReference = new ArtifactReference();
        artifactReference.baseItemTypePredefined = item.predefinedType;
        artifactReference.id = item.id;
        artifactReference.name = item.name;
        artifactReference.typePrefix = item.prefix;

        this.postIncludePickerAction(artifactReference);
    }

    private onNewArtifactCreationError = (error: IApplicationError, newArtifact: IStatefulArtifact): void => {
        if (error instanceof ApplicationError) {
            if (error.statusCode === 404 && error.errorCode === ErrorCode.ProjectNotFound) {
                this.messageService.addError("Create_New_Artifact_Error_404_102", true);
            } else if (error.statusCode === 404 && error.errorCode === ErrorCode.ItemNotFound) {
                // parent not found, we refresh the single project and move to the root
                this.messageService.addError("Create_New_Artifact_Error_404_101", true);
            } else if (error.statusCode === 404 && error.errorCode === ErrorCode.ItemTypeNotFound) {
                // artifact type not found, we refresh the single project
                this.messageService.addError("Create_New_Artifact_Error_404_109", true);
            } else if (error.statusCode === 409 && error.errorCode === ErrorCode.CannotPublishOverValidationErrors) {
                const message = new Message(MessageType.Error, "ST_Process_Include_Creation_Validation_Error_409_121", true, newArtifact.name, newArtifact.id);
                this.messageService.addMessage(message);
            }
            else if (!error.handled) {
                this.messageService.addError("Create_New_Artifact_Error_Generic");
            }
        } else {
            this.messageService.addError("Create_New_Artifact_Error_Generic");
        }
    };

    private canCleanField(): boolean {
        return !this.isReadonly;
    }
}
