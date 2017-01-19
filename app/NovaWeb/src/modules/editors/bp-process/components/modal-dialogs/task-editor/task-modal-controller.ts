import {ICreateArtifactService} from "../../../../../main/components/page-toolbar/create-artifact.svc";
import {IStatefulArtifact} from "../../../../../managers/artifact-manager/artifact";
import {IDialogSettings, IDialogService} from "../../../../../shared";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../../../main/components/bp-artifact-picker";
import {Models, Enums} from "../../../../../main/models";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {IArtifactReference, ArtifactReference} from "../../../models/process-models";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";
import {ILocalizationService} from "../../../../../core/localization/localizationService";

export abstract class TaskModalController<T extends IModalDialogModel> extends BaseModalDialogController<T> {
    public includeArtifactName: string;
    public isReadonly: boolean = false;
    public isIncludeResultsVisible: boolean;
    public isIncludeError: boolean;

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
    protected abstract getModel(): Models.IArtifact;
    protected abstract getNewArtifactName(): string;
    protected abstract getItemTypeId(): number;

    public static $inject = [
        "$scope",
        "$rootScope",
        "$timeout",
        "dialogService",
        "localization",
        "createArtifactService"
    ];

    constructor(
        $scope: IModalScope,
        $rootScope: ng.IRootScopeService,
        private $timeout: ng.ITimeoutService,
        private dialogService: IDialogService,
        protected localization: ILocalizationService,
        protected createArtifactService: ICreateArtifactService,
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

        let msg: string;
        if (model.typePrefix === "<Inaccessible>") {
            msg = this.localization.get("ST_Inaccessible_Include_Artifact_Label");
        } else {
            msg = model.typePrefix + model.id + " - " + model.name;
        }

        return msg;
    }

    public saveData() {
        if (this.dialogModel.isReadonly) {
            throw new Error("Changes cannot be made or saved as this is a read-only item");
        }

        if (this.getAssociatedArtifact() === undefined) {
            this.setAssociatedArtifact(null);
        }

        this.populateTaskChanges();
    }

    private getIncludedArtifactTypes(): Models.ItemTypePredefined[] {
        const selectableArtifactTypes: Models.ItemTypePredefined[] = [
            Models.ItemTypePredefined.BusinessProcess,
            Models.ItemTypePredefined.Document,
            Models.ItemTypePredefined.DomainDiagram,
            Models.ItemTypePredefined.PrimitiveFolder,
            Models.ItemTypePredefined.GenericDiagram,
            Models.ItemTypePredefined.Glossary,
            Models.ItemTypePredefined.Process,
            Models.ItemTypePredefined.Storyboard,
            Models.ItemTypePredefined.TextualRequirement,
            Models.ItemTypePredefined.UIMockup,
            Models.ItemTypePredefined.UseCase,
            Models.ItemTypePredefined.UseCaseDiagram
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
            isItemSelectable: (item: Models.IArtifact) => {
                        return item.id !== subArtifact.parentId &&
                                item.id !== subArtifact.id &&
                                item.id > 0 &&
                                !item.lockedByUser;
                    }
        };

        this.openArtifactPicker(dialogSettings, dialogOption, this.postIncludePickerAction);
    }

    public createNewArtifact = (useModal: boolean): void => {
        this.createArtifactService.createNewArtifact(
            this.dialogModel.artifactId,
            false,
            this.getNewArtifactName(),
            this.getItemTypeId(),
            this.setInclude);
    };

    public openActorPicker() {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get("ST_Select_Include_Artifact_Label")
        };

        const dialogOption: IArtifactPickerOptions = {
            selectableItemTypes: [Models.ItemTypePredefined.Actor],
            isItemSelectable: (item: Models.IArtifact) => {
                        return item.id > 0 &&
                                !item.lockedByUser;
                    }
        };

        this.openArtifactPicker(dialogSettings, dialogOption, this.postActorPickerAction);
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

    private openArtifactPicker(dialogSettings: IDialogSettings,
        dialogOptions: IArtifactPickerOptions,
        postArtifactPickerAction: (artifactReference: ArtifactReference) => void) {

        this.dialogService.open(dialogSettings, dialogOptions).then((items: Models.IItem[]) => {
            if (items.length === 1) {
                this.setInclude(items[0]);
            }
        });
    }

    private setInclude = ((item: Models.IItem) => {
        const artifactReference = new ArtifactReference();
        artifactReference.baseItemTypePredefined = item.predefinedType;
        artifactReference.id = item.id;
        artifactReference.name = item.name;
        artifactReference.typePrefix = item.prefix;
        this.postIncludePickerAction(artifactReference);
    });

    private canCleanField(): boolean {
        return !this.dialogModel.isReadonly;
    }

}
