import {EdgeGeo} from "../../../../models/process-models";
import {Models} from "../../../../../../main";

class ConnectorLine {
    constructor(startPoint: MxPoint,
                    endPoint: MxPoint,
                    edgeGeo: EdgeGeo) {
        this.startPoint = startPoint;
        this.endPoint = endPoint;
        this.edgeGeo = edgeGeo;
    }

    public startPoint: MxPoint;
    public endPoint: MxPoint;
    public edgeGeo: EdgeGeo;
} 

class Bridge {
    constructor (x: number, y: number, image: HTMLDivElement, edgeH: MxCell, edgeV: MxCell) {
        this.x = x;
        this.y = y;
        this.image = image;
        this.hEdges = [];
        this.vEdges = [];
        this.hEdges.push(edgeH);
        this.vEdges.push(edgeV);
    }

    addEdges(edgeH: MxCell, edgeV: MxCell) {
        if (this.hEdges.filter((edge) => edge === edgeH).length === 0) {
            this.hEdges.push(edgeH);
        }
        if (this.vEdges.filter((edge) => edge === edgeV).length === 0) {
            this.vEdges.push(edgeV);
        }
    }

    x: number;
    y: number;
    image: HTMLDivElement;
    hEdges: MxCell[];
    vEdges: MxCell[];
}

export interface IBridgesHandler {
    addConnectorBridges();
    highlightBridges();
}

export class BridgesHandler implements IBridgesHandler {
    private readonly bridges: Bridge[];

    constructor (private mxgraph, private edgesGeo) {
        this.bridges = [];
    }

    public  addConnectorBridges() {
        const verticalLines: ConnectorLine[] = [];
        const horizontalLines: ConnectorLine[] = [];

        for (let edgeGeo of this.edgesGeo) {
            if (edgeGeo) {
                for (let i = 1; i < edgeGeo.state.absolutePoints.length; i++) {

                    const startPoint = edgeGeo.state.absolutePoints[i - 1];
                    const endPoint = edgeGeo.state.absolutePoints[i];

                    if (startPoint.y === endPoint.y) {
                        // horizontal
                        if (startPoint.x < endPoint.x) {
                            horizontalLines.push(new ConnectorLine(startPoint, endPoint, edgeGeo));
                        } else {
                            horizontalLines.push(new ConnectorLine(endPoint, startPoint, edgeGeo));
                        }
                    } else {
                        // vertical
                        if (startPoint.y < endPoint.y) {
                            verticalLines.push(new ConnectorLine(startPoint, endPoint, edgeGeo));
                        } else {
                            verticalLines.push(new ConnectorLine(endPoint, startPoint, edgeGeo));
                        }
                    }
                }
            }
        }

        for (let horizontalLine of horizontalLines) {
            for (let verticalLine of verticalLines) {
                if (horizontalLine.startPoint.x < verticalLine.startPoint.x && 
                    horizontalLine.endPoint.x > verticalLine.startPoint.x &&
                    horizontalLine.startPoint.y > verticalLine.startPoint.y && 
                    horizontalLine.startPoint.y < verticalLine.endPoint.y) {
                        // add bridge
                        this.addConnectorBridge(horizontalLine, verticalLine);
                    }
            }
        }
    }

    public highlightBridges() {
        for (let bridge of this.bridges) {
            let st2 = mxConstants.DEFAULT_VALID_COLOR;
            let st3 = mxConstants.DEFAULT_VALID_COLOR;
            for (let edge of bridge.hEdges) {
                let stateH: any = this.mxgraph.getView().getState(edge);
                if (stateH.shape && stateH.shape.stroke !== mxConstants.DEFAULT_VALID_COLOR) {
                    st2 = stateH.shape.stroke;
                    break;
                }
            }
            for (let edge of bridge.vEdges) {
                const stateV: any = this.mxgraph.getView().getState(edge);
                if (stateV.shape && stateV.shape.stroke !== mxConstants.DEFAULT_VALID_COLOR) {
                    st3 = "none"; //stateV.shape.stroke;
                }
            }
            bridge.image.innerHTML = this.getSvgImageSrc(st2, st3);
        }
    }
    
