import {IProcessShape, IProcessLink} from "../../../../models/processModels";
import {IProcessGraph, ILayout, INotifyModelChanged, ICondition, IDiagramNode} from "./process-graph-interfaces";
import {Direction, NodeType, NodeChange, ElementType} from "./process-graph-constants";
import {IProcessViewModel} from "../../viewmodel/process-viewmodel";


export class ProcessGraph implements IProcessGraph {

    constructor() {
        var test = "test";
    }

    public graph: MxGraph;
    public layout: ILayout;
    public notifyUpdateInModel: INotifyModelChanged;

    public deleteUserTask(userTaskId: number, postDeleteFunction?: INotifyModelChanged) {
    }
    public deleteDecision(decisionId: number, postDeleteFunction?: INotifyModelChanged) {
    }
    public addDecisionBranches(decisionId: number, newConditions: ICondition[]) {
    }
    public deleteDecisionBranches(decisionId: number, targetIds: number[]) {
    }
    public updateMergeNode(decisionId: number, condition: ICondition): boolean {

        throw new Error("not implemented");
    }
    
    public getValidMergeNodes(condition: IProcessLink): IDiagramNode[] {

        throw new Error("not implemented");
    }
    public getNodeById(id: string): IDiagramNode {

        throw new Error("not implemented");
    }
    public getNextLinks(id: number): IProcessLink[] {

        throw new Error("not implemented");
    }
    public redraw(action: any) {
    }
    public saveProcess() {
    }
    public publishProcess() {
    }
    public destroy() {
    }

}