import {ILocalizationService} from "../../../core/localization";

interface IToolbarController {
    add(): void;
    clear(): void;
    execute(evt: ng.IAngularEvent): void;
}


export class Toolbar implements ng.IComponentOptions {
    public template: string;
    public controller: Function;
    public require: string;

    constructor() {
        this.template = require("./toolbar.html");
        this.controller = ToolbarCtrl;
        this.require = "^parent";
    }
}

class ToolbarCtrl implements IToolbarController {

    static $inject = ["localization", "$window"];

    constructor(private localization: ILocalizationService, private $window: ng.IWindowService) {
    }

    add(): void {
    }

    clear(): void {
    }

    execute(evt: any): void {
        evt.preventDefault();
        alert(evt.currentTarget.innerText);
    }
}