module Storyteller {
    export class SubArtifactDecisionEditorModalController extends BaseModalDialogController<SubArtifactDecisionDialogModel> {

        public static getUserTaskIcon(): string {
            return "fonticon-bp-actor";
        }

        public static getDecisionIcon(): string {
            return "fonticon-decision-diamond";
        }

        public static getEndIcon(): string {
            return "fonticon-storyteller-end";
        }

        public static getErrorIcon(): string {
            return "fonticon-error";
        }

        public getLinkableProcesses: (viewValue: string) => ng.IPromise<IArtifactReference[]>;
        private CONDITION_MAX_LENGTH = 40;
        private TASK_LABEL_MAX_LENGTH = 32;

        private deletedConditions: ICondition[] = [];

        private isReadonly: boolean = false;

        public static $inject = [
            "$scope",
            "$uibModalInstance",
            "dialogModel",
            "processModelService",
            "$rootScope",
            "$timeout",
            "$anchorScroll",
            "$location"
        ];

        constructor($scope: IModalScope,
            $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
            dialogModel: SubArtifactDecisionDialogModel,
            public processModelService: IProcessModelService,
            $rootScope: ng.IRootScopeService,
            private $timeout: ng.ITimeoutService,
            private $anchorScroll: ng.IAnchorScrollService,
            private $location: ng.ILocationService
        ) {
            super($rootScope, $scope, $uibModalInstance, dialogModel);

            this.isReadonly = dialogModel.isReadonly;

            // Only get processes once per controller
            var processesPromise: ng.IPromise<IArtifactReference[]>;
            var getProcesses = () => processesPromise || (processesPromise = processModelService.getProcesses(processModelService.processModel.projectId));
            this.getLinkableProcesses = (viewValue) => getProcesses()
                .then((processes: IArtifactReference[]) => {
                    var filtered = processes.filter(p => this.filterByDisplayLabel(p, viewValue));
                    return filtered.slice(0, 10).sort(this.sortById);
                });
           
            this.setNextNode(processModelService);

            this.dialogModel.tabClick = () => {
                this.dialogModel.systemNodeVisible = true;
            }
        }

        public get hasMaxConditions(): boolean {
            return this.dialogModel.conditions.length >= ProcessGraph.MaxConditions;
        }

        public get hasMinConditions(): boolean {
            return this.dialogModel.conditions.length <= ProcessGraph.MinConditions;
        }

        public setNextNode(processModelService: IProcessModelService) {
            this.dialogModel.nextNode = processModelService.getNextNode(<ISystemTaskShape>this.dialogModel.clonedDecision.model);
        }

        private sortById(p1: IArtifactReference, p2: IArtifactReference) {
            return p1.id - p2.id;
        }

        private filterByDisplayLabel(process: IArtifactReference, viewValue: string): boolean {
            //exlude current process
            if (process.id === this.processModelService.processModel.id) {
                return false;
            }
            //show all if viewValue is null/'underfined' or empty string
            if (!viewValue) {
                return true;
            }
            if ((`${process.typePrefix}${process.id}: ${process.name}`).toLowerCase().indexOf(viewValue.toLowerCase()) > -1) {
                return true;
            }
            return false;
        }

        public saveData() {
            this.populateDecisionChanges();
            this.addNewBranchesToGraph();
            this.removeDeletedBranchesFromGraph();

            // TODO Not for MDP
            //this.processModelService.setNextNode(this.dialogModel.clonedSystemDecision.model, this.dialogModel.nextNode);
        }

        public addCondition() {
            const conditionNumber = this.dialogModel.conditions.length + 1;
            let processLink: IProcessLink = <IProcessLink>{
                sourceId: this.dialogModel.clonedDecision.model.id,
                destinationId: null,
                orderindex: null,
                label: (<any>this.$rootScope).config.labels['ST_Decision_Modal_New_System_Task_Edge_Label'] + conditionNumber,
            }
            let validMergeNodes = this.dialogModel.graph.getValidMergeNodes(processLink);
            let newCondition: ICondition = Condition.create(processLink, null, validMergeNodes);
            this.dialogModel.conditions.push(newCondition);

            this.scrollToBottomOfConditionList();
        }

        private scrollToBottomOfConditionList() {
            let self = this;
            this.$timeout(function () {
                const oldHash = self.$location.hash();
                self.$location.hash('decision-modal-list-bottom');
                self.$anchorScroll();
                self.$location.hash(oldHash);
            });
        }

