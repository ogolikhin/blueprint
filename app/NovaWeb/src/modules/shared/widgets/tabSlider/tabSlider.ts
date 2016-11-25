import {TabSliderController} from "./tabSlider-controller";

export class TabSliderComponent implements ng.IComponentOptions {
    public template: string = require("./tabSlider.html");
    public transclude: boolean = true;
    public controller: ng.Injectable<ng.IControllerConstructor> = TabSliderController;
    public bindings: any = {
        // Two-way
        api: "=?",
        // Input
        slideSelector: "@?",
        invalidClass: "@?"
    };
}

