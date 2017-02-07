﻿import {
    IArtifactProperty,
    IUserTaskShape,
    PropertyTypePredefined,
    IArtifactReference
} from "../../../../../models/process-models";
import {ItemIndicatorFlags} from "../../../../../models/enums";
import {ModalDialogType} from "../../../../modal-dialogs/modal-dialog-constants";
import {
    ISystemTask,
    IUserTask,
    IUserStoryProperties,
    IProcessGraph,
    IDiagramNode,
    NodeType,
    ElementType
} from "../models/";
import {IDialogParams} from "../../../../messages/message-dialog";
import {ShapesFactory} from "./shapes-factory";
import {DiagramNodeElement} from "./diagram-element";
import {DiagramNode} from "./diagram-node";
import {NodeFactorySettings} from "./node-factory-settings";
import {Button} from "../buttons/button";
import {DeleteShapeButton} from "../buttons/delete-shape-button";
import {Label, LabelStyle, LabelType, ILabel} from "../labels/label";
import {SystemDecision} from "./";
import {ProcessEvents} from "../../../process-diagram-communication";

export class UserStoryProperties implements IUserStoryProperties {
    public nfr: IArtifactProperty;
    public businessRules: IArtifactProperty;
}

export class UserTask extends DiagramNode<IUserTaskShape> implements IUserTask {

    private USER_TASK_WIDTH = 126;
    private USER_TASK_HEIGHT = 150;
    private LABEL_VIEW_MAXLENGTH = 60;
    private LABEL_EDIT_MAXLENGTH = 140;
    private PERSONA_VIEW_MAXLENGTH = 16;
    private PERSONA_EDIT_MAXLENGTH = 40;
    private BUTTON_SIZE = 16;

    private DEFAULT_BORDER_COLOR: string = "#D4D5DA";
    private HIGHLIGHT_BORDER_COLOR: string = "#53BBED";

    private header: mxCell;
    private personaLabel: ILabel;
    private footerCell: MxCell;
    private commentsButton: Button;
    private deleteShapeButton: Button;
    private detailsButton: Button;
    private previewButton: Button;
    private linkButton: Button;
    private rootScope: any;

    constructor(model: IUserTaskShape, rootScope: any, private nodeFactorySettings: NodeFactorySettings = null,
                private shapesFactoryService: ShapesFactory) {
        super(model);

        this.rootScope = rootScope;

        this.initButtons(model.id.toString(), nodeFactorySettings);

        this.initChildElements();

    }

