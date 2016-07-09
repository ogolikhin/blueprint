import {IDiagram, IConnection, IDiagramElement, IHierarchyElement, IPoint} from "../diagram.svc";
import {IStencilService} from "./stencil.svc";
import {Shapes, ConnectorTypes} from "./utils/constants";
import {MxFactory, MathExtensions, ConnectionExtensions, HierarchyHelper} from "./utils/helpers";
import {Style, Styles} from "./utils/style-builder";
import {IShapeTemplateFactory} from "./abstract-diagram-factory";
import {DiagramLibraryManager} from "./diagram-library-manager";

export interface IDiagramView {
    drawDiagram(diagram: IDiagram);
    addSelectionListener(listener: ISelectionListener);
    setSelectedItems(selectedItems: Array<IDiagramElement>);
    setSelectedItem(id: number);
    getSelectedItems(): Array<IDiagramElement>;
    clearSelection();
    sanitize(html: string): string;
    zoomToRect(x: number, y: number, width: number, height: number);
    disableUserSelection(value: boolean);
    destroy();
    getGraphScale(): number;
}

export interface ISelectionListener {
    (elements: Array<IDiagramElement>): void;
}

export interface IIconRackListener {
    (element: IDiagramElement): void;
}

export class DiagramView implements IDiagramView {

    private graph: MxGraph;

    private selectionListeners: Array<ISelectionListener> = [];

    private createdVertices: { [id: number]: MxCell; } = {};

    private disabledUserSelection: boolean;

    constructor(divContainer: HTMLElement, private stencilService: IStencilService) {
        // Creates the graph inside the given container
        this.graph = MxFactory.graph(divContainer);
        this.initGraph();
    }

    public getGraphScale(): number {
        return this.graph.getView().getScale();
    }

    public drawDiagram(diagram: IDiagram) {
        this.registerStencils(diagram.diagramType, this.stencilService);
        var manager = new DiagramLibraryManager();
        var shapeFactory = manager.getDiagramFactory(diagram.diagramType);

        if (shapeFactory.enabledToolTips()) {
            this.graph.setTooltips(true);

            this.graph.getTooltipForCell = (cell: MxCell) => {
                if (cell.getTooltip != null) {
                    return cell.getTooltip();
                }
                return null;
            };
        }

        this.drawDiagramInternal(diagram, shapeFactory);
    }

    public addSelectionListener(listener: ISelectionListener) {
        if (listener != null) {
            this.selectionListeners.push(listener);
        }
    }

    public setSelectedItems(selectedItems: Array<IDiagramElement>) {
        if (selectedItems != null) {
            var cellsTobeSelected = [];
            selectedItems.forEach((s: IDiagramElement) => {
                var cell = this.createdVertices[s.id];
                if (cell != null) {
                    cellsTobeSelected.push(cell);
                }
            });
            this.graph.setSelectionCells(cellsTobeSelected);
        }
    }

    public setSelectedItem(id: number) {
        var cell = this.createdVertices[id];
        if (cell != null) {
            this.graph.setSelectionCell(cell);
        }
    }

    public getSelectedItems(): Array<IDiagramElement> {
        return this.graph.getSelectionCells().map((cell: MxCell) => this.getDiagramElement(cell));
    }

    public clearSelection() {
        this.graph.clearSelection();
    }

    public disableUserSelection(value: boolean) {
        this.disabledUserSelection = value;
        this.graph.setEnabled(value);
    }

    public sanitize: (html: string) => string = null;

    public zoomToRect(x: number, y: number, width: number, height: number) {
        var rect = MxFactory.rectangle(0, 0, width, height);
        return this.graph.zoomToRect(rect);
    }

    public destroy() {
        this.graph.destroy();
        this.createdVertices = null;
        this.selectionListeners = null;
    }

