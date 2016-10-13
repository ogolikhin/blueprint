export interface IDiagram {
    id: number;
    diagramType: string;
    width: number;
    height: number;
    shapes: IShape[];
    connections: IConnection[];
    libraryVersion: number;
}

export interface IDiagramElement {
    id: number;
    type: string;
    name: string;
    props: IProp[];
    zIndex: number;
    isShape: boolean;
}

export interface IHierarchyDiagram extends IDiagram {
    children: Array<IHierarchyElement>;
}

export interface IHierarchyElement extends IShape, IConnection {
    children: Array<IHierarchyElement>;
    parent: IHierarchyElement;
}

export interface IShape extends IDiagramElement {
    id: number;
    name: string;
    parentId: number;
    type: string;
    height: number;
    width: number;
    x: number;
    y: number;
    zIndex: number;
    angle: number;
    stroke: string;
    strokeOpacity: number;
    strokeWidth: number;
    strokeDashPattern: string;
    fill: string;
    gradientFill: string;
    isGradient: boolean;
    fillOpacity: number;
    shadow: boolean;
    label: string;
    labelTextAlignment: string;
    description: string;
    props: IProp[];
    labelStyle: ILabelStyle;
}

export interface IProp {
    name: string;
    value: any;
}

export interface ILabelStyle {
    textAlignment: string;
    fontFamily: string;
    fontSize: string;
    isItalic: boolean;
    isBold: boolean;
    isUnderline: boolean;
    foreground: string;
}

export interface IConnection extends IDiagramElement {
    id: number;
    type: string;
    parentId: number;
    name: string;
    sourceId: number;
    targetId: number;
    stroke: string;
    strokeOpacity: number;
    strokeWidth: number;
    strokeDashPattern: string;
    label: string;
    sourceLabel: string;
    targetLabel: string;
    points: IPoint[];
    startArrow: string;
    endArrow: string;
    zIndex: number;
    props: IProp[];
}

export interface IPoint {
    y: number;
    x: number;
}

export class Point implements IPoint {
    public y: number;
    public x: number;
}
