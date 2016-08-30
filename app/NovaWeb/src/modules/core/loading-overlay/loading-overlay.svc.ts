export interface ILoadingOverlayService {
    BeginLoading(): void;
    EndLoading(): void;    
    DisplayOverlay: boolean;
    dispose(): void;
}

export class LoadingOverlayService implements ILoadingOverlayService {
    private timers: { [id: number]: ng.IPromise<any>; } = {};
    private count: number = 0;

    public static $inject = [];
    constructor() {
        this.initialize();
    }

    public initialize = () => {
        this.DisplayOverlay = false;
    }

    public DisplayOverlay: boolean

    public dispose(): void {
        //Remove the overlay?
        //When do we dispose?
    }

    public BeginLoading(): void {
        //Increment counter., display overlay.
    }

    public EndLoading(): void {
        //Decrement counter. If 0, remove overlay.
    }
}
