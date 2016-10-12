import {IArtifactProperty, IUserTaskShape} from "../../../../../models/process-models";
import {ItemIndicatorFlags} from "../../../../../models/enums";
import {ModalDialogType} from "../../../../modal-dialogs/modal-dialog-constants";
import {IProcessGraph, IDiagramNode} from "../models/";
import {IDiagramNodeElement, ISystemTask} from "../models/";
import {IUserTask, IUserStoryProperties, ILabel} from "../models/";
import {NodeType, NodeChange, ElementType} from "../models/";
import {IDialogParams} from "../../../../messages/message-dialog";
import {ShapesFactory} from "./shapes-factory";
import {DiagramNodeElement} from "./diagram-element";
import {DiagramNode} from "./diagram-node";
import {NodeFactorySettings} from "./node-factory-settings";
import {Button} from "../buttons/button";
import {Label, LabelStyle} from "../labels/label";
import {SystemDecision} from "./";
import {IModalDialogCommunication} from "../../../../modal-dialogs/modal-dialog-communication";
import {IProcessDiagramCommunication} from "../../../process-diagram-communication";
import {ProcessEvents} from "../../../process-diagram-communication";

export class UserStoryProperties implements IUserStoryProperties {
    public nfr: IArtifactProperty;
    public businessRules: IArtifactProperty;
}

export class UserTask extends DiagramNode<IUserTaskShape> implements IUserTask {
    private USER_TASK_WIDTH = 126;
    private USER_TASK_HEIGHT = 150;
    private LABEL_VIEW_MAXLENGTH = 40;
    private LABEL_EDIT_MAXLENGTH = 40;
    private PERSONA_VIEW_MAXLENGTH = 16;
    private PERSONA_EDIT_MAXLENGTH = 40;
    private BUTTON_SIZE = 16;

    private header: mxCell;
    private personaLabel: ILabel;
    private footerCell: MxCell;
    private commentsButton: Button;
    private deleteShapeButton: Button;
    private detailsButton: Button;
    private previewButton: Button;
    private linkButton: Button;
    private rootScope: any;
    private dialogManager: IModalDialogCommunication;
    private processDiagramManager: IProcessDiagramCommunication;
    
    // #UNUSED
    // private _userStoryId: number;

    public userStoryProperties: IUserStoryProperties;

    constructor(model: IUserTaskShape, rootScope: any, private nodeFactorySettings: NodeFactorySettings = null, private shapesFactoryService: ShapesFactory) {
        super(model, NodeType.UserTask);

        this.rootScope = rootScope;

        this.initButtons(model.id.toString(), nodeFactorySettings);

        this.userStoryProperties = new UserStoryProperties();

        this.initChildElements();
    }

    public cloneUserTask(): UserTask {
        let userTask = new UserTask(this.model, this.rootScope, this.nodeFactorySettings, this.shapesFactoryService);
        userTask.label = this.label;
        userTask.persona = this.persona;
        userTask.objective = this.objective;
        userTask.action = this.action;
        userTask.description = this.description;
        userTask.associatedArtifact = this.associatedArtifact;
        return userTask;
    }

