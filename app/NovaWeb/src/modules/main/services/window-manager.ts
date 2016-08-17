import { IWindowResize } from "../../core";

export enum ResizeCause {
    unknown,
    browserResize,
    sidebarToggle,
    errorMessage
}

export interface IMainWindow {
    width: number;
    height: number;
    contentWidth: number;
    contentHeight: number;
    isLeftSidebarOpen: boolean;
    isRightSidebarOpen: boolean;
    causeOfChange: ResizeCause;
}

export interface IWindowManager {
    mainWindow: Rx.Observable<IMainWindow>;
}

export class WindowManager implements IWindowManager {
    public static $inject: [string] = ["windowResize"];

    private _mainWindow: Rx.BehaviorSubject<IMainWindow>;

    private _width: number;
    private _height: number;
    private _contentWidth: number;
    private _contentHeight: number;
    private _isLeftSidebarOpen: boolean;
    private _isRightSidebarOpen: boolean;
    private _causeOfChange: ResizeCause;

    private sidebarSize: number = 270;

    private _toggleObserver: MutationObserver;
    private _messageObserver: MutationObserver;

    private _subscribers: Rx.IDisposable[];

    constructor(public windowResize: IWindowResize) {
        this._causeOfChange = ResizeCause.unknown;

        let sidebarWrapper = document.querySelector(".bp-sidebar-wrapper") as HTMLElement;
        if (sidebarWrapper) {
            this._isLeftSidebarOpen = sidebarWrapper.classList.contains("left-panel-visible");
            this._isRightSidebarOpen = sidebarWrapper.classList.contains("right-panel-visible");

            this._toggleObserver = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    if (mutation.attributeName === "class") {
                        if (
                            this._isLeftSidebarOpen !== sidebarWrapper.classList.contains("left-panel-visible") ||
                            this._isRightSidebarOpen !== sidebarWrapper.classList.contains("right-panel-visible")
                        ) {
                            this._causeOfChange = ResizeCause.sidebarToggle;
                            this._isLeftSidebarOpen = sidebarWrapper.classList.contains("left-panel-visible");
                            this._isRightSidebarOpen = sidebarWrapper.classList.contains("right-panel-visible");

                            this.onWindowResize();
                        }
                    }
                });
            });
            try {
                this._toggleObserver.observe(sidebarWrapper, { attributes: true, childList: false, characterData: false, subtree: false });
            } catch (ex) {
                //this.messageService.addError(ex.message);
            }
        } else {
            this._isLeftSidebarOpen = false;
            this._isRightSidebarOpen = false;
        }

        let messageContainer: Element = document.querySelector(".message-container");
        if (messageContainer) {
            this._messageObserver = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    this._causeOfChange = ResizeCause.errorMessage;

                    this.onWindowResize();
                });
            });
            try {
                this._messageObserver.observe(messageContainer, { attributes: false, childList: true, characterData: false, subtree: false });
            } catch (ex) {
                //this.messageService.addError(ex.message);
            }
        }

        this._width = window.innerWidth;
        this._contentWidth = this.getContentWidth();

        this._height = window.innerHeight;
        this._contentHeight = this.getContentHeight();

        this._mainWindow = new Rx.BehaviorSubject<IMainWindow>({
            width: this._width,
            height: this._height,
            contentWidth: this._contentWidth,
            contentHeight: this._contentHeight,
            isLeftSidebarOpen: this._isLeftSidebarOpen,
            isRightSidebarOpen: this._isRightSidebarOpen,
            causeOfChange: this._causeOfChange
        });

        this._subscribers = [
            this.windowResize.width.subscribeOnNext(this.onBrowserResize, this),
            this.windowResize.height.subscribeOnNext(this.onBrowserResize, this)
        ];
    };

    private getContentWidth(): number {
        return this._width - (this._isLeftSidebarOpen ? this.sidebarSize : 0) - (this._isRightSidebarOpen ? this.sidebarSize : 0);
    }

    private getContentHeight(): number {
        let height: number = this._height;
        let pageContent = document.querySelector(".page-content") as HTMLElement;
        let pageHeading = document.querySelector(".page-heading") as HTMLElement;
        if (pageContent && pageHeading) {
            height = (pageContent.offsetHeight || 0) - (pageHeading.offsetHeight || 0);
        }
        return height;
    }

    private onBrowserResize() {
        this._causeOfChange = ResizeCause.browserResize;
        this.onWindowResize();
    }

    private onWindowResize() {
        this._width = window.innerWidth;
        this._contentWidth = this.getContentWidth();

        this._height = window.innerHeight;
        this._contentHeight = this.getContentHeight();

        this._mainWindow.onNext({
            width: this._width,
            height: this._height,
            contentWidth: this._contentWidth,
            contentHeight: this._contentHeight,
            isLeftSidebarOpen: this._isLeftSidebarOpen,
            isRightSidebarOpen: this._isRightSidebarOpen,
            causeOfChange: this._causeOfChange
        });
    }

    public dispose() {
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });

        this._mainWindow.dispose();

        try {
            this._toggleObserver.disconnect();
            this._messageObserver.disconnect();

            this._toggleObserver = null;
            this._messageObserver = null;
        } catch (ex) {
            //this.messageService.addError(ex.message);
        }
    };

    public get mainWindow(): Rx.Observable<IMainWindow> {
        return this._mainWindow.asObservable();
    };
}