        public deleteCondition(item: ICondition) {
            var itemToDeleteIndex = this.dialogModel.conditions.indexOf(item);
            if (itemToDeleteIndex > -1) {
                this.dialogModel.conditions.splice(itemToDeleteIndex, 1);

                if (item.destinationId != null) {
                    this.deletedConditions.push(item);
                }
            }
        }

        private getFirstNonMergingPointShapeId(link: DiagramLink) {
            let targetId: number;

            if (link.targetNode.getNodeType() === NodeType.MergingPoint) {
                let outgoingLinks = link.targetNode.getOutgoingLinks(<ProcessGraph>this.dialogModel.graph);
                targetId = outgoingLinks[0].model.destinationId;
            } else {
                targetId = link.targetNode.model.id;
            }

            return targetId;
        }

        private updateExistingEdge(link: DiagramLink) {
            let targetId: number = this.getFirstNonMergingPointShapeId(link);
            let conditionsToUpdate: ICondition[] = this.dialogModel.conditions.filter((condition: ICondition) => condition.destinationId === targetId);

            if (conditionsToUpdate.length === 0) {
                return false;
            }

            let conditionToUpdate: ICondition = conditionsToUpdate[0];

            let isMergeNodeUpdate = this.updateMergeNode(conditionToUpdate);

            if (conditionToUpdate != null) {
                link.label =  conditionToUpdate.label;
            }

            return isMergeNodeUpdate;
        }

        private updateMergeNode(condition: ICondition) {
            return this.dialogModel.graph.updateMergeNode(this.dialogModel.originalDecision.model.id, condition);
        }

        private populateDecisionChanges() {
            this.dialogModel.originalDecision.setLabelWithRedrawUi(this.dialogModel.clonedDecision.label);

            let isMergeNodeUpdate: boolean = false;
            // update edges
            let outgoingLinks: DiagramLink[] = this.dialogModel.originalDecision.getOutgoingLinks(<ProcessGraph>this.dialogModel.graph);
            for (let outgoingLink of outgoingLinks) {
                if (this.updateExistingEdge(outgoingLink)) {
                    isMergeNodeUpdate = true ;
                }
            }
            if (isMergeNodeUpdate) {
                this.dialogModel.graph.notifyUpdateInModel(NodeChange.Update, this.dialogModel.clonedDecision.model.id);
            }
        }

        private removeDeletedBranchesFromGraph() {
            if (this.deletedConditions != null && this.deletedConditions.length > 0) {
                let targetIds: number[] = this.deletedConditions.map((condition: ICondition) => condition.destinationId);
                let decisionId = this.dialogModel.originalDecision.model.id;
                this.dialogModel.graph.deleteDecisionBranches(decisionId, targetIds);
            }
        }

        private addNewBranchesToGraph() {
            const newConditions = this.dialogModel.conditions.filter((condition: ICondition) => condition.destinationId == null);

            if (newConditions.length > 0) {
                let decisionId = this.dialogModel.originalDecision.model.id;
                this.dialogModel.graph.addDecisionBranches(decisionId, newConditions);
            }
        }

        public isLabelAvailable(): boolean {
            return this.dialogModel.clonedDecision.label != null && this.dialogModel.clonedDecision.label != "";
        }
        
        public areMergeNodesEmpty(): boolean {

            for (let i = 0; i < this.dialogModel.conditions.length; i++) {
                var condition = this.dialogModel.conditions[i];
                if (!condition.mergeNode && !this.isFirstBranch(condition)) {
                    return true;
                }
            }
            return false;
        }

        public isFirstBranch(condition: ICondition): boolean {
            // Assumption: the conditions are always sorted by order index.
            return this.dialogModel.conditions[0] === condition;
        }

        public getNodeIcon(node: IDiagramNode): string {
            if (node.getNodeType() === NodeType.UserTask) {
                return SubArtifactDecisionEditorModalController.getUserTaskIcon();
            }
            if (node.getNodeType() === NodeType.UserDecision) {
                return SubArtifactDecisionEditorModalController.getDecisionIcon();
            }
            if (node.getNodeType() === NodeType.ProcessEnd) {
                return SubArtifactDecisionEditorModalController.getEndIcon();
            }
            return SubArtifactDecisionEditorModalController.getErrorIcon();
        }
    
    }

    angular.module("Storyteller").controller("SubArtifactDecisionEditorModalController", SubArtifactDecisionEditorModalController);
}