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

    static $inject: [string] = [
        "$window",
        "$document"
    ];
    private _subject: Rx.BehaviorSubject<VisibilityStatus>;

    private windowsHasFocus: boolean = true;
    //private tabIsHidden: boolean = false;

    // Make use of the Page Visibility API https://www.w3.org/TR/page-visibility/
    private documentHiddenProperty: string = "hidden";

    constructor(
        private $window: ng.IWindowService,
        private $document: ng.IDocumentService) {
        this._subject = new Rx.BehaviorSubject<VisibilityStatus>(null);

        // W3C standards:
        if (this.documentHiddenProperty in this.$document) {
            this.$document.on("visibilitychange", this.windowVisibilityHandler);
        } else if ((this.documentHiddenProperty = "mozHidden") in this.$document) { // Firefox 10+
            this.$document.on("mozvisibilitychange", this.windowVisibilityHandler);
        } else if ((this.documentHiddenProperty = "webkitHidden") in this.$document) { // Chrome 13+
            this.$document.on("webkitvisibilitychange", this.windowVisibilityHandler);
        } else if ((this.documentHiddenProperty = "msHidden") in this.$document) { // IE 10+
            this.$document.on("msvisibilitychange", this.windowVisibilityHandler);
        } else if ("onfocusin" in this.$document) { // IE 9-
            this.$document["onfocusin"] = this.$document["onfocusout"] = this.windowVisibilityHandler;
        }

        this.$window.addEventListener("focus", this.windowFocusHandler);
        this.$window.addEventListener("blur", this.windowBlurHandler);
    };

    public dispose() { 
        this._subject.dispose();

        this.$document.off("visibilitychange", this.windowVisibilityHandler);
        this.$document.off("mozvisibilitychange", this.windowVisibilityHandler);
        this.$document.off("webkitvisibilitychange", this.windowVisibilityHandler);
        this.$document.off("msvisibilitychange", this.windowVisibilityHandler);
        this.$document.off("onfocusin", this.windowVisibilityHandler);
        this.$document.off("onfocusout", this.windowVisibilityHandler);

        this.$window.removeEventListener("focus", this.windowFocusHandler);
        this.$window.removeEventListener("blur", this.windowBlurHandler);
    };

    private windowFocusHandler = (evt: Event) => {
        this.setStatus(evt, VisibilityStatus.Visible);
    };

    private windowBlurHandler = (evt: Event) => {
        // if (document.activeElement === document.body) {
            this.setStatus(evt, VisibilityStatus.Hidden);
        // }
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