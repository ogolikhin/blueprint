import {IProcessShape} from "../../../../../models/process-models";
import {IProcessGraph, IDiagramNode} from "../models/";
import {NodeType} from "../models/";
import {DiagramNode} from "./diagram-node";
import {NodeFactorySettings} from "./node-factory-settings";


export class ProcessEnd extends DiagramNode<IProcessShape> {

    private PROCESS_END_WIDTH = 20;
    private PROCESS_END_HEIGHT = 20;
    private PROCESS_END_SHIFT = 50;

    constructor(model: IProcessShape, nodeFactorySettings: NodeFactorySettings = null) {
        super(model, NodeType.ProcessEnd);
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
        return this.getCenter().x + this.PROCESS_END_SHIFT;
    }

    public getHeight(): number {
        return this.PROCESS_END_HEIGHT;
    }

    public getWidth(): number {
        return this.PROCESS_END_WIDTH;
    }

    public render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode {
        this.insertVertex(graph.getMxGraph(), this.model.id.toString(), this.name, x - this.PROCESS_END_SHIFT, y, this.PROCESS_END_WIDTH,
            this.PROCESS_END_HEIGHT, "shape=ellipse;strokeColor=#d4d5da;strokeWidth=3;fillColor=#ffffff;labelWidth=35;" +
            "verticalLabelPosition=bottom;fontColor=#000000;fontFamily=Open Sans, sans-serif;fontStyle=1;fontSize=11;" +
            " foldable = 0; editable = 0");
        graph.endNode = this;

        graph.getMxGraph().insertVertex(this, "C" + this.model.id.toString(), null, (this.PROCESS_END_WIDTH / 2) - 5,
            (this.PROCESS_END_HEIGHT / 2) - 5, 10, 10, "shape=ellipse;strokeColor=none;fillColor=#d4d5da;editable=0;selectable=0");

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
}
