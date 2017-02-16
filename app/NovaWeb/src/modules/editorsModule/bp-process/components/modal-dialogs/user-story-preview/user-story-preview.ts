import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {UserStoryDialogModel} from "../models/user-story-dialog-model";
import {ICommunicationManager} from "../../../services/communication-manager";

export class UserStoryPreviewController extends BaseModalDialogController<UserStoryDialogModel> {
    private isReadonly: boolean = false;
    private isUserStoryLoaded: boolean = false;
    private userStoryLoadedHandler: string;

    public static $inject = [
        "$rootScope",
        "$scope",
        "$timeout",
        "$uibModalInstance",
        "dialogModel",
        "communicationManager"
    ];

    constructor(
        $rootScope: ng.IRootScopeService,
        $scope: IModalScope,
        $timeout: ng.ITimeoutService,
        $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
        dialogModel: UserStoryDialogModel,
        private communicationManager: ICommunicationManager
    ) {
        super($rootScope, $scope, $timeout, $uibModalInstance, dialogModel);

        this.isReadonly = dialogModel.isReadonly;
        $scope["active_previous"] = 0;
        $scope["active_next"] = 0;

        this.userStoryLoadedHandler = this.communicationManager.modalDialogManager.registerUserStoryLoadedObserver(this.onUserStoryLoaded);

        $uibModalInstance.closed.then(this.onModalClosed);
    }

    private onUserStoryLoaded = (isUserStoryLoaded) => {
        this.isUserStoryLoaded = isUserStoryLoaded;
    }

    public showWings(): boolean {
        return !!this.$scope.dialogModel && this.$scope.dialogModel["isUserSystemProcess"] && this.isUserStoryLoaded;
    }

    private onModalClosed = () => {
        this.communicationManager.modalDialogManager.removeUserStoryLoadedObserver(this.userStoryLoadedHandler);
    }
}
