import {
    IDiagramNode, IProcessShape,
    NodeChange, ProcessShapeType, IProcessLink
} from "./models/";
import {ILayout, ProcessClipboardData, UserTaskShapeModel} from "./models/";
import {IProcessLinkModel, ProcessLinkModel} from "../../../../models/process-models";
import {ShapesFactory} from "./shapes/shapes-factory";
import {DiagramLink} from "./shapes/diagram-link";
import {IProcessGraph, NodeType} from "./models/";
import {ProcessGraph} from "./process-graph";
import {ProcessAddHelper} from "./process-add-helper";
import {SystemTask, DiagramNode, UserTask} from "./shapes/";
import {IClipboardService, ClipboardDataType} from "../../../../services/clipboard.svc";

export class ProcessCopyPasteHelper {

    private static getSelectedCellsForCopy(processGraph: IProcessGraph): MxCell[] {
        const graphSelectedCells = processGraph.getMxGraph().getSelectionCells();

        const sortedCells = _.sortBy(graphSelectedCells, (node: IDiagramNode) => [node.model.propertyValues["x"].value, node.model.propertyValues["y"].value]);
        
        return sortedCells;
    }

    public static copySectedShapes(processGraph: IProcessGraph, clipboard: IClipboardService, shapesFactoryService: ShapesFactory): void {

        if (!clipboard) {
            throw new Error("Clipboard does not exist");
        }
        
        let model: IProcessShape[] = [];        
        const nodes = this.getSelectedCellsForCopy(processGraph);


        // let userDecisionRefs = [];
        // let userDecisionIdsMap = [];
        // let userTaskRefs = [];
        // _.each(nodes, (node) => {
        //     if (node instanceof UserTask) {
        //         const previousShapeIds: number[] = processGraph.viewModel.getPrevShapeIds(node.model.id);
        //         const shape = processGraph.viewModel.getShapeById(previousShapeIds[0]);

        //         if (shape.propertyValues["clientType"]["value"] === NodeType.UserDecision) {
        //             userTaskRefs[node.id.toString()] = {node:node, userDecisionId:shape.id.toString()};
                    
        //             const userDecisionRef = userDecisionRefs[shape.id.toString()];
        //             if (userDecisionRef) {
        //                 userDecisionRef.numberOfBranches++;
        //             } else {
        //                 userDecisionRefs[shape.id.toString()] = {node:shape, numberOfBranches:1};
        //             } 
        //         } else {
        //             userTaskRefs[node.id.toString()] = {node:node, userDecisionId:null};
        //         }
        //     }
        // });

        //
        // Copy from (clone) logic goes here. Implemented simple logic for cloning UserTask + first SystemTask
        // For the UserTasks only a plain collection is sufficient. For the decisions we will need a tree. 
        //
        _.each(nodes, (node) => {
            if (node instanceof UserTask) {
                const userTaskShape = shapesFactoryService.createModelUserTaskShape(-1, -1,  -1, -1, -1);
                // COPY UT PROPERTIES - Can add more here if needed. It can be extracted into a method  
                userTaskShape.name = node.model.name;
                userTaskShape.id =  node.model.id;
                userTaskShape.personaReference = _.cloneDeep(node.model.personaReference); 
                userTaskShape.propertyValues = _.cloneDeep(node.model.propertyValues); 
                model.push(userTaskShape);
                
                const systemTask = node.getNextSystemTasks(processGraph)[0];
                const systemTaskShape = shapesFactoryService.createModelSystemTaskShape(-1, -1,  -1, -1, -1);
                // COPY ST PROPERTIES - Can add more here if needed. It can be extracted into a method  
                systemTaskShape.name = systemTask.model.name; 
                systemTaskShape.personaReference = _.cloneDeep(systemTask.personaReference); 
                systemTaskShape.propertyValues = _.cloneDeep(systemTask.model.propertyValues); 
                
                model.push(systemTaskShape);
            }
        });

        clipboard.setData(new ProcessClipboardData(model));
    };

    public static insertSelectedShapes(edge: MxCell, layout: ILayout, clipboard: IClipboardService, shapesFactoryService: ShapesFactory): void {

        if (!clipboard) {
            throw new Error("Clipboard does not exist");
        }
        
        const processClipboardData = <ProcessClipboardData>clipboard.getData();

        if (!processClipboardData) {
            throw new Error("Clipboard is empty."); 
        }

        if (processClipboardData.type !== ClipboardDataType.Process) {
            throw new Error("Clipboard data has wrong type."); 
        }

        if (layout.viewModel.isWithinShapeLimit(processClipboardData.data.length)) {
            const sourcesAndDestinations = layout.getSourcesAndDestinations(edge);
            const destinationId = sourcesAndDestinations.destinationIds[0];
            const newUserTaskShapeIds: number[] = [];

            let sourceIds = sourcesAndDestinations.sourceIds;

            //
            // Paste to (clone) logic goes here. Implemented simple logic for cloning User/System Tasks
            // Clone Tasks one by one into the process model from the clipboard collection. 
            // This part of code will be more complicated when we'll clone decisions (tree).   
            //
            _.each(processClipboardData.data, (node) => {
                if (node instanceof UserTaskShapeModel) {
                    const userTaskShapeId = ProcessAddHelper.insertClonedUserTaskInternal(layout, shapesFactoryService, <any>node);
                    newUserTaskShapeIds.push(userTaskShapeId);
                    if (sourceIds.length > 1) {
                        layout.updateBranchDestinationId(destinationId, userTaskShapeId);
                    }
                    
                    // update links
                    for (let id of sourceIds) {
                        layout.updateLink(id, destinationId, userTaskShapeId);
                    }

                } else {
                    const systemTaskId = ProcessAddHelper.insertClonedSystemTaskInternal(layout, shapesFactoryService, <any>node);
                    ProcessAddHelper.addLinkInfo(newUserTaskShapeIds[newUserTaskShapeIds.length - 1], systemTaskId, layout);
                    ProcessAddHelper.addLinkInfo(systemTaskId, destinationId, layout);

                    sourceIds = [];
                    sourceIds[0] = systemTaskId;
                }

            });
            const focusUserTaskId = newUserTaskShapeIds.length > 0 ? newUserTaskShapeIds[0] : null;
            layout.viewModel.communicationManager.processDiagramCommunication.modelUpdate(focusUserTaskId);
        }
    };
    
}
