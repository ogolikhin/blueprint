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
        private communicationManager: ICommunicationManager
    ) {
        super($rootScope, $scope, $uibModalInstance, dialogModel);

        this.isReadonly = dialogModel.isReadonly;
        $scope["active"] = 0;
    }

    public saveData() {
        //this.artifactUtilityService.updateTextProperty(
        //    this.dialogModel.clonedUserTask.userStoryId, [this.dialogModel.clonedUserTask.userStoryProperties.nfr, this.dialogModel.clonedUserTask.userStoryProperties.businessRules]);
    }
}
