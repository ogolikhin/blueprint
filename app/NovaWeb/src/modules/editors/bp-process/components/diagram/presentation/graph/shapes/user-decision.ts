import {IProcessShape} from "../../../../../models/process-models";
import {ModalDialogType} from "../../../../modal-dialogs/modal-dialog-constants";
import {IProcessGraph, IDiagramNode} from "../models/";
import {IDecision} from "../models/";
import {NodeType, NodeChange} from "../models/";
import {IDialogParams} from "../../../../messages/message-dialog";
import {DiagramNode} from "./diagram-node";
import {NodeFactorySettings} from "./node-factory-settings";
import {Button} from "../buttons/button";
import {Label, LabelStyle} from "../labels/label";

export class UserDecision extends DiagramNode<IProcessShape> implements IDecision {
    private LABEL_EDIT_MAXLENGTH = 32;
    private LABEL_VIEW_MAXLENGTH = 28;
    private USER_DECISION_WIDTH = 120;
    private USER_DECISION_HEIGHT = 120;
    private USER_DECISION_SHIFT = 33;
    private BUTTON_SIZE = 16;
    private DELETE_SHAPE_OFFSET = 3;
    
    private detailsButton: Button;
    private deleteShapeButton:Button;    

    private rootScope: any;

    constructor(model: IProcessShape, rootScope: any, nodeFactorySettings: NodeFactorySettings = null) {
        super(model, NodeType.UserDecision);

        this.rootScope = rootScope;
        this.initButtons(model.id.toString(), nodeFactorySettings);
    }

    private initButtons(nodeId: string, nodeFactorySettings: NodeFactorySettings = null) {
        //Details button
        this.detailsButton = new Button(`DB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, "/novaweb/static/bp-process/images/adddetails-neutral.svg");

        if (nodeFactorySettings && nodeFactorySettings.isDetailsButtonEnabled) {
            this.detailsButton.setClickAction(() => this.openDialog(ModalDialogType.UserSystemDecisionDetailsDialogType));
        } else {
            this.detailsButton.setClickAction(() => { });
        }

        this.detailsButton.setHoverImage("/novaweb/static/bp-process/images/adddetails-hover.svg");
        this.detailsButton.setDisabledImage("/novaweb/static/bp-process/images/adddetails-mute.svg");
        this.detailsButton.setTooltip(this.rootScope.config.labels["ST_Settings_Label"]);

        //Delete process shape button
        this.deleteShapeButton = new Button(`DS${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, "/novaweb/static/bp-process/images/delete-neutral.svg");

        if (nodeFactorySettings && nodeFactorySettings.isDeleteShapeEnabled) {
            this.deleteShapeButton.setClickAction(() => {
                console.log("Delete User Decision shape clicked");
            });
        } else {
            this.deleteShapeButton.setClickAction(() => { });
        }

        this.deleteShapeButton.setHoverImage("/novaweb/static/bp-process/images/delete-hover.svg");
        this.deleteShapeButton.setDisabledImage("/novaweb/static/bp-process/images/delete-inactive.svg");
        this.deleteShapeButton.setTooltip(this.rootScope.config.labels["ST_Settings_Label"]);
    }

    public setLabelWithRedrawUi(value: string) {
        this.setModelName(value, true);
    }

    protected updateCellLabel(value: string) {
        this.textLabel.text = value;
        this.notify(NodeChange.Update, true);
    }

    public getX(): number {
        return this.getCenter().x + this.USER_DECISION_SHIFT;
    }

    public getHeight(): number {
        return this.USER_DECISION_HEIGHT;
    }

    public getWidth(): number {
        return this.USER_DECISION_WIDTH;
    }

    public addNode(graph: IProcessGraph): IDiagramNode {
        return this;
    }

    public deleteNode(graph: IProcessGraph) {
    }

    public hideMenu(mxGraph: MxGraph) {
        mxGraph.removeCellOverlays(this);
    }

    public showMenu(mxGraph: MxGraph) {
        // #TODO change URL for svg 
        this.addOverlay(mxGraph,
            this,
            "/novaweb/static/bp-process/images/add-neutral.svg",
            16,
            16,
            "Add Task/Decision",
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

    public render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode {

        var mxGraph = graph.getMxGraph();
        var fillColor = "#FFFFFF";

        if (this.model.id < 0) {
            fillColor = justCreated ? this.newShapeColor : "#FBF8E7";
        }
                
        this.insertVertex(
            mxGraph,
            this.model.id.toString(),
            null,
            x - this.USER_DECISION_SHIFT,
            y,
            this.USER_DECISION_WIDTH,
            this.USER_DECISION_HEIGHT,
            "shape=rhombus;strokeColor=#D4D5DA;fillColor=" + fillColor +
            ";fontColor=#4C4C4C;fontFamily=Open Sans, sans-serif;fontStyle=1;fontSize=12;foldable=0;"
        );
        var textLabelStyle: LabelStyle = new LabelStyle(
            "Open Sans",
            12,
            "transparent",
            "#4C4C4C",
            "bold",
            y - 20,
            x - this.USER_DECISION_WIDTH / 2 - 24,
            44,
            this.USER_DECISION_WIDTH - 20,
            "#4C4C4C"
        );
        this.textLabel = new Label((value: string) => { this.label = value; },
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
            this.USER_DECISION_WIDTH / 2 - this.BUTTON_SIZE / 2,
            this.BUTTON_SIZE + this.DELETE_SHAPE_OFFSET,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0"
        );

        this.detailsButton.render(
            mxGraph,
            this,
            this.USER_DECISION_WIDTH / 2 - this.BUTTON_SIZE / 2,
            this.USER_DECISION_HEIGHT - this.BUTTON_SIZE - 10,
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
            this.USER_DECISION_WIDTH / 2,
            this.USER_DECISION_HEIGHT / 2
        );

        return this;
    }

    public getElementTextLength(cell: MxCell): number {
        /*
        * get the maximum length of text that can be entered
        */
        return this.LABEL_EDIT_MAXLENGTH;
    }

    public formatElementText(cell: MxCell, text: string): string {

        /***
         * This function returns formatted text to the getLabel()
         * function to display the label  
         */

        if (cell && text) {
            var maxLen: number = this.LABEL_VIEW_MAXLENGTH;

            if (text.length > maxLen) {
                text = text.substr(0, maxLen) + " ...";
            }
        }

        return text;
    }

    public setElementText(cell: MxCell, text: string) {
        /*
        * save text for the node or for an element within
        * the node
        */
        this.label = text;
    }

    private openDialog(dialogType: ModalDialogType) {
        //#TODO: implement new mechanism to communicate with modal dialogs
        //this.rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName,
        //    this.model.id,
        //    dialogType);
    }

    public getDeleteDialogParameters(): IDialogParams {
        let dialogParams: IDialogParams = {};
        dialogParams.message = this.rootScope.config.labels["ST_Confirm_Delete_User_Decision"];
        return dialogParams;
    }

    public canDelete(): boolean {
        return true;
    }

    public getMergeNode(graph: IProcessGraph, orderIndex: number): IProcessShape {
        var id = graph.getDecisionBranchDestLinkForIndex(this.model.id, orderIndex).destinationId;
        return graph.getShapeById(id);
    }
}