import "angular";

export class ConfirmationDialogCtrl {
    public msg: string;
    public title: string = "Confirmation";
    public acceptButtonName: string = "OK";
    public cancelButtonName: string = "Cancel";
    public hasCloseButton: boolean = true;

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