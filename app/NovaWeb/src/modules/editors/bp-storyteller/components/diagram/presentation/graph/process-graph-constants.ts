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