import {
    IDiagramNode, IProcessShape,
    NodeChange, ProcessShapeType, IProcessLink,
    ILayout, ProcessClipboardData,
    UserTaskShapeModel, SystemTaskShapeModel,
    IProcessGraph, NodeType
} from "./models/";
import {IProcessLinkModel, ProcessLinkModel, IUserTaskShape, ISystemTaskShape} from "../../../../models/process-models";
import {ShapesFactory} from "./shapes/shapes-factory";
import {ProcessAddHelper} from "./process-add-helper";
import {DiagramNode, SystemTask, UserTask, UserDecision, SystemDecision} from "./shapes/";
import {IClipboardService, ClipboardDataType} from "../../../../services/clipboard.svc";
import {ProcessModel, IProcess, ItemTypePredefined} from "../../../../models/process-models";
import {IMessageService} from "../../../../../../core/messages/message.svc";
import {Models} from "../../../../../../main";

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
        this.shapes = {};
        this.links = {};
        this.preprocessorTree = {};
        this.treeIndex = [];
        this.numberOfSubTrees = 0;
    }
    shapes: Models.IHashMap<IProcessShape>;
    links: Models.IHashMap<PreprocessorLink>;
    preprocessorTree: Models.IHashMap<PreprocessorNode>;
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
        this.endPointId = "UNDEFINED";
    }
    taskId: string;
    label: string;
    orderindex: number;
    endPointId: string;
}

// This is used for the user and system decisions with branches
class DecisionPointRef {
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
    private readonly treeStartId = "-99999";
    private readonly treeEndId = "-100000";

    constructor(private processGraph: IProcessGraph, 
                     private clipboard: IClipboardService, 
                     private shapesFactoryService: ShapesFactory,
                     private messageService: IMessageService,
                     private $log: ng.ILogService) {
        this.layout = processGraph.layout;
    }

    public copySelectedShapes(): void {
        if (!this.clipboard) {
            throw new Error("Clipboard does not exist");
        }
        
        const  data: PreprocessorData = new PreprocessorData();
        // baseNodes is a collection of the nodes which goes into clipboard process data
        let  baseNodes = this.processGraph.getMxGraph().getSelectionCells();

        try {
            // 1. Find all User Decisions
            let decisionPointRefs: Models.IHashMap<DecisionPointRef> = {};

            this.findUserDecisions(baseNodes, decisionPointRefs);
                    
            // 2. add user decisions to base nodes

            this.addUserDecisionsToBasenodes(baseNodes, decisionPointRefs);

            // 4. sort base nodes
            baseNodes = _.sortBy(baseNodes, (node: IDiagramNode) => node.model.propertyValues["x"].value * 1000 + 
                                                                                                      node.model.propertyValues["y"].value);

            data.numberOfSubTrees = -1;

            // 5. add user tasks, system tasks and user decisions to clipboard process data
            this.addTasksAndDecisionsToClipboardData(data, baseNodes, decisionPointRefs);

            // 6. connect all subtrees together
            this.connectAllSubtrees(data);

            // 7. add branch links
            this.addBranchLinks(data, decisionPointRefs);

            const processClipboardData = new ProcessClipboardData(this.createProcessModel(data, decisionPointRefs));

            // 8. add logic to determine is pastable
            processClipboardData.isPastableAfterUserDecision = this.isPastableAfterUserDecision(data);                        

            // 9. set clipboard data
            this.clipboard.setData(processClipboardData);

        } catch (error) {
            this.messageService.addError(error);
            this.$log.error(error);
        }
    };

    private findUserDecisions(baseNodes: any, decisionPointRefs: Models.IHashMap<DecisionPointRef>) {
        _.each(baseNodes, (node) => {
            if (node instanceof UserTask) {
                const previousShapeIds: number[] = this.processGraph.viewModel.getPrevShapeIds(node.model.id);
                const shape = this.processGraph.viewModel.getShapeById(previousShapeIds[0]);

                if (shape.propertyValues["clientType"].value === NodeType.UserDecision) {
                    const link: IProcessLink = this.processGraph.getLink(shape.id, node.id);

                    const userDecisionPointRef = decisionPointRefs[shape.id.toString()];
                    if (userDecisionPointRef) {
                        userDecisionPointRef.branches.push(new Branch(node.id, link.label, link.orderindex));
                    } else {
                        decisionPointRefs[shape.id.toString()] = new DecisionPointRef(shape.id.toString(), node.id, link.label, link.orderindex);
                    } 
                }
            }
        });
    }    

