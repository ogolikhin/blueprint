import "angular";
//import {ILocalizationService} from "../../../core/localization";
import {IDialogParams, BaseDialogController} from "./dialog.svc";

export class OpenProjectController extends BaseDialogController {

    private projectId: number = 25;

    static $inject = ["$uibModalInstance", "params"];

    constructor($uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, params: IDialogParams) {
        super($uibModalInstance, params);
        this.hasCloseButton = true;
    }

    public accept = () => {
        this.$instance.close(this.projectId);
    };

}



