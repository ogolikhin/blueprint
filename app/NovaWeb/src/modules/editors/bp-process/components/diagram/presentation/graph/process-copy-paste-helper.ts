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
import {ProcessModel, IProcess, ItemTypePredefined} from "../../../../models/process-models";

enum PreprocessorNodeType {
    UserTask,
    UserDecision,
    SystemTask,
    SystemDecision
}

class PreprocessorNode {
    constructor(id: string, prevId: string, nextIds: string[], type: PreprocessorNodeType, x: number, y: number, subTreeId: number) {
        this.id = id;
        this.prevId = prevId;
        this.nextIds = nextIds; 
        this.type = type;
        this.isProcessed = false;
        this.x = x;
        this.y = y;
        this.subTreeId = subTreeId;
    }
    id: string;
    prevId: string;
    nextIds: string[]; 
    type: PreprocessorNodeType;
    x: number;
    y: number;
    isProcessed: boolean;
    subTreeId: number;
}

class PreprocessorLink {
    constructor(label: string, orderindex: number) {
        this.label = label;
        this.orderindex = orderindex;
    }
    label: string;
    orderindex: number;
}

class PreprocessorData {
    constructor() {
        this.shapes = [];
        this.links = [];
        this.preprocessorTree = [];
        this.treeIndex = [];
        this.numberOfSubTrees = 0;
    }
    shapes: IProcessShape[];
    links: PreprocessorLink[];
    preprocessorTree: PreprocessorNode[];
    treeIndex: string[];
    startId: number;
    numberOfSubTrees: number;

    public addPreprocessorNode(id: string, prevId: string, nextIds: string[], type: PreprocessorNodeType, x: number, y: number, subTreeId: number) {
        this.preprocessorTree[id] = new PreprocessorNode(id, prevId, nextIds, type, x, y, subTreeId);
        this.treeIndex.push(id);
    }

    public sortTree() {
        this.treeIndex = _.sortBy(this.treeIndex, (id: string) =>  (this.preprocessorTree[id].subTreeId + 1) * 1000000 +
                                                                                            this.preprocessorTree[id].x * 1000 + this.preprocessorTree[id].y);
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
        this.endPointId = "UNDEFINED";
    }
    decisionId: string; 
    endPointId: string;
    branches: Branch[]; 
}

export class ProcessCopyPasteHelper {

    private layout: ILayout;
    public static readonly treeStartId = "-99999";
    public static readonly treeEndId = "-100000";

    constructor(private processGraph: IProcessGraph, private clipboard: IClipboardService, private shapesFactoryService: ShapesFactory) {
        this.layout = processGraph.layout;
    }

    public copySelectedShapes(): void {
        
        if (!this.clipboard) {
            throw new Error("Clipboard does not exist");
        }
        
        let data: PreprocessorData = new PreprocessorData();
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
        baseNodes = _.sortBy(baseNodes, (node: IDiagramNode) => node.model.propertyValues["x"].value * 1000 + node.model.propertyValues["y"].value);

        let prevId = "0";
        data.numberOfSubTrees = -1;
        _.each(baseNodes, (node) => {
            // skip processed nodes
            if (!data.preprocessorTree[(<IDiagramNode>node).model.id]) {
                if (node instanceof UserTask) {
                    this.addUserAndSystemTasks(prevId, data, baseNodes, node, taskRefs, ++data.numberOfSubTrees);
                } else if (node instanceof UserDecision) { // user decision
                    this.addUserDecisionAndTasks(prevId, data, baseNodes, node, taskRefs, ++data.numberOfSubTrees);
                } else {
                    throw new Error("Unsupported copy/paste type");
                }
            } 
        });

        data.sortTree();

        // glue subtrees
        let glueId = ProcessCopyPasteHelper.treeEndId; 
        for (let i = data.treeIndex.length - 1; i >= 0; i--) {
            const preprocessorNode: PreprocessorNode = data.preprocessorTree[data.treeIndex[i]];
            if (i === data.treeIndex.length - 1) {
                preprocessorNode.nextIds[0] = glueId;
            } else if (i === 0) {
                preprocessorNode.prevId = ProcessCopyPasteHelper.treeStartId;
            } else  if (!data.preprocessorTree[preprocessorNode.prevId]) {
                    glueId = preprocessorNode.id;
            } else if (!data.preprocessorTree[preprocessorNode.nextIds[0]]) {
                preprocessorNode.nextIds[0] = glueId;
            }
        }        

        // find end points for the DP branches and add branches links
        _.forOwn(taskRefs, (node: TaskRef) => {
            if (!!node && node.branches.length > 1) {
                for (let branch of node.branches) {
                    // add link
                    let link = new PreprocessorLink(branch.label, branch.orderindex);
                    data.links[node.decisionId + ";" + branch.taskId] = link;
                }

                // find end points for the DP branches
                // 1. build search string
                let preprocessorNode: PreprocessorNode = data.preprocessorTree[node.branches[0].taskId];
                let searchString: string = "";
                while (!!preprocessorNode) {
                    searchString +=  "*" + preprocessorNode.nextIds[0] + "*";
                    preprocessorNode = data.preprocessorTree[preprocessorNode.nextIds[0]];
                }

                //2. find match in the search string when traversing through the second branch 
                preprocessorNode = data.preprocessorTree[node.branches[1].taskId];
                while (!!preprocessorNode) {
                    if (searchString.indexOf("*" + preprocessorNode.nextIds[0] + "*") > -1) {
                        node.endPointId = preprocessorNode.nextIds[0];
                        break;
                    }
                    preprocessorNode = data.preprocessorTree[preprocessorNode.nextIds[0]];
                }
            }
        });        

        this.clipboard.setData(new ProcessClipboardData(this.createProcessModel(data, taskRefs)));
    };

