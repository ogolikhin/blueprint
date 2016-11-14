import * as angular from "angular";
import { RowSliderComponent } from "./rowSlider";
import { IRowSliderControllerApi } from "./rowSlider-controller";

angular.module("bp.widgets.rowSlider", [])
    .component("rowSlider", new RowSliderComponent());

export { IRowSliderControllerApi }
