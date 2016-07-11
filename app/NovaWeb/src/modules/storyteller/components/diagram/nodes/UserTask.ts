﻿/// <reference path="../DiagramNode.ts" />
/// <reference path="IUserTask.ts" />

module Storyteller {
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
        private relationshipButton: Button;
        private detailsButton: Button;
        private previewButton: Button;
        private linkButton: Button;
        private rootScope: any;
        private _userStoryId: number;

        public userStoryProperties: IUserStoryProperties;

        constructor(model: IUserTaskShape, rootScope: any, nodeFactorySettings: NodeFactorySettings = null, private shapesFactoryService: ShapesFactoryService) {
            super(model, NodeType.UserTask);

            this.rootScope = rootScope;

            this.initButtons(model.id.toString(), nodeFactorySettings);

            this.userStoryProperties = new UserStoryProperties();

            this.initChildElements();
        }

        public initButtons(nodeId: string, nodeFactorySettings: NodeFactorySettings = null) {

            //Shape Comments
            this.commentsButton = new Button(`CB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("comments-neutral.svg"));
            this.commentsButton.isEnabled = !this.isNew;

            if (nodeFactorySettings && nodeFactorySettings.isCommentsButtonEnabled) {
                this.commentsButton.setClickAction(() => this.openPropertiesDialog(this.rootScope, Shell.UtilityTab.discussions));
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

            //Shape Traces
            this.relationshipButton = new Button(`RB${nodeId}`, this.BUTTON_SIZE, this.BUTTON_SIZE, this.getImageSource("relationship-neutral.svg"));
            this.relationshipButton.isEnabled = !this.isNew;

            if (nodeFactorySettings && nodeFactorySettings.isRelationshipButtonEnabled) {
                this.relationshipButton.setClickAction(() => this.openPropertiesDialog(this.rootScope, Shell.UtilityTab.relationships));
            } else {
                this.relationshipButton.setClickAction(() => { });
            }

            this.relationshipButton.setTooltip(this.rootScope.config.labels["ST_Relationships_Label"]);
            this.relationshipButton.setActiveImage(this.getImageSource("relationship-active.svg"));
            this.relationshipButton.setHoverImage(this.getImageSource("relationship-active.svg"));

            if (this.relationshipButton.isEnabled) {
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
            }
            else {
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
            this.header = new DiagramNodeElement("H" + this.model.id.toString(), ElementType.UserTaskHeader, "", headerGeometry, "shape=label;strokeColor=none;fillColor=#b1b1b1;fontColor=#FFFFFF;fontFamily=Open Sans, sans-serif;fontSize=11;selectable=0;editable=0");
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

        public getPreviousSystemTasks(graph: ProcessGraph): SystemTask[] {
            var result: SystemTask[] = [];
            this.getSourceSystemTasks(graph, this, result);
            return result;
        }

        private getSourceSystemTasks(graph: ProcessGraph, node: IDiagramNode, resultSystemTasks: SystemTask[]) {
            var sources = node.getSources(graph);
            if (sources) {
                for (var i = 0; i < sources.length; i++) {
                    var source = sources[i];
                    if (source.getNodeType() === NodeType.SystemTask) {
                        resultSystemTasks.push(<SystemTask>source);
                    } else {
                        this.getSourceSystemTasks(graph, source, resultSystemTasks);
                    }
                }
            }
        }

        public getNextSystemTasks(graph: ProcessGraph): SystemTask[] {
            let result: SystemTask[] = [];
            this.getTargetSystemTasks(graph, this, result);
            return result;
        }

        private getTargetSystemTasks(graph: ProcessGraph, node: IDiagramNode, resultSystemTasks: SystemTask[]) {
            let targets = this.getTargets(graph);
            if (targets) {
                let firstTarget = targets[0];
                // if next node is a system task, then push it in and return
                if (firstTarget != null && firstTarget.getNodeType() === NodeType.SystemTask) {
                    resultSystemTasks.push(<SystemTask>firstTarget);
                }
                // if next node is system decision, traverse through all the immediate next node after the system decision, and try to push them all into result
                else if (firstTarget != null && firstTarget.getNodeType() === NodeType.SystemDecision) {
                    this.getSystemDecisionFirstTasks(graph, firstTarget, resultSystemTasks);
                }
            }
        }

        private getSystemDecisionFirstTasks(graph: ProcessGraph, node: IDiagramNode, resultSystemTasks: SystemTask[]) {
            let decisionTargets = (<SystemDecision>node).getTargets(graph);
            if (decisionTargets) {
                for (var i = 0; i < decisionTargets.length; i++) {
                    let decisionTarget = decisionTargets[i];
                    if (decisionTarget.getNodeType() === NodeType.SystemTask) {
                        resultSystemTasks.push(<SystemTask>decisionTarget);
                    } else {
                        this.getSystemDecisionFirstTasks(graph, decisionTarget, resultSystemTasks);
                    }
                }
            }
        }

        public addNode(graph: ProcessGraph): IDiagramNode {
            return this;
        }

        public deleteNode(graph: ProcessGraph) {
        }

        public renderLabels() {
            this.textLabel.render();
            this.personaLabel.render();
        }

        public render(graph: ProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode {
            var fillColor = "#FFFFFF";
            if (this.model.id < 0) {
                fillColor = justCreated ? this.newShapeColor : "#FBF8E7";
            }

            this.insertVertex(graph, this.model.id.toString(), null, x, y, this.USER_TASK_WIDTH, this.USER_TASK_HEIGHT, " editable=0;shape=label;strokeColor=#D4D5DA;fillColor=" + fillColor + ";foldable=0;fontColor=#4C4C4C;fontFamily=Open Sans, sans-serif;fontStyle=1;fontSize=12");
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
                graph.container,
                this.model.id.toString(),
                "Label-B" + this.model.id.toString(),
                this.label, 
                textLabelStyle, 
                this.LABEL_EDIT_MAXLENGTH,
                this.LABEL_VIEW_MAXLENGTH,
                graph.viewModel.isReadonly);

            //header
            graph.graph.addCell(this.header, this);
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
                graph.container,
                this.model.id.toString(),
                "Label-H" + this.model.id.toString(),
                this.persona,
                personaLabelStyle,
                this.PERSONA_EDIT_MAXLENGTH,
                this.PERSONA_VIEW_MAXLENGTH,
                graph.viewModel.isReadonly);

            graph.insertVertex(this, "HB" + this.model.id.toString(), null, 0.5, 0.5, this.USER_TASK_WIDTH - 1, 3, "shape=rectangle;strokeColor=none;fillColor=#009CDE;editable=0;selectable=0");

            //footer
            this.footerCell = graph.insertVertex(this, "F" + this.model.id.toString(), null, 0, this.USER_TASK_HEIGHT - 33, this.USER_TASK_WIDTH, 33, "shape=rectangle;strokeColor=#D4D5DA;fillColor=#FFFFFF;gradientColor=#DDDDDD;foldable=0;editable=0;selectable=0");

            this.addOverlays(graph);

            this.commentsButton.render(graph, this.footerCell, this.footerCell.geometry.width - 118, 10, "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
            this.relationshipButton.render(graph, this.footerCell, this.footerCell.geometry.width - 94, 10, "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
            this.linkButton.render(graph, this.footerCell, this.footerCell.geometry.width - 70, 10, "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
            if (graph.viewModel.isReadonly && graph.viewModel.licenseType === Shell.LicenseTypeEnum.Viewer) {
                this.linkButton.disable();
            }
            this.previewButton.render(graph, this.footerCell, this.footerCell.geometry.width - 46, 10, "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
            this.detailsButton.render(graph, this.footerCell, this.footerCell.geometry.width - 22, 10, "shape=ellipse;strokeColor=none;fillColor=none;selectable=0");
            return this;
        }

        private addOverlays(graph: ProcessGraph) {
            var overlays = graph.getCellOverlays(this);

            if (overlays != null) {
                graph.removeCellOverlays(this);
            }

            overlays = graph.getCellOverlays(this.footerCell);

            if (overlays != null) {
                graph.removeCellOverlays(this.footerCell);
            }

            // header overlays
            var personaIcon = "/Areas/Web/Style/images/Storyteller/defaultuser.svg";
            var overlayPersona = this.addOverlay(graph, this, personaIcon, 24, 24, this.rootScope.config.labels["ST_Persona_Label"], mxConstants.ALIGN_LEFT, mxConstants.ALIGN_TOP, 16, 18);

            // DO NOT DELETE!!! this is needed for the labels functionality
            this.addOverlay(graph, this, null, this.USER_TASK_WIDTH, this.USER_TASK_HEIGHT, null, mxConstants.ALIGN_LEFT, mxConstants.ALIGN_TOP, this.USER_TASK_WIDTH / 2, this.USER_TASK_HEIGHT / 2);

            // TODO: re-add for later sprints, when there's functionality attached to it (color coding nodes)
            //var colorsIcon = "/Areas/Web/Style/images/Storyteller/colors-on.png";
            //var overlayColors = this.addOverlay(graph, this, colorsIcon, 20, 20, this.rootScope.config.labels["ST_Colors_Label"], mxConstants.ALIGN_RIGHT, mxConstants.ALIGN_TOP, -12, 14);
        }

        private navigateToProcess() {
            if (this.associatedArtifact == null) {
                return;
            }
            var data: ICommandData = { processId: this.associatedArtifact.id, model: this.model };
            StorytellerCommands.getStorytellerCommands().getNavigateToProcessCommand().execute(data);
        }

        private openDialog(dialogType: ModalDialogType) {
            this.rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName,
                this.model.id,
                dialogType);
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
            }
            else {
                maxLen = this.LABEL_EDIT_MAXLENGTH;
            }
            return maxLen;
        }

        public formatElementText(cell: MxCell, text: string): string {
            /***
            * This function returns formatted text to the getLabel()
            * function to display the node's label and persona.  
            */

            if (cell && text) {
                var maxLen: number = this.LABEL_VIEW_MAXLENGTH;

                var element = <IDiagramNodeElement>cell;
                if (element.getElementType() === ElementType.UserTaskHeader) {
                    maxLen = this.PERSONA_VIEW_MAXLENGTH;
                }
                else {
                    maxLen = this.LABEL_VIEW_MAXLENGTH;
                }

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
            var element = <IDiagramNodeElement>cell;

            if (element.getElementType() === ElementType.UserTaskHeader) {
                this.persona = text;
            }
            else {
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
                    this.relationshipButton.activate();
                }
            }
        }

        public getDeleteDialogParameters(): Shell.IDialogParams {
            let dialogParams: Shell.IDialogParams = {};
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
}
