import {ProcessGraph} from "./process-graph";
import {IProcessViewModel} from "../../viewmodel/process-viewmodel";
import {IProcessGraph} from "./models/process-graph-interfaces";
import {
    IDiagramNode,
    IProcessShape,
    ISystemTaskShape,
    NodeChange,
    ProcessShapeType,
    IProcessLink,
    ILayout
} from "./models/";
import {IProcessLinkModel, ProcessLinkModel} from "../../../../models/process-models";
import {ShapesFactory} from "./shapes/shapes-factory";
import {DiagramLink} from "./shapes/diagram-link";
import {NodeType} from "./models/process-graph-constants";

export class ProcessAddHelper {
    private static MinNoOfShapesAddedPerSystemDecision: number = 1;
    private static MinNoOfShapesAddedPerUserDecision: number = 2;

    public static insertTaskWithUpdate(edge: MxCell, layout: ILayout, shapesFactoryService: ShapesFactory): void {
        // insertTask adds two shapes:
        // user task + system task
        if (layout.viewModel.isWithinShapeLimit(2)) {
            let sourcesAndDestinations = layout.getSourcesAndDestinations(edge);
            let taskId = ProcessAddHelper.insertTask(sourcesAndDestinations.sourceIds, sourcesAndDestinations.destinationIds[0], layout, shapesFactoryService);

            layout.viewModel.communicationManager.processDiagramCommunication.modelUpdate(taskId);
        }
    };


    public static insertTask(sourceIds: number[], destinationId: number, layout: ILayout, shapesFactoryService: ShapesFactory): number {
        let taskId = ProcessAddHelper.insertTaskInternal(sourceIds, destinationId, layout, shapesFactoryService);
        return taskId;
    };

    private static insertTaskInternal(sourceIds: number[], destinationId: number, layout: ILayout, shapesFactoryService: ShapesFactory): number {
        // add user task and system task shapes
        const userTaskShapeId = ProcessAddHelper.insertUserTaskInternal(layout, shapesFactoryService);
        const systemTaskId = ProcessAddHelper.insertSystemTaskInternal(layout, shapesFactoryService);

        if (sourceIds.length > 1) {
            layout.updateBranchDestinationId(destinationId, userTaskShapeId);
        }

        // update links
        for (let id of sourceIds) {
            layout.updateLink(id, destinationId, userTaskShapeId);
        }

        ProcessAddHelper.addLinkInfo(userTaskShapeId, systemTaskId, layout);
        ProcessAddHelper.addLinkInfo(systemTaskId, destinationId, layout);

        return userTaskShapeId;
    }

    public static addLinkInfo(sourceId: number, destinationId: number, layout: ILayout, orderIndex: number = 0, label: string = "",
                              source: IDiagramNode = null, destination: IDiagramNode = null): IProcessLinkModel {
        const link = new ProcessLinkModel(layout.viewModel.id, sourceId, destinationId, orderIndex, label, source, destination);
        layout.viewModel.links.push(link);
        return link;
    }

    public static insertUserTaskInternal(layout: ILayout, shapesFactoryService: ShapesFactory) {
        layout.setTempShapeId(layout.getTempShapeId() - 1);
        const userTaskShape = shapesFactoryService.createModelUserTaskShape(layout.viewModel.id, layout.viewModel.projectId, layout.getTempShapeId(), -1, -1);

        ProcessAddHelper.addShape(userTaskShape, layout, shapesFactoryService);
        layout.updateProcessChangedState(userTaskShape.id, NodeChange.Add, false);

        return userTaskShape.id;
    }

    // #DEBUG
    private static addShape(processShape: IProcessShape, layout: ILayout, shapesFactoryService: ShapesFactory): void {
        if (processShape != null) {
            layout.viewModel.addShape(processShape);
            layout.viewModel.addJustCreatedShapeId(processShape.id);
        }
    }

    public static insertSystemTaskInternal(layout: ILayout, shapesFactoryService: ShapesFactory) {
        layout.setTempShapeId(layout.getTempShapeId() - 1);
        const systemTaskShape = shapesFactoryService.createModelSystemTaskShape(layout.viewModel.id, layout.viewModel.projectId,
            layout.getTempShapeId(), -1, -1);
        ProcessAddHelper.addShape(systemTaskShape, layout, shapesFactoryService);
        layout.updateProcessChangedState(systemTaskShape.id, NodeChange.Add, false);

        return systemTaskShape.id;
    }

    public static insertUserDecision(edge: MxCell, layout: ILayout, shapesFactoryService: ShapesFactory) {
        // insertUserDecision adds five shapes:
        // user condition + user task + system task + user task + system task
        if (layout.viewModel.isWithinShapeLimit(5)) {
            let sourcesAndDestinations = layout.getSourcesAndDestinations(edge);
            let id = ProcessAddHelper.insertUserDecisionInternal(sourcesAndDestinations.sourceIds, sourcesAndDestinations.destinationIds[0],
                layout, shapesFactoryService);

            layout.updateProcessChangedState(id, NodeChange.Add, false);
            layout.viewModel.communicationManager.processDiagramCommunication.modelUpdate(id);
        }
    }

