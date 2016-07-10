module Storyteller {
    export enum ElementType {
        Undefined,
        Shape,
        UserTaskHeader,
        SystemTaskHeader,
        SystemTaskOrigin,
        Button,
        Connector
    }

    export interface IDiagramElement extends MxCell {
        getElementType(): ElementType;
        isHtmlElement(): boolean;
        getX(): number;
        getY(): number;
        getHeight(): number;
        getWidth(): number;
        getCenter(): MxPoint;
        setElementText(cell: MxCell, text: string);
        formatElementText(cell: MxCell, text: string): string;
        getElementTextLength(cell: MxCell): number;
    }

    export interface IDiagramNodeElement extends IDiagramElement {
        getNode(): IDiagramNode;
    }
}