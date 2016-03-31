import "angular";
import {ILocalizationService} from "../../core/localization";

export class ConfirmationDialogCtrl {
    public msg: string;
    public title: string = "Confirmation";
    public acceptButtonName: string = "Ok";
    public cancelButtonName: string = "Cancel";
    public hasCloseButton: boolean = true;

    static $inject: [string] = ["localization","$uibModalInstance"];
    constructor(private localization: ILocalizationService, private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
    }

    public accept() {
        this.$uibModalInstance.close(true);
    }

    public cancel() {
        this.$uibModalInstance.close(false);
    }
}