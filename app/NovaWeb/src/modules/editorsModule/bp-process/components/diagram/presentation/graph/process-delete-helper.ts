import {
    INotifyModelChanged, ProcessShapeType,
    NodeChange, IScopeContext,
    IProcessLink, IConditionContext
} from "./models/";
import {IProcessGraph} from "./models/";
import {ProcessGraph} from "./process-graph";
import {ProcessAddHelper} from "./process-add-helper";
import {ShapesFactory} from "./shapes/shapes-factory";
import {IMessageService} from "../../../../../../main/components/messages/message.svc";
import {ProcessModels} from "../../../../";
import {IProcessViewModel} from "../../viewmodel/process-viewmodel";

export class ProcessDeleteHelper {

    public static deleteUserTasks(userTaskIds: number[], processGraph: IProcessGraph): void {
        const clonedProcess = processGraph.viewModel.getClonedProcessModel();
        for (let userTaskId of userTaskIds) {
            processGraph.viewModel.updateTreeAndFlows();
            if (!!processGraph.viewModel.getShapeById(userTaskId) && !ProcessDeleteHelper.deleteUserTask(userTaskId, null, processGraph)) {
                processGraph.viewModel.updateProcessModel(clonedProcess);
                return;
            }
        }
        processGraph.notifyUpdateInModel(NodeChange.Remove, null);
    }

    public static deleteUserTask(userTaskId: number, postDeleteFunction: INotifyModelChanged = null,
                                 processGraph: IProcessGraph): boolean {

        let newDestinationId: number = processGraph.viewModel.getFirstNonSystemShapeId(userTaskId);
        let previousShapeIds: number[] = processGraph.viewModel.getPrevShapeIds(userTaskId);
        if (!ProcessDeleteHelper.canDeleteUserTask(userTaskId, previousShapeIds, newDestinationId, processGraph)) {
            return false;
        }

        ProcessDeleteHelper.deleteUserTaskInternal(userTaskId, previousShapeIds, newDestinationId, processGraph);

        if (postDeleteFunction) {
            postDeleteFunction(NodeChange.Remove, userTaskId);
        }

        return true;
    }

    public static deleteDecision(decisionId: number, postDeleteFunction: INotifyModelChanged = null,
                                 processGraph: IProcessGraph, shapesFactoryService: ShapesFactory): boolean {

        if (!ProcessDeleteHelper.canDeleteDecision(decisionId, processGraph)) {
            return false;
        }

        let scopeContext = processGraph.getScope(decisionId);
        let shapesToDelete: number[] = Object.keys(scopeContext.visitedIds).map(a => Number(a));
        let firstOutgoingLink = processGraph.getNextLinks(decisionId).reduce((a, b) => a.orderindex < b.orderindex ? a : b);

        ProcessDeleteHelper.reconnectExternalLinksInScope(scopeContext, processGraph);
        processGraph.updateSourcesWithDestinations(decisionId, firstOutgoingLink.destinationId);
        ProcessDeleteHelper.deleteBranchDestinationId(decisionId, processGraph);
        ProcessDeleteHelper.deleteShapesAndLinksByIds(shapesToDelete, processGraph);

        let selectedShapeId: number = firstOutgoingLink.destinationId;

        // if user decision is the last shape between precondition and end, replace it with new task pair
        let preconditionId = processGraph.viewModel.getPreconditionShapeId();
        let endId = processGraph.viewModel.getEndShapeId();
        // filtering links explicitly here because link index in ProcessClientModel might be out-of-date
        if (processGraph.viewModel.links.filter(link => link.sourceId === preconditionId && link.destinationId === endId).length > 0) {
            selectedShapeId = ProcessAddHelper.insertTask([preconditionId], endId, processGraph.layout, shapesFactoryService);
            processGraph.layout.createAutoInsertTaskMessage();
        }

        if (postDeleteFunction) {
            postDeleteFunction(NodeChange.Remove, selectedShapeId);
        }

        return true;
    }