    private initButtons(nodeId: string, nodeFactorySettings: NodeFactorySettings = null) {

        //Delete Shape
        const deleteClickAction = () => {
            this.processDiagramManager.action(ProcessEvents.DeleteShape, this);
        };

        this.deleteShapeButton = new DeleteShapeButton(nodeId, this.BUTTON_SIZE, this.BUTTON_SIZE,
            this.rootScope.config.labels["ST_Shapes_Delete_Tooltip"], nodeFactorySettings, deleteClickAction);


        //Shape Comments
        this.commentsButton = new Button(`CB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("comments-neutral.svg"));
        this.commentsButton.isEnabled = !this.isNew;

        if (nodeFactorySettings && nodeFactorySettings.isCommentsButtonEnabled) {
             this.commentsButton.setClickAction(() => {
                 this.processDiagramManager.action(ProcessEvents.OpenUtilityPanel);
             });
        }

        this.commentsButton.setTooltip(this.rootScope.config.labels["ST_Comments_Label"]);
        this.commentsButton.setActiveImage(this.getImageSource("/comments-active.svg"));
        this.commentsButton.setHoverImage(this.getImageSource("/comments-active.svg"));

        if (this.commentsButton.isEnabled) {
            if (this.model && this.model.flags && this.model.flags.hasComments) {
                this.commentsButton.activate();
            }
        }

        //Included Artifacts Button
        this.linkButton = new Button(`LB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("include-neutral.svg"));

        if (nodeFactorySettings && nodeFactorySettings.isLinkButtonEnabled) {
            this.linkButton.setClickAction(() => this.navigateToProcess());
        }

        this.linkButton.setTooltip(this.rootScope.config.labels["ST_Userstory_Label"]);
        this.linkButton.setActiveImage(this.getImageSource("include-active.svg"));
        this.linkButton.setDisabledImage(this.getImageSource("include-inactive.svg"));

        if (this.model.associatedArtifact) {
            this.linkButton.activate();
        } else {
            this.linkButton.disable();
        }

        //User Story Preview Button
        this.previewButton = new Button(`PB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("userstories-neutral.svg"));

        if (nodeFactorySettings && nodeFactorySettings.isPreviewButtonEnabled) {
            this.previewButton.setClickAction(() => this.openDialog(ModalDialogType.PreviewDialogType));
        }

        this.previewButton.setTooltip(this.rootScope.config.labels["ST_Userstory_Label"]);
        this.previewButton.setDisabledImage(this.getImageSource("userstories-inactive.svg"));
        this.previewButton.setActiveImage(this.getImageSource("userstories-active.svg"));

        if (!this.userStoryId) {
            this.previewButton.disable();
        } else {
            this.previewButton.activate();
        }

        //Modal Dialog
        this.detailsButton = new Button(`DB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("adddetails-neutral.svg"));

        if (nodeFactorySettings && nodeFactorySettings.isDetailsButtonEnabled) {
            this.detailsButton.setClickAction(() => this.openDialog(ModalDialogType.UserTaskDetailsDialogType));
        }

        this.detailsButton.setTooltip(this.rootScope.config.labels["ST_Settings_Label"]);
        this.detailsButton.setHoverImage(this.getImageSource("adddetails-hover.svg"));
        this.detailsButton.setDisabledImage(this.getImageSource("adddetails-mute.svg"));
        this.detailsButton.isEnabled = true;
    }

    private initChildElements() {
        //initialize header
        const headerGeometry = new mxGeometry(0.5, 1, this.USER_TASK_WIDTH - 1, 38);
        headerGeometry.relative = false;
        this.header = new DiagramNodeElement("H" + this.model.id.toString(), ElementType.UserTaskHeader, "", headerGeometry,
            "shape=label;strokeColor=none;fillColor=#b1b1b1;fontColor=#FFFFFF;fontFamily=Open Sans, sans-serif;fontSize=11;selectable=0;editable=0");
        this.header.setVertex(true);
    }

    public get description(): string {
        return this.getPropertyValue("description");
    }

    public set description(value: string) {
        this.setPropertyValue("description", value);
    }

    public get objective(): string {
        return this.getPropertyValue("itemLabel");
    }

    public set objective(value: string) {
        this.setPropertyValue("itemLabel", value);
    }

    public get associatedArtifact(): IArtifactReference {
        return this.model.associatedArtifact;
    }

    public set associatedArtifact(value: IArtifactReference) {
        if (this.model != null && this.model.associatedArtifact !== value) {
            this.model.associatedArtifact = value;
            if (!value || value === null) {
                this.linkButton.disable();
                this.updateStatefulPropertyValue(PropertyTypePredefined.AssociatedArtifact, null);
            } else {
                this.linkButton.activate();
                this.updateStatefulPropertyValue(PropertyTypePredefined.AssociatedArtifact, value.id);
            }
        }
    }

    public get personaReference(): IArtifactReference {
        return this.model.personaReference;
    }

    public set personaReference(reference: IArtifactReference) {
        if (this.model != null && this.model.personaReference !== reference) {
            this.model.personaReference = reference;

            this.updateStatefulPropertyValue(PropertyTypePredefined.PersonaReference, reference.id);

            if (this.personaLabel) {
                this.personaLabel.text = reference.name;
            }

            this.shapesFactoryService.setUserTaskPersona(reference);

            this.processDiagramManager.action(ProcessEvents.PersonaReferenceUpdated, {personaReference: reference, isUserTask: true, isSystemTask: false});
        }
    }

