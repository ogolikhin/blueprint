import {IHashMap} from "../../../../../../main/models/models";
import {IProcessShape, TreeShapeRef} from "../../../../models/process-models";
import {IProcessGraphModel} from "../../viewmodel/process-graph-model";
import {IGraphLayoutPreprocessor} from "./models/";

export class GraphLayoutPreprocessor implements IGraphLayoutPreprocessor {
    private boundaryCurve: number[];
    private branchShapes: IProcessShape[];
    private inBranch: boolean = false;
    private tree: IHashMap<TreeShapeRef>;

    constructor(
        private model: IProcessGraphModel) {
        this.boundaryCurve = [];
        this.tree = model.getTree();
    }

    public setCoordinates(): void {
        this.initializeCoordinates();

        // set x and y coordinates
        this.setXCoordinate(this.model.getStartShapeId());
        this.setYCoordinate(this.model.getStartShapeId());
    }

    private initializeCoordinates(): void {
        // initialize x and y coordinates
        for (let shape of this.model.shapes) {
            shape.propertyValues["x"].value = -1;
            shape.propertyValues["y"].value = -1;
        }
    }

    private updateBoundary(fromX: number, toX: number, y: number): void {
        for (var i = fromX; i <= toX; i++) {
            this.boundaryCurve[i] = y;
        }
    }

    private getDepth(fromX: number, toX: number): number {
        var depth: number = 0;

        for (var i = fromX; i <= toX; i++) {
            if (depth < this.boundaryCurve[i]) {
                depth = this.boundaryCurve[i];
            }
        }

        return depth;
    }

    private addToBranchShapes(shape: IProcessShape): void {
        if (this.inBranch) {
            this.branchShapes.push(shape);
        }
    }

    private setBranchStart(): void {
        if (!this.inBranch) {
            this.branchShapes = [];
            this.inBranch = true;
        }
    }

    private setBranchEnd(): void {
        if (this.inBranch) {
            this.inBranch = false;
            this.updateBranchDepth();
        }
    }

    private updateBranchDepth(): void {
        let decisionX: number = this.branchShapes[0].propertyValues["x"].value;
        let lastShapeX: number = this.branchShapes[this.branchShapes.length - 2].propertyValues["x"].value;
        let mergingX: number = this.branchShapes[this.branchShapes.length - 1].propertyValues["x"].value;

        let fromX: number = decisionX;
        let toX: number;

        if (lastShapeX < mergingX - 1) {
            // merging forward
            toX = mergingX - 1;
        } else {
            // merging back
            toX = lastShapeX;
        }

        let branchDepth: number = 1 + this.getDepth(fromX, toX);
        let branchUpdated = false;

        // update all but decision shape and merging shape
        for (let i = 1; i < this.branchShapes.length - 1; i++) {
            this.branchShapes[i].propertyValues["y"].value = branchDepth;
            branchUpdated = true;
        }

        if (branchUpdated) {
            this.updateBoundary(fromX, toX, branchDepth);
        }
    }

    private isBranchDestination(shape: IProcessShape, branchEndIds: number[]): boolean {
        if (branchEndIds.length === 0) {
            return false;
        }

        return shape.id === branchEndIds[branchEndIds.length - 1];
    }

    private getMaxX(id: number, previousIds: number[], x: number): number {
        let maxX = x;

        for (let previousId of previousIds) {
            let prevX: number = this.model.getShapeById(previousId).propertyValues["x"].value;

            if (prevX >= maxX/* && this.model.isInChildFlow(id, previousId)*/) {
                maxX = prevX + 1;
            }
        }

        return maxX;
    }

    private setXCoordinate(id: number, x: number = 0, branchEndIds: number[] = []): void {
        let shapeRef: TreeShapeRef = this.tree[id.toString()];
        let shape = this.model.shapes[shapeRef.index];

        if (this.isBranchDestination(shape, branchEndIds)) {
            branchEndIds.pop();
            return;
        }

        if (shapeRef.prevShapeIds.length > 1) {
            let maxX = this.getMaxX(id, shapeRef.prevShapeIds, x);
            x = maxX + 1;
        }

        shape.propertyValues["x"].value = x;

        for (let i: number = shapeRef.nextShapeIds.length - 1; i >= 0; i--) {
            let nextShapeId = shapeRef.nextShapeIds[i];

            if (i > 0) {
                let branchEndId: number = this.model.getBranchDestinationId(id, nextShapeId);
                if (!branchEndId) {
                    throw new Error(`Could not retrieve destination id for link between decision ${id} and shape ${nextShapeId}`);
                }

                branchEndIds.push(branchEndId);
            }

            this.setXCoordinate(nextShapeId, x + 1, branchEndIds);
        }
    }

    private setYCoordinate(id: number, isInBranch: boolean = false, branchEndIds: number[] = []): void {
        let shapeRef: TreeShapeRef = this.tree[id.toString()];
        let shape = this.model.shapes[shapeRef.index];

        if (shape.propertyValues["y"].value < 0) {
            shape.propertyValues["y"].value = 0;
        }

        this.addToBranchShapes(shape);

        if (this.isBranchDestination(shape, branchEndIds)) {
            this.setBranchEnd();
            return;
        }

        if (shapeRef.nextShapeIds == null || shapeRef.nextShapeIds.length === 0) {
            // reached the end shape
            return;
        }

        for (let i: number = 0; i < shapeRef.nextShapeIds.length; i++) {
            let nextShapeId = shapeRef.nextShapeIds[i];

            if (i > 0) {
                this.setBranchStart();
                this.addToBranchShapes(shape);

                let branchEndId: number = this.model.getBranchDestinationId(id, nextShapeId);
                if (!branchEndId) {
                    throw new Error(`Could not retrieve destination id for link between decision ${id} and shape ${nextShapeId}`);
                }

                branchEndIds.push(branchEndId);
            }

            this.setYCoordinate(nextShapeId, i > 0, branchEndIds);
        }
    }
}