    private static canDeleteUserTask(userTaskId: number, previousShapeIds: number[],
                                     newDestinationId: number, processGraph: IProcessGraph): boolean {
        let rootScope: any = processGraph.rootScope;
        let messageService: IMessageService = processGraph.messageService;

        let errorMessage: string;
        let canDelete: boolean = true;

        if (!previousShapeIds || !newDestinationId) {
            canDelete = false;
        } else if (ProcessDeleteHelper.isLastInProcess(previousShapeIds, newDestinationId, processGraph)) {
            errorMessage = rootScope.config.labels["ST_Delete_CannotDelete_OnlyUserTask"];
            canDelete = false;
        } else if (ProcessDeleteHelper.isLastUserTaskInCondition(userTaskId, previousShapeIds, newDestinationId, processGraph)) {
            let decisionId = ProcessDeleteHelper.getConnectedDecisionId(previousShapeIds, processGraph);

            if (!ProcessDeleteHelper.canDeleteDecisionConditions(decisionId, [userTaskId], processGraph)) {
                canDelete = false;
            }
        } else if (ProcessDeleteHelper.isUserTaskBetweenTwoUserDecisions(userTaskId, previousShapeIds, newDestinationId, processGraph)) {
            errorMessage = rootScope.config.labels["ST_Delete_CannotDelete_UT_Between_Two_UD"];
            canDelete = false;
        }

        if (!canDelete && errorMessage && messageService) {
            messageService.addError(errorMessage);
        }

        return canDelete;
    }

    private static canDeleteDecision(decisionId: number, processGraph: IProcessGraph): boolean {
        let errorMessage: string;
        let canDelete: boolean = true;
        let messageService: IMessageService = processGraph.messageService;

        let decision = processGraph.viewModel.getShapeById(decisionId);
        if (decision == null) {
            canDelete = false;
        }

        if (!canDelete && errorMessage && messageService) {
            messageService.addError(errorMessage);
        }

        return canDelete;
    }

    private static isLastInProcess(previousShapeIds: number[], nextShapeId: number, processGraph: IProcessGraph): boolean {
        let newDestinationShapeType = processGraph.viewModel.getShapeTypeById(nextShapeId);

        for (let previousShapeId of previousShapeIds) {
            let previousShapeType = processGraph.viewModel.getShapeTypeById(previousShapeId);

            if (previousShapeType === ProcessShapeType.PreconditionSystemTask &&
                newDestinationShapeType === ProcessShapeType.End
            ) {
                return true;
            }
        }

        return false;
    }

    public static canDeleteDecisionConditions(
        decisionId: number,
        targetIds: number[],
        processGraph: IProcessGraph
    ): boolean {
        let rootScope: any = processGraph.rootScope;
        let messageService: IMessageService = processGraph.messageService;

        if (!targetIds || targetIds.length === 0) {
            return false;
        }

        if (ProcessDeleteHelper.hasMinConditions(decisionId, processGraph)) {
            messageService.addError(rootScope.config.labels["ST_Delete_CannotDelete_UD_AtleastTwoConditions"]);
            return false;
        }

        for (let targetId of targetIds) {
            if (!processGraph.viewModel.getBranchDestinationId(decisionId, targetId)) {
                return false;
            }
        }

        return true;
    }

    private static hasMinConditions(decisionId: number, processGraph: IProcessGraph): boolean {
        return processGraph.viewModel.getNextShapeIds(decisionId).length <= ProcessGraph.MinConditions;
    }

    private static deleteUserTaskInternal(
        userTaskId: number,
        previousShapeIds: number[],
        newDestinationId: number,
        processGraph: IProcessGraph
    ): void {

        if (ProcessDeleteHelper.isLastUserTaskInCondition(userTaskId, previousShapeIds, newDestinationId, processGraph)) {
            let decisionId = ProcessDeleteHelper.getConnectedDecisionId(previousShapeIds, processGraph);
            let link = processGraph.getLink(decisionId, userTaskId);
            ProcessDeleteHelper.deleteDecisionBranch(link, processGraph);
        } else {
            let scopeContext = processGraph.getScope(userTaskId);
            let shapesToBeDeletedIds: number[] = Object.keys(scopeContext.visitedIds).map(a => Number(a));

            ProcessDeleteHelper.reconnectExternalLinksInScope(scopeContext, processGraph);
            processGraph.updateSourcesWithDestinations(userTaskId, newDestinationId);
            ProcessDeleteHelper.deleteShapesAndLinksByIds(shapesToBeDeletedIds, processGraph);

            for (let mapping of scopeContext.mappings) {
                ProcessDeleteHelper.deleteBranchDestinationId(mapping.decisionId, processGraph);
            }
        }
    }