    public getHeight(): number {
        return this.USER_TASK_HEIGHT;
    }

    public getWidth(): number {
        return this.USER_TASK_WIDTH;
    }

    public getPreviousSystemTasks(graph: IProcessGraph): ISystemTask[] {
        const result: ISystemTask[] = [];
        this.getSourceSystemTasks(graph, this, result);
        return result;
    }

    private getSourceSystemTasks(graph: IProcessGraph, node: IDiagramNode, resultSystemTasks: ISystemTask[]) {
        const sources = node.getSources(graph.getMxGraphModel());
        if (sources) {
            for (let i = 0; i < sources.length; i++) {
                const source = sources[i];
                if (source.getNodeType() === NodeType.SystemTask) {
                    resultSystemTasks.push(<ISystemTask>source);
                } else {
                    this.getSourceSystemTasks(graph, source, resultSystemTasks);
                }
            }
        }
    }

    public getNextSystemTasks(graph: IProcessGraph): ISystemTask[] {
        let result: ISystemTask[] = [];
        this.getTargetSystemTasks(graph, this, result);
        return result;
    }

    private getTargetSystemTasks(graph: IProcessGraph, node: IDiagramNode, resultSystemTasks: ISystemTask[]) {
        let targets = this.getTargets(graph.getMxGraphModel());
        if (targets) {
            let firstTarget = targets[0];
            // if next node is a system task, then push it in and return
            if (firstTarget != null && firstTarget.getNodeType() === NodeType.SystemTask) {
                resultSystemTasks.push(<ISystemTask>firstTarget);
                // if next node is system decision, traverse through all the immediate next node after the system decision, and try to push them all into result
            } else if (firstTarget != null && firstTarget.getNodeType() === NodeType.SystemDecision) {
                this.getSystemDecisionFirstTasks(graph, firstTarget, resultSystemTasks);
            }
        }
    }

    private getSystemDecisionFirstTasks(graph: IProcessGraph, node: IDiagramNode, resultSystemTasks: ISystemTask[]) {
        //#TODO fix reference to SystemDecision
        let decisionTargets = (<SystemDecision>node).getTargets(graph.getMxGraphModel());
        if (decisionTargets) {
            for (let i = 0; i < decisionTargets.length; i++) {
                let decisionTarget = decisionTargets[i];
                if (decisionTarget.getNodeType() === NodeType.SystemTask) {
                    resultSystemTasks.push(<ISystemTask>decisionTarget);
                } else {
                    this.getSystemDecisionFirstTasks(graph, decisionTarget, resultSystemTasks);
                }
            }
        }
    }

    public renderLabels() {
        this.textLabel.render();
        this.personaLabel.render();
    }

