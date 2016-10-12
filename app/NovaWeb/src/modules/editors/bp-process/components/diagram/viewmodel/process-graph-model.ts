import {ItemTypePredefined} from "../../../../../main/models/enums";
import {Models} from "../../../../../main";
import {ProcessModels, ProcessEnums} from "../../../";

export interface IProcessGraphModel {

    // IProcess wrapper

    id: number;
    name: string;
    typePrefix: string;
    projectId: number;
    baseItemTypePredefined: ItemTypePredefined;
    shapes: ProcessModels.IProcessShape[];
    links: ProcessModels.IProcessLinkModel[];
    propertyValues: ProcessModels.IHashMapOfPropertyValues;
    decisionBranchDestinationLinks: ProcessModels.IProcessLink[];
    status: ProcessModels.IItemStatus;

    updateTree();
    updateTreeAndFlows();
    getTree(): Models.IHashMap<ProcessModels.TreeShapeRef>;
    getLinkIndex(sourceId: number, destinationId: number): number;
    getNextOrderIndex(id: number): number;
    getShapeById(id: number): ProcessModels.IProcessShape;
    getShapeTypeById(id: number): ProcessEnums.ProcessShapeType;
    getShapeType(shape: ProcessModels.IProcessShape): ProcessEnums.ProcessShapeType;
    getNextShapeIds(id: number): number[];
    getPrevShapeIds(id: number): number[];
    getStartShapeId(): number;
    getPreconditionShapeId(): number;
    getEndShapeId(): number;
    hasMultiplePrevShapesById(id: number): boolean;
    getFirstNonSystemShapeId(id: number): number;
    getDecisionBranchDestinationLinks(isMatch: (link: ProcessModels.IProcessLink) => boolean): ProcessModels.IProcessLink[];
    getConnectedDecisionIds(destinationId: number): number[];
    getBranchDestinationIds(decisionId: number): number[];
    getBranchDestinationId(decisionId: number, firstShapeInConditionId: number): number;
    isInSameFlow(id: number, otherId: number): boolean;
    isInChildFlow(id: number, otherId: number): boolean;
    updateDecisionDestinationId(decisionId: number, orderIndex: number, newDestinationId: number);
    isDecision(id: number): boolean;
    destroy();
}

export class ProcessGraphModel implements IProcessGraphModel {
    private process: ProcessModels.IProcess;
    private tree: Models.IHashMap<ProcessModels.TreeShapeRef>;
    private linkIndex: number[] = [];
    private shapeIdToFlow: Models.IHashMap<ProcessModels.IProcessFlow>;
    private flows: ProcessModels.IProcessFlow[];
    private startShapeId: number = null;
    private preconditionShapeId: number = null;
    private endShapeId: number = null;

    constructor(process) {
        this.process = process;
        this.createTree();
        this.createFlows();
    }

    // Note: provides an API for accessing IProcess data structure
    public get id(): number {
        return this.process.id;
    }

    public get name(): string {
        return this.process.name;
    }

    public get typePrefix(): string {
        return this.process.typePrefix;
    }

    public get projectId(): number {
        return this.process.projectId;
    }

    public get baseItemTypePredefined(): ItemTypePredefined {
        return this.process.baseItemTypePredefined;
    }

    public get shapes(): ProcessModels.IProcessShape[] {
        return this.process.shapes;
    }

    public set shapes(newValue: ProcessModels.IProcessShape[]) {
        this.process.shapes = newValue;
    }

    public get links(): ProcessModels.IProcessLinkModel[] {
        return <ProcessModels.IProcessLinkModel[]>this.process.links;
    }

    public set links(newValue: ProcessModels.IProcessLinkModel[]) {
        this.process.links = newValue;
    }

    public get propertyValues(): ProcessModels.IHashMapOfPropertyValues {
        return this.process.propertyValues;
    }

    public set propertyValues(newValue: ProcessModels.IHashMapOfPropertyValues) {
        this.process.propertyValues = newValue;
    }

    public get decisionBranchDestinationLinks(): ProcessModels.IProcessLink[] {
        return this.process.decisionBranchDestinationLinks;
    }

    public set decisionBranchDestinationLinks(newValue: ProcessModels.IProcessLink[]) {
        this.process.decisionBranchDestinationLinks = newValue;
    }

    public get status(): ProcessModels.IItemStatus {
        return this.process.status;
    }

