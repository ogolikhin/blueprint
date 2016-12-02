import {
    IDiagramNode, IProcessShape,
    NodeChange, ProcessShapeType, IProcessLink
} from "./models/";
import {ILayout, ProcessClipboardData, UserTaskShapeModel, SystemTaskShapeModel} from "./models/";
import {IProcessLinkModel, ProcessLinkModel, IUserTaskShape, ISystemTaskShape} from "../../../../models/process-models";
import {ShapesFactory} from "./shapes/shapes-factory";
import {DiagramLink} from "./shapes/diagram-link";
import {IProcessGraph, NodeType} from "./models/";
import {ProcessGraph} from "./process-graph";
import {ProcessAddHelper} from "./process-add-helper";
import {DiagramNode, SystemTask, UserTask, UserDecision, SystemDecision} from "./shapes/";
import {IClipboardService, ClipboardDataType} from "../../../../services/clipboard.svc";

enum CopyPasteNodeType {
    UserTask,
    UserDecision,
    SystemTask,
    SystemDecision
}

class CopyPasteNode {
    constructor(id: string, prevId: string, nextIds: string[], type: CopyPasteNodeType, x: number, y: number) {
        this.id = id;
        this.prevId = prevId;
        this.nextIds = nextIds; 
        this.type = type;
        this.isProcessed = false;
        this.x = x;
        this.y = y;
    }
    id: string;
    prevId: string;
    nextIds: string[]; 
    type: CopyPasteNodeType;
    x: number;
    y: number;
    isProcessed: boolean;
}

class CopyPasteLink {
    constructor(label: string, orderindex: number) {
        this.label = label;
        this.orderindex = orderindex;
    }
    label: string;
    orderindex: number;
}

class CopyPasteData {
    constructor() {
        this.shapes = [];
        this.links = [];
        this.copyPasteTree = [];
        this.treeIndex = [];
    }
    shapes: IProcessShape[];
    links: CopyPasteLink[];
    copyPasteTree: CopyPasteNode[];
    treeIndex: string[];
    startId: number;

    public addCopyPasteNode(id: string, prevId: string, nextIds: string[], type: CopyPasteNodeType, x: number, y: number) {
        this.copyPasteTree[id] = new CopyPasteNode(id, prevId, nextIds, type, x, y);
        this.treeIndex.push(id);
    }

    public sortTree() {
        this.treeIndex = _.sortBy(this.treeIndex, (id: string) =>  this.copyPasteTree[id].x * 1000 + this.copyPasteTree[id].y);
    }
}

class Branch {
    constructor(taskId: string, label: string, orderindex: number) {
        this.taskId = taskId;
        this.label = label;
        this.orderindex = orderindex;
    }
    taskId: string;
    label: string;
    orderindex: number;
}

// This is used for the user and system decisions with branches
class TaskRef {
    constructor(decisionId: string, taskId: string, label: string, orderindex: number) {
        this.decisionId = decisionId;
        this.branches = [];
        this.branches.push(new Branch(taskId, label, orderindex));
    }
    decisionId: string; 
    branches: Branch[]; 
}

export class ProcessCopyPasteHelper {

    private layout: ILayout;

    constructor(private processGraph: IProcessGraph, private clipboard: IClipboardService, private shapesFactoryService: ShapesFactory) {
        this.layout = processGraph.layout;
    }

