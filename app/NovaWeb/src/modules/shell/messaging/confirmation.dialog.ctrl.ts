import "angular";
import {ILocalizationService} from "../../core/localization";

export class ConfirmationDialogCtrl {
    public msg: string;
    public title: string = this.localization.get("App_DialogTitle_Confirmation");
    public acceptButtonName: string = this.localization.get("App_Button_Ok");
    public cancelButtonName: string = this.localization.get("App_Button_Cancel");
    public hasCloseButton: boolean = true;

    static $inject: [string] = ["localization", "$uibModalInstance"];
    constructor(private localization: ILocalizationService, private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
    }

    public accept() {
        this.$uibModalInstance.close(true);
    }

    public cancel() {
        this.$uibModalInstance.close(false);
    }
}