    // tree
    private createTree() {
        this.tree = {};
        this.startShapeId = null;
        this.preconditionShapeId = null;
        this.endShapeId = null;
        this.linkIndex = [];

        for (let i in this.process.shapes) {
            const shape = this.process.shapes[i];

            if (this.startShapeId == null && this.getShapeType(shape) === ProcessEnums.ProcessShapeType.Start) {
                this.startShapeId = shape.id;
            }

            if (this.preconditionShapeId == null && this.getShapeType(shape) === ProcessEnums.ProcessShapeType.PreconditionSystemTask) {
                this.preconditionShapeId = shape.id;
            }

            if (this.endShapeId == null && this.getShapeType(shape) === ProcessEnums.ProcessShapeType.End) {
                this.endShapeId = shape.id;
            }

            // add shape reference to the tree
            const shapeRef = new ProcessModels.TreeShapeRef();
            shapeRef.index = Number(i);
            this.tree[shape.id.toString()] = shapeRef;
        }

        for (let i in this.process.links) {
            const link = this.process.links[i];
            this.linkIndex[link.sourceId.toString() + ";" + link.destinationId.toString()] = i;
            this.tree[link.sourceId.toString()].nextShapeIds.push(link.destinationId);
            this.tree[link.destinationId.toString()].prevShapeIds.push(link.sourceId);
        }
    }

    private isConditionEnd(id: number, conditionEndIds: number[]): boolean {
        if (conditionEndIds.length === 0) {
            return false;
        }

        return id === conditionEndIds[conditionEndIds.length - 1];
    }

    private createFlows(): void {
        this.flows = [];
        this.shapeIdToFlow = {};
        this.createFlow();
    }

    private createFlow(id: number = this.startShapeId,
                       previousId: number = null,
                       flow: ProcessModels.IProcessFlow = null,
                       conditionEndIds: number[] = []) {
        if (id == null) {
            return;
        }

        let shapeRef: ProcessModels.TreeShapeRef = this.tree[id.toString()];
        if (!shapeRef) {
            throw new Error(`Create flows: Shape with id ${id} doesn't exist in the shapes tree.`);
        }

        let shape: ProcessModels.IProcessShape = this.shapes[shapeRef.index];
        if (!shape) {
            throw new Error(`Create flows: Shape with id ${id} doesn't exist in the list of process shapes.`);
        }

        if (flow == null) {
            flow = new ProcessModels.ProcessFlowModel();
            flow.startShapeId = id;
        }

        if (this.isConditionEnd(id, conditionEndIds)) {
            this.flows.push(flow);
            return;
        }

        this.shapeIdToFlow[id.toString()] = flow;
        flow.shapes[id] = shape;

        if (shapeRef.nextShapeIds.length === 0) {
            this.flows.push(flow);
            return;
        }

        for (let i: number = 0; i < shapeRef.nextShapeIds.length; i++) {
            let nextShapeId: number = shapeRef.nextShapeIds[i];

            if (i === 0) {
                this.createFlow(nextShapeId, id, flow, conditionEndIds);
            } else {
                let conditionEndId: number = this.getBranchDestinationId(id, nextShapeId);
                if (!conditionEndId) {
                    throw new Error(`Could not retrieve destination id for link between decision ${id} and shape ${nextShapeId}`);
                }

                conditionEndIds.push(conditionEndId);

                this.createFlow(nextShapeId, id, new ProcessModels.ProcessFlowModel(flow, i, nextShapeId), conditionEndIds);
            }
        }
    }

    public updateTree() {
        this.createTree();
    }

    public updateTreeAndFlows() {
        this.updateTree();
        this.createFlows();
    }

    public getTree(): Models.IHashMap<ProcessModels.TreeShapeRef> {
        return this.tree;
    }

    public getLinkIndex(sourceId: number, destinationId: number): number {
        this.updateTree();
        return this.linkIndex[`${sourceId};${destinationId}`];
    }

    public getNextOrderIndex(id: number): number {
        this.updateTree();
        const shapeRef: ProcessModels.TreeShapeRef = this.tree[id.toString()];
        const nextId: number = shapeRef.nextShapeIds[shapeRef.nextShapeIds.length - 1];
        const link = this.process.links[this.getLinkIndex(id, nextId)];
        return link.orderindex + 1;
    }

    public getShapeById(id: number): ProcessModels.IProcessShape {
        //We should remove the reference to this.createTree() once we can ensure that the tree is always in sync with the process model.
        this.updateTree();
        let shapeRef: ProcessModels.TreeShapeRef = this.tree[id.toString()];
        return shapeRef ? this.process.shapes[shapeRef.index] : null;
    }

    public getShapeTypeById(id: number): ProcessEnums.ProcessShapeType {
        let shape = this.getShapeById(id);
        return this.getShapeType(shape);
    }

    public getShapeType(shape: ProcessModels.IProcessShape): ProcessEnums.ProcessShapeType {
        return shape ? shape.propertyValues["clientType"].value : null;
    }

    public getNextShapeIds(id: number): number[] {
        let shapeRef = this.tree[id.toString()];
        return shapeRef ? shapeRef.nextShapeIds : null;
    }

    public getPrevShapeIds(id: number): number[] {
        let shapeRef = this.tree[id.toString()];
        return shapeRef ? shapeRef.prevShapeIds : null;
    }

