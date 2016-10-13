export interface IBPItemTypeIconController {
    itemTypeId: string;
    itemTypeIcon?: string;
    getImageSource(): string;
}

export class BPItemTypeIconController implements IBPItemTypeIconController {
    public itemTypeId: string;
    public itemTypeIcon: string;

    public getImageSource() {
        let imgUrl: string;

        if (this.itemTypeId && !isNaN(Number(this.itemTypeId))) {
            imgUrl = "/shared/api/itemTypes/" + parseInt(this.itemTypeId, 10).toString() + "/icon";
            if (this.itemTypeIcon && !isNaN(Number(this.itemTypeIcon))) {
                imgUrl += "?" + parseInt(this.itemTypeIcon, 10).toString();
            }
        } else {
            imgUrl = "";
        }

        return imgUrl;
    }
}

export class BPItemTypeIconComponent implements ng.IComponentOptions {
    public template: string = require("./bp-item-icon.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPItemTypeIconController;
    public transclude: boolean = true;
    public bindings: any = {
        itemTypeId: "@",
        itemTypeIcon: "@"
    };
}


