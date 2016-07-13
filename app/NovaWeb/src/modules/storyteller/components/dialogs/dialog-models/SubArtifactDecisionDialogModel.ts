module Storyteller {
    export class SubArtifactDecisionDialogModel implements IDialogModel {
        public clonedUserTask: UserTask;
        public originalUserTask: UserTask;

        public originalSystemTask: SystemTask;
        public clonedSystemTask: SystemTask;

        // new conditions to be added to Graph upon OK click
        public conditions: ICondition[];

        // existing graph nodes linked to the current node
        public originalExistingNodes: IDiagramNode[];
        public clonedExistingNodes: IDiagramNode[];

        public graph: IProcessGraph;
        public isReadonly: boolean;
        public isHistoricalVersion: boolean;

        public clonedDecision: IDecision;
        public originalDecision: IDecision;

        public subArtifactId: number;
        public nextNode: IProcessShape;
        public propertiesMw: any; //TODO correct interface required! 
        public tabClick: Function;
        public systemNodeVisible: boolean;

        public isUserDecision(): boolean {
            return this.clonedDecision.getNodeType() === NodeType.UserDecision;
        }

        public isSystemDecision(): boolean {
            return this.clonedDecision.getNodeType() === NodeType.SystemDecision;
        }
    }
}