﻿import {IProcessShape} from "../../../../../models/processModels";
import {IProcessGraph, IDiagramNode} from "../process-graph-interfaces";
import {NodeType} from "../process-graph-constants";
import {DiagramNode} from "./diagram-node";

export class MergingPoint extends DiagramNode<IProcessShape> {

    private MERGING_POINT_WIDTH = 16;
    //private MERGING_POINT_HEIGHT = 16;

    constructor(model: IProcessShape) {
        super(model, NodeType.MergingPoint);
    }

    public getHeight(): number {
        return this.MERGING_POINT_WIDTH;
    }

    public getWidth(): number {
        return this.MERGING_POINT_WIDTH;
    }

    public addNode(graph: IProcessGraph): IDiagramNode {
        return this;
    }

    public deleteNode(graph: IProcessGraph) {
    }

    public render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode {
        this.insertVertex(graph.getMxGraph(), this.model.id.toString(), null, x, y, this.MERGING_POINT_WIDTH,
            this.MERGING_POINT_WIDTH, "shape=rhombus;strokeColor=#d4d5da;fillColor=#d4d5da;selectable=0;editable=0");
        return this;
    }
}