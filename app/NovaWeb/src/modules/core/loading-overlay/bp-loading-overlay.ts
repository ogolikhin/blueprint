import {ILoadingOverlayService} from "./loading-overlay.svc";

export class BpLoadingOverlayComponent implements ng.IComponentOptions {
    public template: string = require("./bp-loading-overlay.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpLoadingOverlayController;
    public transclude: boolean = true;
}

export interface IBpLoadingOverlayController {
}

export class BpLoadingOverlayController implements IBpLoadingOverlayController {

    public static $inject = ["loadingOverlayService"];

    constructor(private loadingOverlayService: ILoadingOverlayService) {
    }

    private get displayOverlay() {
        return this.loadingOverlayService.displayOverlay;
    }

    public $onDestroy() {
    }
}
