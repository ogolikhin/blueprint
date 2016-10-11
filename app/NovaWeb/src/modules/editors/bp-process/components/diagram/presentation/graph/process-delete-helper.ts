import {
    INotifyModelChanged, ProcessShapeType,
    NodeChange, IScopeContext,
    IProcessLink, IConditionContext
} from "./models/";
import {IProcessGraph} from "./models/";
import {ProcessGraph} from "./process-graph";
import {IMessageService} from "../../../../../../core/";
import {ProcessAddHelper} from "./process-add-helper";
import {ShapesFactory} from "./shapes/shapes-factory";

export class ProcessDeleteHelper {

    public static deleteUserTask(userTaskId: number, postDeleteFunction: INotifyModelChanged = null,
                                 processGraph: IProcessGraph): boolean {

        let newDestinationId: number = processGraph.viewModel.getFirstNonSystemShapeId(userTaskId);
        let previousShapeIds: number[] = processGraph.viewModel.getPrevShapeIds(userTaskId);
        if (!this.canDeleteUserTask(userTaskId, previousShapeIds, newDestinationId, processGraph)) {
            return false;
        }

        this.deleteUserTaskInternal(userTaskId, previousShapeIds, newDestinationId, processGraph);

        if (postDeleteFunction) {
            postDeleteFunction(NodeChange.Remove, userTaskId);
        }

        return true;
    }

