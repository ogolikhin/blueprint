﻿import {IProcessShape} from "../../../../../models/process-models";
import {ModalDialogType} from "../../../../modal-dialogs/modal-dialog-constants";
import {IProcessGraph, IDiagramNode, IUserTaskChildElement} from "../models/";
import {IDecision, ISystemTask} from "../models/";
import {NodeType, NodeChange} from "../models/";
import {UserTaskChildElement} from "./user-task-child-element";
import {IDialogParams} from "../../../../messages/message-dialog";
import {NodeFactorySettings} from "./node-factory-settings";
import {Button} from "../buttons/button";
import {DeleteShapeButton} from "../buttons/delete-shape-button";
import {Label, LabelStyle} from "../labels/label";
import {IProcessDiagramCommunication} from "../../../process-diagram-communication";
import {IModalDialogCommunication} from "../../../../modal-dialogs/modal-dialog-communication";
import {ProcessEvents} from "../../../process-diagram-communication";

export class SystemDecision extends UserTaskChildElement<IProcessShape> implements IDecision, IUserTaskChildElement {
    private SYSTEM_DECISION_WIDTH = 120;
    private SYSTEM_DECISION_HEIGHT = 120;
    private LABEL_EDIT_MAXLENGTH = 32;
    private LABEL_VIEW_MAXLENGTH = 28;
    private DEFAULT_FILL_COLOR: string = "#E2F3FF";
    private DEFAULT_BORDER_COLOR: string = "#53BBED";
    private NEW_FILL_COLOR: string = "#FBF8E7";
    private MENU_SIZE: number = 16;
    private BUTTON_SIZE: number = 16;
    private DELETE_SHAPE_OFFSET = 3;

    private detailsButton: Button;
    private deleteShapeButton: Button;

    private rootScope: any;
    private processDiagramManager: IProcessDiagramCommunication;
    private dialogManager: IModalDialogCommunication;

    constructor(model: IProcessShape, rootScope: any, nodeFactorySettings: NodeFactorySettings = null) {
        super(model, NodeType.SystemDecision);

        this.rootScope = rootScope;
        this.initButtons(model.id.toString(), nodeFactorySettings);
    }

