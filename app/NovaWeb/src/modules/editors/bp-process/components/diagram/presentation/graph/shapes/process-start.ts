import {IProcessShape} from "../../../../../models/process-models";
import {IProcessGraph, IDiagramNode} from "../models/";
import {NodeType} from "../models/";
import {DiagramNode} from "./diagram-node";
import {NodeFactorySettings} from "./node-factory-settings";

export class ProcessStart extends DiagramNode<IProcessShape> {

    private PROCESS_START_WIDTH = 20;
    private PROCESS_START_HEIGHT = 20;
    private PROCESS_START_SHIFT = 40;

    constructor(model: IProcessShape, nodeFactorySettings: NodeFactorySettings = null) {
        super(model);
    }

    public get label(): string {
        return this.name;
    }

    public get name(): string {
        return this.model.name;
    }

    public set name(value: string) {
        // Disallow update of name
    }

    public getX(): number {
        return this.getCenter().x - this.PROCESS_START_SHIFT;
    }

    public getHeight(): number {
        return this.PROCESS_START_HEIGHT;
    }

    public getWidth(): number {
        return this.PROCESS_START_WIDTH;
    }

    public render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode {
        this.insertVertex(graph.getMxGraph(), this.model.id.toString(), this.name, x + this.PROCESS_START_SHIFT, y,
            this.PROCESS_START_WIDTH, this.PROCESS_START_HEIGHT, "shape=ellipse;strokeColor=#d4d5da;strokeWidth=3;fillColor=#ffffff;" +
            "labelWidth=45;verticalLabelPosition=bottom;fontColor=#000000;fontFamily=Open Sans, sans-serif;fontStyle=1;fontSize=11;editable=0");
        graph.startNode = this;
        return this;
    }

    public editLabelText(graph: MxGraph, evt: any): any {
        // don't allow editing of label for this node
        mxEvent.consume(evt);
    }

    public pasteLabelText(graph: MxGraph, evt: any): any {
        // don't allow pasting text into this node
        mxEvent.consume(evt);
    }

    public formatElementText(cell: MxCell, text: string): string {
        return text.toUpperCase();
    }

    public getNodeType() {
        return NodeType.ProcessStart;
    }

}