    public static deleteDecisionBranch(
        branchLinkToDelete: IProcessLink,
        graph: IProcessGraph,
        triggerModelUpdate: boolean = true
    ): boolean {
        if (!this.canDeleteDecisionConditions(branchLinkToDelete.sourceId, [branchLinkToDelete.destinationId], graph)) {
            return false;
        }

        const branchLinkToDeleteIndex = graph.viewModel.getLinkIndex(branchLinkToDelete.sourceId, branchLinkToDelete.destinationId);

        const scopeContext = graph.getBranchScope(branchLinkToDelete, graph.defaultNextIdsProvider);
        ProcessDeleteHelper.reconnectExternalLinksInScope(scopeContext, graph);

        const shapeIdsToDelete: number[] = Object.keys(scopeContext.visitedIds).map(a => _.toNumber(a));

        graph.viewModel.links.splice(branchLinkToDeleteIndex, 1);
        _.remove(
            graph.viewModel.decisionBranchDestinationLinks,
            link => link.sourceId === branchLinkToDelete.sourceId && link.orderindex === branchLinkToDelete.orderindex
        );
        ProcessDeleteHelper.deleteShapesAndLinksByIds(shapeIdsToDelete, graph);
        if (triggerModelUpdate) {
            graph.notifyUpdateInModel(NodeChange.Remove, branchLinkToDelete.sourceId);
        }

        return true;
    }

    private static getConnectedDecisionId(previousIds: number[], processGraph: IProcessGraph): number {
        for (let previousId of previousIds) {
            let id = Number(previousId);

            if (processGraph.viewModel.isDecision(id)) {
                return id;
            }
        }

        return null;
    }

    public static isUserTaskBetweenTwoUserDecisions(userTaskId: number, previousIds: number[], nextShapeId: number, processGraph: IProcessGraph): boolean {
        let decisionId = ProcessDeleteHelper.getConnectedDecisionId(previousIds, processGraph);

        return decisionId &&
            processGraph.viewModel.getShapeTypeById(decisionId) === ProcessShapeType.UserDecision &&
            processGraph.viewModel.getShapeTypeById(nextShapeId) === ProcessShapeType.UserDecision;
    }

    public static isLastUserTaskInCondition(userTaskId: number, previousShapeIds: number[], nextShapeId: number, processGraph: IProcessGraph): boolean {
        let decisionId = ProcessDeleteHelper.getConnectedDecisionId(previousShapeIds, processGraph);
        if (!decisionId) {
            return false;
        }

        let destinationId = processGraph.viewModel.getBranchDestinationId(decisionId, userTaskId);
        return destinationId && destinationId === nextShapeId;
    }

