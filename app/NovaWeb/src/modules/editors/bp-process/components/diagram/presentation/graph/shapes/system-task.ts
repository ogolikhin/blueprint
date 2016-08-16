﻿import {ISystemTaskShape} from "../../../../../models/processModels";
import {ItemIndicatorFlags, ProcessShapeType} from "../../../../../models/enums";
import {ModalDialogType} from "../../../../dialogs/modal-dialog-constants";
import {IProcessGraph, IDiagramNode, IUserTaskChildElement} from "../process-graph-interfaces";
import {IDiagramNodeElement, ISystemTask} from "../process-graph-interfaces";
import {ILabel} from "../process-graph-interfaces";
import {NodeType, NodeChange, ElementType} from "../process-graph-constants";
import {UserTaskChildElement} from "./user-task-child-element";
import {ShapesFactory} from "./shapes-factory";
import {DiagramNodeElement} from "./diagram-element";
import {NodeFactorySettings} from "./node-factory";
import {Button} from "../buttons/button";
import {Label, LabelStyle} from "../labels/label";

export class SystemTask extends UserTaskChildElement<ISystemTaskShape> implements ISystemTask, IUserTaskChildElement {

    private LABEL_EDIT_MAXLENGTH = 35;
    private LABEL_VIEW_MAXLENGTH = 35;
    private PERSONA_EDIT_MAXLENGTH = 40;
    private PERSONA_VIEW_MAXLENGTH = 12;
    private SYSTEM_TASK_WIDTH = 120;
    private SYSTEM_TASK_HEIGHT = 100;
    private SYSTEM_TASK_SHIFT = 20;
    private PRECONDITION_TASK_SHIFT = 20;
    private BUTTON_SIZE = 16;
    private ORIGIN_DIAMETER = 8;

    private origin: DiagramNodeElement;
    private header: DiagramNodeElement;
    private personaLabel: ILabel;
    private bodyCell: DiagramNodeElement;
    private footerCell: MxCell;
    private commentsButton: Button;
    private relationshipButton: Button;
    private detailsButton: Button;
    private linkButton: Button;
    private mockupButton: Button;
    private rootScope: ng.IRootScopeService;

    public callout: DiagramNodeElement;

    constructor(
        model: ISystemTaskShape,
        rootScope: any,
        private defaultPersonaValue: string,
        nodeFactorySettings: NodeFactorySettings = null,
        private shapesFactory: ShapesFactory
    ) {
        super(model, NodeType.SystemTask);

        this.rootScope = rootScope;

        this.initButtons(model.id.toString(), nodeFactorySettings);
    }

    private initChildElements(justCreated: boolean) {
        var modelId = this.model.id.toString();

        // initialize origin
        var originGeometry = new mxGeometry(this.SYSTEM_TASK_WIDTH / 2 - this.ORIGIN_DIAMETER / 2, 
                                            this.SYSTEM_TASK_HEIGHT, this.ORIGIN_DIAMETER, this.ORIGIN_DIAMETER);
        originGeometry.relative = false;
        this.origin = new DiagramNodeElement("O" + modelId, ElementType.SystemTaskOrigin, null, originGeometry, 
                                             "shape=ellipse;strokeColor=#d4d5da;fillColor=#d4d5da;selectable=0;editable=0");
        this.origin.setVertex(true);

        //initialize call-out
        var calloutGeometry = new mxGeometry(0, 0, this.SYSTEM_TASK_WIDTH, this.SYSTEM_TASK_HEIGHT);
        this.callout = new DiagramNodeElement("C" + modelId, ElementType.Shape, null, calloutGeometry, 
            "shape=systemTask;strokeColor=#53BBED;fillColor=#FFFFFF;fontColor=#4C4C4C;fontFamily=Open Sans," +
            " sans-serif;fontStyle=1;fontSize=11;foldable=0;shadow=0;editable=0;selectable=0");
        this.callout.setVertex(true);

        //initialize header
        var persona = this.persona;
        if (persona == null) {
            persona = this.defaultPersonaValue;
        }

        var headerGeometry = new mxGeometry(0.5, 0.5, this.SYSTEM_TASK_WIDTH - 1, 20);
        this.header = new DiagramNodeElement("H" + modelId, ElementType.SystemTaskHeader, null, headerGeometry, 
        "shape=label;strokeColor=none;fillColor=#E2F3FF;fontColor=#009cde;fontFamily=Open Sans, sans-serif;fontSize=11;editable=0;selectable=0");
        this.header.setVertex(true);

        //initialize body
        var fillColor = "#FFFFFF";
        if (this.model.id < 0) {
            fillColor = justCreated ? this.newShapeColor : "#FBF8E7";
        }

        var bodyGeometry = new mxGeometry(0.5, 20.5, this.SYSTEM_TASK_WIDTH - 1.5, 47);
        this.bodyCell = new DiagramNodeElement("B" + modelId, ElementType.Shape, null, bodyGeometry, 
            "shape=label;strokeColor=none;fillColor=" + fillColor + ";fontColor=#4C4C4C;fontFamily=Open Sans, sans-serif;fontStyle=1;fontSize=11;" +
            "foldable=0;shadow=0;editable=0;selectable=0");
        this.bodyCell.setVertex(true);
    }