    public copySectedShapes(): void {
        
        if (!this.clipboard) {
            throw new Error("Clipboard does not exist");
        }
        
        let data: CopyPasteData = new CopyPasteData();
        let  baseNodes = this.processGraph.getMxGraph().getSelectionCells();

        // 1. Find all UDs 
        let taskRefs: TaskRef[] = [];
        _.each(baseNodes, (node) => {
            if (node instanceof UserTask) {
                const previousShapeIds: number[] = this.processGraph.viewModel.getPrevShapeIds(node.model.id);
                const shape = this.processGraph.viewModel.getShapeById(previousShapeIds[0]);

                if (shape.propertyValues["clientType"].value === NodeType.UserDecision) {
                    const link: IProcessLink = this.processGraph.getLink(shape.id, node.id);

                    const userTaskRef = taskRefs[shape.id.toString()];
                    if (userTaskRef) {
                        userTaskRef.branches.push(new Branch(node.id, link.label, link.orderindex));
                    } else {
                        taskRefs[shape.id.toString()] = new TaskRef(shape.id.toString(), node.id, link.label, link.orderindex);
                    } 
                }
            }
        });

        // 2. add user decisions to base nodes
        _.forOwn(taskRefs, (node) => {
            if (!!node && node.branches.length > 1) {
                // sort branches by orderindex 
                node.branches = _.sortBy(node.branches, (branch: any) => branch.orderindex);
                baseNodes.push(this.layout.getNodeById(node.decisionId));
            }
        });        

        // sort base nodes
        baseNodes = _.sortBy(baseNodes, (node: IDiagramNode) =>
                                                     [node.model.propertyValues["x"].value, node.model.propertyValues["y"].value]);

        let prevId = "0";
        _.each(baseNodes, (node) => {
            // skip processed nodes
            if (!data.copyPasteTree[(<IDiagramNode>node).model.id]) {
                if (node instanceof UserTask) {
                    this.addUserAndSystemTasks(prevId, data, node, taskRefs);
                } else if (node instanceof UserDecision) { // user decision
                    this.addUserDecisionAndTasks(prevId, data, node, taskRefs);
                } else {
                    throw new Error("Unsupported copy/paste type");
                }
            } 
        });

        data.sortTree();

        // add branches links
        _.forOwn(taskRefs, (node: TaskRef) => {
            if (!!node && node.branches.length > 1) {
                for (let branch of node.branches) {
                    let link = new CopyPasteLink(branch.label, branch.orderindex);
                    data.links[node.decisionId + ";" + branch.taskId] = link;
                }
            }
        });        
        
        this.clipboard.setData(new ProcessClipboardData(data));
    };

    private  addUserDecisionAndTasks(prevId: string, data: CopyPasteData, node: UserDecision, taskRefs: TaskRef[]) {
        const userDecisionShape = this.createUserDecisionShape(node);
        const userDecisionId: string = userDecisionShape.id.toString();
        data.shapes[userDecisionId] = userDecisionShape;

        const userTasks: string[] = []; 
        _.each(taskRefs[userDecisionId].branches, (branch: Branch) => {
            let userTask = this.processGraph.getNodeById(branch.taskId);
            userTasks.push(branch.taskId);
            this.addUserAndSystemTasks(userDecisionId, data, <UserTask>userTask, taskRefs);
        });

        data.addCopyPasteNode(userDecisionId, prevId, userTasks, 
                                            CopyPasteNodeType.UserDecision, 
                                            userDecisionShape.propertyValues["x"].value,
                                            userDecisionShape.propertyValues["y"].value);
    }

    private  addSystemDecisionAndTasks(prevId: string, data: CopyPasteData, node: SystemDecision, taskRefs: TaskRef[]) {
        const systemDecisionShape = this.createSystemDecisionShape(node);
        const systemDecisionId: string = systemDecisionShape.id.toString();
        data.shapes[systemDecisionId] = systemDecisionShape;

        const systemTasks = node.getNextNodes();
        const systemTaskIds: string[] = []; 
        _.each(systemTasks, (systemTask: SystemTask) => {

            const link: IProcessLink = this.processGraph.getLink(systemDecisionShape.id, systemTask.id);
            if (!taskRefs[systemDecisionId]) {
                taskRefs[systemDecisionId] = new TaskRef(systemDecisionId, systemTask.model.id.toString(), link.label, link.orderindex);
            } else {
                taskRefs[systemDecisionId].branches.push(new Branch(systemTask.model.id.toString(), link.label, link.orderindex));
            }

            systemTaskIds.push(systemTask.id);
            const nextId = this.addSystemTask(systemDecisionId, data, systemTask);
        });

        data.addCopyPasteNode(systemDecisionId, prevId, systemTaskIds, 
                                            CopyPasteNodeType.SystemDecision, 
                                            systemDecisionShape.propertyValues["x"].value,
                                            systemDecisionShape.propertyValues["y"].value);
    }

