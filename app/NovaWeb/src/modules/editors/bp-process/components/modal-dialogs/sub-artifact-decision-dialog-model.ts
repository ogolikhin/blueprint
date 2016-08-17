import {IModalDialogModel} from "./modal-dialog-model";

export class SubArtifactDecisionDialogModel implements IModalDialogModel {
    // TODO: replace definitions:
    // public clonedUserTask: UserTask;
    // public originalUserTask: UserTask;
    public clonedUserTask: any;
    public originalUserTask: any;

    // TODO: replace definitions:
    // public originalSystemTask: SystemTask;
    // public clonedSystemTask: SystemTask;
    public originalSystemTask: any;
    public clonedSystemTask: any;

    // new conditions to be added to Graph upon OK click
    // TODO: replace definitions:
    // public conditions: ICondition[];
    public conditions: any[];

    // existing graph nodes linked to the current node
    // TODO: replace definitions:
    // public originalExistingNodes: IDiagramNode[];
    // public clonedExistingNodes: IDiagramNode[];
    public originalExistingNodes: any[];
    public clonedExistingNodes: any[];

    // TODO: replace definitions:
    // public graph: IProcessGraph;
    public graph: any;
    public isReadonly: boolean;
    public isHistoricalVersion: boolean;

    // TODO: replace definitions:
    // public clonedDecision: IDecision;
    // public originalDecision: IDecision;
    public clonedDecision: any;
    public originalDecision: any;

    public subArtifactId: number;
    // TODO: replace definitions:
    // public nextNode: IProcessShape;
    public nextNode: any;
    public propertiesMw: any; //TODO correct interface required! 
    public tabClick: Function;
    public systemNodeVisible: boolean;

    public isUserDecision(): boolean {
        // TODO: replace code:
        //return this.clonedDecision.getNodeType() === NodeType.UserDecision;
        return this.clonedDecision.getNodeType() === 6;
    }

    public isSystemDecision(): boolean {
        // TODO: replace code:
        //return this.clonedDecision.getNodeType() === NodeType.SystemDecision;
        return this.clonedDecision.getNodeType() === 4;
    }
}