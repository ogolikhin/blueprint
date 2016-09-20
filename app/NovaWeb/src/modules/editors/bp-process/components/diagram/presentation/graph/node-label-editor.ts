//import ExecutionEnvironmentDetector = ExecutionEnvironment.ExecutionEnvironmentDetector;

export class NodeLabelEditor {
    private divs;
    private divIndex = {};
    private currentDiv = null;
    private executionEnvironmentDetector: any;

    constructor(private container: HTMLElement) {
        // This is temporary code. It will be replaced with 
        // a class that wraps this global functionality.   
        let w: any = window; 
        this.executionEnvironmentDetector = new w.executionEnvironmentDetector();

        // Make sure the "blur" event will be fired on every outside click 
        if (this.isIe11()) {
            container.addEventListener("pointerdown", this.pointerDown, true);
        } else {
            container.addEventListener("mousedown", this.pointerDown, true);
        }

        // Start editing if a label was double-clicked
        container.addEventListener("dblclick", this.containerDblClick, true);

        container.addEventListener("mousemove", this.onMouseOver, true);
    }

    private onMouseOver = (e) => {
        let element = this.getLabel(e);

        if (this.currentDiv != null && this.currentDiv !== element) {
            this.fireCustomEvent(this.currentDiv, "labelmouseout");
            this.currentDiv = null;
        }

        if (element && element.className === "processEditorCustomLabel" && this.currentDiv === null) {
            this.fireCustomEvent(element, "labelmouseover");
            this.currentDiv = element;
        }
    }

    public init() {
        this.divs = this.container.getElementsByClassName("processEditorCustomLabel");
        this.createDivIndex();
    }

    private createDivIndex() {
        for (let div of this.divs) {
            let position = {
                x: Number(div.getAttribute("stLabelX")),
                y: Number(div.getAttribute("stLabelY"))
            };

            if (position != null) {
                this.addPoint(div, position.x, position.y);
                this.addPoint(div, position.x + div.clientWidth, position.y);
                this.addPoint(div, position.x, position.y + div.clientHeight);
                this.addPoint(div, position.x + div.clientWidth, position.y + div.clientHeight);
            }
        }
    }

    private addPoint(div, x, y) {
        let index = this.getCellIndex(x, y);
        let cell = this.getCell(index);
        this.addDivToCell(cell, div);
    }

    private getCell(index: string) {
        let cell = this.divIndex[index];
        if (cell == null) {
            cell = [];
            this.divIndex[index] = cell;
        }
        return cell;
    }

    private addDivToCell(cell, div) {
        for (let member of cell) {
            if (member === div) {
                // div is already in, do not add it
                return;
            }
        }
        cell.push(div);
    }

    private getCellIndex(x: number, y: number): string {
        let value =  Math.floor(x / 200).toString() + ";" + Math.floor(y / 200).toString();
        //window.console.log("x: " + x + " y: " + y + " value: " + value);
        return value;
    }

    private isIe11(): boolean {
        let myBrowser = this.executionEnvironmentDetector.getBrowserInfo();
        let ver = parseInt(myBrowser.version, 10);
        return (myBrowser.msie && (ver === 11));
    }

    private pointerDown = (e) => {
        if (e.target.className !== "processEditorCustomLabel") {
            for (let div of this.divs) {
                if (div.contentEditable === "true") {
                    this.fireEvent(div, "blur");
                    return;
                }
            }
        }
    }

    private containerDblClick = (e) => {
        let element = this.getLabel(e);
        if (element && element.className === "processEditorCustomLabel") {
            this.fireCustomEvent(element, "labeldblclick");
        }

        this.cancelDefaultAction(e);
    }

    private getLabel(e) {
        var x = 0;
        var y = 0;
        let zoom = 1;

        e = e || window.event;

        // zoom is needed for Chrome only.
        if (!this.isIe11() && 
            (e.target.nodeName === "rect" || e.target.nodeName === "image" || e.target.nodeName === "text" || 
             e.target.nodeName === "path" || e.target.nodeName === "ellipse" || e.target.nodeName === "process-graph-container")) {
            zoom = window.outerWidth / window.innerWidth; 
        }
        
        x = e.offsetX * zoom;
        y = e.offsetY * zoom;
        let index = this.getCellIndex(x, y);
        let cell = this.divIndex[index];
        if (cell != null) {
            for (let div of cell) {
                if (this.hitTest(div, x, y)) {
                    return div;
                }
            }
        }
        return null;
    }

    private hitTest(element, x, y): boolean {
        let position = {
            x: Number(element.getAttribute("stLabelX")),
            y: Number(element.getAttribute("stLabelY"))
        };

        if (position.x <= x && position.x + element.clientWidth >= x &&
            position.y <= y && position.y + element.clientHeight >= y) {
            return true;
        }
        return false;
    }

    public fireEvent(element, eventName: string) {
        var evt: Event = document.createEvent("UIEvents");
        evt.initEvent(eventName, true, true);
        element.dispatchEvent(evt);
    }

    public fireCustomEvent(element, eventName: string) {
        var evt = document.createEvent("CustomEvent");
        evt.initCustomEvent(eventName, true, true, null);
        element.dispatchEvent(evt);
    }

    public dispose() {
        if (this.container != null) {
            this.container.removeEventListener("dblclick", this.containerDblClick, true);
            if (this.isIe11()) {
                this.container.removeEventListener("pointerdown", this.pointerDown, true);
            } else {
                this.container.removeEventListener("mousedown", this.pointerDown, true);
            }
            this.container.removeEventListener("mousemove", this.onMouseOver, true);
        }
    }

    private cancelDefaultAction(e) {
        var evt = e ? e : window.event;
        if (evt.preventDefault) {
            evt.preventDefault();
        }
        if (evt.stopPropagation) {
            evt.stopPropagation();
        }
        evt.returnValue = false;
    }
}
