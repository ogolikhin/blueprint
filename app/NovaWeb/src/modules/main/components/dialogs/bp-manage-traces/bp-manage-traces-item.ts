import * as _ from "lodash";
import { ILocalizationService } from "../../../../core";
import { IDialogService } from "../../../../shared";
import { Relationships } from "../../../models";


export class BPManageTracesItem implements ng.IComponentOptions {
    public template: string = require("./bp-manage-traces-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPManageTracesItemController;
    public bindings: any = {
        item: "<",
        selectedTraces: "=",
        deleteTrace: "&",
        isItemReadOnly: "<"
    };
}

interface IBPManageTracesItemController {
    deleteTrace: Function;
}

export class BPManageTracesItemController implements IBPManageTracesItemController, ng.IComponentController  {
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

    public selectTrace() {
        if (this.selectedTraces) {
            let index = _.findIndex(this.selectedTraces, {itemId: this.item.itemId});

            if (!this.item.isSelected) {
                if (index === -1) {
                    this.selectedTraces.push(this.item);
                }
            } else {
                if (index > -1) {
                    this.selectedTraces.splice(index, 1);
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
            this.item.suspect = !this.item.suspect;
        }
    }
}