    private addUserDecisionsToBasenodes(baseNodes: any, decisionPointRefs: Models.IHashMap<DecisionPointRef>) {
        _.forOwn(decisionPointRefs, (node) => {
            if (!!node && node.branches.length > 1) {
                // sort branches by orderindex 
                node.branches = _.sortBy(node.branches, (branch: any) => branch.orderindex);
                baseNodes.push(this.layout.getNodeById(node.decisionId));
            }
        });        
    }

    private addTasksAndDecisionsToClipboardData(data: PreprocessorData, baseNodes, 
                                                decisionPointRefs: Models.IHashMap<DecisionPointRef>) {
        let prevId = "UNDEFINED";
        _.each(baseNodes, (node) => {
            if (node instanceof UserTask) {
                this.addUserAndSystemTasks(prevId, data, baseNodes, node, decisionPointRefs, ++data.numberOfSubTrees);
            } else if (node instanceof UserDecision) { // user decision
                this.addUserDecisionAndTasks(prevId, data, baseNodes, node, decisionPointRefs, ++data.numberOfSubTrees);
            } else {
                throw new Error("Unsupported copy/paste type");
            }
        });
        
        data.sortTree();
    }

    private connectAllSubtrees(data: PreprocessorData) {
        let connectionNodeId = this.treeEndId;  
        for (let i = data.treeIndex.length - 1; i >= 0; i--) {
            const preprocessorNode: PreprocessorNode = data.preprocessorTree[data.treeIndex[i]];
            if (i === data.treeIndex.length - 1) {
                if (!data.preprocessorTree[preprocessorNode.nextIds[0]]) {
                    preprocessorNode.nextIds[0] = connectionNodeId;
                }
            } else if (i === 0) {
                preprocessorNode.prevId = this.treeStartId;
            } else  if (!data.preprocessorTree[preprocessorNode.prevId]) {
                    connectionNodeId = preprocessorNode.id;
            } else if (!data.preprocessorTree[preprocessorNode.nextIds[0]]) {
                preprocessorNode.nextIds[0] = connectionNodeId;
            }
        }       
    }

    private addBranchLinks(data: PreprocessorData, decisionPointRefs: Models.IHashMap<DecisionPointRef>) {
        _.forOwn(decisionPointRefs, (node) => {
            if (!!node && node.branches.length > 1) {
                for (let branch of node.branches) {
                    // add link
                    let link = new PreprocessorLink(branch.label, branch.orderindex);
                    data.links[node.decisionId + ";" + branch.taskId] = link;
                }

                // find end points for the DP branches
                // 1. build search string
                let preprocessorNode: PreprocessorNode = data.preprocessorTree[node.branches[0].taskId];
                let searchString = "*" + node.decisionId + "*";
                while (!!preprocessorNode) {
                    searchString += "*" + preprocessorNode.id + "*" + preprocessorNode.nextIds[0] + "*";
                    preprocessorNode = data.preprocessorTree[preprocessorNode.nextIds[0]];
                }

                //2. find match in the search string when traversing through the second branch
                for (let i = 1; i < node.branches.length; i++) { 
                    preprocessorNode = data.preprocessorTree[node.branches[i].taskId];
                    while (!!preprocessorNode) {
                        if (searchString.indexOf("*" + preprocessorNode.nextIds[0] + "*") > -1) {
                            node.branches[i].endPointId = preprocessorNode.nextIds[0];
                            if (preprocessorNode.type === PreprocessorNodeType.UserTask) {
                                node.branches[i].endPointId = preprocessorNode.id;
                            }
                            break;
                        }
                        preprocessorNode = data.preprocessorTree[preprocessorNode.nextIds[0]];
                    }
                }
            }
        });        
    }

    private isPastableAfterUserDecision(data: PreprocessorData): boolean {
        return (<PreprocessorNode>data.preprocessorTree[data.treeIndex[0]]).type === PreprocessorNodeType.UserTask;
    }

