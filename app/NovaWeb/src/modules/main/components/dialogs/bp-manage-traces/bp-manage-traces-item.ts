import { ILocalizationService } from "../../../../core";
import { IDialogService } from "../../../../shared";
import { Relationships } from "../../../models";

export class BPManageTracesItem implements ng.IComponentOptions {
    public template: string = require("./bp-manage-traces-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPManageTracesItemController;
    public bindings: any = {
        item: "=",
        deleteTrace: "&"
    };
}

export class BPManageTracesItemController {
    public static $inject: [string] = [
        "localization",
        "dialogService"
    ];

    public isSelected: boolean = false;

    constructor(
        private localization: ILocalizationService,
        private dialogService: IDialogService
    ) {
    }

    public selectTrace() {
       this.isSelected = !this.isSelected;
    }

    public setDirection(direction: Relationships.TraceDirection): void {
        alert("run fn setDirection");
    }

    public toggleFlag() {
        alert("run fn toggleFlag");
    }
}