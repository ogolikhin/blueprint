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
            let client = {
                width: Number(div.getAttribute("stLabelWidth")),
                height: Number(div.getAttribute("stLabelHeight"))
            };
            if (position != null) {
                this.addPoint(div, position.x, position.y);
                this.addPoint(div, position.x + client.width, position.y);
                this.addPoint(div, position.x, position.y + client.height);
                this.addPoint(div, position.x + client.width, position.y + client.height);
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
        let value = Math.floor(x / 200).toString() + ";" + Math.floor(y / 200).toString();
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
        let x = 0;
        let y = 0;

        e = e || window.event;

        x = e.offsetX;
        y = e.offsetY;

        console.log(`>>>>>>>>>>>>>>>>> x = ${x} | y = ${y}`);

        let index = this.getCellIndex(x, y);
        console.log(`>>>>>>>>>>>>>>>>> index = ${index}`);
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
        const evt: Event = document.createEvent("UIEvents");
        evt.initEvent(eventName, true, true);
        element.dispatchEvent(evt);
    }

    public fireCustomEvent(element, eventName: string) {
        const evt = document.createEvent("CustomEvent");
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
        const evt = e ? e : window.event;
        if (evt.preventDefault) {
            evt.preventDefault();
        }
        if (evt.stopPropagation) {
            evt.stopPropagation();
        }
        evt.returnValue = false;
    }
}
