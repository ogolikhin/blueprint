import {IDiagramService} from "../../../../../editors/bp-diagram/diagram.svc";
import {IDialogSettings, IDialogService} from "../../../../../shared";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../../../main/components/bp-artifact-picker";
import {Models} from "../../../../../main/models";
import {ILocalizationService} from "../../../../../core";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {SubArtifactUserTaskDialogModel} from "../models/sub-artifact-dialog-model";
import {IArtifactReference, ArtifactReference} from "../../../models/process-models";
import {IModalProcessViewModel} from "../models/modal-process-view-model";
import {ICommunicationManager} from "../../../services/communication-manager";
import {TaskModalController} from "./task-modal-controller";

export class UserTaskModalController extends TaskModalController<SubArtifactUserTaskDialogModel> {
    public static $inject = [
        "$scope",
        "communicationManager",
        "$rootScope",
        "$q",
        "$timeout",
        "$sce", 
        "dialogService",
        "localization"
    ];

    constructor(
                $scope: IModalScope,
                communicationManager: ICommunicationManager,
                $rootScope: ng.IRootScopeService,
                $q: ng.IQService,
                $timeout: ng.ITimeoutService,
                $sce: ng.ISCEService,
                dialogService: IDialogService,
                localization: ILocalizationService  ) {

        super($scope, communicationManager, $rootScope, $q, $timeout, $sce, dialogService, localization);

        this.actionOnBlur();
    }

    protected  getAssociatedArtifact(): IArtifactReference {
        return this.dialogModel.clonedItem.associatedArtifact;
    }

    protected setAssociatedArtifact(value: IArtifactReference) {
        this.dialogModel.clonedItem.associatedArtifact = value;
    }

    public actionOnFocus = () => {
        this.actionPlaceHolderText = this.localization.get("ST_User_Task_Name_Label");
    }

    public actionOnBlur = () => {
        if (this.dialogModel.clonedItem) {
            if (this.dialogModel.clonedItem.action) {
                this.actionOnFocus();
            } else {
                this.actionPlaceHolderText = this.localization.get("ST_User_Task_Name_Label") + " " + this.dialogModel.clonedItem.label;
            }
        }
    }

    protected populateTaskChanges() {

        if (this.dialogModel.originalItem && this.dialogModel.clonedItem) {
            this.dialogModel.originalItem.persona = this.dialogModel.clonedItem.persona;
            this.dialogModel.originalItem.action = this.dialogModel.clonedItem.action;            
            this.dialogModel.originalItem.objective = this.dialogModel.clonedItem.objective;            
            this.dialogModel.originalItem.associatedArtifact = this.dialogModel.clonedItem.associatedArtifact;
        }
    }

    public getActiveHeader(): string {

        if (this.dialogModel.isUserTask) {
            return this.dialogModel.clonedItem.label;
        }

        return null;
    }
}