    private initGraph() {
        // Enables wrapping for vertex labels
        this.graph.isWrapping = (cell: MxCell) => {
            return cell.isVertex();
        };

        // Enables clipping of vertex labels if no offset is defined
        this.graph.isLabelClipped = (cell: MxCell) => {
            if (cell.isVertex()) {
                var state = this.graph.getView().getState(cell);
                var align = mxUtils.getValue(state.style, mxConstants.STYLE_VERTICAL_ALIGN, mxConstants.ALIGN_MIDDLE);
                var overflow = mxUtils.getValue(state.style, mxConstants.STYLE_OVERFLOW, null);
                return align === mxConstants.ALIGN_MIDDLE || overflow === "hidden";
            }
            return false;
        };

        this.graph.getLabel = (cell: MxCell) => {
            if (cell.getLabel) {
                return cell.getLabel();
            }
            if (cell.isVertex()) {
                if (cell.value != null && cell.value.label != null) {
                    if (this.graph.isHtmlLabel(cell)) {
                        return this.sanitizeInternal(cell.value.label);
                    }
                    return cell.value.label;
                }
            } else if (cell.isEdge()) {
                var connection = <IConnection>cell.value;
                if (connection != null && connection.label != null) {
                    return this.sanitizeInternal(connection.label);
                }
            }
            return cell.value;
        };
        //init readonly mode
        this.graph.edgeLabelsMovable = false;
        this.graph.isCellDisconnectable = () => false;
        this.graph.isTerminalPointMovable = () => false;
        this.graph.isCellMovable = () => false;
        this.graph.isCellResizable = () => false;
        this.graph.isCellEditable = () => false;
        this.graph.isCellDeletable = () => false;
        this.graph.isCellsLocked = () => true;

        this.applyDefaultSyles();

        this.graph.isHtmlLabel = (cell: MxCell) => {
            if (cell.value != null && cell.value.isRichText != null) {
                return cell.value.isRichText;
            }
            return true;
        };
        this.initSelection();
    }

    private initSelection() {
        this.graph.getSelectionModel().setSingleSelection(true);
        var baseIsEventIgnored = this.graph.isEventIgnored;
        this.graph.isEventIgnored = (evtName, me, sender) => {
            if (this.disabledUserSelection) {
                return true;
            }
            return baseIsEventIgnored.call(this.graph, evtName, me, sender);
        };
        this.graph.popupMenuHandler.selectOnPopup = false;
        this.graph.graphHandler.getInitialCellForEvent = this.getInitialCellForEvent;

        this.graph.getSelectionModel().addListener(mxEvent.CHANGE, (sender, evt) => {
            var cell = this.getLastSelectedCell();
            if (cell != null) {
                var element = this.getDiagramElement(cell);
                this.selectionListeners.forEach((listener: ISelectionListener) => {
                    listener([element]);
                });
            }
            evt.consume();
        });
    }

    private getLastSelectedCell() {
        var selectedCells = this.graph.getSelectionCells();
        return selectedCells[selectedCells.length - 1];
    }

    private getInitialCellForEvent = (me: MxMouseEvent) => {
        var cell = me.getCell();

        var state = this.graph.getView().getState(cell);
        var style = (state != null) ? state.style : this.graph.getCellStyle(cell);

        while (cell != null && style[Styles.STYLE_SELECTABLE] === 0) {
            cell = cell.getParent();

            state = this.graph.getView().getState(cell);
            style = (state != null) ? state.style : this.graph.getCellStyle(cell);

            if (style[Styles.STYLE_SELECTABLE] !== 0) {
                break;
            }
        }
        return this.getSelectedCell(cell);
    }

    private getSelectedCell(sourceCell: MxCell): MxCell {
        var selectedElement = <IHierarchyElement>this.getDiagramElement(sourceCell);
        var parent = selectedElement != null ? selectedElement.parent : null;
        var parentGroup: IHierarchyElement = null;
        var lastSelectedElement = this.getLastSelectedElement();
        var commonParent: IHierarchyElement = null;
        if (lastSelectedElement !== selectedElement) {
            commonParent = this.getCommonParent(lastSelectedElement, selectedElement);
        }
        while (parent != null && lastSelectedElement !== parent && commonParent !== parent) {
            if (parent.type === Shapes.GROUP) {
                parentGroup = parent;
            }
            parent = parent.parent;
        }
        return parentGroup != null ? this.createdVertices[parentGroup.id] : sourceCell;
    }

