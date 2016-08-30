export interface ILoadingOverlayService {
    BeginLoading(): void;
    EndLoading(): void;    
    DisplayOverlay: boolean;
    dispose(): void;
}

export class LoadingOverlayService implements ILoadingOverlayService {

    public static $inject = [];
    constructor() {
        this.initialize();
    }

    public initialize = () => {
        this._displayOverlay = false;

        window.overlayservice = this; //TODO: Remove once finished debugging
    }

    private _displayOverlay: boolean;

    public get DisplayOverlay(): boolean {
        return this._displayOverlay;
    }

    public dispose(): void {
        //Remove the overlay?
        //When do we dispose?
    }

    public BeginLoading(): void {
        this._displayOverlay = true;
        console.log(this._displayOverlay);
        //Increment counter., display overlay.
    }

    public EndLoading(): void {
        this._displayOverlay = false;
        console.log(this._displayOverlay);
        //Decrement counter. If 0, remove overlay.
    }
}
