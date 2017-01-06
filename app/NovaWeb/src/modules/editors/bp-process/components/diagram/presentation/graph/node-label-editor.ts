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
        const element = this.getLabel(e);

        if (this.currentDiv != null && this.currentDiv !== element) {
            this.fireCustomEvent(this.currentDiv, "labelmouseout");
            this.currentDiv = null;
        }

        if (element && element.className === "processEditorCustomLabel" && this.currentDiv === null) {
            this.fireCustomEvent(element, "labelmouseover");
            this.currentDiv = element;
        }
    };

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
        return Math.floor(x / 200).toString() + ";" + Math.floor(y / 200).toString();
    }

    private isIe11(): boolean {
        const myBrowser = this.executionEnvironmentDetector.getBrowserInfo();
        const ver = parseInt(myBrowser.version, 10);
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
    };

    private containerDblClick = (e) => {
        const element = this.getLabel(e);
        if (element && element.className === "processEditorCustomLabel") {
            this.fireCustomEvent(element, "labeldblclick");
        }

        this.cancelDefaultAction(e);
    };

    private getLabel(e) {
        e = e || window.event;

        if (e.target && e.target.contentEditable === "true") {
            return null;
        }

        // In IE and Chrome offsetX/Y return the horizontal/vertical coordinates of the mouse pointer between the event and
        // the padding edge of the SVG container. In Firefox they return the coordinates relative to the current target node.
        // In IE and Chrome layerX/Y return the same values as offsetX/Y +1, as they take into account the SVG border. In
        // Firefox they work like offsetX/Y in IE/Chrome. Therefore if layerX/Y - offsetX/Y > 1 we know that offsetX is actually
        // returning coordinates relative to the current target node and not the SVG container, so we use layerX/Y instead. We
        // also _.round as IE seems to return float at times for layerX/Y
        const x = _.round(e.layerX - e.offsetX) > 1 ? e.layerX : e.offsetX;
        const y = _.round(e.layerY - e.offsetY) > 1 ? e.layerY : e.offsetY;
        const index = this.getCellIndex(x, y);
        const cell = this.divIndex[index];
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
        const evt = new CustomEvent(eventName, {
            bubbles: true,
            cancelable: true
        });
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
