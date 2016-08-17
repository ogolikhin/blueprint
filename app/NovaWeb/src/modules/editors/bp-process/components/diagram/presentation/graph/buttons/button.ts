import {IOverlayHandler, IMouseEventHandler} from "../models/";
import {DiagramElement} from "../shapes/diagram-element";
import {ElementType} from "../models/";


export class Button extends DiagramElement implements IOverlayHandler, IMouseEventHandler {
    private neutralImageUrl: string;
    private hoverImageUrl: string;
    private disabledImageUrl: string;
    private activeImageUrl: string;
    private clickImageUrl: string;
    private currentImageUrl: string;
    private tooltip: string;

    private width: number;
    private height: number;
    private clickAction: any;
    private _canMouseOver: boolean;
    private _isActive: boolean;

    public isEnabled: boolean = true;

    constructor(id: string, width: number, height: number, imageUrl: string) {
        super(id, ElementType.Button);

        this.width = width;
        this.height = height;
        this.neutralImageUrl = imageUrl;
        this.currentImageUrl = this.neutralImageUrl;
    }

    public getTooltip(): string {
        return this.tooltip;
    }

    public setTooltip(tooltip: string) {
        this.tooltip = tooltip;
    }

    public setClickAction(clickAction: any) {
        this.clickAction = clickAction;
    }

    public setClickImage(clickImageUrl: string) {
        this.clickImageUrl = clickImageUrl;
    }

    public setHoverImage(hoverImageUrl: string) {
        this.hoverImageUrl = hoverImageUrl;
        //set a flag only if imageurl is assigned. 
        this.canMouseOver(Boolean(this.hoverImageUrl));
    }

    public setDisabledImage(disabledImageUrl: string) {
        this.disabledImageUrl = disabledImageUrl;
    }

    public setActiveImage(activeImageUrl: string) {
        this.activeImageUrl = activeImageUrl;
    }

    public canMouseOver(value: boolean) {
        this._canMouseOver = value;
    }

    public setActive(value: boolean) {
        this._canMouseOver = value;
    }

    public activate() {
        this.isEnabled = true;

        if (this.activeImageUrl != null) {
            this.currentImageUrl = this.activeImageUrl;
        }

        this._isActive = true;
        this.raiseButtonUpdatedEvent();
    }

    public deactivate() {
        this._isActive = false;

        if (this.neutralImageUrl != null) {
            this.currentImageUrl = this.neutralImageUrl;
        }

        this.raiseButtonUpdatedEvent();
    }

    public disable() {
        this.isEnabled = false;

        if (this.disabledImageUrl != null) {
            this.currentImageUrl = this.disabledImageUrl;
        }

        this.raiseButtonUpdatedEvent();
    }

    public render(graph: MxGraph, parent: MxCell, x: number, y: number, style: string) {
        this.geometry = new mxGeometry(x, y, this.width, this.height);
        this.geometry.relative = false;
        this.setVertex(true);
        this.setEdge(false);
        this.setStyle(style);
        this.setConnectable(false);
        graph.addCell(this, parent);

        this.updateOverlay(graph);
    }

    public updateOverlay(graph: MxGraph) {
        graph.removeCellOverlays(this);
        const cursor = this.isEnabled ? "pointer" : "default";
        this.addOverlay(graph, this.currentImageUrl, this.width, this.height, cursor);
    }

    public onMouseEnter(sender: MxGraph, evt) {
        //update current image
        if (this._canMouseOver && this.isEnabled) {
            this.currentImageUrl = this.hoverImageUrl;
        }

        this.updateOverlay(sender);
    }

    public onMouseLeave(sender: MxGraph, evt) {
        if (this._canMouseOver) {
            //update current image bease on current state
            var img = this._isActive ? this.activeImageUrl : (this.isEnabled ? this.neutralImageUrl : this.disabledImageUrl);
            //verify if image is assigned
            if (img) {
                this.currentImageUrl = img;
            }
        }

        this.updateOverlay(sender);
    }

    public onMouseDown(sender, evt) {
        if (this.isEnabled) {
            this.clickAction();
        }
    }

    public onMouseUp(sender: MxGraph, evt) {
    }

    private raiseButtonUpdatedEvent() {
        var evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("buttonUpdated", true, true, { id: this.getId() });
        window.dispatchEvent(evt);
    }

    private createOverlay(imageUrl: string, width: number, height: number, cursor: string = "default") {
        var overlay = new mxCellOverlay(new mxImage(imageUrl, width, height), this.tooltip);
        overlay.cursor = cursor;
        overlay.align = mxConstants.ALIGN_CENTER;
        overlay.verticalAlign = mxConstants.ALIGN_MIDDLE;
        overlay.offset = new mxPoint(0, 0);
        return overlay;
    }

    private addOverlay(graph: MxGraph, imageUrl: string, width: number, height: number, cursor: string = "default") {
        var overlay = this.createOverlay(imageUrl, width, height, cursor);
        graph.addCellOverlay(this, overlay);

        return overlay;
    }
}