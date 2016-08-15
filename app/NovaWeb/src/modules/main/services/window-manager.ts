import { IWindowResize } from "../../core";

export enum ToggleAction {
    none,
    leftOpen,
    leftClose,
    rightOpen,
    rightClose
}

export interface IAvailableContentArea {
    width: number;
    height: number;
}

export interface IWindowManager {
    areBothSidebarsVisible: Rx.Observable<boolean>;
    isLeftSidebarVisible: Rx.Observable<boolean>;
    isRightSidebarVisible: Rx.Observable<boolean>;
    isWindowChanged: Rx.Observable<boolean>;
    isWidthChanged: Rx.Observable<boolean>;
    isHeightChanged: Rx.Observable<boolean>;
    getAvailableArea: Rx.Observable<IAvailableContentArea>;

    //@mtalis
    isConfigurationChanged: Rx.Observable<ToggleAction>;
}

export class WindowManager implements IWindowManager {
    public static $inject: [string] = ["windowResize"];

    private _areBothSidebarsVisible: Rx.BehaviorSubject<boolean>;
    private _isLeftSidebarVisible: Rx.BehaviorSubject<boolean>;
    private _isRightSidebarVisible: Rx.BehaviorSubject<boolean>;
    private _isWindowChanged: Rx.BehaviorSubject<boolean>;
    private _isWidthChanged: Rx.BehaviorSubject<boolean>;
    private _isHeightChanged: Rx.BehaviorSubject<boolean>;
    private _getAvailableArea: Rx.BehaviorSubject<IAvailableContentArea>;

    //@mtalis
    private _isConfigurationChanged: Rx.BehaviorSubject<ToggleAction>;

    private isLeftVisible: boolean;
    private isRightVisible: boolean;

    private sidebarSize: number = 270;
    private _width: number;
    private _height: number;

    private _toggleObserver: MutationObserver;
    private _messageObserver: MutationObserver;
    private _subscribers: Rx.IDisposable[];

