import * as _ from "lodash";
import {IDialogService} from "../../../../shared";
import {Relationships} from "../../../models";
import {ILocalizationService} from "../../../../core/localization/localizationService";


export class BPManageTracesItem implements ng.IComponentOptions {
    public template: string = require("./bp-manage-traces-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPManageTracesItemController;
    public bindings: any = {
        item: "<",
        selectedTraces: "=",
        deleteTrace: "&",
        isItemReadOnly: "<",
        toggleFlag: "&",
        setTraceDirection: "&"
    };
}

interface IBPManageTracesItemController {
    deleteTrace: Function;
    toggleFlag: Function;
    setTraceDirection: Function;
}

export class BPManageTracesItemController implements IBPManageTracesItemController, ng.IComponentController {
    public static $inject: [string] = [
        "localization",
        "dialogService"
    ];

    public deleteTrace: Function;
    public toggleFlag: Function;
    public setTraceDirection: Function;
    public selectedTraces: Relationships.IRelationshipView[];
    public item: Relationships.IRelationshipView;
    public isItemReadOnly: boolean;
    public traceIcon: string;

    constructor(private localization: ILocalizationService,
                private dialogService: IDialogService) {
    }

    public $onInit() {
        this.item.directionIcon = this.getDirectionIcon(this.item.traceDirection);
        this.item.traceIcon = this.item.suspect ? "trace-icon-suspect" : "trace-icon-regular";
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

    public getDirectionIcon (direction: Relationships.TraceDirection) {
        let icon = "fonticon2-relationship-";

        switch (direction) {
            case 0:
                icon += "right";
                break;
            case 1:
                icon += "left";
                break;
            case 2:
                icon += "bi";
                break;
            default:
                icon += "right";
                break;
        }

        return icon;
    }
}
