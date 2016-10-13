export interface IWindowVisibility {
    isHidden: Rx.Observable<boolean>;
}

export class WindowVisibility implements IWindowVisibility {
    private _isHidden: Rx.BehaviorSubject<boolean>;

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
        } else { // everything else
            window.onpageshow
                = window.onpagehide
                = window.onfocus
                = window.onblur
                = this.windowVisibilityHandler;
        }

        window.addEventListener("beforeunload", () => {
            this.dispose();
        });
    };

    public dispose() {
        this._isHidden.dispose();

        window.removeEventListener("visibilitychange", this.windowVisibilityHandler);
        window.removeEventListener("mozvisibilitychange", this.windowVisibilityHandler);
        window.removeEventListener("webkitvisibilitychange", this.windowVisibilityHandler);
        window.removeEventListener("msvisibilitychange", this.windowVisibilityHandler);
        window.removeEventListener("onfocusin", this.windowVisibilityHandler);
        window.removeEventListener("onfocusout", this.windowVisibilityHandler);
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
            this._isHidden.onNext(eventMap[evt.type]);
        } else {
            this._isHidden.onNext(document[this.documentHiddenProperty]);
        }
    };

    public get isHidden(): Rx.Observable<boolean> {
        return this._isHidden.distinctUntilChanged().asObservable();
    };
}