    public initButtons(nodeId: string, nodeFactorySettings: NodeFactorySettings = null) {
        //Delete Shape
        this.deleteShapeButton = new Button(`DS${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("delete-neutral.svg"));
        this.deleteShapeButton.isEnabled = true;

        if (nodeFactorySettings && nodeFactorySettings.isRelationshipButtonEnabled) {            
            this.deleteShapeButton.setClickAction(() => 
            {
                this.processDiagramManager.action(ProcessEvents.DeleteShape);
            });
        } else {
            this.deleteShapeButton.setClickAction(() => { });
        }

        this.deleteShapeButton.setTooltip(this.rootScope.config.labels["ST_Relationships_Label"]);
        this.deleteShapeButton.setActiveImage(this.getImageSource("delete-active.svg"));
        this.deleteShapeButton.setHoverImage(this.getImageSource("delete-hover.svg"));
        
        //Shape Comments
        this.commentsButton = new Button(`CB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("comments-neutral.svg"));
        this.commentsButton.isEnabled = !this.isNew;

        if (nodeFactorySettings && nodeFactorySettings.isCommentsButtonEnabled) {
            // #TODO integrate with utility panel in Nova
            // this.commentsButton.setClickAction(() => this.openPropertiesDialog(this.rootScope, Shell.UtilityTab.discussions));
        } else {
            this.commentsButton.setClickAction(() => { });
        }

        this.commentsButton.setTooltip(this.rootScope.config.labels["ST_Comments_Label"]);
        this.commentsButton.setActiveImage(this.getImageSource("/comments-active.svg"));
        this.commentsButton.setHoverImage(this.getImageSource("/comments-active.svg"));

        if (this.commentsButton.isEnabled) {
            if (this.model.flags && this.model.flags.hasComments) {
                this.commentsButton.activate();
            }
        }

        

        //Included Artifacts Button
        this.linkButton = new Button(`LB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("include-neutral.svg"));
        this.linkButton.isEnabled = !this.isNew;

        if (nodeFactorySettings && nodeFactorySettings.isLinkButtonEnabled) {
            this.linkButton.setClickAction(() => this.navigateToProcess());
        } else {
            this.linkButton.setClickAction(() => { });
        }

        this.linkButton.setTooltip(this.rootScope.config.labels["ST_Userstory_Label"]);
        this.linkButton.setActiveImage(this.getImageSource("include-active.svg"));
        this.linkButton.setDisabledImage(this.getImageSource("include-inactive.svg"));

        if (this.linkButton.isEnabled) {
            if (this.model.associatedArtifact) {
                this.linkButton.activate();
            } else {
                this.linkButton.disable();
            }
        }

        //User Story Preview Button
        this.previewButton = new Button(`PB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("userstories-neutral.svg"));

        if (nodeFactorySettings && nodeFactorySettings.isPreviewButtonEnabled) {
            this.previewButton.setClickAction(() => this.openDialog(ModalDialogType.PreviewDialogType));
        } else {
            this.previewButton.setClickAction(() => { });
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
            this.detailsButton.setClickAction(() => this.openDialog(ModalDialogType.UserSystemTaskDetailsDialogType));
        } else {
            this.detailsButton.setClickAction(() => { });
        }

        this.detailsButton.setTooltip(this.rootScope.config.labels["ST_Settings_Label"]);
        this.detailsButton.setHoverImage(this.getImageSource("adddetails-hover.svg"));
        this.detailsButton.setDisabledImage(this.getImageSource("adddetails-mute.svg"));
        this.detailsButton.isEnabled = true;
    }

    private initChildElements() {
        //initialize header
        var headerGeometry = new mxGeometry(0.5, 1, this.USER_TASK_WIDTH - 1, 38);
        headerGeometry.relative = false;
        this.header = new DiagramNodeElement("H" + this.model.id.toString(), ElementType.UserTaskHeader, "", headerGeometry,
            "shape=label;strokeColor=none;fillColor=#b1b1b1;fontColor=#FFFFFF;fontFamily=Open Sans, sans-serif;fontSize=11;selectable=0;editable=0");
        this.header.setVertex(true);
    }

    public get persona(): string {
        return this.getPropertyValue("persona");
    }

    public set persona(value: string) {
        var valueChanged = this.setPropertyValue("persona", value);
        if (valueChanged) {
            if (this.personaLabel) {
                this.personaLabel.text = value;
                this.shapesFactoryService.setUserTaskPersona(value);
            }

            this.notify(NodeChange.Update, false);
        }
    }

    public get description(): string {
        return this.getPropertyValue("description");
    }

    public set description(value: string) {
        var valueChanged = this.setPropertyValue("description", value);
        if (valueChanged) {
            this.notify(NodeChange.Update, false);
        }
    }

    public get objective(): string {
        return this.getPropertyValue("itemLabel");
    }

    public set objective(value: string) {
        var valueChanged = this.setPropertyValue("itemLabel", value);
        if (valueChanged) {
            this.notify(NodeChange.Update);
        }
    }

    public get associatedArtifact(): any {
        return this.model.associatedArtifact;
    }