    private getCommonParent(lastSelectedElement: IHierarchyElement, selectedElement: IHierarchyElement): IHierarchyElement {
        var allParentIds = Object.create(null);
        var parent = lastSelectedElement != null ? lastSelectedElement.parent : null;
        while (parent != null) {
            allParentIds[parent.id] = true;
            parent = parent.parent;
        }
        parent = selectedElement != null ? selectedElement.parent : null;
        while (parent != null) {
            if (parent.id in allParentIds) {
                return parent;
            }
            parent = parent.parent;
        }
        return null;
    }

    private getLastSelectedElement(): IHierarchyElement {
        var cell = this.graph.getSelectionCell();
        return cell != null ? <IHierarchyElement>this.getDiagramElement(cell) : null;
    }

    private getDiagramElement(cell: MxCell): IDiagramElement {
        if (cell != null && cell.value != null && cell.value.id > 0) {
            return cell.value;
        }
        return null;
    }

    private applyDefaultSyles() {
        //Selection styles
        mxConstants.VERTEX_SELECTION_COLOR = "#32CFFF";
        mxConstants.VERTEX_SELECTION_STROKEWIDTH = 2;
        mxConstants.VERTEX_SELECTION_DASHED = false;
        mxConstants.EDGE_SELECTION_COLOR = "#385D8A";
        mxConstants.EDGE_SELECTION_STROKEWIDTH = 2;
        mxConstants.EDGE_SELECTION_DASHED = false;
        mxConstants.LOCKED_HANDLE_FILLCOLOR = "#22AA06";
        mxConstants.CURSOR_BEND_HANDLE = "default";
        mxConstants.CURSOR_TERMINAL_HANDLE = "default";
        // Creates the default style for vertices
        mxConstants.SHADOWCOLOR = "black";
        var style = [];
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
        style[mxConstants.STYLE_STROKECOLOR] = "black";
        style[mxConstants.STYLE_FILLCOLOR] = "white";
        this.graph.getStylesheet().putDefaultVertexStyle(style);

        this.graph.getStylesheet().putDefaultEdgeStyle(style);
    }

    private registerStencils(diagramType: string, stencilService: IStencilService): void {
        var stencil = stencilService.getStencil(diagramType);
        if (stencil != null) {
            var shape = stencil.firstChild;
            while (shape != null) {
                if (shape.nodeType === mxConstants.NODETYPE_ELEMENT) {
                    var shapeName = (<Element>shape).getAttribute("name");
                    mxStencilRegistry.addStencil(shapeName.toLowerCase(), new mxStencil(shape));
                }
                shape = shape.nextSibling;
            }
        }
    }

    private drawDiagramInternal(diagram: IDiagram, shapeFactory: IShapeTemplateFactory) {
        // Gets the default parent for inserting new cells. This
        // is normally the first child of the root (ie. layer 0).
        var parent = this.graph.getDefaultParent();

        // Adds cells to the model in a single step
        this.graph.getModel().beginUpdate();
        try {
            var hierarchyDiagram = HierarchyHelper.createHierarchy(diagram, true);
            this.drawDiagramElementsRecursively(hierarchyDiagram.children, shapeFactory, parent);

            // Extended Connections for Use Case Diagrams need to be drawn after all shapes are drawn
            hierarchyDiagram.children.forEach((diagramElement) => {
                if (!diagramElement.isShape && diagramElement.points === null) {
                    this.drawDiagramConnection(diagramElement, shapeFactory, parent);
                }
            });
        }
        finally {
            this.graph.getModel().endUpdate();
        }
    }