    private  addUserDecisionAndTasks(prevId: string, data: PreprocessorData, baseNodes, 
                                                      node: UserDecision, decisionPointRefs: Models.IHashMap<DecisionPointRef>, subTreeId: number) {

        if (!!data.preprocessorTree[node.id]) {
            return;
        }
                                                          
        const userDecisionShape = this.createUserDecisionShape(node);
        const userDecisionId: string = userDecisionShape.id.toString();
        data.shapes[userDecisionId] = userDecisionShape;

        const userTasks: string[] = []; 
        _.each(decisionPointRefs[userDecisionId].branches, (branch: Branch) => {
            let userTask = this.processGraph.getNodeById(branch.taskId);
            userTasks.push(branch.taskId);
            this.addUserAndSystemTasks(userDecisionId, data, baseNodes, <UserTask>userTask, decisionPointRefs, subTreeId);
        });

        if (!!data.preprocessorTree[node.id]) {
            return;
        }

        data.addPreprocessorNode(userDecisionId, prevId, userTasks, 
                                            PreprocessorNodeType.UserDecision, 
                                            userDecisionShape.propertyValues["x"].value,
                                            userDecisionShape.propertyValues["y"].value,
                                            subTreeId);
    }      

    private  addSystemDecisionAndTasks(prevId: string, data: PreprocessorData, node: SystemDecision, 
                                        decisionPointRefs: Models.IHashMap<DecisionPointRef>, subTreeId: number, baseNode) {
        const systemDecisionShape = this.createSystemDecisionShape(node);
        const systemDecisionId: string = systemDecisionShape.id.toString();
        let nextId = "";

        data.shapes[systemDecisionId] = systemDecisionShape;

        const shapes = node.getNextNodes();
        const shapeIds: string[] = []; 
        _.each(shapes, (shape) => {

            const link: IProcessLink = this.processGraph.getLink(systemDecisionShape.id, shape.model.id);
            if (!decisionPointRefs[systemDecisionId]) {
                decisionPointRefs[systemDecisionId] = new DecisionPointRef(systemDecisionId, shape.model.id.toString(), link.label, link.orderindex);
            } else {
                decisionPointRefs[systemDecisionId].branches.push(new Branch(shape.model.id.toString(), link.label, link.orderindex));
            }

            shapeIds.push(shape.model.id.toString());
            if (shape instanceof  SystemTask) {
                this.addSystemTask(systemDecisionId, data, shape, subTreeId, baseNode, decisionPointRefs);
            } else {
                this.addSystemDecisionAndTasks(systemDecisionId, data, <SystemDecision>shape, decisionPointRefs, subTreeId, baseNode);
            }
        });

        data.addPreprocessorNode(systemDecisionId, prevId, shapeIds, 
                                            PreprocessorNodeType.SystemDecision, 
                                            systemDecisionShape.propertyValues["x"].value,
                                            systemDecisionShape.propertyValues["y"].value,
                                            subTreeId);
    }    

    private addSystemTask(prevId: string, data: PreprocessorData, node: SystemTask, subTreeId: number, 
                                baseNodes, decisionPointRefs: Models.IHashMap<DecisionPointRef>) {
        const systemTaskShape = this.createSystemTask(node);
        let nextId = this.processGraph.viewModel.getNextShapeIds(systemTaskShape.id)[0].toString();
        if (this.processGraph.viewModel.getPrevShapeIds(_.toNumber(nextId)).length > 1) {
            nextId = this.treeEndId;
        }

        const systemTaskId = systemTaskShape.id.toString();
        data.addPreprocessorNode(systemTaskId, prevId, [nextId], 
                                            PreprocessorNodeType.SystemTask, 
                                            systemTaskShape.propertyValues["x"].value,
                                            systemTaskShape.propertyValues["y"].value,
                                            subTreeId);
        data.shapes[systemTaskId] = systemTaskShape;

        if (nextId !== this.treeEndId) {
            this.addNextNode(baseNodes, prevId, nextId, data, decisionPointRefs, subTreeId);
        }
    }

