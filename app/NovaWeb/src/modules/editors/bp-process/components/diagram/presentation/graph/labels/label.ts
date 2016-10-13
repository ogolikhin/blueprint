import * as angular from "angular";
export var ELLIPSIS_SYMBOL = String.fromCharCode(8230);

export interface ILabel {
    render(): void;
    text: string;
    setVisible(value: boolean);
    onDispose(): void;
}

enum divMode {
    VIEW = 0,
    EDIT = 1,
}

enum action {
    ENTER = 13,
    CANCEL = 27,
    DELETE = 46,
    BACKSPASE = 8,
    LEFT = 37,
    UP = 38,
    RIGHT = 39,
    DOWN = 40,
    HOME = 47,
    END = 35
}

class SelectionOffsets {
    caretOffset: number;
    selectionStart: number;
    selectionEnd: number;
}

export class LabelStyle {
    constructor(private _fontFamily: string,
                private _fontSize: number,
                private _backColor: string,
                private _textColor: string,
                private _fontWeight: string,
                private _top: number,
                private _left: number,
                private _height: number,
                private _width: number,
                private _highlitedTextColor) {
    }

    public get fontFamily(): string {
        return this._fontFamily;
    }

    public get fontSize(): number {
        return this._fontSize;
    }

    public get backColor(): string {
        return this._backColor;
    }

    public get textColor(): string {
        return this._textColor;
    }

    public get fontWeight(): string {
        return this._fontWeight;
    }

    public get top(): number {
        return this._top;
    }

    public get left(): number {
        return this._left;
    }

    public get height(): number {
        return this._height;
    }

    public get width(): number {
        return this._width;
    }

    public get highlitedTextColor(): string {
        return this._highlitedTextColor;
    }
}

export class Label implements ILabel {
    public wrapperDiv: HTMLDivElement;
    private div: HTMLDivElement;
    private mode: divMode;
    // #UNUSED
    //private shortContent: string;
    private visibility: string;
    private executionEnvironmentDetector: any;

    public get text() {
        return this._text;
    }

    public set text(value) {
        this._text = value;
        this.setShortText();
    }

    public setVisible(value: boolean) {
        this.visibility = (value) ? "visible" : "hidden";
        if (this.wrapperDiv != null) {
            this.wrapperDiv.style.visibility = this.visibility;
            this.div.style.visibility = this.visibility;
        }
    }

    private isIe11(): boolean {
        let myBrowser = this.executionEnvironmentDetector.getBrowserInfo();
        let ver = parseInt(myBrowser.version, 10);
        return (myBrowser.msie && (ver === 11));
    }

    constructor(private callback: any,
                private container: HTMLElement,
                private parentId: string,
                private id: string,
                private _text: string,
                private style: LabelStyle,
                private maxTextLength: number,
                private maxVisibleTextLength: number,
                private isReadOnly: boolean,
                private textAlign: string = "center") {
        if (!_text) {
            this._text = "";
        }

        // This is temporary code. It will be replaced with
        // a class that wraps this global functionality.
        let w: any = window;
        this.executionEnvironmentDetector = new w.executionEnvironmentDetector();
        this.mode = divMode.VIEW;
    }

    private onEdit = (e) => {
        if (this.mode === divMode.VIEW) {
            this.setEditMode();
        }
        e.stopPropagation();
        this.cancelDefaultAction(e);
    }

    private onKeyDown = (e) => {
        if (e.keyCode === action.ENTER) {
            this.update();
            this.cancelDefaultAction(e);
            return false;
        } else if (e.keyCode === action.CANCEL) {
            this.undo();
            this.cancelDefaultAction(e);
            return false;
        }

        let offsets = this.getCaretCharacterOffsetWithin();

        if ((this.div.innerText.length >= this.maxTextLength) && (offsets.selectionEnd === offsets.selectionStart) &&
            (e.keyCode !== action.DELETE) && (e.keyCode !== action.BACKSPASE) &&
            (e.keyCode !== action.LEFT) && (e.keyCode !== action.RIGHT) &&
            (e.keyCode !== action.UP) && (e.keyCode !== action.DOWN) &&
            (e.keyCode !== action.HOME) && (e.keyCode !== action.END)) {
            this.cancelDefaultAction(e);
            return false;
        }
    }