    private initButtons(nodeId: string, nodeFactorySettings: NodeFactorySettings = null) {
        //Modal Dialog
        this.detailsButton = new Button(`DB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("adddetails-neutral.svg"));
        if (nodeFactorySettings && nodeFactorySettings.isDetailsButtonEnabled) {
            this.detailsButton.setClickAction(() => this.openDialog(ModalDialogType.UserSystemDecisionDetailsDialogType));
        } else {
            this.detailsButton.setClickAction(() => {
                //fixme: empty blocks should be removed or undefined
            });
        }

        this.detailsButton.setHoverImage(this.getImageSource("adddetails-hover.svg"));
        this.detailsButton.setDisabledImage(this.getImageSource("adddetails-mute.svg"));
        this.detailsButton.setTooltip(this.rootScope.config.labels["ST_Settings_Label"]);

        //Delete process shape button
        const clickAction = () => {
                this.processDiagramManager.action(ProcessEvents.DeleteShape);
        };

        this.deleteShapeButton = new DeleteShapeButton(nodeId, this.BUTTON_SIZE, this.BUTTON_SIZE,
            this.rootScope.config.labels["ST_Shapes_Delete_Tooltip"], nodeFactorySettings, clickAction);
    }

    public setLabelWithRedrawUi(value: string) {
        this.setModelName(value, true);
    }

    protected updateCellLabel(value: string) {
        this.textLabel.text = value;
        this.notify(NodeChange.Update, true);
    }

    public showMenu(mxGraph: MxGraph) {
        // #TODO change the URL to load svg
        this.addOverlay(mxGraph, this, "/novaweb/static/bp-process/images/add-neutral.svg",
            this.MENU_SIZE, this.MENU_SIZE, "Add Branch", mxConstants.ALIGN_CENTER, mxConstants.ALIGN_BOTTOM, 0, 7, "hand");
        this.detailsButton.setVisible(true);
    }

    public hideMenu(mxGraph: MxGraph) {
        mxGraph.removeCellOverlays(this);
        this.detailsButton.setVisible(false);
    }

    public renderLabels() {
        this.textLabel.render();
    }

    public render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode {
        this.dialogManager = graph.viewModel.communicationManager.modalDialogManager;
        this.processDiagramManager = graph.viewModel.communicationManager.processDiagramCommunication;

        const mxGraph = graph.getMxGraph();

        let fillColor = this.DEFAULT_FILL_COLOR;

        if (this.model.id < 0) {
            fillColor = justCreated ? this.newShapeColor : this.NEW_FILL_COLOR;
        }

        this.insertVertex(mxGraph, this.model.id.toString(), null, x, y, this.SYSTEM_DECISION_WIDTH, this.SYSTEM_DECISION_HEIGHT,
            "shape=rhombus;strokeColor=" + this.DEFAULT_BORDER_COLOR + ";fillColor=" + fillColor +
            ";fontColor=#4C4C4C;fontFamily=Open Sans, sans-serif;fontStyle=1;fontSize=12;foldable=0;");

        const textLabelStyle: LabelStyle = new LabelStyle(
            "Open Sans",
            12,
            "transparent",
            "#4C4C4C",
            "bold",
            y - 20,
            x - this.SYSTEM_DECISION_WIDTH / 2 + 15,
            44,
            this.SYSTEM_DECISION_WIDTH - 30,
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
            this.SYSTEM_DECISION_WIDTH / 2 - this.BUTTON_SIZE / 2,
            this.BUTTON_SIZE + this.DELETE_SHAPE_OFFSET,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0"
        );

        this.detailsButton.render(mxGraph, this, this.SYSTEM_DECISION_WIDTH / 2 - this.BUTTON_SIZE / 2, this.SYSTEM_DECISION_HEIGHT - this.BUTTON_SIZE - 10,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");

        // DO NOT DELETE!!! this is needed for the labels functionality
        this.addOverlay(mxGraph, this.detailsButton, null, this.BUTTON_SIZE, this.BUTTON_SIZE, null,
            mxConstants.ALIGN_LEFT, mxConstants.ALIGN_TOP, this.BUTTON_SIZE / 2, this.BUTTON_SIZE / 2);

        return this;
    }

    // get the maximum length of text that can be entered
    public getElementTextLength(cell: MxCell): number {
        return this.LABEL_EDIT_MAXLENGTH;
    }

    // This function returns formatted text to the getLabel() function to display the label
    public formatElementText(cell: MxCell, text: string): string {
        if (cell && text) {
            const maxLen: number = this.LABEL_VIEW_MAXLENGTH;

            if (text.length > maxLen) {
                text = text.substr(0, maxLen) + " ...";
            }
        }

        return text;
    }

    // Save text for the node or for an element within the node
    public setElementText(cell: MxCell, text: string) {
        this.label = text;
    }

    public getFirstSystemTask(graph: IProcessGraph): ISystemTask {
        const targets = this.getTargets(graph.getMxGraphModel());

        if (targets) {
            const firstTarget = targets[0];

            if (firstTarget != null && firstTarget.getNodeType() === NodeType.SystemTask) {
                return <ISystemTask>firstTarget;
            }

            if (firstTarget.getNodeType() === NodeType.SystemDecision) {
                return (<SystemDecision>firstTarget).getFirstSystemTask(graph);
            }
        }

        return null;
    }

    // Returns an array of targets, which an be either SystemTasks or SystemDecisions
    // @param graph
    public getSystemNodes(mxGraph: MxGraph): IDiagramNode[] {
        return this.getTargets(mxGraph.getModel());
    }

    private openDialog(dialogType: ModalDialogType) {
        this.dialogManager.openDialog(this.model.id, dialogType);
    }

    public getDeleteDialogParameters(): IDialogParams {
        let dialogParams: IDialogParams = {};
        dialogParams.message = this.rootScope.config.labels["ST_Confirm_Delete_System_Decision"];
        return dialogParams;
    }

    public canDelete(): boolean {
        return true;
    }

    public getMergeNode(graph: IProcessGraph, orderIndex: number): IProcessShape {
        const id = graph.getDecisionBranchDestLinkForIndex(this.model.id, orderIndex).destinationId;
        return graph.getShapeById(id);
    }
}