    private  addUserAndSystemTasks(prevId: string, data: PreprocessorData, baseNodes, node: UserTask, 
                                                    decisionPointRefs: Models.IHashMap<DecisionPointRef>, subTreeId: number) {

        if (!!data.preprocessorTree[node.id]) {
            return;
        }
                                                        
        const userTaskShape = this.createUserTaskShape(node);
        const systemTasks = node.getNextSystemTasks(this.processGraph);

        const userTaskId: string = userTaskShape.id.toString();
        let systemTaskId: string = systemTasks[0].model.id.toString();

        data.shapes[userTaskId] = userTaskShape;

        if (systemTasks.length === 1) {
            data.addPreprocessorNode(userTaskId, prevId, [systemTaskId], 
                                                PreprocessorNodeType.UserTask, 
                                                userTaskShape.propertyValues["x"].value,
                                                userTaskShape.propertyValues["y"].value,
                                                subTreeId);
            this.addSystemTask(userTaskId, data, <SystemTask>systemTasks[0], subTreeId, baseNodes, decisionPointRefs);
        } else { // add system decision + system tasks
            const systemDecision = <SystemDecision>node.getNextNodes()[0];
            data.addPreprocessorNode(userTaskId, prevId, [systemDecision.model.id.toString()], 
                                                PreprocessorNodeType.UserTask, 
                                                userTaskShape.propertyValues["x"].value,
                                                userTaskShape.propertyValues["y"].value,
                                                subTreeId);
            this.addSystemDecisionAndTasks(userTaskId, data, systemDecision, decisionPointRefs, subTreeId, baseNodes);
        }
    }