    private addConnectorBridge(horizontalLine: ConnectorLine, verticalLine: ConnectorLine) {
        const x = verticalLine.startPoint.x;
        const y = horizontalLine.startPoint.y;
        const edgeH = horizontalLine.edgeGeo.edge;
        const edgeV = verticalLine.edgeGeo.edge;

        const existing = this.bridges.filter(bridge => bridge.x === x && bridge.y === y);
        if (existing.length === 0) {
            const image = this.addBridgeImage(this.mxgraph, x, y);
            this.bridges.push(new Bridge(x, y, image, edgeH, edgeV));
        } else {
            existing[0].addEdges(edgeH, edgeV);
        }
    }

    private getSvgImageSrc(st2: string, st3: string): string {
        return `<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" version="1.1" xml:space="preserve" 
        width="16px" height="16px" viewBox="0 0 300 300" class="width: 16px; height: 16px;">
        <style type="text/css">
            .st0{fill:none;}
            .st1{fill:#FFFFFF;}
            .st2{fill:${st2};}
            .st3{fill:${st3};}
        </style>
        <g class="currentLayer"><title>Layer 1</title><rect class="st3" stroke-dashoffset="" fill-rule="nonzero" id="svg_11" 
        x="106.76315307617188" y="88.71052551269531" width="85.78947448730469" height="100" style="color: rgb(0, 0, 0);" />
        <g id="Layer_3" ><rect class="st0" width="300" height="300" id="svg_1" y="0" x="0.6578947305679321"/>
        </g><path class="st1" d="M232.5,129.5c0,0-46-34.3-82.2-34.3c-37.8,0-84.6,29.1-84.6,29.1L48,33c0,0,27.5-31.9,102-31.9  
        C225,1.1,251,39,251,39L232.5,129.5z" id="svg_2"/><rect fill="#ffffff" stroke="#ffffff" stroke-dashoffset="" fill-rule="nonzero" id="svg_4" 
        x="184.11403906345367" y="90.26315307617188" width="76" height="97.73684692382812" style="color: rgb(0, 0, 0);"  
        stroke-opacity="1"/><rect fill="#ffffff" stroke="#ffffff" stroke-dashoffset="" fill-rule="nonzero" x="40.72807061672211" y="89.60525512695312" 
        width="76" height="97.73684692382812" style="color: rgb(0, 0, 0);" stroke-opacity="1" id="svg_7"/><path class="st2" 
        d="M280.9,114c-16-41-56.9-93.8-131.1-93.8C72.2,20.2,31,68,16.6,113H0v75h23.1c0,0,0.1,0,0.1,0.1l49.1,0  c-6.7-11.4-10.4-24.3-10.4-38.1c0-44.7,
        32.1-80.3,86.5-80.3c58.7,0,90.1,35.6,90.1,80.3c0,13.9-3.7,26.9-10.4,38.3l19.9,0V188h52  v-74H280.9z" id="svg_3"/><path fill="" stroke-dashoffset="" 
        fill-rule="nonzero" marker-start="" marker-mid="" marker-end="" id="svg_9" d="M194.5486905700087,148.17502788322489 
        L194.8980155857922,148.17502788322489 L194.8980155857922,148.5243528990084 L194.5486905700087,148.5243528990084 
        L194.5486905700087,148.17502788322489 zM194.58944515518343,147.86645745261615 L194.58944515518343,148.13427329805015 
        L194.85726100061746,148.13427329805015 L194.85726100061746,147.86645745261615 L194.58944515518343,147.86645745261615 z" 
        style="color: rgb(0, 0, 0);" /></g></svg>`.replace(`class="st3"`, `fill=${st3}`).replace(`class="st2"`, `fill=${st2}`);
    }

    private addBridgeImage(graph: MxGraph, x: number, y: number): HTMLDivElement {
        const div = graph.container;
        const image =  document.createElement("div");
        image.style.position = "absolute";
        image.style.width = "16px";
        image.style.height = "16px";
        image.style.top = `${y - 8}px`;
        image.style.left = `${x - 7.5}px`;
        image.className = "process-graph__bridge";
        div.appendChild(image);
        image.innerHTML = this.getSvgImageSrc(mxConstants.DEFAULT_VALID_COLOR, mxConstants.DEFAULT_VALID_COLOR);

        return image;
    }
}