    public static deleteDecision(decisionId: number, postDeleteFunction: INotifyModelChanged = null,
                                 processGraph: IProcessGraph, shapesFactoryService: ShapesFactory): boolean {

        if (!this.canDeleteDecision(decisionId, processGraph)) {
            return false;
        }

        let scopeContext = processGraph.getScope(decisionId);
        let shapesToDelete: number[] = Object.keys(scopeContext.visitedIds).map(a => Number(a));
        let firstOutgoingLink = processGraph.getNextLinks(decisionId).reduce((a, b) => a.orderindex < b.orderindex ? a : b);

        this.reconnectExternalLinksInScope(scopeContext, processGraph);
        processGraph.updateSourcesWithDestinations(decisionId, firstOutgoingLink.destinationId);
        this.deleteBranchDestinationId(decisionId, processGraph);
        this.deleteShapesAndLinksByIds(shapesToDelete, processGraph);

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
        } else if (this.isLastInProcess(previousShapeIds, newDestinationId, processGraph)) {
            errorMessage = rootScope.config.labels["ST_Delete_CannotDelete_OnlyUserTask"];
            canDelete = false;
        } else if (this.isLastUserTaskInCondition(userTaskId, previousShapeIds, newDestinationId, processGraph)) {
            let decisionId = this.getConnectedDecisionId(previousShapeIds, processGraph);

            if (!this.canDeleteDecisionConditions(decisionId, [userTaskId], processGraph)) {
                canDelete = false;
            }
        } else if (this.isUserTaskBetweenTwoUserDecisions(userTaskId, previousShapeIds, newDestinationId, processGraph)) {
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

    private static canDeleteDecisionConditions(decisionId: number, targetIds: number[],
                                               processGraph: IProcessGraph): boolean {
        let rootScope: any = processGraph.rootScope;
        let messageService: IMessageService = processGraph.messageService;

        let canDelete: boolean = true;
        let errorMessage: string;

        if (!targetIds || targetIds.length === 0) {
            canDelete = false;
        } else if (this.hasMinConditions(decisionId, processGraph)) {
            errorMessage = rootScope.config.labels["ST_Delete_CannotDelete_UD_AtleastTwoConditions"];
            canDelete = false;
        } else {
            for (let targetId of targetIds) {
                if (!processGraph.viewModel.getBranchDestinationId(decisionId, targetId)) {
                    canDelete = false;
                }
            }
        }

        if (!canDelete && errorMessage && messageService) {
            messageService.addError(errorMessage);
        }

        return canDelete;
    }

    private static hasMinConditions(decisionId: number, processGraph: IProcessGraph): boolean {
        return processGraph.viewModel.getNextShapeIds(decisionId).length <= ProcessGraph.MinConditions;
    }

    private static deleteUserTaskInternal(userTaskId: number, previousShapeIds: number[], newDestinationId: number,
                                          processGraph: IProcessGraph): void {

        if (this.isLastUserTaskInCondition(userTaskId, previousShapeIds, newDestinationId, processGraph)) {
            let decisionId = this.getConnectedDecisionId(previousShapeIds, processGraph);
            this.deleteDecisionBranches(decisionId, [userTaskId], processGraph);
        } else {
            let scopeContext = processGraph.getScope(userTaskId);
            let shapesToBeDeletedIds: number[] = Object.keys(scopeContext.visitedIds).map(a => Number(a));

            this.reconnectExternalLinksInScope(scopeContext, processGraph);
            processGraph.updateSourcesWithDestinations(userTaskId, newDestinationId);
            this.deleteShapesAndLinksByIds(shapesToBeDeletedIds, processGraph);

            for (let mapping of scopeContext.mappings) {
                this.deleteBranchDestinationId(mapping.decisionId, processGraph);
            }
        }
    }

    public static deleteDecisionBranches(decisionId: number, targetIds: number[],
                                         processGraph: IProcessGraph): boolean {

        if (!this.canDeleteDecisionConditions(decisionId, targetIds, processGraph)) {
            return false;
        }

        for (let targetId of targetIds) {
            // delete the link connecting decision to target
            let decisionToShapeLinkIndex = processGraph.viewModel.getLinkIndex(decisionId, targetId);
            let toBeRemovedLink = processGraph.viewModel.links[decisionToShapeLinkIndex];

            let scopeContext = processGraph.getBranchScope(toBeRemovedLink, processGraph.defaultNextIdsProvider);
            this.reconnectExternalLinksInScope(scopeContext, processGraph);

            let shapeIdsToDelete: number[] = Object.keys(scopeContext.visitedIds).map(a => Number(a));

            let indexOfMapping: number;
            for (indexOfMapping = 0; indexOfMapping < processGraph.viewModel.decisionBranchDestinationLinks.length; indexOfMapping++) {
                let condition = processGraph.viewModel.decisionBranchDestinationLinks[indexOfMapping];
                if (condition.sourceId === decisionId && condition.orderindex === toBeRemovedLink.orderindex) {
                    break;
                }
            }

            processGraph.viewModel.links.splice(decisionToShapeLinkIndex, 1);
            processGraph.viewModel.decisionBranchDestinationLinks.splice(indexOfMapping, 1);
            this.deleteShapesAndLinksByIds(shapeIdsToDelete, processGraph);
        }

        processGraph.notifyUpdateInModel(NodeChange.Remove, decisionId);

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
        let decisionId = this.getConnectedDecisionId(previousIds, processGraph);

        return decisionId &&
            processGraph.viewModel.getShapeTypeById(decisionId) === ProcessShapeType.UserDecision &&
            processGraph.viewModel.getShapeTypeById(nextShapeId) === ProcessShapeType.UserDecision;
    }

    public static isLastUserTaskInCondition(userTaskId: number, previousShapeIds: number[], nextShapeId: number, processGraph: IProcessGraph): boolean {
        let decisionId = this.getConnectedDecisionId(previousShapeIds, processGraph);
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

                let mapping: IConditionContext = this.getConditionFromIdInScope(id, scopeContext);
                if (!mapping) {
                    continue;
                }

                for (let prevShapeId of prevShapeIds) {
                    let isExternal = !scopeContext.visitedIds[prevShapeId] && prevShapeId !== originalDecisionId;
                    if (isExternal) {
                        let link = processGraph.getLink(prevShapeId, id);
                        let newDestinationId = mapping.targetId;

                        if (this.isInfiniteLoop(mapping.targetId, mapping, processGraph)) {
                            //if end shape's condition is coming back to this condition, then need to connect it back to the main flow.
                            newDestinationId = processGraph.layout.getConditionDestination(originalDecisionId).id;
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
            processGraph.viewModel.shapes = processGraph.viewModel.shapes.filter(shape => {
                return shape.id !== shapesToBeDeletedIds[i];
            });
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
