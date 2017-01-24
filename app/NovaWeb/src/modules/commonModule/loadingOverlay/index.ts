//3rd party(external) library dependencies used for this module
import "angular";
//internal dependencies used for this module
import {LoadingOverlayService} from "./loadingOverlay.service";
import {BpLoadingOverlayComponent} from "./loadingOverlay.controller";

export const LoadingOverlay = angular.module("loadingOverlay", [])
    .service("loadingOverlayService", LoadingOverlayService)
    .component("bpLoadingOverlay", new BpLoadingOverlayComponent())
    .name;

//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {ILoadingOverlayService} from "./loadingOverlay.service"
