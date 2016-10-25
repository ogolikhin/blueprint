import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {UserStoryDialogModel} from "../models/user-story-dialog-model";
import {ICommunicationManager} from "../../../services/communication-manager";

export class UserStoryPreviewController extends BaseModalDialogController<UserStoryDialogModel> {

    private isReadonly: boolean = false;

    public static $inject = [
        "$rootScope",
        "$scope",
        "$uibModalInstance",
        "dialogModel",
        "communicationManager"
    ];

    constructor($rootScope: ng.IRootScopeService,
                $scope: IModalScope,
                $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
                dialogModel: UserStoryDialogModel,
                private communicationManager: ICommunicationManager) {
        super($rootScope, $scope, $uibModalInstance, dialogModel);

        this.isReadonly = dialogModel.isReadonly;
        $scope["active_previous"] = 0;
        $scope["active_next"] = 0;
    }

    public saveData() {
        alert("Publishing user story has not been migrated over yet");
        //this.artifactUtilityService.updateTextProperty(
        //    this.dialogModel.userStoryId, 
        //    [this.dialogModel.userStoryProperties.nfr, this.dialogModel.userStoryProperties.businessRules]);
    }
}