    public getStartShapeId(): number {
        return this.startShapeId;
    }

    public getPreconditionShapeId(): number {
        return this.preconditionShapeId;
    }

    public getEndShapeId(): number {
        return this.endShapeId;
    }

    public hasMultiplePrevShapesById(id: number): boolean {
        this.updateTree();
        const shape: ProcessModels.TreeShapeRef = this.tree[id.toString()];
        return (shape.prevShapeIds.length > 1);
    }

    public getFirstNonSystemShapeId(id: number): number {
        let nextIds = this.getNextShapeIds(id);
        if (!nextIds || nextIds.length === 0) {
            return null;
        }

        let shapeId = Number(nextIds[0]);
        let type = this.getShapeTypeById(shapeId);

        while (type === ProcessEnums.ProcessShapeType.SystemTask || type === ProcessEnums.ProcessShapeType.SystemDecision) {
            let nextShapeIds = this.getNextShapeIds(shapeId);
            if (nextShapeIds == null || nextShapeIds.length === 0) {
                return null;
            }

            shapeId = Number(nextShapeIds[0]);
            type = this.getShapeTypeById(shapeId);
        }

        return shapeId;
    }

    public getDecisionBranchDestinationLinks(isMatch: (link: ProcessModels.IProcessLink) => boolean): ProcessModels.IProcessLink[] {
        if (this.process.decisionBranchDestinationLinks == null) {
            return [];
        }
        return this.process.decisionBranchDestinationLinks.filter(isMatch);
    }

    public getConnectedDecisionIds(destinationId: number): number[] {
        const branchDestinationLinks: ProcessModels.IProcessLink[] = this.getDecisionBranchDestinationLinks(
            (link: ProcessModels.IProcessLink) => link.destinationId === destinationId
        );

        if (!branchDestinationLinks || branchDestinationLinks.length === 0) {
            return [];
        }

        return branchDestinationLinks.map((link: ProcessModels.IProcessLink) => link.sourceId);
    }

    public getBranchDestinationIds(decisionId: number): number[] {
        const branchDestinationLinks: ProcessModels.IProcessLink[] = this.getDecisionBranchDestinationLinks(
            (link: ProcessModels.IProcessLink) => link.sourceId === decisionId
        );

        if (!branchDestinationLinks || branchDestinationLinks.length === 0) {
            return [];
        }

        return branchDestinationLinks.map((link: ProcessModels.IProcessLink) => link.destinationId);
    }

    public getBranchDestinationId(decisionShapeId: number, firstShapeInConditionId: number): number {
        let linkIndex: number = this.linkIndex[`${decisionShapeId};${firstShapeInConditionId}`];
        if (linkIndex == null) {
            return null;
        }

        let link: ProcessModels.IProcessLink = this.links[linkIndex];
        let branchDestinationLinks = this.getDecisionBranchDestinationLinks(
            (l) => l.sourceId === link.sourceId && l.orderindex === link.orderindex
        );

        if (branchDestinationLinks.length > 0) {
            return branchDestinationLinks[0].destinationId;
        }

        return null;
    }

    public isInSameFlow(id: number, otherId: number): boolean {
        if (!id || !otherId) {
            return undefined;
        }

        let currentFlow: ProcessModels.IProcessFlow = this.shapeIdToFlow[id];
        if (currentFlow == null) {
            return undefined;
        }

        let otherFlow: ProcessModels.IProcessFlow = this.shapeIdToFlow[otherId];
        if (otherFlow == null) {
            return undefined;
        }

        return currentFlow === otherFlow;
    }

    public isInChildFlow(id: number, otherId: number): boolean {
        if (!id || !otherId) {
            return undefined;
        }

        let currentFlow: ProcessModels.IProcessFlow = this.shapeIdToFlow[id];
        if (currentFlow == null) {
            return undefined;
        }

        let otherFlow: ProcessModels.IProcessFlow = this.shapeIdToFlow[otherId];
        if (otherFlow == null) {
            return undefined;
        }

        return currentFlow === otherFlow.parentFlow;
    }


    public isDecision(id: number) {
        let type: ProcessEnums.ProcessShapeType = this.getShapeTypeById(id);
        return type === ProcessEnums.ProcessShapeType.UserDecision || type === ProcessEnums.ProcessShapeType.SystemDecision;
    }

    public updateDecisionDestinationId(decisionId: number, orderIndex: number, newDestinationId: number) {
        let link = this.getDecisionBranchDestinationLinks(lnk => lnk.sourceId === decisionId && lnk.orderindex === orderIndex)[0];
        if (link) {
            link.destinationId = newDestinationId;
        }
    }

    public destroy() {
        this.tree = null;
        this.linkIndex = [];
        this.startShapeId = null;
        this.endShapeId = null;
        // remove the reference to the process artifact
        this.process = null;
    }

}