    private offsetConnectors(sourceCell: MxCell, targetCell: MxCell, connection: IConnection) {
        if (connection.points == null) {
            return;
        }

        var sourceOffset = this.computeOffset(sourceCell);
        var targetOffset = this.computeOffset(targetCell);

        var i = 0;
        connection.points.forEach((point: MxPoint) => {
            if (i === 0) {
                //apply source offset to first point
                point.x -= sourceOffset.x;
                point.y -= sourceOffset.y;
            } else if (i === connection.points.length - 1) {
                //apply target offset to last point
                point.x -= targetOffset.x;
                point.y -= targetOffset.y;
            }
            i++;
        });
    }

    private computeOffset(cell: MxCell): MxPoint {
        var offset = MxFactory.point(0, 0);
        if (cell != null) {
            var sourceParentCell: MxCell = cell.getParent();
            if (sourceParentCell != null) {
                //add geometry of all parents
                do {
                    if (sourceParentCell.geometry != null) {
                        offset.x += sourceParentCell.geometry.x;
                        offset.y += sourceParentCell.geometry.y;
                    }
                    sourceParentCell = sourceParentCell.getParent();
                } while (sourceParentCell != null);
            }
        }
        return offset;
    }

    private drawDiagramElementsRecursively(children: Array<IHierarchyElement>, factory: IShapeTemplateFactory, parent: MxCell): Array<MxCell> {
        var vertex: MxCell = null;
        var createdCells = [];
        children.forEach((diagramElement) => {
            if (diagramElement.isShape) {
                // Draw shape
                var createCellFunct = factory.createShapeTemplate(diagramElement.type);
                if (createCellFunct != null) {
                    vertex = createCellFunct(diagramElement);
                    this.createdVertices[diagramElement.id] = vertex;

                    createdCells.push(vertex);
                    if (diagramElement.children.length > 0) {
                        // If shape has children shapes - draw these too
                        var cells = this.drawDiagramElementsRecursively(diagramElement.children, factory, parent);
                        if (diagramElement.type === Shapes.GROUP) {
                            this.graph.groupCells(vertex, null, cells);
                        } else {
                            this.graph.addCell(vertex, parent);
                            cells.forEach((cell) => {
                                cell.geometry.x = cell.geometry.x - vertex.geometry.x;
                                cell.geometry.y = cell.geometry.y - vertex.geometry.y;
                            });
                            this.graph.addCells(cells, vertex);
                        }
                    } else {
                        this.graph.addCell(vertex, parent);
                    }
                }
            } else {
                // Draw connection
                this.drawDiagramConnection(diagramElement, factory, parent);
            }
        });
        return createdCells;
    }

    private drawDiagramConnection(diagramElement: IHierarchyElement, factory: IShapeTemplateFactory, parent: MxCell) {
        var createConnectionFunct = factory.createConnectorTemplate();
        var sourceCell = this.createdVertices[diagramElement.sourceId];
        var targetCell = this.createdVertices[diagramElement.targetId];

        this.offsetConnectors(sourceCell, targetCell, diagramElement);

        var index = this.graph.getModel().getChildCount(parent);
        var edge = createConnectionFunct(diagramElement);

        this.initConnectionAnchorPoint(edge, sourceCell, targetCell, diagramElement);

        if (sourceCell != null && targetCell != null && sourceCell.getParent() === targetCell.getParent()) {
            index = Math.max(sourceCell.getParent().getIndex(sourceCell), targetCell.getParent().getIndex(targetCell));
        }

        this.graph.addCell(edge, parent, index, sourceCell, targetCell);
        this.drawConnection(edge, diagramElement, parent);
    }