    private onPaste = (e) => {
        let win: any = window;
        let text: string;
        //#UNUSED
        //let oldValue: string = this.div.innerText;
        let clipboardData = win.clipboardData || e.clipboardData || e.originalEvent.clipboardData;
        text = clipboardData.getData("Text");
        let offsets = this.getCaretCharacterOffsetWithin();
        let tmpStr: string = this.splice(this.div.innerText, offsets.selectionStart, offsets.selectionEnd - offsets.selectionStart, text);

        // replace eol with space
        tmpStr = tmpStr.replace(/\r\n/g, " ").replace(/\r/g, " ").replace(/\n/g, " ");

        if (tmpStr.length <= this.maxTextLength) {
            this.div.innerText = tmpStr;
        } else {
            this.div.innerText = tmpStr.substring(0, this.maxTextLength - 1);
        }
        this.cancelDefaultAction(e);
    }

    private update() {
        if (this.mode === divMode.EDIT) {
            const innerText = this.div.innerText.replace(/\n/g, "");
            this._text = innerText;
            this.callback(innerText);
            //window.console.log("update() ");
            this.setViewMode();
        }
    }

    private setShortText() {
        this.div.innerText = this.getShortText();
    }

    public getShortText(): string {
        let value: string = this._text;
        if (this._text.length > this.maxVisibleTextLength) {
            value = this._text.substring(0, this.maxVisibleTextLength - 1) + ELLIPSIS_SYMBOL;
        }
        return value;
    }

    private undo() {
        //window.console.log("undo() ");

        this.setViewMode();
    }

    private setEditMode() {
        this.div.removeEventListener("blur", this.onBlur, true);

        window.focus();
        this.mode = divMode.EDIT;
        this.setMouseoverStyle();
        this.div.innerText = this._text;
        this.div.setAttribute("contenteditable", "true");
        this.div.focus();
        this.wrapperDiv.style.pointerEvents = "auto";
        //window.console.log("setEditMode this.mode = " + this.mode);
        this.selectText();

        setTimeout(this.addDeferedListener, 300, this.div);
    }

    private addDeferedListener = (elem) => {
        elem.addEventListener("blur", this.onBlur, true);
    }

    private setViewMode() {
        this.mode = divMode.VIEW;
        this.div.setAttribute("contenteditable", "false");
        this.setShortText();
        this.wrapperDiv.style.pointerEvents = "none";

        //window.console.log("setEditMode this.mode = " + this.mode);

    }

    private onMouseover = (e) => {
        this.setMouseoverStyle();
    }

    private onMouseout = (e) => {
        this.setMouseoutStyle();
    }

    private setMouseoverStyle() {
        if (this.mode === divMode.VIEW) {
            this.div.style.background = "url('/novaweb/static/bp-process/images/pencil.png') no-repeat top right";
            this.div.style.borderStyle = "dashed";
            this.div.style.borderWidth = "thin";
            this.div.style.borderColor = "#666";
            this.div.style.backgroundColor = "#c7edf8";
            this.div.style.color = this.style.highlitedTextColor;
        } else {
            this.setMouseoutStyle();
        }
    }

    private setMouseoutStyle() {
        this.div.style.border = "none";
        this.div.style.background = "none";
        this.div.style.backgroundColor = this.style.backColor;
        this.div.style.color = this.style.textColor;
    }

    private onBlur = (e) => {
        this.update();
    }

    public render() {
        this.wrapperDiv = document.createElement("div");
        this.wrapperDiv.style.display = "table";
        this.wrapperDiv.style.position = "absolute";
        this.wrapperDiv.style.top = this.numberToPx(this.style.top);
        this.wrapperDiv.style.left = this.numberToPx(this.style.left);
        this.wrapperDiv.style.width = this.numberToPx(this.style.width);
        this.wrapperDiv.style.minHeight = this.numberToPx(this.style.height);
        this.wrapperDiv.style.height = this.numberToPx(this.style.height);
        this.wrapperDiv.style.backgroundColor = this.style.backColor;
        this.wrapperDiv.style.visibility = this.visibility;

        this.div = document.createElement("div");
        this.div.id = this.id;
        this.div.className = "processEditorCustomLabel";
        this.div.style.overflow = "hidden";
        this.div.style.fontFamily = this.style.fontFamily;
        this.div.style.fontSize = this.numberToPx(this.style.fontSize);
        this.div.style.fontWeight = this.style.fontWeight;
        this.div.style.display = "table-cell";
        this.div.style.textAlign = this.textAlign;
        this.div.style.verticalAlign = "middle";
        this.div.setAttribute("stLabelX", this.style.left.toString());
        this.div.setAttribute("stLabelY", this.style.top.toString());
        //this.div.style.wordBreak = "break-all";
        //this.div.style.whiteSpace = "wrap";
        this.div.style.wordWrap = "break-word";
        this.div.style.width = this.numberToPx(this.style.width);
        this.div.style.maxWidth = this.numberToPx(this.style.width);
        this.div.style.lineHeight = "1";
        this.div.style.visibility = this.visibility;

        this.div.style.backgroundColor = this.style.backColor;
        this.div.style.color = this.style.textColor;

        // Border for the debug purposes
        //this.wrapperDiv.style.borderStyle = "solid";
        //this.wrapperDiv.style.borderWidth = "thin";
        //this.wrapperDiv.style.borderColor = "black";

        this.setViewMode();

        // create element
        this.container.appendChild(this.wrapperDiv);
        this.wrapperDiv.appendChild(this.div);

        if (!this.isReadOnly) {
            //event handlers
            angular.element(this.div).on("labeldblclick", (e) => this.onEdit(e));
            this.div.addEventListener("blur", this.onBlur, true);
            this.div.addEventListener("labelmouseover", this.onMouseover, true);
            this.div.addEventListener("labelmouseout", this.onMouseout, true);

            angular.element(this.div).on("keydown", (e) => this.onKeyDown(e));
            angular.element(this.div).on("paste", (e) => this.onPaste(e));
            angular.element(this.div).on("dispose", () => this.onDispose());
        }
    }

