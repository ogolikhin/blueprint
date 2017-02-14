import {IWindowResize} from "../../commonModule/services/windowResize";

export enum ResizeCause {
    unknown,
    browserResize,
    sidebarToggle
}

export enum SidebarToggled {
    none,
    left,
    right,
    both
}

export interface IMainWindow {
    width: number;
    height: number;
    contentWidth: number;
    contentHeight: number;
    isLeftSidebarOpen: boolean;
    isLeftSidebarExpanded: boolean;
    isRightSidebarOpen: boolean;
    causeOfChange: ResizeCause;
    sidebarToggled: SidebarToggled;
}

export interface IWindowManager {
    mainWindow: Rx.Observable<IMainWindow>;
}

export class WindowManager implements IWindowManager {
    public static $inject: [string] = ["windowResize", "$window", "$timeout"];

    private _mainWindow: Rx.BehaviorSubject<IMainWindow>;

    private _width: number;
    private _height: number;
    private _contentWidth: number;
    private _contentHeight: number;
    private _isLeftSidebarOpen: boolean = false;
    private _isLeftSidebarExpanded: boolean = false;
    private _isRightSidebarOpen: boolean = false;
    private _causeOfChange: ResizeCause;
    private _sidebarToggled: SidebarToggled;

    private _toggleObserver: MutationObserver;
    private _messageObserver: MutationObserver;
    private _sidebarTransitionDuration: number = 500; // default transition duration in ms

    private _subscribers: Rx.IDisposable[];

