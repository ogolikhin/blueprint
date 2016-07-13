module Storyteller {
    export class UserStoryPreviewController extends BaseModalDialogController<UserStoryDialogModel> {

        private isReadonly: boolean = false;

        public static $inject = [
            "$rootScope",
            "$scope",
            "$uibModalInstance",
            "dialogModel",
            "artifactUtilityService"
        ];

        constructor($rootScope: ng.IRootScopeService,
            $scope: IModalScope,
            $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
            dialogModel: UserStoryDialogModel,
            private artifactUtilityService: Shell.IArtifactUtilityService
        ) {
            super($rootScope, $scope, $uibModalInstance, dialogModel);

            this.isReadonly = dialogModel.isReadonly;
        }

        public saveData() {
            this.artifactUtilityService.updateTextProperty(
                this.dialogModel.clonedUserTask.userStoryId, [this.dialogModel.clonedUserTask.userStoryProperties.nfr, this.dialogModel.clonedUserTask.userStoryProperties.businessRules]);
        }
    }

    angular.module("Storyteller").controller("UserStoryPreviewController", UserStoryPreviewController);
}