    addSystemTask(prevId: string, data: CopyPasteData, node: SystemTask): string {
        const systemTaskShape = this.createSystemTask(node);
        const nextId = this.processGraph.viewModel.getNextShapeIds(systemTaskShape.id)[0].toString();
        const systemTaskId = systemTaskShape.id.toString();
        data.addCopyPasteNode(systemTaskId, prevId, [nextId], 
                                            CopyPasteNodeType.SystemTask, 
                                            systemTaskShape.propertyValues["x"].value,
                                            systemTaskShape.propertyValues["y"].value);
        data.shapes[systemTaskId] = systemTaskShape;

        return nextId;
    }

    private  addUserAndSystemTasks(prevId: string, data: CopyPasteData, node: UserTask, taskRefs: TaskRef[]) {
        const userTaskShape = this.createUserTaskShape(node);
        const systemTasks = node.getNextSystemTasks(this.processGraph);

        const userTaskId: string = userTaskShape.id.toString();
        let systemTaskId: string = systemTasks[0].model.id.toString();

        data.shapes[userTaskId] = userTaskShape;
        data.addCopyPasteNode(userTaskId, prevId, [systemTaskId], 
                                            CopyPasteNodeType.UserTask, 
                                            userTaskShape.propertyValues["x"].value,
                                            userTaskShape.propertyValues["y"].value);

        if (systemTasks.length === 1) {
            const nextId = this.addSystemTask(userTaskId, data, <SystemTask>systemTasks[0]);
            const nextNode = data.copyPasteTree[nextId];
            if (!!nextNode) { // there is next selected user task or user decision 
                if (nextNode.type === CopyPasteNodeType.UserTask) {
                    const nextUserTask = <UserTask>this.layout.getNodeById(nextId);
                    this.addUserAndSystemTasks(systemTaskId, data, nextUserTask, taskRefs);
                } else if (nextNode.type === CopyPasteNodeType.UserDecision) {
                    const nextUserDecision = <UserDecision>this.layout.getNodeById(nextId);
                    this.addUserDecisionAndTasks(systemTaskId, data, nextUserDecision, taskRefs);
                }
            } else {
                ;
            }
        } else { // add system decision + system tasks
            const systemDecision = <SystemDecision>node.getNextNodes()[0];
            this.addSystemDecisionAndTasks(userTaskId, data, systemDecision, taskRefs);
        }
    }

    private createUserDecisionShape(node: UserDecision): IProcessShape {
        const userDecision = this.shapesFactoryService.createModelUserDecisionShape(-1, -1, node.model.id, -1, -1);
        // COPY UT PROPERTIES - Can add more here if needed. It can be extracted into a method  
        userDecision.name = node.model.name;
        userDecision.id =  node.model.id;
        userDecision.personaReference = _.cloneDeep(node.model.personaReference); 
        userDecision.propertyValues = _.cloneDeep(node.model.propertyValues);

        return userDecision; 
    } 

    private createSystemDecisionShape(node: SystemDecision): IProcessShape {
        const systemDecision = this.shapesFactoryService.createModelUserDecisionShape(-1, -1, node.model.id, -1, -1);
        // COPY UT PROPERTIES - Can add more here if needed. It can be extracted into a method  
        systemDecision.name = node.model.name;
        systemDecision.id =  node.model.id;
        systemDecision.personaReference = _.cloneDeep(node.model.personaReference); 
        systemDecision.propertyValues = _.cloneDeep(node.model.propertyValues);

        return systemDecision; 
    } 

    private createUserTaskShape(node: UserTask): IUserTaskShape {
        const userTaskShape = this.shapesFactoryService.createModelUserTaskShape(-1, -1,  node.model.id, -1, -1);
        // COPY UT PROPERTIES - Can add more here if needed. It can be extracted into a method  
        userTaskShape.name = node.model.name;
        userTaskShape.id =  node.model.id;
        userTaskShape.personaReference = _.cloneDeep(node.model.personaReference); 
        userTaskShape.propertyValues = _.cloneDeep(node.model.propertyValues);

        return userTaskShape; 
    } 

    private createSystemTask(node: SystemTask ): ISystemTaskShape {
        const systemTaskShape = this.shapesFactoryService.createModelSystemTaskShape(-1, -1,  node.model.id, -1, -1);
        // COPY ST PROPERTIES - Can add more here if needed. It can be extracted into a method  
        systemTaskShape.name = node.model.name; 
        systemTaskShape.personaReference = _.cloneDeep(node.personaReference); 
        systemTaskShape.propertyValues = _.cloneDeep(node.model.propertyValues);

        return systemTaskShape; 
    }

