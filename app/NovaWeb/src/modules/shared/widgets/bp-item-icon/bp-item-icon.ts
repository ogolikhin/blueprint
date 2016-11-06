import * as angular from "angular";
import {Models} from "../../../main/models";

export interface IBPItemTypeIconController {
    itemTypeId: number;
    itemTypeVersionId?: number;
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
        itemTypeVersionId: "<",
        predefinedType: "<"
    };
}

export class BPItemTypeIconController implements IBPItemTypeIconController {
    public itemTypeId: number;
    public itemTypeVersionId: number;
    public predefinedType: number;
    public showBasicIcon: boolean;

    private artifactTypeDescription: string;

    constructor() {
        this.showBasicIcon = !_.isUndefined(this.predefinedType) && _.isNumber(this.predefinedType);
        this.artifactTypeDescription = Models.ItemTypePredefined[this.predefinedType] || "Document";
    }

    public getImageSource() {
        let imgUrl: string = "";
        if (_.isFinite(this.itemTypeId)) {
            imgUrl = `/shared/api/itemTypes/${this.itemTypeId}/icon`;
            if (_.isFinite(this.itemTypeVersionId)) {
                imgUrl += `?${this.itemTypeVersionId}`;
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
