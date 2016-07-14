module Storyteller {
    export interface IProcessGraph {
        graph: MxGraph;
        layout: Layout;

        deleteUserTask(userTaskId: number, postDeleteFunction?: INotifyModelChanged);
        deleteDecision(decisionId: number, postDeleteFunction?: INotifyModelChanged);
        addDecisionBranches(decisionId: number, newConditions: ICondition[]);
        deleteDecisionBranches(decisionId: number, targetIds: number[]);
        updateMergeNode(decisionId: number, condition: ICondition): boolean;
        notifyUpdateInModel: INotifyModelChanged;
        getValidMergeNodes(condition: IProcessLink): IDiagramNode[];
        getNodeById(id: string): IDiagramNode;
        getNextLinks(id: number): IProcessLink[];
        redraw(action: any);
        saveProcess();
        publishProcess();
        destroy();
    }
}