    public insertSelectedShapes(edge: MxCell): void {
        let idMap: number[] = [];

        if (!this.clipboard) {
            throw new Error("Clipboard does not exist");
        }
        
        const processClipboardData = <ProcessClipboardData>this.clipboard.getData();

        if (!processClipboardData) {
            throw new Error("Clipboard is empty."); 
        }

        if (processClipboardData.type !== ClipboardDataType.Process) {
            throw new Error("Clipboard data has wrong type."); 
        }

        if (this.layout.viewModel.isWithinShapeLimit(processClipboardData.data.length)) {
            let sourcesAndDestinations = this.layout.getSourcesAndDestinations(edge);
            let sourceIds = sourcesAndDestinations.sourceIds;
            let destinationId = sourcesAndDestinations.destinationIds[0];
            
            const data = <CopyPasteData>processClipboardData.data;

            for (let index in data.treeIndex) {
               
                const copyPasteNode = (<CopyPasteNode>data.copyPasteTree[data.treeIndex[index]]); 
                if (copyPasteNode.isProcessed) {
                    continue;
                } else {
                    copyPasteNode.isProcessed = true;
                }

                const node = data.shapes[data.treeIndex[index]];
                this.addShape(idMap, copyPasteNode, sourceIds, destinationId, node, data);

            }

            this.layout.viewModel.communicationManager.processDiagramCommunication.modelUpdate(null); //sourceIds[0]);
        }
    }

    private addShape(idMap, copyPasteNode: CopyPasteNode, sourceIds, destinationId, node, data: CopyPasteData) {
        if (copyPasteNode.type === CopyPasteNodeType.UserTask) {
            const userTaskId = ProcessAddHelper.insertClonedUserTaskInternal(this.layout, this.shapesFactoryService, <any>node);

            idMap[copyPasteNode.id] = userTaskId;
            
            if (sourceIds.length > 1) {
                this.layout.updateBranchDestinationId(destinationId, userTaskId);
            }
            
            // update links
            for (let id of sourceIds) {
                this.layout.updateLink(id, destinationId, userTaskId);
            }

        } else if (copyPasteNode.type === CopyPasteNodeType.SystemTask) {
            const systemTaskId = ProcessAddHelper.insertClonedSystemTaskInternal(this.layout, this.shapesFactoryService, <any>node);

            idMap[copyPasteNode.id] = systemTaskId;
            const userTaskId = idMap[copyPasteNode.prevId];

            ProcessAddHelper.addLinkInfo(userTaskId, systemTaskId, this.layout);
            ProcessAddHelper.addLinkInfo(systemTaskId, destinationId, this.layout);

            sourceIds = [];
            sourceIds[0] = systemTaskId;

        } else if (copyPasteNode.type === CopyPasteNodeType.UserDecision) {
            const userDecisionId = ProcessAddHelper.insertClonedUserDecisionInternal(sourceIds, destinationId, 
                                                                                this.layout, this.shapesFactoryService, <any>node);
            idMap[copyPasteNode.id] = userDecisionId;
            
            let link: CopyPasteLink = data.links[copyPasteNode.id + ";" + copyPasteNode.nextIds[0]];
            ProcessAddHelper.addLinkInfo(userDecisionId, destinationId, this.layout, link.orderindex, link.label);

            sourceIds = [];
            sourceIds[0] = userDecisionId;

        } else if (copyPasteNode.type === CopyPasteNodeType.SystemDecision) {
             //const systemDecisionId = ProcessAddHelper.insertClonedSystemDecision(this.layout, this.shapesFactoryService, <any>node);
             //idMap[copyPasteNode.id] = systemDecisionId;
            
            // ProcessAddHelper.addLinkInfo(userTaskId, systemTaskId, this.layout);
            // ProcessAddHelper.addLinkInfo(systemTaskId, destinationId, this.layout);

            // sourceIds = [];
            // sourceIds[0] = systemDecisionId;
        }

        for (let nextId of copyPasteNode.nextIds) {
            if (!!data.shapes[nextId]) {
                data.copyPasteTree[nextId].isProcessed = true;
                this.addShape(idMap, data.copyPasteTree[nextId], sourceIds, destinationId, data.shapes[nextId], data);
            }
        }
    }
}
