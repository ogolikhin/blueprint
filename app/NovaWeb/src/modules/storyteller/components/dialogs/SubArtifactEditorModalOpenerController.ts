module Storyteller {
    import ModalSettings = angular.ui.bootstrap.IModalSettings;
    
    export class SubArtifactEditorModalOpenerController {

        private animationsEnabled: boolean = true;

        private isReadonly: boolean; 
        private isHistorical: boolean; 

        public static $inject = [
            "$scope",
            "$uibModal",
            "$rootScope",
            "processModelService"
        ];

        constructor(private $scope: ng.IScope,
            private $uibModal: angular.ui.bootstrap.IModalService,
            private $rootScope: ng.IRootScopeService,
            public processModelService: IProcessModelService
        ) {
            $scope.$on(BaseModalDialogController.dialogOpenEventName, (event: any, id: number, dialogType: ModalDialogType) => {
                var graph = <ProcessGraph>$scope.$parent["vm"].graph;
                var propertiesMw = this.getUtilityPanel();

                // get read-only status from viewmodel's isReadonly property 
                
                let viewModel = <IStorytellerViewModel>($scope.$parent["vm"].storytellerDiagram.storytellerViewModel);
                if (viewModel) {
                    this.isReadonly = viewModel.isReadonly;
                    this.isHistorical = viewModel.isHistorical;
                }
                else {
                    throw new Error("StorytellerViewModel is null in SubArtifactEditorModalOpenerController");              
                }                                            
                
                if (dialogType === ModalDialogType.UserSystemTaskDetailsDialogType) {
                    this.openUserSystemTaskDetailsModalDialog($scope, id, graph, propertiesMw);

                } else if (dialogType === ModalDialogType.UserSystemDecisionDetailsDialogType) {
                    this.openUserSystemDecisionDetailsModalDialog($scope, id, graph, propertiesMw);

                } else if (dialogType === ModalDialogType.PreviewDialogType) {
                    this.openPreviewModalDialog($scope, id, graph, propertiesMw);
                }
            });
        }

        private getUtilityPanel(): Shell.IPropertiesMw{
            let propertiesSvc = this.$scope.$root["propertiesSvc"];
            return propertiesSvc ? propertiesSvc() : null;
        }

        public getSubArtifactDialogModel(shapeId: number, graph: ProcessGraph, propertiesMw: Shell.IPropertiesMw): SubArtifactDialogModel {
            var userTaskDialogModel = new SubArtifactDialogModel();
            userTaskDialogModel.subArtifactId = shapeId;
            var node = graph.getNodeById(userTaskDialogModel.subArtifactId.toString());
            var userTaskNode: UserTask;
            var systemTaskNode: SystemTask;
            if (node.getNodeType() === NodeType.UserTask) {
                userTaskNode = <UserTask>node;
                let nextNodes = userTaskNode.getNextSystemTasks(graph);
                if (nextNodes){
                    systemTaskNode = nextNodes[0];
                }
                userTaskDialogModel.isUserTask = true;
            } else if (node.getNodeType() === NodeType.SystemTask) {
                systemTaskNode = <SystemTask>node;
                userTaskNode = <UserTask>systemTaskNode.getUserTask(graph);
                userTaskDialogModel.isUserTask = false;
            } else {
                return;
            }
            userTaskDialogModel.isSystemTask = !userTaskDialogModel.isUserTask;
            userTaskDialogModel.originalUserTask = userTaskNode;
            userTaskDialogModel.clonedUserTask = this.cloneNode(userTaskDialogModel.originalUserTask);
            userTaskDialogModel.originalSystemTask = systemTaskNode;
            userTaskDialogModel.clonedSystemTask = this.cloneNode(userTaskDialogModel.originalSystemTask);
            userTaskDialogModel.propertiesMw = propertiesMw;

           // set dialog model isReadonly property to enable/disable input controls
            userTaskDialogModel.isReadonly = this.isReadonly;
            userTaskDialogModel.isHistoricalVersion = this.isHistorical;

            return userTaskDialogModel;
        }

        public getUserSystemDecisionDialogModel(shapeId: number, graph: ProcessGraph, propertiesMw: any): SubArtifactDecisionDialogModel {
            let node = graph.getNodeById(shapeId.toString());

            if (node == null || (node.getNodeType() !== NodeType.UserDecision && node.getNodeType() !== NodeType.SystemDecision)) {
                return null;
            }

            let dialogModel: SubArtifactDecisionDialogModel = new SubArtifactDecisionDialogModel();
            let systemTasks: SystemTask[] = [];

            dialogModel.graph = graph;
            dialogModel.originalExistingNodes = systemTasks;
            dialogModel.clonedExistingNodes = <IDiagramNode[]>this.cloneArray(dialogModel.originalExistingNodes);

            dialogModel.conditions = [];

            // cloning decision
            dialogModel.originalDecision = <IDecision>node;
            dialogModel.clonedDecision = this.cloneNode(dialogModel.originalDecision);

            // populate existing conditions
            let decisionId: number = node.model.id;
            let outgoingLinks: IProcessLink[] = dialogModel.graph.getNextLinks(decisionId);

            for (let index = 0; index < outgoingLinks.length; index++){
                let outgoingLink: IProcessLink = outgoingLinks[index];
                let mergePoint: IDiagramNode = null;

                // We do not display change merge node option for first branch
                if (index !== 0) {
                    let mergeNodeId: string = dialogModel.originalDecision.getMergeNode(graph, outgoingLink.orderindex).id.toString();
                    mergePoint = graph.getNodeById(mergeNodeId);
                }

                let validMergeNodes: IDiagramNode[] = dialogModel.graph.getValidMergeNodes(outgoingLink);
                let condition: ICondition = Condition.create(outgoingLink, mergePoint, validMergeNodes);
                dialogModel.conditions.push(condition);
            }
            
            // clone edges one by one
            dialogModel.clonedDecision.edges = [];
            for (let edge of dialogModel.originalDecision.edges) {
                dialogModel.clonedDecision.edges.push(this.cloneNode(edge));
            }

            dialogModel.propertiesMw = propertiesMw;

            // set dialog model isReadonly property to enable/disable input controls
            dialogModel.isReadonly = this.isReadonly;
            dialogModel.isHistoricalVersion = this.isHistorical;

            return dialogModel;
        }

        private openUserSystemTaskDetailsModalDialog($scope: ng.IScope, shapeId: number, graph: ProcessGraph, propertiesMw: Shell.IPropertiesMw) {
            this.open("",
                "/Areas/Web/App/Components/Storyteller/components/dialogs/subartifact-editor/SubArtifactEditorModalTemplate.html",
                "SubArtifactEditorModalController",
                this.getSubArtifactDialogModel(shapeId, graph, propertiesMw),
                "storyteller-modal");
        }

        // @todo: replace with proper controller / template
        private openUserSystemDecisionDetailsModalDialog($scope: ng.IScope, shapeId: number, graph: ProcessGraph, propertiesMw: Shell.IPropertiesMw) {
            this.open("",
                "/Areas/Web/App/Components/Storyteller/components/dialogs/subartifact-decision-editor/SubArtifactDecisionEditorModalTemplate.html",
                "SubArtifactDecisionEditorModalController",
                this.getUserSystemDecisionDialogModel(shapeId, graph, propertiesMw),
                "storyteller-modal");
        }

        public getUserStoryDialogModel(shapeId: number, graph: ProcessGraph, propertiesMw: Shell.IPropertiesMw): UserStoryDialogModel {
            var userStoryDialogModel = new UserStoryDialogModel();
            userStoryDialogModel.subArtifactId = shapeId;
            var node = graph.getNodeById(userStoryDialogModel.subArtifactId.toString());
            var userTaskNode = <UserTask>node;
            userStoryDialogModel.previousSytemTasks = userTaskNode.getPreviousSystemTasks(graph);
            userStoryDialogModel.nextSystemTasks = userTaskNode.getNextSystemTasks(graph);
            userStoryDialogModel.originalUserTask = userTaskNode;
            userStoryDialogModel.clonedUserTask = this.cloneNode(userStoryDialogModel.originalUserTask);
            userStoryDialogModel.propertiesMw = propertiesMw;
            if (this.$scope.$parent["vm"].processModelService)
                userStoryDialogModel.isUserSystemProcess = this.$scope.$parent["vm"].processModelService.isUserToSystemProcess();
            userStoryDialogModel.isUserSystemProcess = graph.IsUserSystemProcess;

            // set dialog model isReadonly property to enable/disable input controls
            userStoryDialogModel.isReadonly = this.isReadonly;
            userStoryDialogModel.isHistoricalVersion = this.isHistorical;

            return userStoryDialogModel;
        }

        private openPreviewModalDialog($scope: ng.IScope, shapeId: number, graph: ProcessGraph, propertiesMw: Shell.IPropertiesMw) {

            this.open("",
                "/Areas/Web/App/Components/Storyteller/components/dialogs/userstory-preview/UserStoryPreviewTemplate.html",
                "UserStoryPreviewController",
                this.getUserStoryDialogModel(shapeId, graph, propertiesMw),
                "preview-modal");
        }

        public cloneNode = (node): any => {
            //return angular.extend({}, model);
            return jQuery.extend(true, {}, node);
        }

        public cloneArray = (arr:any[]): any[] => {
            return jQuery.extend(true, [], arr);
        }

        public open = (size, htmlFileName: string, controllerClassName: string, dialogModel: any, windowClass: string) => {
            this.$uibModal.open(<ModalSettings>{
                animation: this.animationsEnabled,
                templateUrl: htmlFileName,
                controller: controllerClassName,
                controllerAs: "vm",
                windowClass: windowClass,
                size: size,
                resolve: {
                    dialogModel: () => {
                        return dialogModel;
                    }
                }
            });
        }
    }



    angular.module("Storyteller").controller("SubArtifactEditorModalOpenerController", SubArtifactEditorModalOpenerController);
}