    constructor(public windowResize: IWindowResize, private $window: ng.IWindowService, private $timeout: ng.ITimeoutService) {
        this._causeOfChange = ResizeCause.unknown;
        this._sidebarToggled = SidebarToggled.none;

        const sidebarWrapper = this.$window.document.getElementsByClassName("bp-sidebar-wrapper").item(0) as HTMLElement;
        if (sidebarWrapper) {
            this._isLeftSidebarOpen = sidebarWrapper.classList.contains("left-panel-visible");
            this._isLeftSidebarExpanded = sidebarWrapper.classList.contains("left-panel-expanded");
            this._isRightSidebarOpen = sidebarWrapper.classList.contains("right-panel-visible");

            this._sidebarTransitionDuration = this.getTransitionDuration() || this._sidebarTransitionDuration;

            this._toggleObserver = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    if (mutation.attributeName === "class") {
                        const isLeftSidebarOpen = sidebarWrapper.classList.contains("left-panel-visible");
                        const isLeftSidebarExpanded = sidebarWrapper.classList.contains("left-panel-expanded");
                        const isRightSidebarOpen = sidebarWrapper.classList.contains("right-panel-visible");

                        if (
                            this._isLeftSidebarOpen !== isLeftSidebarOpen ||
                            this._isLeftSidebarExpanded !== isLeftSidebarExpanded ||
                            this._isRightSidebarOpen !== isRightSidebarOpen
                        ) {
                            this._causeOfChange = ResizeCause.sidebarToggle;

                            if (this._isLeftSidebarOpen !== isLeftSidebarOpen && this._isRightSidebarOpen !== isRightSidebarOpen) {
                                this._sidebarToggled = SidebarToggled.both;
                            } else if (this._isRightSidebarOpen !== isRightSidebarOpen) {
                                this._sidebarToggled = SidebarToggled.right;
                            } else {
                                this._sidebarToggled = SidebarToggled.left;
                            }

                            this._isLeftSidebarOpen = isLeftSidebarOpen;
                            this._isLeftSidebarExpanded = isLeftSidebarExpanded;
                            this._isRightSidebarOpen = isRightSidebarOpen;

                            this.onWindowResize();
                        }
                    }
                });
            });
            try {
                this._toggleObserver.observe(sidebarWrapper, {
                    attributes: true,
                    childList: false,
                    characterData: false,
                    subtree: false
                });
            } catch (ex) {
                //this.messageService.addError(ex.message);
            }
        }

        this._width = this.$window.innerWidth;
        this._contentWidth = this.getContentWidth();

        this._height = this.$window.innerHeight;
        this._contentHeight = this.getContentHeight();

        this._mainWindow = new Rx.BehaviorSubject<IMainWindow>({
            width: this._width,
            height: this._height,
            contentWidth: this._contentWidth,
            contentHeight: this._contentHeight,
            isLeftSidebarOpen: this._isLeftSidebarOpen,
            isLeftSidebarExpanded: this._isLeftSidebarExpanded,
            isRightSidebarOpen: this._isRightSidebarOpen,
            causeOfChange: this._causeOfChange,
            sidebarToggled: this._sidebarToggled
        });

        this._subscribers = [
            this.windowResize.width.subscribeOnNext(this.onBrowserResize, this),
            this.windowResize.height.subscribeOnNext(this.onBrowserResize, this)
        ];
    };

    private getTransitionDuration(): number {
        let transitionDuration: number;

        const sidebar = this.$window.document.getElementsByClassName("sidebar").item(0) as HTMLElement;
        if (sidebar) {
            const computedTransitionDuration = this.$window.getComputedStyle(sidebar).getPropertyValue("transition-duration");
            transitionDuration = parseFloat(computedTransitionDuration);
            transitionDuration = isNaN(transitionDuration) ?
                null :
                transitionDuration * (computedTransitionDuration.indexOf("ms") === -1 ? 1000 : 1); // from seconds to milliseconds
        }

        return transitionDuration;
    }

    private getContentWidth(): number {
        let sidebarsWidth = 0;

        const leftSidebar = this.$window.document.querySelector(".sidebar.left-panel") as HTMLElement;
        if (this._isLeftSidebarOpen && leftSidebar) {
            const leftSidebarWidth = _.parseInt(this.$window.getComputedStyle(leftSidebar).getPropertyValue("width"));
            sidebarsWidth += isNaN(leftSidebarWidth) ? 0 : leftSidebarWidth;
        }

        const rightSidebar = this.$window.document.querySelector(".sidebar.right-panel") as HTMLElement;
        if (this._isRightSidebarOpen && rightSidebar) {
            const rightSidebarWidth = _.parseInt(this.$window.getComputedStyle(rightSidebar).getPropertyValue("width"));
            sidebarsWidth += isNaN(rightSidebarWidth) ? 0 : rightSidebarWidth;
        }
        return this._width - sidebarsWidth;
    }

    private getContentHeight(): number {
        let height: number = this._height;
        const pageContent = this.$window.document.getElementsByClassName("page-content").item(0) as HTMLElement;
        const pageHeading = this.$window.document.getElementsByClassName("page-heading").item(0) as HTMLElement;
        if (pageContent && pageHeading) {
            height = (pageContent.offsetHeight || 0) - (pageHeading.offsetHeight || 0);
        }
        return height;
    }

    private onBrowserResize() {
        this._causeOfChange = ResizeCause.browserResize;
        this._sidebarToggled = SidebarToggled.none;
        this.onWindowResize();
    }

    private onWindowResize() {
        let timeout = 0;
        if (this._causeOfChange === ResizeCause.sidebarToggle) {
            // if the resize event has been caused by a sidebar toggle, we need to wait for the animation to finish before emitting
            timeout = this._sidebarTransitionDuration;
            // we add a small delay so to make sure the transition has ended
            timeout += 50;
        }

        this.$timeout(() => {
            this._width = this.$window.innerWidth;
            this._contentWidth = this.getContentWidth();

            this._height = this.$window.innerHeight;
            this._contentHeight = this.getContentHeight();

            this._mainWindow.onNext({
                width: this._width,
                height: this._height,
                contentWidth: this._contentWidth,
                contentHeight: this._contentHeight,
                isLeftSidebarOpen: this._isLeftSidebarOpen,
                isLeftSidebarExpanded: this._isLeftSidebarExpanded,
                isRightSidebarOpen: this._isRightSidebarOpen,
                causeOfChange: this._causeOfChange,
                sidebarToggled: this._sidebarToggled
            });
        }, timeout);
    }

    public dispose() {
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });

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