    public onDispose = () => {
        if (this.div) {
            if (!this.isReadOnly) {
                angular.element(this.div).off("labeldblclick", (e) => this.onEdit(e));
                this.div.removeEventListener("blur", this.onBlur, true);
                this.div.removeEventListener("labelmouseover", this.onMouseover, true);
                this.div.removeEventListener("labelmouseout", this.onMouseout, true);
                angular.element(this.div).off("keydown", (e) => this.onKeyDown(e));
                angular.element(this.div).off("paste", (e) => this.onPaste(e));
                angular.element(this.div).off("dispose", () => this.onDispose());
            }
            this.wrapperDiv.removeChild(this.div);
            this.container.removeChild(this.wrapperDiv);
        }
    }

    private numberToPx(val: number): string {
        return `${val}px`;
    }

    //#UNUSED
    //private numberToPt(val: number): string {
    //    return `${val}pt`;
    //}

    private cancelDefaultAction(e) {
        let evt = e ? e : window.event;
        if (evt != null) {
            if (evt.preventDefault) {
                evt.preventDefault();
            }
            evt.returnValue = false;
        }
    }

    private selectText = () => {
        let range, selection;
        let body: any = document.body;
        try {
            if (body.createTextRange) { //ms
                range = body.createTextRange();
                range.moveToElementText(this.div);
                range.select();
            } else if (window.getSelection) { //all others
                selection = window.getSelection();
                range = document.createRange();
                range.selectNodeContents(this.div);
                selection.removeAllRanges();
                selection.addRange(range);
            }
        } catch (error) {
            //ErrorLog.toConsole(error);
        }
    }

    private getCaretCharacterOffsetWithin(): SelectionOffsets {
        let offsets = new SelectionOffsets();
        //#UNUSED
        //let caretOffset = 0;
        const element: any = this.div;
        const doc = element.ownerDocument || element.document;
        const win = doc.defaultView || doc.parentWindow;
        let sel;
        if (typeof win.getSelection !== "undefined") {
            sel = win.getSelection();
            if (sel.rangeCount > 0) {
                const range = win.getSelection().getRangeAt(0);
                const preCaretRange = range.cloneRange();
                preCaretRange.selectNodeContents(element);
                preCaretRange.setEnd(range.endContainer, range.endOffset);
                offsets.selectionStart = range.startOffset;
                offsets.selectionEnd = range.endOffset;
                offsets.caretOffset = preCaretRange.toString().length;
            }
        } else if ((sel = doc.selection) && sel.type !== "Control") {
            const textRange = sel.createRange();
            const preCaretTextRange = doc.body.createTextRange();
            preCaretTextRange.moveToElementText(element);
            preCaretTextRange.setEndPoint("EndToEnd", textRange);
            offsets.caretOffset = preCaretTextRange.text.length;
        }

        // This is to fix FF and Chrome behavior
        if (!this.isIe11() && offsets.selectionEnd < offsets.caretOffset) {
            offsets.selectionEnd = offsets.caretOffset;
        }

        return offsets;
    }

    private splice(str, start, delCount, newSubStr) {
        return str.slice(0, start) + newSubStr + str.slice(start + Math.abs(delCount));
    }
}