    private initButtons(nodeId: string, nodeFactorySettings: NodeFactorySettings = null) {
        //Shape Comments
        this.commentsButton = new Button(`CB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("comments-neutral.svg"));
        this.commentsButton.isEnabled = !this.isNew;

        if (nodeFactorySettings && nodeFactorySettings.isCommentsButtonEnabled) {
            // #TODO interaction with utility panel is different in Nova
            //this.commentsButton.setClickAction(() => this.openPropertiesDialog(this.rootScope, Shell.UtilityTab.discussions));
        } else {
            this.commentsButton.setClickAction(() => { });
        }

        this.commentsButton.setTooltip(this.getLocalizedLabel("ST_Comments_Label"));

        if (this.commentsButton.isEnabled) {
            this.commentsButton.setActiveImage(this.getImageSource("/comments-active.svg"));
            this.commentsButton.setHoverImage(this.getImageSource("/comments-active.svg"));

            if (this.model.flags && this.model.flags.hasComments) {
                this.commentsButton.activate();
            }
        }

        //Shape Traces
        this.relationshipButton = new Button(`RB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("relationship-neutral.svg"));
        this.relationshipButton.isEnabled = !this.isNew;

        if (nodeFactorySettings && nodeFactorySettings.isRelationshipButtonEnabled) {
            // TODO interaction with utility panel is different in Nova
            // this.relationshipButton.setClickAction(() => this.openPropertiesDialog(this.rootScope, Shell.UtilityTab.relationships));
        } else {
            this.relationshipButton.setClickAction(() => { });
        }

        this.relationshipButton.setTooltip(this.getLocalizedLabel("ST_Relationships_Label"));

        if (this.relationshipButton.isEnabled) {
            this.relationshipButton.setActiveImage(this.getImageSource("relationship-active.svg"));
            this.relationshipButton.setHoverImage(this.getImageSource("relationship-active.svg"));

            if (this.model.flags && this.model.flags.hasTraces) {
                this.relationshipButton.activate();
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

        this.linkButton.setTooltip(this.getLocalizedLabel("ST_Userstory_Label"));
        this.linkButton.setDisabledImage(this.getImageSource("include-inactive.svg"));
        this.linkButton.setActiveImage(this.getImageSource("include-active.svg"));

        if (this.linkButton.isEnabled) {
            if (this.model.associatedArtifact) {
                this.linkButton.activate();
            } else {
                this.linkButton.disable();
            }
        }

        //Mockup Preview Button
        this.mockupButton = new Button(`MB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("mockup-neutral.svg"));

        if (nodeFactorySettings && nodeFactorySettings.isMockupButtonEnabled) {
            this.mockupButton.setClickAction(() => this.openDialog(ModalDialogType.UserSystemTaskDetailsDialogType));
        } else {
            this.mockupButton.setClickAction(() => { });
        }

        this.mockupButton.setTooltip(this.getLocalizedLabel("ST_Mockup_Label"));
        this.mockupButton.setActiveImage(this.getImageSource("mockup-active.svg"));
        this.mockupButton.setHoverImage(this.getImageSource("mockup-active.svg"));
        this.mockupButton.setDisabledImage(this.getImageSource("mockup-inactive.svg"));

        if (this.imageId) {
            this.mockupButton.activate();
        }

        //Modal Dialog
        this.detailsButton = new Button(`DB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("adddetails-neutral.svg"));

        if (nodeFactorySettings && nodeFactorySettings.isDetailsButtonEnabled) {
            this.detailsButton.setClickAction(() => this.openDialog(ModalDialogType.UserSystemTaskDetailsDialogType));
        } else {
            this.detailsButton.setClickAction(() => { });
        }

        this.detailsButton.setTooltip(this.getLocalizedLabel("ST_Settings_Label"));
        this.detailsButton.setHoverImage(this.getImageSource("adddetails-hover.svg"));
        this.detailsButton.setDisabledImage(this.getImageSource("adddetails-mute.svg"));
        this.detailsButton.isEnabled = true;
    }

    private getLocalizedLabel(key: string) {
        return this.rootScope["config"].labels[key];
    }

    public getConnectableElement(): IDiagramNodeElement {
        return this.origin;
    }

    public get persona(): string {
        return this.getPropertyValue("persona");
    }

    public set persona(value: string) {
        var valueChanged = this.setPropertyValue("persona", value);

        if (valueChanged) {
            if (this.personaLabel) {
                this.personaLabel.text = value;
                this.shapesFactory.setSystemTaskPersona(value);
            }
            this.notify(NodeChange.Update);
        }
    }

    public get description(): string {
        return this.getPropertyValue("description");
    }

    public set description(value: string) {
        var valueChanged = this.setPropertyValue("description", value);
        if (valueChanged) {
            this.notify(NodeChange.Update);
        }
    }

    public get associatedImageUrl(): string {
        return this.getPropertyValue("associatedImageUrl");
    }

    public set associatedImageUrl(value: string) {
        var valueChanged = this.setPropertyValue("associatedImageUrl", value);
        if (valueChanged) {
            this.notify(NodeChange.Update);
        }
    }

    public get imageId(): string {
        return this.getPropertyValue("imageId");
    }

    public set imageId(value: string) {
        var valueChanged = this.setPropertyValue("imageId", value);
        if (valueChanged) {
            this.notify(NodeChange.Update);
            if (!Boolean(value)) {
                this.mockupButton.deactivate();
            } else {
                this.mockupButton.activate();
            }
        }
    }

    public get associatedArtifact(): any {
        return this.model.associatedArtifact;
    }

    public set associatedArtifact(value: any) {
        if (this.model != null && this.model.associatedArtifact !== value) {
            this.model.associatedArtifact = value;
            this.notify(NodeChange.Update);
            if (!Boolean(value)) {
                this.linkButton.disable();
            } else {
                this.linkButton.activate();
            }
        }
    }

    public getX(): number {
        var shift = this.SYSTEM_TASK_SHIFT;
        if (this.model.propertyValues["clientType"].value === ProcessShapeType.PreconditionSystemTask) {
            shift = this.PRECONDITION_TASK_SHIFT;
        }

        return this.getCenter().x + shift;
    }

    public getY(): number {
        return this.origin.getCenter().y;
    }

    public getHeight(): number {
        return this.SYSTEM_TASK_HEIGHT + this.ORIGIN_DIAMETER;
    }

    public getWidth(): number {
        return this.SYSTEM_TASK_WIDTH;
    }

    public isPrecondition(): boolean {
        return this.model.propertyValues["clientType"].value === ProcessShapeType.PreconditionSystemTask;
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

        var mxGraph = graph.getMxGraph();

        var shift = this.SYSTEM_TASK_SHIFT;

        this.initChildElements(justCreated);

        if (this.model.propertyValues["clientType"].value === ProcessShapeType.PreconditionSystemTask) {
            shift = this.PRECONDITION_TASK_SHIFT;
        }

        var modelId = this.model.id.toString();

        // shape
        var shapeWidth = this.SYSTEM_TASK_WIDTH;
        var shapeHeight = this.SYSTEM_TASK_HEIGHT + this.ORIGIN_DIAMETER;
        this.insertVertex(mxGraph, modelId, null, x - shift, y - (shapeHeight / 2) + this.ORIGIN_DIAMETER / 2, shapeWidth, shapeHeight, 
                          "shape=systemTask;strokeColor=none;fillColor=none;foldable=0;editable=0");

        // origin
        mxGraph.addCell(this.origin, this);

        // call-out
        mxGraph.addCell(this.callout, this);

        //header
        var personaLabelStyle: LabelStyle = new LabelStyle(
            "Open Sans",
            11,
            "#E2F3FF",
            "#009cde",
            "normal",
            y - this.SYSTEM_TASK_HEIGHT - 2,
            x - this.SYSTEM_TASK_WIDTH / 2 - this.SYSTEM_TASK_SHIFT + 3,
            this.header.getHeight() - 4,
            this.header.getWidth() - 4,
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

        var cell = mxGraph.addCell(this.header, this.callout);

        // body
        var textLabelStyle: LabelStyle = new LabelStyle(
            "Open Sans",
            12,
            "transparent",
            "#4C4C4C",
            "bold",
            y - this.SYSTEM_TASK_HEIGHT + this.header.getHeight(),
            x - this.SYSTEM_TASK_WIDTH / 2 - this.SYSTEM_TASK_SHIFT + 3,
            this.bodyCell.getHeight() - 4,
            this.bodyCell.getWidth() - 4,
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

        cell = mxGraph.addCell(this.bodyCell, this.callout);

        // footer
        this.footerCell = mxGraph.insertVertex(this.callout, "F" + modelId, null, 0.5, this.SYSTEM_TASK_HEIGHT - 34.5, this.SYSTEM_TASK_WIDTH - 1, 24,
            "shape=rectangle;foldable=0;strokeColor=none;fillColor=#FFFFFF;gradientColor=#DDDDDD;selectable=0");

        this.addOverlays(mxGraph);

        this.commentsButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 116, 4, 
                                   "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
        this.relationshipButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 92, 4, 
                                       "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
        this.linkButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 68, 4, "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
        // #TODO get license info fron Nova shell
        //if (graph.viewModel.isReadonly && graph.viewModel.licenseType === Shell.LicenseTypeEnum.Viewer) {
        //    this.linkButton.disable();
        //}
        this.mockupButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 44, 4, 
                                 "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
        this.detailsButton.render(mxGraph, this.footerCell, this.footerCell.geometry.width - 20, 4, 
                                  "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");

        return this;
    }

    private addOverlays(graph: MxGraph) {
        var overlays = graph.getCellOverlays(this);

        if (overlays != null) {
            graph.removeCellOverlays(this);
        }

        overlays = graph.getCellOverlays(this.bodyCell);

        if (overlays != null) {
            graph.removeCellOverlays(this.bodyCell);
        }

        overlays = graph.getCellOverlays(this.footerCell);

        if (overlays != null) {
            graph.removeCellOverlays(this.footerCell);
        }

        // DO NOT DELETE!!! this is needed for the labels functionality
        this.addOverlay(graph, this, null, this.SYSTEM_TASK_WIDTH, this.SYSTEM_TASK_HEIGHT, null, 
                        mxConstants.ALIGN_LEFT, mxConstants.ALIGN_TOP, this.SYSTEM_TASK_WIDTH / 2, this.SYSTEM_TASK_HEIGHT / 2);
    }

    public setCellVisible(graph: MxGraph, value: boolean) {
        graph.getModel().setVisible(this.callout, value);
        this.textLabel.setVisible(value);
        this.personaLabel.setVisible(value);
    }

    private navigateToProcess() {
        if (this.associatedArtifact == null) {
            return;
        }
        // #TODO fix up reference to ProcessCommands 
        // var data: ICommandData = { processId: this.associatedArtifact.id, model: this.model };
        // ProcessCommands.getProcessCommands().getNavigateToProcessCommand().execute(data);
    }

    private openDialog(dialogType: ModalDialogType) {
        // #TODO implement new mechanism for opening modal dialogs
        //this.rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName,
        //    this.model.id,
        //    dialogType);
    }

    public getElementTextLength(cell: MxCell): number {
         
        // get the maximum length of text that can be entered
        
        var maxLen: number = this.LABEL_EDIT_MAXLENGTH;

        var element = <IDiagramNodeElement>cell;
        if (element.getElementType() === ElementType.SystemTaskHeader) {
            maxLen = this.PERSONA_EDIT_MAXLENGTH;
        } else {
            maxLen = this.LABEL_EDIT_MAXLENGTH;
        }
        return maxLen;
    }

    public formatElementText(cell: MxCell, text: string): string {

         
         // This function returns formatted text to the getLabel()
         // function to display the node's label and persona  
         

        if (cell && text) {
            var maxLen: number = this.LABEL_VIEW_MAXLENGTH;

            var element = <IDiagramNodeElement>cell;
            if (element.getElementType() === ElementType.SystemTaskHeader) {
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

        if (element.getElementType() === ElementType.SystemTaskHeader) {
            this.persona = text;
        } else {
            this.label = text;
        }
    }
    public getLabelCell(): MxCell {
        return this.bodyCell;
    }

    public activateButton(flag: ItemIndicatorFlags) {
        if (flag === ItemIndicatorFlags.HasComments) {
            this.model.flags.hasComments = true;
            this.commentsButton.activate();
        }
    }
}