    public render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode {

        this.dialogManager = graph.viewModel.communicationManager.modalDialogManager;
        this.processDiagramManager = graph.viewModel.communicationManager.processDiagramCommunication;

        const mxGraph = graph.getMxGraph();
        let fillColor = "#FFFFFF";
        if (this.model.id < 0) {
            fillColor = justCreated ? this.newShapeColor : "#FBF8E7";
        }

        this.insertVertex(mxGraph, this.model.id.toString(), null, x, y, this.USER_TASK_WIDTH, this.USER_TASK_HEIGHT,
            " editable=0;shape=label;strokeColor=#D4D5DA;fillColor=" + fillColor + ";foldable=0;fontColor=#4C4C4C;" +
            "fontFamily=Open Sans, sans-serif;fontStyle=1;fontSize=12;dashed=0");

        const textLabelStyle: LabelStyle = new LabelStyle(
            "Open Sans",
            12,
            "trasparent",
            "#4C4C4C",
            "bold",
            y - 30,
            x - this.USER_TASK_WIDTH / 2 + 4,
            66,
            this.USER_TASK_WIDTH - 8,
            "#4C4C4C",
            "white"
        );
        this.textLabel = new Label(
            LabelType.Text,
            graph.getHtmlElement(),
            this.model.id.toString(),
            "Label-B" + this.model.id.toString(),
            this.label,
            textLabelStyle,
            this.LABEL_EDIT_MAXLENGTH,
            this.LABEL_VIEW_MAXLENGTH,
            graph.viewModel.isReadonly
        );

        // handle label change event
        this.textLabel.onTextChange = (value: string) => {
            this.label = value;
        };

        //header
        mxGraph.addCell(this.header, this);
        const personaLabelStyle: LabelStyle = new LabelStyle(
            "Open Sans",
            11,
            "#b1b1b1",
            "#FFFFFF",
            "normal",
            y - this.USER_TASK_HEIGHT / 2 + 12,
            x + 30 - this.USER_TASK_WIDTH / 2,
            22,
            this.USER_TASK_WIDTH - 40,
            "#4C4C4C"
        );
        // Note: the persona label is readonly
        this.personaLabel = new Label(
            LabelType.Persona,
            graph.getHtmlElement(),
            this.model.id.toString(),
            "Label-H" + this.model.id.toString(),
            this.personaReference.name,
            personaLabelStyle,
            this.PERSONA_EDIT_MAXLENGTH,
            this.PERSONA_VIEW_MAXLENGTH,
            true // readonly
        );

        // handle persona label double click event
        // open modal dialog so user can change the persona

        this.personaLabel.onDblClick = () => {
            this.openDialog(ModalDialogType.UserTaskDetailsDialogType);
        };

        mxGraph.insertVertex(this, "HB" + this.model.id.toString(), null, 0.5, 0.5, this.USER_TASK_WIDTH - 1, 3,
            "shape=rectangle;strokeColor=none;fillColor=#009CDE;editable=0;selectable=0");

        mxGraph.insertVertex(this, "FB" + this.model.id.toString(), null, 0.5, this.USER_TASK_HEIGHT - 35, this.USER_TASK_WIDTH - 1, 1,
            "shape=rectangle;strokeColor=none;fillColor=#D4D5DA;editable=0;selectable=0");

        //footer
        this.footerCell = mxGraph.insertVertex(this, "F" + this.model.id.toString(), null, 1, this.USER_TASK_HEIGHT - 34, this.USER_TASK_WIDTH - 2, 33,
            "shape=rectangle;strokeColor=none;fillColor=#FFFFFF;gradientColor=#DDDDDD;foldable=0;editable=0;selectable=0");

        this.addOverlays(mxGraph);

        this.deleteShapeButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 118, 10,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
        this.commentsButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 94, 10,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
        this.linkButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 70, 10,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
        // #TODO: get license type information from Nova shell
        //if (graph.viewModel.isReadonly && graph.viewModel.licenseType === LicenseTypeEnum.Viewer) {
        //    this.linkButton.disable();
        //}
        this.previewButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 46, 10,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
        this.detailsButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 22, 10,
            "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
        return this;
    }

