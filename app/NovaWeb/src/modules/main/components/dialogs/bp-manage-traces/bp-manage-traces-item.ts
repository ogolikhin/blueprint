import { ILocalizationService } from "../../../../core";
import { IDialogService } from "../../../../shared";
import { Relationships } from "../../../models";


export class BPManageTracesItem implements ng.IComponentOptions {
    public template: string = require("./bp-manage-traces-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPManageTracesItemController;
    public bindings: any = {
        item: "=",
        selectedTraces: "=",
        deleteTrace: "&",
        isItemReadOnly: "<"
    };
}

export interface IResult {
    found: boolean;
    index: number;
}

interface IBPManageTracesItemController {
    deleteTrace: Function;
}

export class BPManageTracesItemController implements IBPManageTracesItemController {
    public static $inject: [string] = [
        "localization",
        "dialogService"
    ];

    public deleteTrace: Function;
    public selectedTraces: Relationships.IRelationship[];
    public item: Relationships.IRelationship;
    public isItemReadOnly: boolean;

    constructor(
        private localization: ILocalizationService,
        private dialogService: IDialogService
    ) {
    }

    public inArray(array) {
        let found = false,
            index = -1;
        if (array) {
            for (let i = 0; i < array.length; i++) {
                if (array[i].itemId === this.item.itemId) {
                    found = true;
                    index = i;
                    break;
                }
            }
        }

        return <IResult>{ "found": found, "index": index };
    }

    public selectTrace() {
        if (!this.item.isSelected) {
            if (this.selectedTraces) {
                let res = this.inArray(this.selectedTraces);
                if (!res.found) {
                    this.selectedTraces.push(this.item);
                }
            }
        } else {
            if (this.selectedTraces) {
                let res = this.inArray(this.selectedTraces);
                if (res.found) {
                    this.selectedTraces.splice(res.index, 1);
                }
            }
        }

        this.item.isSelected = !this.item.isSelected;
    }

    public setDirection(direction: Relationships.TraceDirection): void {
        if (this.item.hasAccess) {
            this.item.traceDirection = direction;
        }
    }

    public toggleFlag() {
        if (this.item.hasAccess) {
            this.item.suspect = this.item.suspect === true ? false : true;
        }
    }
}