    public set associatedArtifact(value: any) {
        if (this.model != null && this.model.associatedArtifact !== value) {
            this.model.associatedArtifact = value;
            this.notify(NodeChange.Update);
            if (!value || value === null) {
                this.linkButton.disable();
            } else {
                this.linkButton.activate();
            }
        }
    }

    public getHeight(): number {
        return this.USER_TASK_HEIGHT;
    }

    public getWidth(): number {
        return this.USER_TASK_WIDTH;
    }

    public getPreviousSystemTasks(graph: IProcessGraph): ISystemTask[] {
        var result: ISystemTask[] = [];
        this.getSourceSystemTasks(graph, this, result);
        return result;
    }

    private getSourceSystemTasks(graph: IProcessGraph, node: IDiagramNode, resultSystemTasks: ISystemTask[]) {
        var sources = node.getSources(graph.getMxGraphModel());
        if (sources) {
            for (var i = 0; i < sources.length; i++) {
                var source = sources[i];
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
            for (var i = 0; i < decisionTargets.length; i++) {
                let decisionTarget = decisionTargets[i];
                if (decisionTarget.getNodeType() === NodeType.SystemTask) {
                    resultSystemTasks.push(<ISystemTask>decisionTarget);
                } else {
                    this.getSystemDecisionFirstTasks(graph, decisionTarget, resultSystemTasks);
                }
            }
        }
    }

    public addNode(graph: IProcessGraph): IDiagramNode {
        return this;
    }

    public deleteNode(graph: IProcessGraph) {
    }

    public renderLabels() {
        this.textLabel.render();
        this.personaLabel.render();
    }

    public render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode {
        this.dialogManager = graph.viewModel.communicationManager.modalDialogManager;
        this.processDiagramManager = graph.viewModel.communicationManager.processDiagramCommunication;

        var mxGraph = graph.getMxGraph();
        var fillColor = "#FFFFFF";
        if (this.model.id < 0) {
            fillColor = justCreated ? this.newShapeColor : "#FBF8E7";
        }

        this.insertVertex(mxGraph, this.model.id.toString(), null, x, y, this.USER_TASK_WIDTH, this.USER_TASK_HEIGHT,
            " editable=0;shape=label;strokeColor=#D4D5DA;fillColor=" + fillColor + ";foldable=0;fontColor=#4C4C4C;" +
            "fontFamily=Open Sans, sans-serif;fontStyle=1;fontSize=12");
        var textLabelStyle: LabelStyle = new LabelStyle(
            "Open Sans",
            12,
            "transparent",
            "#4C4C4C",
            "bold",
            y - 30,
            x - this.USER_TASK_WIDTH / 2 + 4,
            66,
            this.USER_TASK_WIDTH - 8,
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

        //header
        mxGraph.addCell(this.header, this);
        var personaLabelStyle: LabelStyle = new LabelStyle(
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
        this.personaLabel = new Label((value: string) => { this.persona = value; },
            graph.getHtmlElement(),
            this.model.id.toString(),
            "Label-H" + this.model.id.toString(),
            this.persona,
            personaLabelStyle,
            this.PERSONA_EDIT_MAXLENGTH,
            this.PERSONA_VIEW_MAXLENGTH,
            graph.viewModel.isReadonly);

        mxGraph.insertVertex(this, "HB" + this.model.id.toString(), null, 0.5, 0.5, this.USER_TASK_WIDTH - 1, 3,
            "shape=rectangle;strokeColor=none;fillColor=#009CDE;editable=0;selectable=0");

        //footer
        this.footerCell = mxGraph.insertVertex(this, "F" + this.model.id.toString(), null, 0, this.USER_TASK_HEIGHT - 33, this.USER_TASK_WIDTH, 33,
            "shape=rectangle;strokeColor=#D4D5DA;fillColor=#FFFFFF;gradientColor=#DDDDDD;foldable=0;editable=0;selectable=0");

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
        
        var overlays = mxGraph.getCellOverlays(this);

        if (overlays != null) {
            mxGraph.removeCellOverlays(this);
        }

        overlays = mxGraph.getCellOverlays(this.footerCell);

        if (overlays != null) {
            mxGraph.removeCellOverlays(this.footerCell);
        }

        // header overlays
       
        var personaIcon = "/novaweb/static/bp-process/images/defaultuser.svg";
        this.addOverlay(mxGraph, this, personaIcon, 24, 24, this.rootScope.config.labels["ST_Persona_Label"], 
        mxConstants.ALIGN_LEFT, mxConstants.ALIGN_TOP, 16, 18);

        // DO NOT DELETE!!! this is needed for the labels functionality
        this.addOverlay(mxGraph, this, null, this.USER_TASK_WIDTH, this.USER_TASK_HEIGHT, null, mxConstants.ALIGN_LEFT,
            mxConstants.ALIGN_TOP, this.USER_TASK_WIDTH / 2, this.USER_TASK_HEIGHT / 2);

        // TODO: re-add for later sprints, when there's functionality attached to it (color coding nodes)
        //var colorsIcon = "/novaweb/static/bp-process/images/colors-on.png";
        //var overlayColors = this.addOverlay(graph, this, colorsIcon, 20, 20, this.rootScope.config.labels["ST_Colors_Label"], 
        // mxConstants.ALIGN_RIGHT, mxConstants.ALIGN_TOP, -12, 14);
    }

    private navigateToProcess() {
        if (this.associatedArtifact == null) {
            return;
        }

        this.processDiagramManager.navigateToAssociatedArtifact(this.associatedArtifact.id, true);
    }

    private openDialog(dialogType: ModalDialogType) {
        window.console.log(`UserTask.openDialog, ${dialogType}`);
        
        this.dialogManager.openDialog(this.model.id, dialogType);
        
        // #TODO use new dialog communication mechanism to open modal dialog
        //this.rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName,
        //    this.model.id,
        //    dialogType);
    }

    public getElementTextLength(cell: MxCell): number {
        /*
        * get the maximum length of text that can be entered in 
        * the cell 
        */
        var maxLen: number = this.LABEL_EDIT_MAXLENGTH;

        var element = <IDiagramNodeElement>cell;
        if (element.getElementType() === ElementType.UserTaskHeader) {
            maxLen = this.PERSONA_EDIT_MAXLENGTH;
        } else {
            maxLen = this.LABEL_EDIT_MAXLENGTH;
        }
        return maxLen;
    }

    public formatElementText(cell: MxCell, text: string): string {
       
        // This function returns formatted text to the getLabel()
        // function to display the node's label and persona.  
        

        if (cell && text) {
            var maxLen: number = this.LABEL_VIEW_MAXLENGTH;

            var element = <IDiagramNodeElement>cell;
            if (element.getElementType() === ElementType.UserTaskHeader) {
                maxLen = this.PERSONA_VIEW_MAXLENGTH;
            } else {
                maxLen = this.LABEL_VIEW_MAXLENGTH;
            }

            if (text.length > maxLen) {
                text = text.substr(0, maxLen) + " ...";
            }
        }

        return text;
    }

    public setElementText(cell: MxCell, text: string) {
       
        // save text for the node or for an element within
        // the node
       
        var element = <IDiagramNodeElement>cell;

        if (element.getElementType() === ElementType.UserTaskHeader) {
            this.persona = text;
        } else {
            this.label = text;
        }
    }

    public get userStoryId(): number {
        var storyLinksValue = this.getPropertyValue("storyLinks");
        if (storyLinksValue != null) {
            return storyLinksValue["associatedReferenceArtifactId"] || null;
        }
        return null;
    }

    public set userStoryId(value: number) {
        if (this.userStoryId !== value) {
            this.setPropertyValue("storyLinks", { associatedReferenceArtifactId: value });
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

    public canDelete(): boolean {
        return true;
    }

    public canGenerateUserStory(): boolean {
        return true;
    }

    public activateButton(flag: ItemIndicatorFlags) {
        if (flag === ItemIndicatorFlags.HasComments) {
            this.model.flags.hasComments = true;
            this.commentsButton.activate();
        }
    }
}