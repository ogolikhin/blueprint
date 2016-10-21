import {IDialogSettings, IDialogService} from "../../../../../shared";
import {ILocalizationService} from "../../../../../core";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {UserTaskDialogModel} from "./sub-artifact-dialog-model";
import {IArtifactReference, ArtifactReference} from "../../../models/process-models";
import {ICommunicationManager} from "../../../services/communication-manager";
import {TaskModalController} from "./task-modal-controller";

export class UserTaskModalController extends TaskModalController<UserTaskDialogModel> {
    actionPlaceHolderText: string;

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
    }

    //public methods
    nameOnFocus() {
        this.actionPlaceHolderText = this.localization.get("ST_User_Task_Name_Label");
    }

    nameOnBlur() {
        if (this.dialogModel.action) {
                this.nameOnFocus();
            } else {
                this.actionPlaceHolderText = this.localization.get("ST_User_Task_Name_Label") + " " + this.dialogModel.label;
            }
    }

    getActiveHeader(): string {
        return this.dialogModel.label;
    }

    //protected methods
    protected  getAssociatedArtifact(): IArtifactReference {
        return this.dialogModel.associatedArtifact;
    }

    protected setAssociatedArtifact(value: IArtifactReference) {
        this.dialogModel.associatedArtifact = value;
    }

    protected populateTaskChanges() {

        if (this.dialogModel.originalItem && this.dialogModel) {
            this.dialogModel.originalItem.persona = this.dialogModel.persona;
            this.dialogModel.originalItem.action = this.dialogModel.action;            
            this.dialogModel.originalItem.objective = this.dialogModel.objective;            
            this.dialogModel.originalItem.associatedArtifact = this.dialogModel.associatedArtifact;
        }
    }    
}
