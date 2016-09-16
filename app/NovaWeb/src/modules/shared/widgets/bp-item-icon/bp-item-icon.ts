export interface IBPItemTypeIconController {
    itemTypeId: string;
    getImageSource(): string;
}

export class BPItemTypeIconController implements IBPItemTypeIconController {
    public itemTypeId: string;

    public getImageSource() {
        return "/shared/api/itemTypes/" + parseInt(this.itemTypeId, 10) + "/icon";
    }
}

export class BPItemTypeIconComponent implements ng.IComponentOptions {
    public template: string = require("./bp-item-icon.html");
    public controller: Function = BPItemTypeIconController;
    public transclude: boolean = true;
    public bindings: any = {     
        itemTypeId: "@"
    };
}


