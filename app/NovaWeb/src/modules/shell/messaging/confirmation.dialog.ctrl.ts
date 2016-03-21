import "angular";

export class ConfirmationDialogCtrl {
    public msg: string;
    public acceptButtonName: string = "OK";
    public cancelButtonName: string = "Cancel";

    static $inject: [string] = ["$uibModalInstance"];
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
    }

    public accept() {
        this.$uibModalInstance.close(true);
    }

    public cancel() {
        this.$uibModalInstance.close(false);
    }
}