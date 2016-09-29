import { BaseDialogController, IDialogSettings } from "../../../../shared";

export class ManageTracesDialogController extends BaseDialogController {
    public static $inject = ["$uibModalInstance", "dialogSettings"];

    constructor($uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, dialogSettings: IDialogSettings ) {
        super($uibModalInstance, dialogSettings);
    };
}