    constructor(public windowResize: IWindowResize) {
        let sidebarWrapper = document.querySelector(".bp-sidebar-wrapper") as HTMLElement;
        if (sidebarWrapper) {
            this.isLeftVisible = sidebarWrapper.classList.contains("left-panel-visible");
            this.isRightVisible = sidebarWrapper.classList.contains("right-panel-visible");

            this._areBothSidebarsVisible = new Rx.BehaviorSubject<boolean>(this.isLeftVisible || this.isRightVisible);
            this._isLeftSidebarVisible = new Rx.BehaviorSubject<boolean>(this.isLeftVisible);
            this._isRightSidebarVisible = new Rx.BehaviorSubject<boolean>(this.isRightVisible);
            this._isWidthChanged = new Rx.BehaviorSubject<boolean>(true);

            //@mtalis
            this._isConfigurationChanged = new Rx.BehaviorSubject<ToggleAction>(ToggleAction.none);

            this._toggleObserver = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    if (mutation.attributeName === "class") {
                        this.isLeftVisible = sidebarWrapper.classList.contains("left-panel-visible");
                        this.isRightVisible = sidebarWrapper.classList.contains("right-panel-visible");

                        if (this._isLeftSidebarVisible.getValue() !== this.isLeftVisible || this._isRightSidebarVisible.getValue() !== this.isRightVisible) {
                            this.onAvailableAreaResized();

                            //@mtalis
                            let toggleAction: ToggleAction = (this.isLeftVisible && !this._isLeftSidebarVisible.getValue() ? 1 : 0) * ToggleAction.leftClose +
                                (!this.isLeftVisible && this._isLeftSidebarVisible.getValue() ? 1 : 0) * ToggleAction.leftOpen +
                                (this.isRightVisible && !this._isRightSidebarVisible.getValue() ? 1 : 0) * ToggleAction.rightClose +
                                (!this.isRightVisible && this._isRightSidebarVisible.getValue() ? 1 : 0) * ToggleAction.rightOpen;
                            this._isConfigurationChanged.onNext(toggleAction);
                        }
                        this._areBothSidebarsVisible.onNext(this.isLeftVisible || this.isRightVisible);
                        this._isLeftSidebarVisible.onNext(this.isLeftVisible);
                        this._isRightSidebarVisible.onNext(this.isRightVisible);
                    }
                });
            });
            try {
                this._toggleObserver.observe(sidebarWrapper, { attributes: true, childList: false, characterData: false, subtree: false });
            } catch (ex) {
                //this.messageService.addError(ex.message);
            }
        } else {
            this._areBothSidebarsVisible = new Rx.BehaviorSubject<boolean>(false);
            this._isLeftSidebarVisible = new Rx.BehaviorSubject<boolean>(false);
            this._isRightSidebarVisible = new Rx.BehaviorSubject<boolean>(false);
            this._isWidthChanged = new Rx.BehaviorSubject<boolean>(false);

            this.isLeftVisible = false;
            this.isRightVisible = false;

            //@mtalis
            this._isConfigurationChanged = new Rx.BehaviorSubject<ToggleAction>(ToggleAction.none);
        }

        let messageContainer: Element = document.querySelector(".message-container");
        if (messageContainer) {
            this._isHeightChanged = new Rx.BehaviorSubject<boolean>(true);

            this._messageObserver = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    this.onAvailableAreaResized();
                });
            });
            try {
                this._messageObserver.observe(messageContainer, { attributes: false, childList: true, characterData: false, subtree: false });
            } catch (ex) {
                //this.messageService.addError(ex.message);
            }
        } else {
            this._isHeightChanged = new Rx.BehaviorSubject<boolean>(false);
        }

        this._isWindowChanged = new Rx.BehaviorSubject<boolean>(true);

        this._getAvailableArea = new Rx.BehaviorSubject<IAvailableContentArea>({
            width: window.innerWidth,
            height: window.innerHeight
        });

        this._subscribers = [
            this.windowResize.width.subscribeOnNext(this.onAvailableAreaResized, this),
            this.windowResize.height.subscribeOnNext(this.onAvailableAreaResized, this)
        ];
    };

    private onAvailableAreaResized() {
        let width: number = this._width;
        this._width = window.innerWidth - (this.isLeftVisible ? this.sidebarSize : 0) - (this.isRightVisible ? this.sidebarSize : 0);

        let height: number = this._height;
        let pageContent = document.querySelector(".page-content") as HTMLElement;
        let pageHeading = document.querySelector(".page-heading") as HTMLElement;
        if (pageContent && pageHeading) {
            this._height = (pageContent.offsetHeight || 0) - (pageHeading.offsetHeight || 0);
        }

        if (this._width !== width || this._height !== height) {
            this._isWindowChanged.onNext(true);
            this._getAvailableArea.onNext({
                width: this._width,
                height: this._height
            });

            if (this._width !== width) {
                this._isWidthChanged.onNext(true);
            }

            if (this._height !== height) {
                this._isHeightChanged.onNext(true);
            }
        }
    }

    public dispose() {
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });

        this._areBothSidebarsVisible.dispose();
        this._isLeftSidebarVisible.dispose();
        this._isRightSidebarVisible.dispose();
        this._isWindowChanged.dispose();
        this._isWidthChanged.dispose();
        this._isHeightChanged.dispose();

        //@mtalis
        this._isConfigurationChanged.dispose();

        try {
            this._toggleObserver.disconnect();
            this._messageObserver.disconnect();
        } catch (ex) {
            //this.messageService.addError(ex.message);
        }
    };

    public get areBothSidebarsVisible(): Rx.Observable<boolean> {
        return this._areBothSidebarsVisible.distinctUntilChanged().asObservable();
    };

    public get isLeftSidebarVisible(): Rx.Observable<boolean> {
        return this._isLeftSidebarVisible.distinctUntilChanged().asObservable();
    };

    public get isRightSidebarVisible(): Rx.Observable<boolean> {
        return this._isRightSidebarVisible.distinctUntilChanged().asObservable();
    };

    public get isWindowChanged(): Rx.Observable<boolean> {
        return this._isWindowChanged.asObservable();
    };

    public get isWidthChanged(): Rx.Observable<boolean> {
        return this._isWidthChanged.asObservable();
    };

    public get isHeightChanged(): Rx.Observable<boolean> {
        return this._isHeightChanged.asObservable();
    };

    public get getAvailableArea(): Rx.Observable<IAvailableContentArea> {
        return this._getAvailableArea.asObservable();
    };

    //@mtalis
    public get isConfigurationChanged(): Rx.Observable<ToggleAction> {
        return this._isConfigurationChanged.asObservable();
    };
}
