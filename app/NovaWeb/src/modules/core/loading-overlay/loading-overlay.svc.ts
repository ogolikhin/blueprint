export interface ILoadingOverlayService {

    /**
      * Display the loading overlay
      * @return {number} The id to pass to endLoading()
      */
    beginLoading(): number;

    /**
      * Hide the loading overlay (if nothing else is using it)
      * @param {number} id - The id given by beginLoading()
      */
    endLoading(id: number): void;

    displayOverlay: boolean;
    dispose(): void;
}

export class LoadingOverlayService implements ILoadingOverlayService {

    public static $inject = [];
    constructor() {
        this.initialize();
    }

    private loadingIds: Array<number>;

    public initialize = () => {
        this.loadingIds = new Array<number>();
    }

    public get displayOverlay(): boolean {
        return this.loadingIds.length > 0;
    }

    public dispose(): void {
        this.loadingIds = new Array<number>();
    }

    public beginLoading(): number {
        let randomId = Math.random();
        this.loadingIds.push(randomId);
        return randomId;
    }


    public endLoading(id: number): void {
        let index = this.loadingIds.indexOf(id);
        if (index < 0) {
            //Error, endLoading was called twice, or beginLoading wasn't called, or dispose was called
        }
        else {
            this.loadingIds.splice(index, 1);
        }
    }
}
