
export interface IWindowResize {
    isResizing: Rx.Observable<boolean>;
    width: Rx.Observable<number>;
    height: Rx.Observable<number>;
}

export class WindowResize implements IWindowResize {
    private _isResizing: Rx.BehaviorSubject<boolean>;
    private _width: Rx.BehaviorSubject<number>;
    private _height: Rx.BehaviorSubject<number>;

    private tick: boolean = false;

    constructor() {
        this._isResizing = new Rx.BehaviorSubject<boolean>(false);
        this._width = new Rx.BehaviorSubject<number>(window.innerWidth);
        this._height = new Rx.BehaviorSubject<number>(window.innerHeight);

        window.addEventListener("resize", this.windowResizeHandler);
        window.addEventListener("beforeunload", () => {
            this.dispose();
        });
    };

    public dispose() {
        this._isResizing.dispose();
        this._width.dispose();
        this._height.dispose();

        window.removeEventListener("resize", this.windowResizeHandler);
    };

    private windowResizeHandler = () => {
        if (!this.tick) {
            // resize events can fire at a high rate. We throttle the event using requestAnimationFrame
            // ref: https://developer.mozilla.org/en-US/docs/Web/API/window/requestAnimationFrame
            window.requestAnimationFrame(() => {
                this._isResizing.onNext(true);
                this._width.onNext(window.innerWidth);
                this._height.onNext(window.innerHeight);

                this.tick = false;
            });
        }
        this.tick = true;
    };

    public get isResizing(): Rx.Observable<boolean> {
        return this._isResizing.asObservable();
    };

    public get width(): Rx.Observable<number> {
        return this._width.distinctUntilChanged().asObservable();
    };

    public get height(): Rx.Observable<number> {
        return this._height.distinctUntilChanged().asObservable();
    };
}
