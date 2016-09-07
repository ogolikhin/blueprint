import { ILoadingOverlayService, LoadingOverlayService } from "./loading-overlay.svc";
import { BpLoadingOverlayComponent } from "./bp-loading-overlay";

angular.module("bp.core.loadingOverlay", [])
    .service("loadingOverlayService", LoadingOverlayService)
    .component("bpLoadingOverlay", new BpLoadingOverlayComponent());

export { ILoadingOverlayService, LoadingOverlayService, BpLoadingOverlayComponent }