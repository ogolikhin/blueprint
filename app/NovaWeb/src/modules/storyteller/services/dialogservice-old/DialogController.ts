module Shell {
    export class DialogController {

        public static $inject = [
            "$uibModalInstance",
            "params"
        ];

        constructor(private $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
            private params: IDialogParams) {
        }

        public ok = () => {
            this.$uibModalInstance.close();
        };

        public cancel = () => {
            this.$uibModalInstance.dismiss("cancel");
        };
    }

    angular.module("Shell").controller("dialogController", DialogController);
}
