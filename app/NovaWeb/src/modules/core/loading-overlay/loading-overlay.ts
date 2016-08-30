import { ILoadingOverlayService } from "./loading-overlay.svc";

export class LoadingOverlayComponent implements ng.IComponentOptions {
    public template: string = require("./loading-overlay.html");
    public controller: Function = LoadingOverlayController;
    public transclude: boolean = true;
}

export interface ILoadingOverlayController { }

export class LoadingOverlayController implements ILoadingOverlayController { 

    public static $inject = ["loadingOverlayService"];
    constructor(private loadingOverlayService: ILoadingOverlayService) {}
    
    private get displayOverlay() {
        return this.loadingOverlayService.DisplayOverlay;
    }

    public $onDestroy() {
    }
}