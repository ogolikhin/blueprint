require("./tabSlider.scss");

import {TabSliderComponent} from "./tabSlider";
import {ITabSliderControllerApi} from "./tabSlider-controller";

angular.module("bp.widgets.tabSlider", [])
    .component("tabSlider", new TabSliderComponent());

export {ITabSliderControllerApi}
