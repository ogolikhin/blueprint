export interface IWindowVisibility {
    isHidden: Rx.Observable<boolean>;
}

export class WindowVisibility implements IWindowVisibility {
    private _isHidden: Rx.BehaviorSubject<boolean>;

    private windowsHasFocus: boolean = true;
    private tabIsHidden: boolean = false;

    // Make use of the Page Visibility API https://www.w3.org/TR/page-visibility/
    private documentHiddenProperty: string = "hidden";

    constructor() {
        this._isHidden = new Rx.BehaviorSubject<boolean>(false);

        // W3C standards:
        if (this.documentHiddenProperty in document) {
            document.addEventListener("visibilitychange", this.windowVisibilityHandler);
        } else if ((this.documentHiddenProperty = "mozHidden") in document) { // Firefox 10+
            document.addEventListener("mozvisibilitychange", this.windowVisibilityHandler);
        } else if ((this.documentHiddenProperty = "webkitHidden") in document) { // Chrome 13+
            document.addEventListener("webkitvisibilitychange", this.windowVisibilityHandler);
        } else if ((this.documentHiddenProperty = "msHidden") in document) { // IE 10+
            document.addEventListener("msvisibilitychange", this.windowVisibilityHandler);
        } else if ("onfocusin" in document) { // IE 9-
            document["onfocusin"] = document["onfocusout"] = this.windowVisibilityHandler;
        }

        window.addEventListener("focus", this.windowFocusHandler);
        window.addEventListener("blur", this.windowBlurHandler);
    };

    public dispose() {
        this._isHidden.dispose();

        document.removeEventListener("visibilitychange", this.windowVisibilityHandler);
        document.removeEventListener("mozvisibilitychange", this.windowVisibilityHandler);
        document.removeEventListener("webkitvisibilitychange", this.windowVisibilityHandler);
        document.removeEventListener("msvisibilitychange", this.windowVisibilityHandler);
        document.removeEventListener("onfocusin", this.windowVisibilityHandler);
        document.removeEventListener("onfocusout", this.windowVisibilityHandler);

        window.removeEventListener("focus", this.windowFocusHandler);
        window.removeEventListener("blur", this.windowBlurHandler);
    };

    private windowFocusHandler = () => {
        this.windowsHasFocus = true;
        this.checkGlobalVisibility();
    };

    private windowBlurHandler = () => {
        this.windowsHasFocus = false;
        this.checkGlobalVisibility();
    };

    private windowVisibilityHandler = (evt: Event) => {
        let visible: boolean = false, hidden: boolean = true;
        let eventMap = {
            focus: visible,
            focusin: visible,
            pageshow: visible,
            blur: hidden,
            focusout: hidden,
            pagehide: hidden
        };

        evt = evt || window.event;
        if (evt.type in eventMap) {
            this.tabIsHidden = eventMap[evt.type];
        } else {
            this.tabIsHidden = document[this.documentHiddenProperty];
        }
        this.checkGlobalVisibility();
    };

    private checkGlobalVisibility = () => {
        this._isHidden.onNext(this.windowsHasFocus || !this.tabIsHidden);
    };

    public get isHidden(): Rx.Observable<boolean> {
        return this._isHidden.asObservable();
    };
}