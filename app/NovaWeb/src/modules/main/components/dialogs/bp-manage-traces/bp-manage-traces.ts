import { BaseDialogController, IDialogSettings } from "../../../../shared";

export class ManageTracesDialogController extends BaseDialogController {
    public static $inject = ["$uibModalInstance", "dialogSettings"];


    constructor($uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, dialogSettings: IDialogSettings ) {
        super($uibModalInstance, dialogSettings);
    };

    public options = [
        { value: "1", label: "To" },
        { value: "2", label: "From" },
        { value: "3", label: "Bidirectional" },
    ];

    public  artifacts = [{
            name: "test",
            desc: "test2"
        },
        {
            name: "test",
            desc: "test3"
    }];

    public selectTrace(): string {
        return "";
    }

    public deleteTrace(): string {
        return "";
    }
    // public sT() {
    //     return [{name: "Test name"}];
    // };
}
