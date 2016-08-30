import { ILoadingOverlayService, LoadingOverlayService } from "./loading-overlay.svc";
import { LoadingOverlayComponent } from "./loading-overlay";

angular.module("bp.core.loadingOverlay", [])
    .service("loadingOverlayService", LoadingOverlayService)
    .component("loadingOverlay", new LoadingOverlayComponent());

export { ILoadingOverlayService, LoadingOverlayService, LoadingOverlayComponent }