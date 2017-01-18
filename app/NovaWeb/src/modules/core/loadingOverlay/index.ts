import {LoadingOverlayService} from "./loadingOverlay.service";
import {BpLoadingOverlayComponent} from "./loadingOverlay.controller";

export const LoadingOverlay = angular.module("loadingOverlay", [])
    .service("loadingOverlayService", LoadingOverlayService)
    .component("bpLoadingOverlay", new BpLoadingOverlayComponent())
    .name;

export {ILoadingOverlayService} from "./loadingOverlay.service"
