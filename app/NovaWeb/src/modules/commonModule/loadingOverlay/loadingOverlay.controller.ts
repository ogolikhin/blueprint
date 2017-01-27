import "./loadingOverlay.scss";

import {ILoadingOverlayService} from "./loadingOverlay.service";

export class BpLoadingOverlayComponent implements ng.IComponentOptions {
    public template: string = require("./loadingOverlay.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpLoadingOverlayController;
    public transclude: boolean = true;
}

export class BpLoadingOverlayController {

    public static $inject = ["loadingOverlayService"];

    constructor(private loadingOverlayService: ILoadingOverlayService) {
    }

    private get displayOverlay() {
        return this.loadingOverlayService.displayOverlay;
    }

    public $onDestroy() {
//fixme: if block is empty remove it.
    }
}