    private addNextNode(baseNodes, 
                                  nodeId: string, 
                                  nextId: string, 
                                  data: PreprocessorData, 
                                  decisionPointRefs: Models.IHashMap<DecisionPointRef>, 
                                  subTreeId: number) {
        const nextNode = _.find(baseNodes, (node: IDiagramNode) =>  { return node.model.id.toString() === nextId; }); //data.preprocessorTree[nextId];
        
        if (!!nextNode && !data.preprocessorTree[nextId]) { // there is next selected user task or user decision 
            if (nextNode instanceof UserTask) {
                //const nextUserTask = <UserTask>this.layout.getNodeById(nextId);
                this.addUserAndSystemTasks(nodeId, data, baseNodes, nextNode, decisionPointRefs, subTreeId);
            } else if (nextNode instanceof  UserDecision) {
                //const nextUserDecision = <UserDecision>this.layout.getNodeById(nextId);
                this.addUserDecisionAndTasks(nodeId, data, baseNodes, nextNode, decisionPointRefs, subTreeId);
            } else if (nextNode instanceof  SystemDecision) {
                //const nextUserDecision = <UserDecision>this.layout.getNodeById(nextId);
                this.addSystemDecisionAndTasks(nodeId, data, nextNode, decisionPointRefs, subTreeId, baseNodes);
            }
        } else {
            return;
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
                userTaskShape.associatedArtifact = _.cloneDeep(node.model.associatedArtifact); 
                userTaskShape.propertyValues = _.cloneDeep(node.model.propertyValues); 
        
        return userTaskShape; 
    } 
                
    private createSystemTask(node: SystemTask ): ISystemTaskShape {
        const systemTaskShape = this.shapesFactoryService.createModelSystemTaskShape(-1, -1,  node.model.id, -1, -1);
                // COPY ST PROPERTIES - Can add more here if needed. It can be extracted into a method  
        systemTaskShape.name = node.model.name; 
        systemTaskShape.personaReference = _.cloneDeep(node.personaReference); 
        systemTaskShape.associatedArtifact = _.cloneDeep(node.associatedArtifact);
        systemTaskShape.propertyValues = _.cloneDeep(node.model.propertyValues);

        return systemTaskShape; 
    }

    private createProcessModel(data: PreprocessorData, decisionPointRefs: Models.IHashMap<DecisionPointRef>): IProcess {
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
        _.forOwn(decisionPointRefs, (node: DecisionPointRef) => {
            if (!!node && node.branches.length > 1) {
                for (let i = 1; i < node.branches.length; i++) {
                    let branch = node.branches[i];
                    procModel.decisionBranchDestinationLinks.push(new ProcessLinkModel(-1, _.toNumber(node.decisionId), 
                                                                                                    _.toNumber(branch.endPointId), branch.orderindex));
                }
            }
        });        

        return procModel;
    }

    public insertSelectedShapes(sourceIds: number[], destinationId: number): void {
        let idMap = {};

        if (!this.clipboard) {
            throw new Error("Clipboard does not exist");
        }
        
        const processClipboardData =  _.cloneDeep(<ProcessClipboardData>this.clipboard.getData());

        if (!processClipboardData) {
            throw new Error("Clipboard is empty."); 
        }

        if (processClipboardData.getType() !== ClipboardDataType.Process) {
            throw new Error("Clipboard data has wrong type."); 
        }

        const data = <IProcess>processClipboardData.getData();
        let connectionStartId = null;
        if (this.layout.viewModel.isWithinShapeLimit(data.shapes.length)) {
            
            // 1. create idMap and update shape ids and projectId and insert shapes
            this.pasteAndUpdateShapes(data, idMap);

            // 2. connect the original graph to link to the start of pasted model
            connectionStartId = data.shapes[0].id;
            this.connectToPastedShapesStart(connectionStartId, data, destinationId, sourceIds);

            // 3. update original branch destination ids
            this.updateOriginalDecisionBranchDestinationLinks(destinationId, connectionStartId, sourceIds);

            // 4. insert links with new shapes id
            this.pasteAndUpdateLinks(data, idMap, destinationId);

            // 5. insert new branch destination ids
            this.pasteAndIpdateDecisionBranchDestinationLinks(data, idMap, destinationId);

        }

        this.layout.viewModel.communicationManager.processDiagramCommunication.modelUpdate(_.toNumber(connectionStartId)); 
    }    

    private pasteAndUpdateShapes(data: IProcess, idMap: any): void {
    for (let shape of data.shapes) {
            const newId = this.layout.getTempShapeId() - 1;
            this.layout.setTempShapeId(newId);
            idMap[shape.id.toString()] = newId;
            shape.id = newId;
            shape.propertyValues["x"].value = -1;
            shape.propertyValues["y"].value = -1;
            shape.projectId = this.layout.viewModel.projectId;
            shape.parentId = this.layout.viewModel.id;
            this.layout.viewModel.addShape(shape);
        }
    }

    private connectToPastedShapesStart(connectionStartId: number, data: IProcess, destinationId: number, sourceIds: number[]) {
        let links = _.filter(this.layout.viewModel.links, (link) => { 
            return destinationId === link.destinationId && 
                        _.filter(sourceIds, (sourceId) => { return sourceId === link.sourceId; }).length  > 0; 
        });
        _.each(links, (link) => {
            link.destinationId = connectionStartId;
        });
    }

    private updateOriginalDecisionBranchDestinationLinks(destinationId: number, connectionStartId: number, sourceIds: number[]) {            
        const decisionBranchDestinationLinks = _.filter(this.layout.viewModel.decisionBranchDestinationLinks, (link) => { 
            return  link.destinationId === destinationId && sourceIds.length > 1; 
        });

        _.each(decisionBranchDestinationLinks, (link) => {
            link.destinationId = connectionStartId;
        });
    }

    private pasteAndUpdateLinks(data: IProcess, idMap: any, destinationId: number) {            
        for (let link of data.links) {
            link.sourceId = idMap[link.sourceId.toString()];
            if (link.destinationId.toString() === this.treeEndId) {
                link.destinationId = destinationId;
            } else {
                link.destinationId = idMap[link.destinationId.toString()];
            }
            this.layout.viewModel.links.push(new ProcessLinkModel(this.layout.viewModel.id, 
                                                                                            link.sourceId, 
                                                                                            link.destinationId, 
                                                                                            link.orderindex, 
                                                                                            link.label));
        }
    }

    private pasteAndIpdateDecisionBranchDestinationLinks(data: IProcess, idMap: any, destinationId: number) {
        for (let link of data.decisionBranchDestinationLinks) {
            link.sourceId = idMap[link.sourceId.toString()]; 
            if (link.destinationId.toString() === this.treeEndId) {
                link.destinationId = destinationId;
            } else {
                link.destinationId = idMap[link.destinationId.toString()];
            }
            if (!this.layout.viewModel.decisionBranchDestinationLinks) {
                this.layout.viewModel.decisionBranchDestinationLinks = [];
            }
            this.layout.viewModel.decisionBranchDestinationLinks.push(link); 
        }
    }
}