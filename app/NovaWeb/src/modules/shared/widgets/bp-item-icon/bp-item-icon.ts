import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";

export class BPItemTypeIconComponent implements ng.IComponentOptions {
    public template: string = require("./bp-item-icon.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPItemTypeIconController;
    public transclude: boolean = true;
    public bindings: any = {
        itemTypeId: "<",
        itemTypeIconId: "<",
        predefinedType: "<",
        fallback: "<?"
    };
}

export interface IBPItemTypeIconController {
    itemTypeId: number;
    itemTypeIconId: number;
    predefinedType: number;
    fallback?: boolean;
}

export class BPItemTypeIconController implements IBPItemTypeIconController {
    public itemTypeId: number;
    public itemTypeIconId: number;
    public predefinedType: number;
    public fallback: boolean;

    public iconClass: string;
    public imageSource: string;
    public altText: string;
    public showBasicIcon: boolean;

    private artifactTypeDescription: string;

    public $onChanges = () => {
        this.showBasicIcon = _.isFinite(this.predefinedType) && !_.isFinite(this.itemTypeIconId);
        this.artifactTypeDescription = ItemTypePredefined[this.predefinedType] || "Document";
        this.iconClass = "icon-" + (_.kebabCase(this.artifactTypeDescription));
        this.altText = _.startCase(this.artifactTypeDescription);
        this.imageSource = this.getImageSource();
    };

    private getImageSource() {
        let imgUrl: string = "";
        if (_.isFinite(this.itemTypeId)) {
            imgUrl = `/shared/api/itemTypes/${this.itemTypeId}/icon`;
            if (_.isFinite(this.itemTypeIconId)) {
                imgUrl += `?id=${this.itemTypeIconId}`;
            }
        }
        return imgUrl;
    }
}
