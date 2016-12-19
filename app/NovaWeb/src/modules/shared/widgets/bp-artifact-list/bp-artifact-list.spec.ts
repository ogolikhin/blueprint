import * as angular from "angular";
import "angular-mocks";
import {BPArtifactListComponent, BPArtifactListController} from "./bp-artifact-list";

describe("BPArtifactListComponent", () => {
    angular.module("bp.widgets.artifactList", [])
        .component("bpArtifactList", new BPArtifactListComponent());

    let scope: ng.IScope;

    beforeEach(angular.mock.module("bp.widgets.artifactList"));

    const artifactList = [{id: 3, projectId: 1}, {id: 4, projectId: 1}, {id: 5, projectId: 2}];
    const projectList = [{id: 1, name: "Project 1"}, {id: 2, name: "Project 2"}];

    it("Values are bound", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        scope = $rootScope.$new();
        scope["artifactList"] = artifactList;
        scope["projectList"] = projectList;
        const element = `<bp-artifact-list
            artifact-list="artifactList"
            project-list="projectList"
            selected-project="1"
            limit="50" />`;

        // Act
        const controller = $compile(element)(scope).controller("bpArtifactList") as BPArtifactListController;

        // Assert
        expect(controller.artifactList).toEqual(artifactList);
        expect(controller.projectList).toEqual(projectList);
        expect(controller.selectedProject).toEqual(1);
        expect(controller.limit).toEqual(50);
    }));
});
