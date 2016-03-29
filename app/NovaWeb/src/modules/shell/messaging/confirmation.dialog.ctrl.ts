import "angular";
import {ILocalizationService} from "../../core/localization";

export class ConfirmationDialogCtrl {
    public msg: string;
    public title: string = "Confirmation";
    public acceptButtonName: string = "Ok";
    public cancelButtonName: string = "Cancel";
    public hasCloseButton: boolean = true;

    static $inject: [string] = ["$uibModalInstance","localization"];
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private localization: ILocalizationService) {
    }

    public accept() {
        this.$uibModalInstance.close(true);
    }

    public cancel() {
        this.$uibModalInstance.close(false);
    }
}