    private static insertUserDecisionInternal(sourceIds: number[], destinationId: number, layout: ILayout, shapesFactoryService: ShapesFactory): number {
        layout.setTempShapeId(layout.getTempShapeId() - 1);
        const userDecisionShape = shapesFactoryService.createModelUserDecisionShape(layout.viewModel.id,
            layout.viewModel.projectId, layout.getTempShapeId(), -1, -1);

        ProcessAddHelper.addShape(userDecisionShape, layout, shapesFactoryService);

        // update source decision references
        if (sourceIds.length > 1) {
            layout.updateBranchDestinationId(destinationId, userDecisionShape.id);
        }

        // update links
        for (let id of sourceIds) {
            layout.updateLink(id, destinationId, userDecisionShape.id);
        }

        ProcessAddHelper.addLinkInfo(userDecisionShape.id, destinationId, layout, 0, layout.getDefaultBranchLabel(userDecisionShape.id, NodeType.UserDecision));

        // add tasks before end
        const nextShapeType = layout.viewModel.getShapeTypeById(destinationId);

        if (nextShapeType === ProcessShapeType.End || layout.viewModel.hasMultiplePrevShapesById(destinationId)) {
            ProcessAddHelper.insertTaskInternal([userDecisionShape.id], destinationId, layout, shapesFactoryService);
        }

        // add new branch
        const branchDestination: IProcessShape = layout.getConditionDestination(userDecisionShape.id);
        ProcessAddHelper.insertUserDecisionConditionInternal(userDecisionShape.id, branchDestination.id, layout, shapesFactoryService);

        return userDecisionShape.id;
    }

    private static insertUserDecisionConditionInternal(
        userDecisionId: number,
        branchDestinationId: number,
        layout: ILayout,
        shapesFactoryService: ShapesFactory,
        label?: string,
        orderIndex?: number
    ): number {
        // add user task and system task shapes
        let userTaskShapeId = ProcessAddHelper.insertUserTaskInternal(layout, shapesFactoryService);
        let systemTaskId = ProcessAddHelper.insertSystemTaskInternal(layout, shapesFactoryService);
        let currentOrderIndex = orderIndex || layout.viewModel.getNextOrderIndex(userDecisionId);
        let currentLabel: string = label || layout.getDefaultBranchLabel(userDecisionId, NodeType.UserDecision);

        // add links
        let condition = ProcessAddHelper.addLinkInfo(userDecisionId, userTaskShapeId, layout, currentOrderIndex, currentLabel);
        ProcessAddHelper.addLinkInfo(userTaskShapeId, systemTaskId, layout);
        ProcessAddHelper.addLinkInfo(systemTaskId, branchDestinationId, layout);

        const branchDestinationLink: IProcessLink = {
            sourceId: userDecisionId,
            destinationId: branchDestinationId,
            orderindex: condition.orderindex,
            label: null
        };

        ProcessAddHelper.updateBranchDestination(branchDestinationLink, layout);

        return userTaskShapeId;
    }

    private static updateBranchDestination(processLink: IProcessLink, layout: ILayout) {
        if (processLink == null) {
            return;
        }

        if (layout.viewModel.decisionBranchDestinationLinks == null) {
            layout.viewModel.decisionBranchDestinationLinks = new Array<IProcessLink>();
            layout.viewModel.decisionBranchDestinationLinks.push(processLink);
        } else {
            const matchingLinks = layout.viewModel.getDecisionBranchDestinationLinks((link: IProcessLink) => {
                return processLink.sourceId === link.sourceId &&
                    processLink.destinationId === link.destinationId &&
                    processLink.orderindex === link.orderindex;
            });

            if (matchingLinks.length === 0) {
                layout.viewModel.decisionBranchDestinationLinks.push(processLink);
            }
        }
    }

    public static insertUserDecisionConditionWithUpdate(
        decisionId: number,
        layout: ILayout,
        shapesFactoryService: ShapesFactory,
        label?: string,
        conditionDestinationId?: number
    ): number {
        // insertUserDecisionCondition adds 2 shapes:
        // user task + system task
        if (layout.viewModel.isWithinShapeLimit(2)) {
            let id = ProcessAddHelper.insertUserDecisionCondition(decisionId, layout, shapesFactoryService, label, conditionDestinationId);
            layout.viewModel.communicationManager.processDiagramCommunication.modelUpdate(id);

            return id;
        }
    }

    public static insertUserDecisionCondition(
        decisionId: number,
        layout: ILayout,
        shapesFactoryService: ShapesFactory,
        label?: string,
        orderIndex?: number,
        conditionDestinationId?: number
    ): number {
        if (!conditionDestinationId) {
            let branchDestination: IProcessShape = layout.getConditionDestination(decisionId);
            conditionDestinationId = branchDestination.id;
        }

        return ProcessAddHelper.insertUserDecisionConditionInternal(decisionId, conditionDestinationId,
            layout, shapesFactoryService, label, orderIndex);
    }

