import { ILocalizationService } from "../../../../core";
import { IDialogService } from "../../../../shared";

export class BPManageTracesItem implements ng.IComponentOptions {
    public template: string = require("./bp-manage-traces-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPManageTracesItemController;
    public bindings: any = {
        item: "="
    };
}

export class BPManageTracesItemController {
    public static $inject: [string] = [
        "localization",
        "dialogService"
    ];

    constructor(
        private localization: ILocalizationService,
        private dialogService: IDialogService
    ) {

    }
}