    private addOverlays(mxGraph: MxGraph) {

        let overlays = mxGraph.getCellOverlays(this);

        if (overlays != null) {
            mxGraph.removeCellOverlays(this);
        }

        overlays = mxGraph.getCellOverlays(this.footerCell);

        if (overlays != null) {
            mxGraph.removeCellOverlays(this.footerCell);
        }

        // header overlays

        const personaIcon = "/novaweb/static/bp-process/images/defaultuser.svg";

        this.addOverlay(mxGraph, this, personaIcon, 24, 24, this.rootScope.config.labels["ST_Persona_Label"],
            mxConstants.ALIGN_LEFT, mxConstants.ALIGN_TOP, 18, 22);

        this.addAlert = _.bind(this.addAlertIcon, this, mxGraph);
        this.removeAlert = _.bind(this.removeAlertIcon, this, mxGraph);

        // DO NOT DELETE!!! this is needed for the labels functionality
        this.addOverlay(mxGraph, this, null, this.USER_TASK_WIDTH, this.USER_TASK_HEIGHT, null, mxConstants.ALIGN_LEFT,
            mxConstants.ALIGN_TOP, this.USER_TASK_WIDTH / 2, this.USER_TASK_HEIGHT / 2);

        // TODO: re-add for later sprints, when there's functionality attached to it (color coding nodes)
        //var colorsIcon = "/novaweb/static/bp-process/images/colors-on.png";
        //var overlayColors = this.addOverlay(graph, this, colorsIcon, 20, 20, this.rootScope.config.labels["ST_Colors_Label"],
        // mxConstants.ALIGN_RIGHT, mxConstants.ALIGN_TOP, -12, 14);
    }

    public highlight(mxGraph: MxGraph, color?: string): void {
        if (!color) {
            color = this.HIGHLIGHT_BORDER_COLOR;
        }

        mxGraph.setCellStyles(mxConstants.STYLE_STROKECOLOR, color, [this]);
        mxGraph.setCellStyles(mxConstants.STYLE_STROKEWIDTH, "1.5", [this]);
        mxGraph.setCellStyles(mxConstants.STYLE_DASHED, "1", [this]);
    }

    public clearHighlight(mxGraph: MxGraph): void {
        mxGraph.setCellStyles(mxConstants.STYLE_STROKECOLOR, this.DEFAULT_BORDER_COLOR, [this]);
        mxGraph.setCellStyles(mxConstants.STYLE_STROKEWIDTH, "1", [this]);
        mxGraph.setCellStyles(mxConstants.STYLE_DASHED, "0", [this]);
    }

    private navigateToProcess() {
        if (this.associatedArtifact == null) {
            return;
        }

        this.processDiagramManager.action(ProcessEvents.NavigateToAssociatedArtifact, {
            id: this.associatedArtifact.id,
            version: this.associatedArtifact.version,
            enableTracking: true,
            isAccessible: this.associatedArtifact.typePrefix !== this.rootScope.config.labels["ST_Breadcrumb_InaccessibleArtifact"]
        });
    }

    private openDialog(dialogType: ModalDialogType) {
        this.dialogManager.openDialog(this.model.id, dialogType);

        // #TODO use new dialog communication mechanism to open modal dialog
        //this.rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName,
        //    this.model.id,
        //    dialogType);
    }

    public get userStoryId(): number {
        const storyLinksValue = this.getPropertyValue("storyLinks");
        if (storyLinksValue != null) {
            return storyLinksValue["associatedReferenceArtifactId"] || null;
        }
        return null;
    }

    public set userStoryId(value: number) {
        if (this.userStoryId !== value) {
            this.setPropertyValue("storyLinks", {associatedReferenceArtifactId: value});
            if (this.previewButton && value > 0) {
                this.previewButton.activate();
            }
        }
    }

    public getDeleteDialogParameters(): IDialogParams {
        let dialogParams: IDialogParams = {};
        let nextNodes = this.getNextNodes();
        if (nextNodes && nextNodes.length > 0) {
            let firstNextNode = nextNodes[0];
            if (firstNextNode.getNodeType() === NodeType.SystemTask) {
                dialogParams.message = this.rootScope.config.labels["ST_Confirm_Delete_User_Task"];
            } else if (firstNextNode.getNodeType() === NodeType.SystemDecision) {
                dialogParams.message = this.rootScope.config.labels["ST_Confirm_Delete_User_Task_System_Decision"];
            }
        }
        return dialogParams;
    }

    public get canCopy(): boolean {
        return true;
    }

    public canDelete(): boolean {
        return true;
    }

    public canGenerateUserStory(): boolean {
        return true;
    }

    public getNodeType() {
        return NodeType.UserTask;
    }

}