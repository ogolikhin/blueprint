import {IDialogService} from "../../../../../shared";
import {IModalScope} from "../base-modal-dialog-controller";
import {UserTaskDialogModel} from "./sub-artifact-dialog-model";
import {IArtifactReference} from "../../../models/process-models";
import {TaskModalController} from "./task-modal-controller";
import {ILocalizationService} from "../../../../../core/localization/localizationService";

export class UserTaskModalController extends TaskModalController<UserTaskDialogModel> {
    public actionPlaceHolderText: string;

    public static $inject = [
        "$scope",
        "$rootScope",
        "$timeout",
        "dialogService",
        "localization"
    ];

    constructor(
        $scope: IModalScope,
        $rootScope: ng.IRootScopeService,
        $timeout: ng.ITimeoutService,
        dialogService: IDialogService,
        localization: ILocalizationService,
        $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
        dialogModel?: UserTaskDialogModel
    ) {
        super($scope, $rootScope, $timeout, dialogService, localization, $uibModalInstance, dialogModel);
    }

    public nameOnFocus() {
        this.actionPlaceHolderText = this.localization.get("ST_User_Task_Name_Label");
    }

    public nameOnBlur() {
        if (this.dialogModel.action) {
            this.nameOnFocus();
        } else {
            this.actionPlaceHolderText = this.localization.get("ST_User_Task_Name_Label") + " " + this.dialogModel.label;
        }
    }

    public getActiveHeader(): string {
        return this.dialogModel.label;
    }

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
