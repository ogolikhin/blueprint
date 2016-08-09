﻿import {IDiagramElement, IDiagramNode} from "./../process-graph-interfaces";
import {IDiagramNodeElement, ILabel} from "./../process-graph-interfaces";
import {ElementType, NodeChange} from "./../process-graph-constants";

export class DiagramElement extends mxCell implements IDiagramElement {
    private elementType: ElementType;
    textLabel: ILabel;

    constructor(id: any, type: ElementType = ElementType.Undefined, value?: string, geometry?: MxGeometry, style?: string) {
        super(value, geometry, style);

        this.setId(id);
        this.elementType = type;
    }

    public getElementType(): ElementType {
        return this.elementType;
    }

    public isHtmlElement(): boolean {
        return this.getElementType() === ElementType.Shape;
    }

    public getX(): number {
        return this.getCenter().x;
    }

    public getY(): number {
        return this.getCenter().y;
    }

    public getHeight(): number {
        var geometry = <MxGeometry>this.getGeometry();
        return geometry.height;
    }

    public getWidth(): number {
        var geometry = <MxGeometry>this.getGeometry();
        return geometry.width;
    }

    public getCenter(): MxPoint {
        var geometry = this.getGeometry();
        if (geometry) {
            var point = new mxPoint(geometry.getCenterX(), geometry.getCenterY());
            return point;
        }

        return new mxPoint(0, 0);
    }
    public setElementText(cell: MxCell, text: string) {
        // override in descendant classes
    }

    public getElementTextLength(cell: MxCell): number {
        // override in descendant classes 
        return null;
    }

    public formatElementText(cell: MxCell, text: string): string {
        // override in descendant classes
        return null;
    }

    protected getParentId(): number {
        // override in descendant classes
        return null;
    }


    protected _isChanged: boolean;

    public get isChanged(): boolean {
        return this._isChanged;
    }

    private _redraw: boolean;
    private _isNotificationPending: boolean = false;

    public notify(change: NodeChange, redraw: boolean = false, callback?: any) {
        this._isChanged = true;

        if (this._redraw || redraw) {
            this._redraw = true;
        } else {
            this._redraw = false;
        }

        if (!this._isNotificationPending) {
            this._isNotificationPending = true;

            this.raiseChangedEvent(change, this._redraw);
            this._isNotificationPending = false;
            this._redraw = false;

            if (callback != null) {
                callback();
            }
        } else {
            if (callback != null) {
                callback();
            }
        }
    }

    private raiseChangedEvent(change: NodeChange, redraw: boolean) {
        var eventArguments = {
            processId: this.getParentId(),
            nodeChanges: [
                {
                    nodeId: this.getId(),
                    change: change,
                    redraw: redraw
                }
            ]
        };
        var evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("graphUpdated", true, true, eventArguments);
        window.dispatchEvent(evt);
    }
}

export class DiagramNodeElement extends DiagramElement implements IDiagramNodeElement {
    public getNode(): IDiagramNode {
        var parent = this.parent;

        while (parent) {
            if (parent["getNodeType"]) {
                return parent;
            }
            parent = (<mxCell>parent).parent;
        }
        return null;
    }

    public getCenter(): MxPoint {
        var geometry = <MxGeometry>this.geometry;
        if (geometry) {
            if (this.parent) {
                var parentCenterX = (<IDiagramNodeElement>this.parent).getCenter().x;
                var parentCenterY = (<IDiagramNodeElement>this.parent).getCenter().y;
                var parentX = parentCenterX - (this.parent.getWidth() / 2);
                var parentY = parentCenterY - (this.parent.getHeight() / 2);

                if (geometry.relative) {
                    return new mxPoint(geometry.x * this.parent.getWidth() + geometry.width / 2, geometry.y * this.parent.getHeight() + geometry.height / 2);
                } else {
                    return new mxPoint(geometry.getCenterX() + parentX, geometry.getCenterY() + parentY);
                }
            }

            return new mxPoint(geometry.getCenterX(), geometry.getCenterY());
        }

        return new mxPoint(0, 0);
    }

    public setElementText(cell: MxCell, text: string) {
        if (this.getNode()) {
            this.getNode().setElementText(cell, text);
        }
    }

    public getElementTextLength(cell: MxCell): number {
        if (this.getNode()) {
            return this.getNode().getElementTextLength(cell);
        }
        return null;
    }

    public formatElementText(cell: MxCell, text: string): string {

        if (this.getNode()) {
            return this.getNode().formatElementText(cell, text);
        }
        return null;
    }
}