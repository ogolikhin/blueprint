import {RowSliderController} from "./rowSlider-controller";

export class RowSliderComponent implements ng.IComponentOptions {
    public template: string = require("./rowSlider.html");
    public transclude: boolean = true;
    public controller: ng.Injectable<ng.IControllerConstructor> = RowSliderController;
    public bindings: any = {
        // Two-way
        api: "=?",
        // Input
        slideSelector: "@?",
        invalidClass: "@?"
    };
}

