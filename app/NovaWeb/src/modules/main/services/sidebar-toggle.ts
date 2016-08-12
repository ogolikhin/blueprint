export enum ToggleAction {
    none,
    leftOpen,
    leftClose,
    rightOpen,
    rightClose
}

export interface ISidebarToggle {
    areBothSidebarsVisible: Rx.Observable<boolean>;
    isLeftSidebarVisible: Rx.Observable<boolean>;
    isRightSidebarVisible: Rx.Observable<boolean>;
    isConfigurationChanged: Rx.Observable<ToggleAction>;
}

export class SidebarToggle implements ISidebarToggle {
    private _areBothSidebarsVisible: Rx.BehaviorSubject<boolean>;
    private _isLeftSidebarVisible: Rx.BehaviorSubject<boolean>;
    private _isRightSidebarVisible: Rx.BehaviorSubject<boolean>;
    private _isConfigurationChanged: Rx.BehaviorSubject<ToggleAction>;

    private _observer: MutationObserver;

    constructor() {
        let wrapper: Element = document.querySelector(".bp-sidebar-wrapper");
        if (wrapper) {
            let isLeftVisible: boolean = wrapper.classList.contains("left-panel-visible");
            let isRightVisible: boolean = wrapper.classList.contains("right-panel-visible");

            this._areBothSidebarsVisible = new Rx.BehaviorSubject<boolean>(isLeftVisible || isRightVisible);
            this._isLeftSidebarVisible = new Rx.BehaviorSubject<boolean>(isLeftVisible);
            this._isRightSidebarVisible = new Rx.BehaviorSubject<boolean>(isRightVisible);
            this._isConfigurationChanged = new Rx.BehaviorSubject<ToggleAction>(ToggleAction.none);

            this._observer = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    if (mutation.attributeName === "class") {
                        isLeftVisible = wrapper.classList.contains("left-panel-visible");
                        isRightVisible = wrapper.classList.contains("right-panel-visible");

                        if (this._isLeftSidebarVisible.getValue() !== isLeftVisible || this._isRightSidebarVisible.getValue() !== isRightVisible) {
                            let toggleAction: ToggleAction = (isLeftVisible && !this._isLeftSidebarVisible.getValue() ? 1 : 0) * ToggleAction.leftClose +
                                                             (!isLeftVisible && this._isLeftSidebarVisible.getValue() ? 1 : 0) * ToggleAction.leftOpen +
                                                             (isRightVisible && !this._isRightSidebarVisible.getValue() ? 1 : 0) * ToggleAction.rightClose +
                                                             (!isRightVisible && this._isRightSidebarVisible.getValue() ? 1 : 0) * ToggleAction.rightOpen;

                            this._isConfigurationChanged.onNext(toggleAction);
                        }
                        this._areBothSidebarsVisible.onNext(isLeftVisible || isRightVisible);
                        this._isLeftSidebarVisible.onNext(isLeftVisible);
                        this._isRightSidebarVisible.onNext(isRightVisible);
                    }
                });
            });
            try {
                this._observer.observe(wrapper, { attributes: true });
            } catch (ex) {
                //this.messageService.addError(ex.message);
            }
        } else {
            this._areBothSidebarsVisible = new Rx.BehaviorSubject<boolean>(false);
            this._isLeftSidebarVisible = new Rx.BehaviorSubject<boolean>(false);
            this._isRightSidebarVisible = new Rx.BehaviorSubject<boolean>(false);
            this._isConfigurationChanged = new Rx.BehaviorSubject<ToggleAction>(ToggleAction.none);
        }
    };

    public dispose() {
        this._areBothSidebarsVisible.dispose();
        this._isLeftSidebarVisible.dispose();
        this._isRightSidebarVisible.dispose();
        this._isConfigurationChanged.dispose();

        try {
            this._observer.disconnect();
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

    public get isConfigurationChanged(): Rx.Observable<ToggleAction> {
        return this._isConfigurationChanged.asObservable();
    };
}
