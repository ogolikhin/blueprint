import { ILoadingOverlayService } from "./loading-overlay.svc";

export class LoadingOverlayComponent implements ng.IComponentOptions {
    public template: string = require("./loading-overlay.html");
    public controller: Function = LoadingOverlayController;
    public transclude: boolean = true;
}

export interface ILoadingOverlayController { }

export class LoadingOverlayController implements ILoadingOverlayController { 

    private displayOverlay: boolean;

    public static $inject = ["loadingOverlayService"];
    constructor(private loadingOverlayService: ILoadingOverlayService) {
        window.overlaycontroller = this; //TODO: Remove once finished debugging
        this.displayOverlay = loadingOverlayService.DisplayOverlay;
    }
   
    public $onDestroy() {
    }
}