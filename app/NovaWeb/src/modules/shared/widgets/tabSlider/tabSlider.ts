import {TabSliderController} from "./tabSlider.controller";

/**
 * Usage:
 *
 * <tab-slider
 *      slide-selector="li"
 *      invalid-class="invalid"
 *      active-class="active"
 *      transition-delay="500"  // ms, to be used if changing the transition delay in CSS
 *      responsive="true"
 *      slide-select="$ctrl.setActive
 *      slides-collection="$ctrl.richTextFields">
 * </tab-slider>
 */

export class TabSliderComponent implements ng.IComponentOptions {
    public template: string = require("./tabSlider.html");
    public transclude: boolean = true;
    public controller: ng.Injectable<ng.IControllerConstructor> = TabSliderController;
    public bindings: any = {
        slideSelector: "@?",
        invalidClass: "@?",
        activeClass: "@?",
        transitionDelay: "<?",
        responsive: "<?",
        slideSelect: "&?",
        slidesCollection: "<?"
    };
}

