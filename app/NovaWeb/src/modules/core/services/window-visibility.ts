export enum VisibilityStatus {
    Visible,
    Hidden,
    Focus,
    Blur
}

export interface IWindowVisibility {
    visibilityObservable: Rx.Observable<VisibilityStatus>;
}


export class WindowVisibility implements IWindowVisibility {
    private _subject: Rx.BehaviorSubject<VisibilityStatus>;

    private windowsHasFocus: boolean = true;
    //private tabIsHidden: boolean = false;

    // Make use of the Page Visibility API https://www.w3.org/TR/page-visibility/
    private documentHiddenProperty: string = "hidden";

    constructor() {
        this._subject = new Rx.BehaviorSubject<VisibilityStatus>(null);

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
        this._subject.dispose();

        document.removeEventListener("visibilitychange", this.windowVisibilityHandler);
        document.removeEventListener("mozvisibilitychange", this.windowVisibilityHandler);
        document.removeEventListener("webkitvisibilitychange", this.windowVisibilityHandler);
        document.removeEventListener("msvisibilitychange", this.windowVisibilityHandler);
        document.removeEventListener("onfocusin", this.windowVisibilityHandler);
        document.removeEventListener("onfocusout", this.windowVisibilityHandler);

        window.removeEventListener("focus", this.windowFocusHandler);
        window.removeEventListener("blur", this.windowBlurHandler);
    };

    private windowFocusHandler = (evt: Event) => {
        this.setStatus(evt, VisibilityStatus.Visible);
    };

    private windowBlurHandler = (evt: Event) => {
        if (document.activeElement === document.body) {
            this.setStatus(evt, VisibilityStatus.Hidden);
        }
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
        let status: boolean;
        evt = evt || window.event;
        
        if (evt.type in eventMap) {
            status = eventMap[evt.type];
        } else {
            status = document[this.documentHiddenProperty];
        }
        this.setStatus(evt, status ? VisibilityStatus.Hidden : VisibilityStatus.Visible);
    };

    private setStatus = (evt: Event, status: VisibilityStatus) => {
        this._subject.onNext(status);
    };

    public get visibilityObservable(): Rx.Observable<VisibilityStatus> {
        return this._subject.filter(it => it !== null).asObservable();
    };
}