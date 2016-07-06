export interface IItemTypeIconController {
    predefinedType: string;
    getImageSource(): string;
}

export class ItemTypeIconController implements IItemTypeIconController {
    public predefinedType: string;

    public getImageSource() {
        return "/Shared/api/itemTypes/" + parseInt(this.predefinedType, 10) + "/icon";
    }
}

export class ItemTypeIconComponent implements ng.IComponentOptions {
    public template: string = require("./bp-item-icon.html");
    public controller: Function = ItemTypeIconController;
    public transclude: boolean = true;
    public bindings: any = {     
        predefinedType: "@"
    };
}


