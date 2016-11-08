import {LoadingOverlayService} from "./loading-overlay.svc";
import {BpLoadingOverlayComponent} from "./bp-loading-overlay";

angular.module("bp.core", [])
    .service("loadingOverlayService", LoadingOverlayService)
    .component("bpLoadingOverlay", new BpLoadingOverlayComponent());

