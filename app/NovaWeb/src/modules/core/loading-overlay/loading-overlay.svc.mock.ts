import {ILoadingOverlayService} from "./loading-overlay.svc";

export class LoadingOverlayServiceMock implements ILoadingOverlayService {

    public get displayOverlay(): boolean {
        return true;
    }

    public dispose(): void {
        return undefined;
    }

    public beginLoading(): number {
        return 0;
    }


    public endLoading(id: number): void {
        return undefined;
    }
}
