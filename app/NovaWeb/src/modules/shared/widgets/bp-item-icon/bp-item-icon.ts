export interface IBPItemTypeIconController {
    predefinedType: string;
    getImageSource(): string;
}

export class BPItemTypeIconController implements IBPItemTypeIconController {
    public predefinedType: string;

    public getImageSource() {
        return "/Shared/api/itemTypes/" + parseInt(this.predefinedType, 10) + "/icon";
    }
}

export class BPItemTypeIconComponent implements ng.IComponentOptions {
    public template: string = require("./bp-item-icon.html");
    public controller: Function = BPItemTypeIconController;
    public transclude: boolean = true;
    public bindings: any = {     
        predefinedType: "@"
    };
}