    private drawConnection(edge: MxCell, connection: IConnection, parent: MxCell) {
        if (connection.points != null && connection.points.length >= 2) {
            var source = connection.points[0];
            var target = connection.points[connection.points.length - 1];

            if (connection.sourceLabel) {
                var sourceLabel = this.createConnectionLabel(connection, true);
                this.graph.addCell(sourceLabel, parent);
            }

            if (connection.targetLabel) {
                var targetLabel = this.createConnectionLabel(connection, false);
                this.graph.addCell(targetLabel, parent);
            }

            edge.geometry.setTerminalPoint(MxFactory.point(source.x, source.y), true);
            edge.geometry.setTerminalPoint(MxFactory.point(target.x, target.y), false);

            var points: Array<MxPoint> = [];
            if (connection.points.length === 2 && source.x === target.x && source.y === target.y) {
                points.push(MxFactory.point(connection.points[0].x + 3, connection.points[0].y));
            } else {
                if (connection.type) {
                    switch (connection.type.toLowerCase()) {
                        case ConnectorTypes.CURVED:
                            var centerX = Math.min(target.x, source.x) + Math.abs(target.x - source.x) / 2;
                            points.push(MxFactory.point(centerX, source.y));
                            points.push(MxFactory.point(centerX, target.y));
                            break;
                        case ConnectorTypes.RIGHT_ANGLED:
                            for (var i = 1; i < connection.points.length - 1; i++) {
                                var point = connection.points[i];
                                points.push(MxFactory.point(point.x, point.y));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            edge.geometry.points = points;
        }
    }

    private createConnectionLabel(connection: IConnection, isSource: boolean): MxCell {
        var style = new Style();
        style[mxConstants.STYLE_RESIZABLE] = 0;
        style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
        style[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_FILLCOLOR] = mxConstants.NONE;

        var value = isSource ? connection.sourceLabel : connection.targetLabel;
        var label = MxFactory.vertex(value, MxFactory.geometry(0, 0, 0, 0), style.convertToString());
        var labelSize = this.getPreferredSizeForCell(label);
        var labelPosition = ConnectionExtensions.transformToConnectionPoint(connection, labelSize, isSource);
        label.geometry.x = labelPosition.x;
        label.geometry.y = labelPosition.y;
        label.geometry.width = labelSize.width;
        label.geometry.height = labelSize.height;
        return label;
    }

    private getPreferredSizeForCell(cell: MxCell) {
        var cellSize = this.graph.getPreferredSizeForCell(cell);
        cellSize.width += 5;
        return cellSize;
    }

    private initConnectionAnchorPoint(edge: MxCell, source: MxCell, target: MxCell, connection: IConnection) {
        var sourcePoint: IPoint;
        var targetPoint: IPoint;
        if (connection.points == null || connection.points.length < 2) {
            if (source == null || target == null) {
                return;
            }
            var points = ConnectionExtensions.closestConnectionPoints(source.getGeometry(), target.getGeometry());
            sourcePoint = points[0];
            targetPoint = points[1];
        } else {
            sourcePoint = connection.points[0];
            targetPoint = connection.points[connection.points.length - 1];
        }
        if (sourcePoint != null && source != null) {
            this.setConnectionAnchor(edge, source, sourcePoint, true);
        }
        if (targetPoint != null && target != null) {
            this.setConnectionAnchor(edge, target, targetPoint, false);
        }
    }

    private setConnectionAnchor(edge: MxCell, vertex: MxCell, point: IPoint, source: boolean) {
        var styleX = source ? mxConstants.STYLE_EXIT_X : mxConstants.STYLE_ENTRY_X;
        var styleY = source ? mxConstants.STYLE_EXIT_Y : mxConstants.STYLE_ENTRY_Y;
        var relativePoint = MathExtensions.toRelativePoint(vertex.getGeometry(), point);
        var style = mxUtils.setStyle(edge.getStyle(), styleX, relativePoint.x);
        edge.setStyle(mxUtils.setStyle(style, styleY, relativePoint.y));
    }

    private sanitizeInternal(html: string): string {
        if (this.sanitize != null) {
            return this.sanitize(html);
        }
        return html;
    }
}