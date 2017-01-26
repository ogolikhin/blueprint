import {ItemTypePredefined} from "../../../../../main/models/enums";
import {ApplicationError, IApplicationError} from "../../../../../shell/error/applicationError";
import {ILoadingOverlayService} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {Artifact, IArtifact, IItem} from "../../../../../main/models/models";
import {ICreateArtifactService} from "../../../../../main/components/projectControls/create-artifact.svc";
import {IArtifactService, IStatefulArtifact, IStatefulArtifactFactory} from "../../../../../managers/artifact-manager/artifact";
import {IDialogSettings, IDialogService} from "../../../../../shared";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../../../main/components/bp-artifact-picker";
import {Models, Enums} from "../../../../../main/models";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {IArtifactReference, ArtifactReference} from "../../../models/process-models";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";

export abstract class TaskModalController<T extends IModalDialogModel> extends BaseModalDialogController<T> {
    public includeArtifactName: string;
    public isReadonly: boolean = false;
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
        "loadingOverlayService"
    ];

    constructor(
        $scope: IModalScope,
        $rootScope: ng.IRootScopeService,
        private $timeout: ng.ITimeoutService,
        private dialogService: IDialogService,
        protected $q: ng.IQService,
        protected localization: ILocalizationService,
        protected createArtifactService: ICreateArtifactService,
        protected statefulArtifactFactory: IStatefulArtifactFactory,
        protected messageService: IMessageService,
        protected artifactService: IArtifactService,
        protected loadingOverlayService: ILoadingOverlayService,
        $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
        dialogModel?: T
    ) {
        super($rootScope, $scope, $uibModalInstance, dialogModel);

        this.isReadonly = this.dialogModel.isReadonly || this.dialogModel.isHistoricalVersion;

        this.nameOnBlur();

        if (this.getAssociatedArtifact()) {
            this.prepIncludeField();
        }

        this.isIncludeError = false;

        let createNewArtifactLoadingId = this.loadingOverlayService.beginLoading();

        this.artifactService.getArtifact(this.dialogModel.artifactId)
            .then((artifact) => {
                this.everPublished = artifact.version > 0;
            })
            .finally(() => {
                this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
            });
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

    // This is a workaround to force re-rendering of the dialog
    public refreshView() {
        const element: HTMLElement = document.getElementsByClassName("modal-dialog")[0].parentElement;

        if (!element) {
            return;
        }

        const node = document.createTextNode(" ");
        element.appendChild(node);

        this.$timeout(
            () => {
                node.parentNode.removeChild(node);
            },
            20,
            false
        );
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

    public saveData() {
        if (this.dialogModel.isReadonly) {
            throw new Error("Changes cannot be made or saved as this is a read-only item");
        }

        let associatedArtifact = this.getAssociatedArtifact();
        if (!associatedArtifact) {
            this.setAssociatedArtifact(null);
        } else if (associatedArtifact.id < 0) {
            this.createArtifactService.createNewArtifact(
                this.dialogModel.artifactId,
                null,
                false,
                associatedArtifact.name,
                this.getItemTypeId(),
                this.onNewArtifactCreated,
                this.onNewArtifactCreationError);
        } else {
            this.populateTaskChanges();
        }
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

    private onNewArtifactCreated = (newArtifactId: number): ng.IPromise<void> => {
        let newArtifact: IStatefulArtifact = null;

        return this.statefulArtifactFactory.createStatefulArtifactFromId(newArtifactId)
            .then((artifact: IStatefulArtifact) => {
                newArtifact = artifact;
                return newArtifact.publish();
            })
            .then(() => {
                this.setInclude(newArtifact);
                this.populateTaskChanges();
                return this.$q.resolve();
            });
    };

    private onNewArtifactCreationError = (error: IApplicationError): void => {
        if (error instanceof ApplicationError) {
            if (error.statusCode === 404 && error.errorCode === 102) {
                this.messageService.addError("Create_New_Artifact_Error_404_102", true);
            } else if (error.statusCode === 404 && error.errorCode === 101) {
                // parent not found, we refresh the single project and move to the root
                this.messageService.addError("Create_New_Artifact_Error_404_101", true);
            } else if (error.statusCode === 404 && error.errorCode === 109) {
                // artifact type not found, we refresh the single project
                this.messageService.addError("Create_New_Artifact_Error_404_109", true);
            } else if (!error.handled) {
                this.messageService.addError("Create_New_Artifact_Error_Generic");
            }
        } else {
            this.messageService.addError("Create_New_Artifact_Error_Generic");
        }
    };

    private canCleanField(): boolean {
        return !this.dialogModel.isReadonly;
    }
}
