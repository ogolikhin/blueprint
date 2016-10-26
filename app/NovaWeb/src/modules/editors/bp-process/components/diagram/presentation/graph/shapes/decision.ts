
import { IProcessShape } from "../../../../../models/process-models";
import { ModalDialogType } from "../../../../modal-dialogs/modal-dialog-constants";
import { IProcessGraph, IDiagramNode } from "../models/";
import { IDecision, NodeType } from "../models/";
import { IDialogParams } from "../../../../messages/message-dialog";
import { DiagramNode } from "./diagram-node";
import { NodeFactorySettings } from "./node-factory-settings";
import { Button } from "../buttons/button";
import { DeleteShapeButton } from "../buttons/delete-shape-button";
import { Label, LabelStyle } from "../labels/label";
import { ProcessEvents } from "../../../process-diagram-communication";

export abstract class Decision extends DiagramNode<IProcessShape> implements IDecision {
    protected abstract DECISION_SHIFT: number;
    protected abstract DEFAULT_FILL_COLOR: string;
    protected abstract DEFAULT_BORDER_COLOR: string;
    protected abstract textLabelLeft;
    protected abstract textLabelWidth;

    protected BUTTON_SIZE: number = 16;
    protected MENU_SIZE: number = 16;

    protected rootScope: ng.IRootScopeService;
    protected detailsButton: Button;
    protected deleteShapeButton: Button;

    protected DECISION_WIDTH = 120;
    protected DECISION_HEIGHT = 120;
    protected LABEL_EDIT_MAXLENGTH = 32;
    protected LABEL_VIEW_MAXLENGTH = 28;
    protected DELETE_SHAPE_OFFSET = 3;

    protected get NEW_FILL_COLOR(): string {
        return "#FBF8E7";
    }

    constructor(
        model: IProcessShape,
        rootScope: ng.IRootScopeService,
        private nodeFactorySettings: NodeFactorySettings = null
    ) {
        super(model);

        this.rootScope = rootScope;
        this.initButtons(model.id.toString(), nodeFactorySettings);
    }

    public render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode {
        this.dialogManager = graph.viewModel.communicationManager.modalDialogManager;
        this.processDiagramManager = graph.viewModel.communicationManager.processDiagramCommunication;

        const mxGraph = graph.getMxGraph();
        let fillColor = this.DEFAULT_FILL_COLOR;
        if (this.model.id < 0) {
            fillColor = justCreated ? this.newShapeColor : this.NEW_FILL_COLOR;
        }

        this.insertVertex(
            mxGraph,
            this.model.id.toString(),
            null,
            x - this.DECISION_SHIFT,
            y,
            this.DECISION_WIDTH,
            this.DECISION_HEIGHT,
            "shape=rhombus;strokeColor=" + this.DEFAULT_BORDER_COLOR + ";fillColor=" + fillColor +
            ";fontColor=#4C4C4C;fontFamily=Open Sans, sans-serif;fontStyle=1;fontSize=12;foldable=0;"
        );


        const textLabelStyle: LabelStyle = new LabelStyle(
            "Open Sans",
            12,
            "transparent",
            "#4C4C4C",
            "bold",
            y - 20,
            x - this.textLabelLeft,
            44,
            this.textLabelWidth,
            "#4C4C4C"
        );


        this.textLabel = new Label((value: string) => {
            this.label = value;
        },
            graph.getHtmlElement(),
            this.model.id.toString(),
            "Label-B" + this.model.id.toString(),
            this.label,
            textLabelStyle,
            this.LABEL_EDIT_MAXLENGTH,
            this.LABEL_VIEW_MAXLENGTH,
            graph.viewModel.isReadonly);

        if (!graph.viewModel.isReadonly) {
            this.showMenu(mxGraph);
        }


        this.deleteShapeButton.render(
            mxGraph,
            this,
            this.DECISION_WIDTH / 2 - this.BUTTON_SIZE / 2,
            this.BUTTON_SIZE + this.DELETE_SHAPE_OFFSET,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0"
        );

        this.detailsButton.render(
            mxGraph,
            this,
            this.DECISION_WIDTH / 2 - this.BUTTON_SIZE / 2,
            this.DECISION_HEIGHT - this.BUTTON_SIZE - 10,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0"
        );


        // DO NOT DELETE!!! this is needed for the labels functionality
        this.addOverlay(
            mxGraph,
            this.detailsButton,
            null,
            this.BUTTON_SIZE,
            this.BUTTON_SIZE,
            null,
            mxConstants.ALIGN_LEFT,
            mxConstants.ALIGN_TOP,
            this.DECISION_WIDTH / 2,
            this.DECISION_HEIGHT / 2
        );

        return this;
    }

    public setLabelWithRedrawUi(value: string) {
        this.setModelName(value, true);
    }

    public hideMenu(mxGraph: MxGraph) {
        mxGraph.removeCellOverlays(this);
    }

    public showMenu(mxGraph: MxGraph) {
        // #TODO change URL for svg
        this.addOverlay(mxGraph,
            this,
            "/novaweb/static/bp-process/images/add-neutral.svg",
            this.MENU_SIZE,
            this.MENU_SIZE,
            null, // tooltip
            mxConstants.ALIGN_CENTER,
            mxConstants.ALIGN_BOTTOM,
            0,
            7,
            "hand"
        );
    }

    public renderLabels() {
        this.textLabel.render();
    }

    public canDelete(): boolean {
        return true;
    }

    protected updateCellLabel(value: string) {
        this.textLabel.text = value;
    }

    public getMergeNode(graph: IProcessGraph, orderIndex: number): IProcessShape {
        const id = graph.getDecisionBranchDestLinkForIndex(this.model.id, orderIndex).destinationId;
        return graph.getShapeById(id);
    }

    private initButtons(nodeId: string, nodeFactorySettings: NodeFactorySettings = null) {
        //Details button
        this.detailsButton = new Button(`DB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("adddetails-neutral.svg"));

        if (nodeFactorySettings && nodeFactorySettings.isDetailsButtonEnabled) {
            this.detailsButton.setClickAction(() => this.openDialog(ModalDialogType.UserSystemDecisionDetailsDialogType));
        }

        this.detailsButton.setHoverImage(this.getImageSource("adddetails-hover.svg"));
        this.detailsButton.setDisabledImage(this.getImageSource("adddetails-mute.svg"));
        this.detailsButton.setTooltip(this.rootScope["config"].labels["ST_Settings_Label"]);

        //Delete process shape button
        const clickAction = () => {
            this.processDiagramManager.action(ProcessEvents.DeleteShape);
        };

        this.deleteShapeButton = new DeleteShapeButton(nodeId, this.BUTTON_SIZE, this.BUTTON_SIZE,
            this.rootScope["config"].labels["ST_Shapes_Delete_Tooltip"], nodeFactorySettings, clickAction);
    }


    private openDialog(dialogType: ModalDialogType) {
        this.dialogManager.openDialog(this.model.id, dialogType);
    }

}