    public static insertSystemDecision(connector: DiagramLink, layout: ILayout, shapesFactoryService: ShapesFactory) {
        // insertSystemDecision adds two shapes:
        // system decision + system task
        if (layout.viewModel.isWithinShapeLimit(2)) {
            let id: number = ProcessAddHelper.insertSystemDecisionInternal(connector.model, layout, shapesFactoryService);

            layout.viewModel.communicationManager.processDiagramCommunication.modelUpdate(id);
        }
    }

    private static insertSystemDecisionInternal(link: IProcessLink, layout: ILayout, shapesFactoryService: ShapesFactory): number {
        const sourceId = link.sourceId;
        const destinationId = link.destinationId;
        layout.setTempShapeId(layout.getTempShapeId() - 1);
        const systemDecision = shapesFactoryService.createSystemDecisionShapeModel(layout.getTempShapeId(),
            layout.viewModel.id, layout.viewModel.projectId, -1, -1);
        ProcessAddHelper.addShape(systemDecision, layout, shapesFactoryService);
        layout.updateProcessChangedState(systemDecision.id, NodeChange.Add, false);

        layout.updateLink(sourceId, destinationId, systemDecision.id);
        ProcessAddHelper.addLinkInfo(systemDecision.id, destinationId, layout, 0, layout.getDefaultBranchLabel(systemDecision.id, NodeType.SystemDecision));

        const branchDestination: IProcessShape = layout.getConditionDestination(systemDecision.id);
        ProcessAddHelper.insertSystemDecisionConditionInternal(systemDecision.id, branchDestination.id, layout, shapesFactoryService);

        return systemDecision.id;
    }

    private static insertSystemDecisionConditionInternal(
        systemDecisionId: number,
        branchDestinationId: number,
        layout: ILayout,
        shapesFactoryService: ShapesFactory,
        label?: string,
        orderIndex?: number
    ): number {
        let systemTaskId = ProcessAddHelper.insertSystemTaskInternal(layout, shapesFactoryService);

        let currentOrderIndex: number = orderIndex || layout.viewModel.getNextOrderIndex(systemDecisionId);
        let currentLabel: string = label || layout.getDefaultBranchLabel(systemDecisionId, NodeType.SystemDecision);
        let condition = ProcessAddHelper.addLinkInfo(systemDecisionId, systemTaskId, layout, currentOrderIndex, currentLabel);

        ProcessAddHelper.addLinkInfo(systemTaskId, branchDestinationId, layout);

        let branchDestinationLink: IProcessLink = {
            sourceId: systemDecisionId,
            destinationId: branchDestinationId,
            orderindex: condition.orderindex,
            label: null
        };

        ProcessAddHelper.updateBranchDestination(branchDestinationLink, layout);

        return systemTaskId;
    }

    public static insertSystemDecisionConditionWithUpdate(
        decisionId: number,
        layout: ILayout,
        shapesFactoryService: ShapesFactory,
        label?: string,
        conditionDestinationId?: number
    ): number {
        // insertSystemDecisionCondition adds 1 shape:
        // system task
        if (layout.viewModel.isWithinShapeLimit(1)) {
            let id = ProcessAddHelper.insertSystemDecisionCondition(decisionId, layout, shapesFactoryService, label, conditionDestinationId);
            layout.viewModel.communicationManager.processDiagramCommunication.modelUpdate(id);

            return id;
        }
    }

    public static insertSystemDecisionCondition(
        decisionId: number,
        layout: ILayout,
        shapesFactoryService: ShapesFactory,
        label?: string,
        orderIndex?: number,
        conditionDestinationId?: number
    ): number {
        if (!conditionDestinationId) {
            let branchDestination: IProcessShape = layout.getConditionDestination(decisionId);
            conditionDestinationId = branchDestination.id;
        }

        return ProcessAddHelper.insertSystemDecisionConditionInternal(decisionId, conditionDestinationId,
            layout, shapesFactoryService, label, orderIndex);
    };

    private static hasMaxConditions(decisionId: number, viewModel: IProcessViewModel): boolean {
        return viewModel.getNextShapeIds(decisionId).length >= ProcessGraph.MaxConditions;
    }

    public static canAddDecisionConditions(decisionId: number, numberOfConditions: number, graph: IProcessGraph): boolean {
        let shapeType = graph.viewModel.getShapeTypeById(decisionId);

        if (!numberOfConditions) {
            return false;
        }

        if (this.hasMaxConditions(decisionId, graph.viewModel)) {
            graph.messageService.addError(graph.rootScope.config.labels["ST_Add_CannotAdd_MaximumConditionsReached"]);
            return false;
        }

        if (shapeType === ProcessShapeType.SystemDecision &&
            !graph.viewModel.isWithinShapeLimit(numberOfConditions * this.MinNoOfShapesAddedPerSystemDecision)) {
            return false;
        }

        if (shapeType === ProcessShapeType.UserDecision &&
            !graph.viewModel.isWithinShapeLimit(numberOfConditions * this.MinNoOfShapesAddedPerUserDecision)) {
            return false;
        }

        return true;
    }
}
