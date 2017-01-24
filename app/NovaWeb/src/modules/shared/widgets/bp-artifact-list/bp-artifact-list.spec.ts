import "angular";
import "angular-mocks";
import "lodash";
import {IBPArtifactListController, BPArtifactListComponent, BPArtifactListController} from "./bp-artifact-list";
import {Models} from "../../../main/models";

describe("BPArtifactListComponent", () => {
    angular.module("bp.widgets.artifactList", [])
        .component("bpArtifactList", new BPArtifactListComponent());

    // the mock contains:
    // - 253 artifacts from project 469556 (Product Management - Release 6.2.1)
    // - 1 artifact from project 1 (Performance Test Main Project)
    // - 1 artifact from project 70422 ([7.4.0.x] [ngstars] BP Air)
    const mockData = JSON.parse(require("./bp-artifact-list.mock.json"));
    const artifactList = mockData.artifacts;
    const projectList = mockData.projects;
    const selectedProject = 70422;

    let controller: BPArtifactListController;

    beforeEach(angular.mock.module("bp.widgets.artifactList"));

    beforeEach(inject(($componentController) => {
        const bindings = {
            artifactList: artifactList,
            projectList: projectList,
            selectedProject: selectedProject,
            limit: 50
        } as IBPArtifactListController;
        controller = $componentController("bpArtifactList", null, bindings);
    }));

    it("Values are bound", () => {
        // Arrange

        // Act

        // Assert
        expect(controller.artifactList).toEqual(artifactList);
        expect(controller.projectList).toEqual(projectList);
        expect(controller.selectedProject).toEqual(selectedProject);
        expect(controller.limit).toEqual(50);
    });

    it("[Show more] button is initially visible if the artifact list is longer than limit", () => {
        // Arrange

        // Act

        // Assert
        expect(controller.showLoadMore).toBeTruthy();
    });

    it("[Show more] button is removed after click", () => {
        // Arrange

        // Act
        controller.loadMore();

        // Assert
        expect(controller.showLoadMore()).toBeFalsy();
    });

    it("Project name is visible for first artifact in list", () => {
        // Arrange
        controller.$onInit();
        const artifact1 = controller.sortedList[0] as Models.IArtifactWithProject;

        // Act

        // Assert
        expect(controller.showProject(artifact1, 0)).toBeTruthy();
    });

    it("Project name is visible when artifact is from different project than previous artifact in list", () => {
        // Arrange
        controller.$onInit();
        const artifact2 = controller.sortedList[1] as Models.IArtifactWithProject;
        const artifact3 = controller.sortedList[2] as Models.IArtifactWithProject;
        const artifact4 = controller.sortedList[3] as Models.IArtifactWithProject;

        // Act

        // Assert
        expect(controller.showProject(artifact2, 1)).toBeTruthy();
        expect(controller.showProject(artifact3, 2)).toBeTruthy();
        expect(controller.showProject(artifact4, 3)).toBeFalsy();
    });

    // TODO: change/remove the following tests once the back-end provides presorted lists
    it("Artifacts from selected project are first", () => {
        // Arrange
        controller.$onInit();
        const artifact1 = controller.sortedList[0] as Models.IArtifactWithProject;

        // Act

        // Assert
        expect(artifact1.projectId).toEqual(selectedProject);
    });

    it("Subsequent artifacts are ordered by project name and by artifact name", () => {
        // Arrange
        controller.$onInit();
        const artifact1 = controller.sortedList[0] as Models.IArtifactWithProject;
        const artifact2 = controller.sortedList[1] as Models.IArtifactWithProject;
        const artifact3 = controller.sortedList[2] as Models.IArtifactWithProject;
        const artifact4 = controller.sortedList[3] as Models.IArtifactWithProject;

        // Act

        // Assert
        expect(artifact1.projectName < artifact2.projectName).toBeFalsy();
        expect(artifact2.projectName < artifact3.projectName).toBeTruthy();

        expect(artifact2.name < artifact3.name).toBeFalsy();
        expect(artifact3.name < artifact4.name).toBeTruthy();
    });
});
