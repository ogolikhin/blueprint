import {ProcessGraphModel} from "../../viewmodel/process-graph-model";
import {GraphLayoutPreprocessor} from "./graph-layout-preprocessor";
import * as TestModels from "../../../../models/test-model-factory";


function assertEqualCoordinates(actualModel: any, expectedModel: any) {
    expect(actualModel.shapes.length).toEqual(expectedModel.shapes.length);

    for (let i in actualModel.shapes) {
        expect(actualModel.shapes[i].propertyValues["x"].value).toEqual(expectedModel.shapes[i].propertyValues["x"].value);
        expect(actualModel.shapes[i].propertyValues["y"].value).toEqual(expectedModel.shapes[i].propertyValues["y"].value);
    }
}

describe("GraphLayoutPreprocessor", () => {
    describe("the setCoordinates method", () => {
        it("correctly calculates x and y for default process", () => {
            // Arrange
            let actualModel = TestModels.createDefaultProcessModelWithoutXAndY();
            let expectedModel = TestModels.createDefaultProcessModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with single user decision", () => {
            // Arrange
            let actualModel = TestModels.createSimpleCaseModelWithoutXandY();
            let expectedModel = TestModels.createSimpleCaseModelAfterAutoLayout();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with decision with multiple branches case", () => {
            // Arrange
            let actualModel = TestModels.createMultiDecisionBranchModelWithoutXAndY();
            let expectedModel = TestModels.createMultiDecisionBranchModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with two merge points", () => {
            // Arrange
            let actualModel = TestModels.createTwoMergePointsModelWithoutXAndY();
            let expectedModel = TestModels.createTwoMergePointsModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with multiple merge points and multiple branches", () => {
            // Arrange
            let actualModel = TestModels.createMultipleMergePointsWithMultipleBranchesModelWithoutXAndY();
            let expectedModel = TestModels.createMultipleMergePointsWithMultipleBranchesModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with system decision before user decision in a branch", () => {
            // Arrange
            let actualModel = TestModels.createSystemDecisionBeforeUserDecisionInBranchModelWithoutXAndY();
            let expectedModel = TestModels.createSystemDecisionBeforeUserDecisionInBranchModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with user decision loop", () => {
            // Arrange
            let actualModel = TestModels.createUserDecisionLoopModelWithoutXAndY();
            let expectedModel = TestModels.createUserDecisionLoopModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with system decision loop", () => {
            // Arrange
            let actualModel = TestModels.createSystemDecisionLoopModelWithoutXAndY();
            let expectedModel = TestModels.createSystemDecisionLoopModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with user decision having infinite loop", () => {
            // Arrange
            let actualModel = TestModels.createUserDecisionInfiniteLoopModelWithoutXAndY();
            let expectedModel = TestModels.createUserDecisionInfiniteLoopModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with system decision having infinite loop", () => {
            // Arrange
            let actualModel = TestModels.createSystemDecisionInfiniteLoopModelWithoutXAndY();
            let expectedModel = TestModels.createSystemDecisionInfiniteLoopModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with two user decisions", () => {
            // Arrange
            let actualModel = TestModels.createTwoUserDecisionsBackToBackModelWithoutXAndY();
            let expectedModel = TestModels.createTwoUserDecisionsBackToBackModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with user decision merging into another user decision", () => {
            // Arrange
            let actualModel = TestModels.createMergingUserDecisionModelWithoutXAndY();
            let expectedModel = TestModels.createMergingUserDecisionModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with user decision contained in another user decision", () => {
            // Arrange
            let actualModel = TestModels.createContainedUserDecisionModelWithoutXAndY();
            let expectedModel = TestModels.createContainedUserDecisionModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with two user decisions with non-overlapping loop", () => {
            // Arrange
            let actualModel = TestModels.createTwoUserDecisionsWithNonOverlappingLoopModelWithoutXAndY();
            let expectedModel = TestModels.createTwoUserDecisionsWithNonOverlappingLoopModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with two user decisions with overlapping loop", () => {
            // Arrange
            let actualModel = TestModels.createTwoUserDecisionsWithOverlappingLoopModelWithoutXAndY();
            let expectedModel = TestModels.createTwoUserDecisionsWithOverlappingLoopModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with nested system decisions with loop", () => {
            // Arrange
            let actualModel = TestModels.createNestedSystemDecisionsWithLoopModelWithoutXAndY();
            let expectedModel = TestModels.createNestedSystemDecisionsWithLoopModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with nested loops", () => {
            // Arrange
            let actualModel = TestModels.createNestedLoopsModelWithoutXAndY();
            let expectedModel = TestModels.createNestedLoopsModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with three nested user tasks", () => {
            // Arrange
            let actualModel = TestModels.createThreeNestedUserTasksModelWithoutXAndY();
            let expectedModel = TestModels.createThreeNestedUserTasksModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
        it("correctly calculates x and y for process with two nested user tasks and system task", () => {
            // Arrange
            let actualModel = TestModels.createTwoNestedUserTasksWithSystemTaskModelWithoutXAndY();
            let expectedModel = TestModels.createTwoNestedUserTasksWithSystemTaskModel();
            let process = new ProcessGraphModel(actualModel);
            let preprocessor = new GraphLayoutPreprocessor(process);

            // Act
            preprocessor.setCoordinates();

            // Assert
            assertEqualCoordinates(actualModel, expectedModel);
        });
    });
});