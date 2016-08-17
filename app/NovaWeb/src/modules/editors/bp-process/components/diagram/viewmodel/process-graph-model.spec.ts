import {ProcessGraphModel} from "./process-graph-model";
import {createTwoNestedUserTasksWithSystemTaskModel} from "../../../models/test-model-factory";

describe("ProcessGraphModel", () => {
    describe("isInSameFlow", () => {
        it("returns undefined if id is null", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let ud1 = process.shapes[2];

            // Act
            let actual = clientModel.isInSameFlow(null, ud1.id);

            // Assert
            expect(actual).toBe(undefined);
        });
        it("returns undefined if otherId is null", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let ud1 = process.shapes[2];

            // Act
            let actual = clientModel.isInSameFlow(ud1.id, null);

            // Assert
            expect(actual).toBe(undefined);
        });
        it("returns undefined for ids that don't exist in the process", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);

            // Act
            let actual = clientModel.isInSameFlow(999, 998);

            // Assert
            expect(actual).toBe(undefined);
        });
        it("returns true for shapes in same flow", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let st2 = process.shapes[6];
            let st3 = process.shapes[9];

            // Act
            let actual = clientModel.isInSameFlow(st2.id, st3.id);

            // Assert
            expect(actual).toEqual(true);
        });
        it("returns false for shapes in different flows", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let ut4 = process.shapes[12];
            let ud1 = process.shapes[2];

            // Act
            let actual = clientModel.isInSameFlow(ut4.id, ud1.id);

            // Assert
            expect(actual).toEqual(false);
        });
    });
    describe("isInChildFlow", () => {
        it("returns undefined if id is null", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let ud1 = process.shapes[2];

            // Act
            let actual = clientModel.isInChildFlow(null, ud1.id);

            // Assert
            expect(actual).toBe(undefined);
        });
        it("returns undefined if otherId is null", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let ud1 = process.shapes[2];

            // Act
            let actual = clientModel.isInChildFlow(ud1.id, null);

            // Assert
            expect(actual).toBe(undefined);
        });
        it("returns undefined for id that don't exist in the process", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let shape = process.shapes[3];

            // Act
            let actual = clientModel.isInChildFlow(999, shape.id);

            // Assert
            expect(actual).toBe(undefined);
        });
        it("returns undefined for otherId that don't exist in the process", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let shape = process.shapes[3];

            // Act
            let actual = clientModel.isInChildFlow(shape.id, 998);

            // Assert
            expect(actual).toBe(undefined);
        });
        it("returns false for shapes in same flow", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let st2 = process.shapes[6];
            let st3 = process.shapes[9];

            // Act
            let actual = clientModel.isInChildFlow(st2.id, st3.id);

            // Assert
            expect(actual).toEqual(false);
        });
        it("returns true for shape in child flows", () => {
            // Arrange
            let process = createTwoNestedUserTasksWithSystemTaskModel();
            let clientModel = new ProcessGraphModel(process);
            let ut4 = process.shapes[12];
            let ud1 = process.shapes[2];

            // Act
            let actual = clientModel.isInChildFlow(ud1.id, ut4.id);

            // Assert
            expect(actual).toEqual(true);
        });
    });
});