module Storyteller {

    export enum Direction {
        LeftToRight = 0,
        RightToLeft
    };

    export enum NodeType {
        Undefined,
        MergingPoint,
        ProcessEnd,
        ProcessStart,
        SystemDecision,
        SystemTask,
        UserDecision,
        UserTask
    };

    export enum NodeChange {
        Undefined,
        Add,
        Update,
        Remove
    }

    export interface IDiagramNode extends IDiagramNodeElement, MxCell, IDeletable, IUserStoryProvider {
        model: IProcessShape;
        direction: Direction;
        action: string;
        label: string;
        row: number;
        column: number;
        newShapeColor: string;

        getId(): string;
        setId(value: string);
        getNodeType(): NodeType;
        getConnectableElement(): IDiagramNodeElement;
        // returns array of incoming diagram links ordered by asc. order index
        getIncomingLinks(graph: ProcessGraph): DiagramLink[];
        // returns array of outgoing diagram links ordered by asc. order index
        getOutgoingLinks(graph: ProcessGraph): DiagramLink[];
        // returns array of connected sources
        getSources(graph: ProcessGraph): IDiagramNode[];
        // return array of connected targets
        getTargets(graph: ProcessGraph): IDiagramNode[];
        render(graph: ProcessGraph, x: number, y: number, justCreated: boolean): MxCell;
        renderLabels(): void;
        addNode(graph: ProcessGraph): IDiagramNode;
        deleteNode(graph: ProcessGraph);
        // gets immediate successor nodes
        getNextNodes(): IDiagramNode[];
        // gets immediate precursor nodes
        getPreviousNodes(): IDiagramNode[];
        notify(change: NodeChange);

        getDeleteDialogParameters(): Shell.IDialogParams;

        getLabelCell(): MxCell;        
    }
}
