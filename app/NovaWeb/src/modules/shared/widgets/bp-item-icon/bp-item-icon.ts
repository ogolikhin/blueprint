import * as angular from "angular";
import {Models} from "../../../main/models";
import {Helper} from "../../../shared";
export interface IBPItemTypeIconController {
    itemTypeId: number;
    itemTypeIcon?: number;
    predefinedType?: number;
    getImageSource(): string;
    getIconClass(): string;
}

export class BPItemTypeIconComponent implements ng.IComponentOptions {
    public template: string = require("./bp-item-icon.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPItemTypeIconController;
    public transclude: boolean = true;
    public bindings: any = {
        itemTypeId: "<",
        itemTypeIcon: "<",
        predefinedType: "<"
    };
}

export class BPItemTypeIconController implements IBPItemTypeIconController {
    public itemTypeId: number;
    public itemTypeIcon: number;
    public predefinedType: number;
    public showBasicIcon: boolean;

    private artifactTypeDescription: string;

    constructor() {
        this.showBasicIcon = !_.isUndefined(this.predefinedType) && _.isNumber(this.predefinedType);
        this.artifactTypeDescription = Models.ItemTypePredefined[this.predefinedType] || "Document";
    }

    public getImageSource() {
        let imgUrl: string;

        if (this.itemTypeId && !isNaN(Number(this.itemTypeId))) {
            imgUrl = "/shared/api/itemTypes/" + this.itemTypeId.toString() + "/icon";
            if (this.itemTypeIcon && !isNaN(Number(this.itemTypeIcon))) {
                imgUrl += "?" + this.itemTypeIcon.toString();
            }
        } else {
            imgUrl = "";
        }

        return imgUrl;
    }

    public getIconClass() {
        return "icon-" + (_.kebabCase(this.artifactTypeDescription));
    }

    public getAltText() {
        return _.startCase(this.artifactTypeDescription);
    }
}
