export interface ILoadingOverlayService {
    beginLoading(): void;
    endLoading(): void;    
    displayOverlay: boolean;
    dispose(): void;
}

export class LoadingOverlayService implements ILoadingOverlayService {

    public static $inject = [];
    constructor() {
        this.initialize();
    }

    private loadingCounter: number;

    public initialize = () => {
        this._displayOverlay = false;
        this.loadingCounter = 0;
    }

    private _displayOverlay: boolean;
    public get displayOverlay(): boolean {
        return this._displayOverlay;
    }

    public dispose(): void {
        //Remove the overlay?
        //When do we dispose?
    }

    public beginLoading(): void {
        this._displayOverlay = true;
        this.loadingCounter++;
        
    }

    public endLoading(): void {
        this.loadingCounter--;
        if (this.loadingCounter < 0) {
            //TODO: Error; something called loading in the wrong order.
            this.loadingCounter = 0;
        }

        if (this.loadingCounter === 0) {
            this._displayOverlay = false;
        }
    }
}
