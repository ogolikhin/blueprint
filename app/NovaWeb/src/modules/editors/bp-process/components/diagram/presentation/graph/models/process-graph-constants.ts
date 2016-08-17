export var GRAPH_LEFT: number = 30;
export var GRAPH_TOP: number = 140;
export var GRAPH_COLUMN_WIDTH: number = 150;
export var GRAPH_ROW_HEIGHT: number = 220;

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

export enum ElementType {
    Undefined,
    Shape,
    UserTaskHeader,
    SystemTaskHeader,
    SystemTaskOrigin,
    Button,
    Connector
}