    private static reconnectExternalLinksInScope(scopeContext: IScopeContext, processGraph: IProcessGraph) {
        if (scopeContext.mappings.length === 0) {
            return;
        }

        let originalDecisionId = scopeContext.mappings[0].decisionId;

        for (let visitedId in scopeContext.visitedIds) {
            if (scopeContext.visitedIds.hasOwnProperty(visitedId)) {
                const id: number = Number(visitedId);
//fixme: using continue is a bad practice. program defensivly and check for condition and if not meet throw errors
                if (id === originalDecisionId) {
                    continue;
                }

                let prevShapeIds: number[] = processGraph.viewModel.getPrevShapeIds(id);
                if (prevShapeIds.length <= 1) {
                    continue;
                }

                let mapping: IConditionContext = ProcessDeleteHelper.getConditionFromIdInScope(id, scopeContext);
                if (!mapping) {
                    continue;
                }

                for (let prevShapeId of prevShapeIds) {
                    let isExternal = !scopeContext.visitedIds[prevShapeId] && prevShapeId !== originalDecisionId;
                    if (isExternal) {
                        let link = processGraph.getLink(prevShapeId, id);
                        let newDestinationId = mapping.targetId;

                        if (ProcessDeleteHelper.isInfiniteLoop(mapping.targetId, mapping, processGraph)) {
                            //if end shape's condition is coming back to ProcessDeleteHelper condition, then need to connect it back to the main flow.
                            newDestinationId = processGraph.layout.getConditionDestination(originalDecisionId).id;
                        } else {
                            newDestinationId = ProcessDeleteHelper.getNextAvailableDestinationId(
                                                                    scopeContext, processGraph, newDestinationId, originalDecisionId);
                        }

                        link.destinationId = newDestinationId;

                        let branchEndMappings: IProcessLink[] = processGraph.viewModel.decisionBranchDestinationLinks.filter(link => link.destinationId === id);
                        for (let decisionBranchDestinationLink of branchEndMappings) {
                            decisionBranchDestinationLink.destinationId = Number(newDestinationId);
                        }
                    }
                }
            }
        }
    }

    private static getNextAvailableDestinationId(scopeContext: IScopeContext, processGraph: IProcessGraph, targetId: number, originalDecisionId: number) {
        let newDestinationId = targetId;
        while (scopeContext.visitedIds[newDestinationId]) {
            newDestinationId = processGraph.viewModel.getNextShapeIds(newDestinationId)[0];
            if (!!scopeContext.visitedIds[newDestinationId]) {
                const innerParent = scopeContext.visitedIds[newDestinationId].innerParentCondition();
                if (innerParent) {
                    if (ProcessDeleteHelper.isInfiniteLoop(innerParent.targetId, innerParent, processGraph)) {
                            newDestinationId = processGraph.layout.getConditionDestination(originalDecisionId).id;
                    }
                }
            }
        }
        return newDestinationId;
    }

    private static getConditionFromIdInScope(id: number, scopeContext: IScopeContext): IConditionContext {
        for (let mapIndex = scopeContext.mappings.length - 1; mapIndex > -1; mapIndex--) {
            let map = scopeContext.mappings[mapIndex];

            if (map.shapeIdsInCondition[id]) {
                return map;
            }
        }

        return null;
    }

    private static isInfiniteLoop(targetId: number, currentCondition: IConditionContext, processGraph: IProcessGraph): boolean {
        let mappingTargetId = targetId;

        let mappingTargetCondition = processGraph.globalScope.visitedIds[mappingTargetId].innerParentCondition();
        if (!mappingTargetCondition) {
            return false;
        }

        let mappingTargetConditionTargetCondition = processGraph.globalScope.visitedIds[mappingTargetCondition.targetId].innerParentCondition();
        if (!mappingTargetConditionTargetCondition) {
            return false;
        }

        return mappingTargetConditionTargetCondition.decisionId === currentCondition.decisionId &&
            mappingTargetConditionTargetCondition.orderindex === currentCondition.orderindex;
    }

    private static deleteShapesAndLinksByIds(shapesToBeDeletedIds: number[], processGraph: IProcessGraph) {
        for (let i in shapesToBeDeletedIds) {
            processGraph.viewModel.removeShape(shapesToBeDeletedIds[i]);
            processGraph.viewModel.links = processGraph.viewModel.links.filter(link => {
                return link.sourceId !== shapesToBeDeletedIds[i];
            });
        }
    }

    private static deleteBranchDestinationId(decisionShapeId: number, processGraph: IProcessGraph) {
        if (processGraph.viewModel.decisionBranchDestinationLinks != null) {
            // select the last available branch destination as destination for new branch
            for (let i = processGraph.viewModel.decisionBranchDestinationLinks.length - 1; i > -1; i--) {
                let condition = processGraph.viewModel.decisionBranchDestinationLinks[i];
                if (condition.sourceId === decisionShapeId) {
                    processGraph.viewModel.decisionBranchDestinationLinks.splice(i, 1);
                }
            }
        }
    }
}
