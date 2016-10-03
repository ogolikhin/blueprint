import { ILocalizationService } from "../../../../core";
import { BaseDialogController, IDialogSettings } from "../../../../shared";
import { Relationships } from "../../../models";

export class ManageTracesDialogController extends BaseDialogController {
    public static $inject = ["$uibModalInstance", "dialogSettings", "localization"];

    constructor($uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, dialogSettings: IDialogSettings,
                private localization: ILocalizationService) {
        super($uibModalInstance, dialogSettings);
    };

    public options = [
        { value: "1", label: "To" },
        { value: "2", label: "From" },
        { value: "3", label: "Bidirectional" },
    ];

    public traces = [{
            name: "test",
            desc: "test2"
        },
        {
            name: "test",
            desc: "test3"
    }];

    public toggleTraces(artifacts: Relationships.IRelationship[]): void {
        alert("run fn toggleTraces");
    }

    public deleteTraces(artifacts: Relationships.IRelationship[]): void {
        alert("run fn deleteTraces");
    }

    public deleteTrace(artifacts: Relationships.IRelationship): void {
        alert("run fn deleteTrace");
    }
}