    private  addUserDecisionAndTasks(prevId: string, data: PreprocessorData, baseNodes, node: UserDecision, taskRefs: TaskRef[], subTreeId: number) {
        const userDecisionShape = this.createUserDecisionShape(node);
        const userDecisionId: string = userDecisionShape.id.toString();
        data.shapes[userDecisionId] = userDecisionShape;

        const userTasks: string[] = []; 
        _.each(taskRefs[userDecisionId].branches, (branch: Branch) => {
            let userTask = this.processGraph.getNodeById(branch.taskId);
            userTasks.push(branch.taskId);
            this.addUserAndSystemTasks(userDecisionId, data, baseNodes, <UserTask>userTask, taskRefs, subTreeId);
        });

        data.addPreprocessorNode(userDecisionId, prevId, userTasks, 
                                            PreprocessorNodeType.UserDecision, 
                                            userDecisionShape.propertyValues["x"].value,
                                            userDecisionShape.propertyValues["y"].value,
                                            subTreeId);
    }

    private  addSystemDecisionAndTasks(prevId: string, data: PreprocessorData, node: SystemDecision, taskRefs: TaskRef[], subTreeId: number) {
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
            const nextId = this.addSystemTask(systemDecisionId, data, systemTask, subTreeId);
        });

        data.addPreprocessorNode(systemDecisionId, prevId, systemTaskIds, 
                                            PreprocessorNodeType.SystemDecision, 
                                            systemDecisionShape.propertyValues["x"].value,
                                            systemDecisionShape.propertyValues["y"].value,
                                            subTreeId);
    }

    private addSystemTask(prevId: string, data: PreprocessorData, node: SystemTask, subTreeId: number): string {
        const systemTaskShape = this.createSystemTask(node);
        const nextId = this.processGraph.viewModel.getNextShapeIds(systemTaskShape.id)[0].toString();
        const systemTaskId = systemTaskShape.id.toString();
        data.addPreprocessorNode(systemTaskId, prevId, [nextId], 
                                            PreprocessorNodeType.SystemTask, 
                                            systemTaskShape.propertyValues["x"].value,
                                            systemTaskShape.propertyValues["y"].value,
                                            subTreeId);
        data.shapes[systemTaskId] = systemTaskShape;

        return nextId;
    }

    private  addUserAndSystemTasks(prevId: string, data: PreprocessorData, baseNodes, node: UserTask, taskRefs: TaskRef[], subTreeId: number) {
        const userTaskShape = this.createUserTaskShape(node);
        const systemTasks = node.getNextSystemTasks(this.processGraph);

        const userTaskId: string = userTaskShape.id.toString();
        let systemTaskId: string = systemTasks[0].model.id.toString();

        data.shapes[userTaskId] = userTaskShape;
        data.addPreprocessorNode(userTaskId, prevId, [systemTaskId], 
                                            PreprocessorNodeType.UserTask, 
                                            userTaskShape.propertyValues["x"].value,
                                            userTaskShape.propertyValues["y"].value,
                                            subTreeId);

        if (systemTasks.length === 1) {
            const nextId = this.addSystemTask(userTaskId, data, <SystemTask>systemTasks[0], subTreeId);
            const nextNode = _.find(baseNodes, (node: IDiagramNode) =>  { return node.model.id.toString() === nextId; }); //data.preprocessorTree[nextId];
            
            if (!!nextNode && !data.preprocessorTree[nextId]) { // there is next selected user task or user decision 
                if (nextNode instanceof UserTask) {
                    //const nextUserTask = <UserTask>this.layout.getNodeById(nextId);
                    this.addUserAndSystemTasks(systemTaskId, data, baseNodes, nextNode, taskRefs, subTreeId);
                } else if (nextNode instanceof  UserDecision) {
                    //const nextUserDecision = <UserDecision>this.layout.getNodeById(nextId);
                    this.addUserDecisionAndTasks(systemTaskId, data, baseNodes, nextNode, taskRefs, subTreeId);
                }
            } else {
                ;
            }
        } else { // add system decision + system tasks
            const systemDecision = <SystemDecision>node.getNextNodes()[0];
            this.addSystemDecisionAndTasks(userTaskId, data, systemDecision, taskRefs, subTreeId);
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

    private createProcessModel(data: PreprocessorData, taskRefs: TaskRef[]): IProcess {
        const procModel: IProcess = new ProcessModel(
            -1,
            "CP",
            "CP",
            -1,
            ItemTypePredefined.Process,
            [],
            [],
            null,
            []
        );
        
        // set process shapes and links.
        for (let index of data.treeIndex) {
            procModel.shapes.push(data.shapes[index]);

            const preprocessorNode: PreprocessorNode = data.preprocessorTree[index];
            _.each(preprocessorNode.nextIds, (id) => {
                const preprocessorLink: PreprocessorLink = data.links[preprocessorNode.id + ";" + id];
                if (!!preprocessorLink) {
                    procModel.links.push(new ProcessLinkModel(-1, _.toNumber(preprocessorNode.id), _.toNumber(id), 
                                                    preprocessorLink.orderindex, preprocessorLink.label));
                } else {
                    procModel.links.push(new ProcessLinkModel(-1, _.toNumber(preprocessorNode.id), _.toNumber(id)));
                }
            });
        }

        // set process decisionBranchDestinationLinks.
        _.forOwn(taskRefs, (node: TaskRef) => {
            if (!!node && node.branches.length > 1) {
                for (let branch of node.branches) {
                    procModel.decisionBranchDestinationLinks.push(new ProcessLinkModel(-1, _.toNumber(node.decisionId), 
                                                                                                    _.toNumber(node.endPointId), branch.orderindex));
                }
            }
        });        

        return procModel;
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
            
            const data = <PreprocessorData>processClipboardData.data;

            for (let index in data.treeIndex) {
               
                const copyPasteNode = (<PreprocessorNode>data.preprocessorTree[data.treeIndex[index]]); 
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

    private addShape(idMap, copyPasteNode: PreprocessorNode, sourceIds, destinationId, node, data: PreprocessorData) {
        if (copyPasteNode.type === PreprocessorNodeType.UserTask) {
            const userTaskId = ProcessAddHelper.insertClonedUserTaskInternal(this.layout, this.shapesFactoryService, <any>node);

            idMap[copyPasteNode.id] = userTaskId;
            
            if (sourceIds.length > 1) {
                this.layout.updateBranchDestinationId(destinationId, userTaskId);
            }
            
            // update links
            for (let id of sourceIds) {
                this.layout.updateLink(id, destinationId, userTaskId);
            }

        } else if (copyPasteNode.type === PreprocessorNodeType.SystemTask) {
            const systemTaskId = ProcessAddHelper.insertClonedSystemTaskInternal(this.layout, this.shapesFactoryService, <any>node);

            idMap[copyPasteNode.id] = systemTaskId;
            const userTaskId = idMap[copyPasteNode.prevId];

            ProcessAddHelper.addLinkInfo(userTaskId, systemTaskId, this.layout);
            ProcessAddHelper.addLinkInfo(systemTaskId, destinationId, this.layout);

            sourceIds = [];
            sourceIds[0] = systemTaskId;

        } else if (copyPasteNode.type === PreprocessorNodeType.UserDecision) {
            const userDecisionId = ProcessAddHelper.insertClonedUserDecisionInternal(sourceIds, destinationId, 
                                                                                this.layout, this.shapesFactoryService, <any>node);
            idMap[copyPasteNode.id] = userDecisionId;
            
            let link: PreprocessorLink = data.links[copyPasteNode.id + ";" + copyPasteNode.nextIds[0]];
            ProcessAddHelper.addLinkInfo(userDecisionId, destinationId, this.layout, link.orderindex, link.label);

            sourceIds = [];
            sourceIds[0] = userDecisionId;

        } else if (copyPasteNode.type === PreprocessorNodeType.SystemDecision) {
             //const systemDecisionId = ProcessAddHelper.insertClonedSystemDecision(this.layout, this.shapesFactoryService, <any>node);
             //idMap[copyPasteNode.id] = systemDecisionId;
            
            // ProcessAddHelper.addLinkInfo(userTaskId, systemTaskId, this.layout);
            // ProcessAddHelper.addLinkInfo(systemTaskId, destinationId, this.layout);

            // sourceIds = [];
            // sourceIds[0] = systemDecisionId;
        }

        for (let nextId of copyPasteNode.nextIds) {
            if (!!data.shapes[nextId]) {
                data.preprocessorTree[nextId].isProcessed = true;
                this.addShape(idMap, data.preprocessorTree[nextId], sourceIds, destinationId, data.shapes[nextId], data);
            }
        }
    }
}
