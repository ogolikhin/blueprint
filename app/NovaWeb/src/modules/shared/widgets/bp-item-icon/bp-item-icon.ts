import * as angular from "angular";
import {Models} from "../../../main/models";

export interface IBPItemTypeIconController {
    itemTypeId: number;
    itemTypeIconId?: number;
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
        itemTypeIconId: "<",
        predefinedType: "<"
    };
}

export class BPItemTypeIconController implements IBPItemTypeIconController {
    public itemTypeId: number;
    public itemTypeIconId: number;
    public predefinedType: number;
    public showBasicIcon: boolean;

    private artifactTypeDescription: string;

    constructor() {
        this.showBasicIcon = _.isFinite(this.predefinedType) && !_.isFinite(this.itemTypeIconId);
        this.artifactTypeDescription = Models.ItemTypePredefined[this.predefinedType] || "Document";
    }

    public getImageSource() {
        let imgUrl: string = "";
        if (_.isFinite(this.itemTypeId)) {
            imgUrl = `/shared/api/itemTypes/${this.itemTypeId}/icon`;
            if (_.isFinite(this.itemTypeIconId)) {
                imgUrl += `?id=${this.itemTypeIconId}`;
            }
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
