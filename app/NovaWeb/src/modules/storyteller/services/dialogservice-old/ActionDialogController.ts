module Shell {
    export class ActionDialogController {

        public static $inject = [
            "$uibModalInstance",
            "params"
        ];

        constructor(private $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
            private params: IDialogParams) {
        }

        public ok = (btn) => {
            switch (btn) {
                case 1:
                    this.params.buttonType = ButtonType.Publish;
                    break;
                case 2:
                    this.params.buttonType = ButtonType.Save;
                    break;
                case 3:
                    this.params.buttonType = ButtonType.Discard;
                    break;
            }
           
            this.$uibModalInstance.close();
        };
     
        public cancel = () => {
            this.$uibModalInstance.dismiss("cancel");
        };
    }

    angular.module("Shell").controller("actionDialogController", ActionDialogController);
}
