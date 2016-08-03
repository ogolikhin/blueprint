import "angular";
import "angular-mocks";
import { ConfigValueHelper } from "../../../../core";
import { MessageService } from "../../../../shell/";
import { ProjectManager, 
    IProjectManager, 
    Models } from "../../../";
import { ArtifactPickerController } from "./bp-artifact-picker";

import { BPTreeControllerMock, ITreeNode } from "../../../../shared/widgets/bp-tree/bp-tree.mock";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";
import { ProjectRepositoryMock } from "../../../services/project-repository.mock";
import { ModalServiceInstanceMock } from "../open-project.spec.ts";


describe("Project Explorer Test", () => {
    let isReloadCalled: number = 0;
    let $scope;
    let elem;
    var controller: ArtifactPickerController;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("configValueHelper", ConfigValueHelper);
        $provide.service("messageService", MessageService);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
        $provide.service("explorer", ArtifactPickerController);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager, $compile: ng.ICompileService) => {
        $rootScope["config"] = {
            "settings": {
                "StorytellerMessageTimeout": `{ "Warning": 0, "Info": 3000, "Error": 0 }`
            }
        };
        projectManager.initialize();
        projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
        $rootScope.$digest();

        $scope = $rootScope.$new();

        elem = angular.element(`<div ag-grid="ctrl.gridOptions" class="ag-grid"></div>`);

        controller = new ArtifactPickerController(
            $scope,
            new LocalizationServiceMock(),
            new ModalServiceInstanceMock(),
            projectManager,
            null,
            null,
            null);

        controller["tree"] = new BPTreeControllerMock();      

        $compile(elem)($scope);
        $scope.$digest();
    }));


    xit("check property map", inject((explorer: ArtifactPickerController) => {
        // Arrange
        // Act
        // Assert

        expect(explorer.propertyMap).toBeDefined();
        expect(explorer.propertyMap["id"]).toEqual("id");
        expect(explorer.propertyMap["itemTypeId"]).toEqual("type");
        expect(explorer.propertyMap["name"]).toEqual("name");
        expect(explorer.propertyMap["hasChildren"]).toEqual("hasChildren");
        expect(explorer.propertyMap["artifacts"]).toEqual("children");

    }));

    xit("Load project", inject((projectManager: ProjectManager, explorer: ArtifactPickerController) => {
        // Arrange
        isReloadCalled = 0;
        explorer.tree.reload = function (data: any[], id?: number) {
            isReloadCalled += 1;
        };


        // Act
        projectManager.projectCollection.onNext([{ id: 1, name: "Project 1" } as Models.IProject]);

        // Assert
        expect(isReloadCalled).toEqual(1);

    }));
    xit("Load project children call", inject((projectManager: IProjectManager, explorer: ArtifactPickerController) => {
        // Arrange
        isReloadCalled = 0;
        explorer.tree.selectNode = function (id: number) {
            isReloadCalled += 1;
        };

        // Act
        projectManager.currentArtifact.onNext({ id: 1, name: "Artifact 1" } as Models.IArtifact);

        // Assert
        expect(isReloadCalled).toEqual(1);
    }));

    xit("close project", inject((projectManager: ProjectManager, explorer: ArtifactPickerController) => {
        // Arrange
        isReloadCalled = 0;
        explorer.tree.reload = function (data: any[], id?: number) {
            isReloadCalled += 1;
        };

        // Act
        projectManager.projectCollection.onNext([{ id: 1, name: "Project 1" } as Models.IProject]);

        projectManager.closeProject();

        // Assert
        expect(isReloadCalled).toEqual(1);

    }));

    xit("doLoad", inject(($rootScope: ng.IRootScopeService, explorer: ArtifactPickerController) => {
        // Arrange
        isReloadCalled = 0;
        explorer.tree.reload = function (data: any[], id?: number) {
            isReloadCalled += 1;
        };
        explorer.doLoad(new Models.Project({ id: 1, name: "Project 1" }));
        $rootScope.$digest();

        // Act
        //expect(isReloadCalled).toEqual(1);

    }));

    xit("doLoad (nothing)", inject(($rootScope: ng.IRootScopeService, explorer: ArtifactPickerController) => {
        // Arrange
        isReloadCalled = 0;
        explorer.tree.reload = function (data: any[], id?: number) {
            isReloadCalled += 1;
        };
        explorer.doLoad(null);
        $rootScope.$digest();

        // Act
        expect(isReloadCalled).toEqual(0);

    }));

    xit("doSelect", inject(($rootScope: ng.IRootScopeService, explorer: ArtifactPickerController) => {
        // Arrange
        isReloadCalled = 0;
        explorer.tree.selectNode = function (id?: number) {
            isReloadCalled += 1;
        };
        explorer["projectManager"].projectCollection.onNext([new Models.Project({ id: 1, name: "Project 1" })]);
        $rootScope.$digest();
        explorer.doSelect(({ id: 1, name: "Project 1" } as ITreeNode));
        $rootScope.$digest();

        // Act
        expect(isReloadCalled).toEqual(1);

    }));


    xit("close current project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
        // Arrange
        isReloadCalled = 1;
        projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
        $rootScope.$digest();

        // Act
        projectManager.closeProject();
        $rootScope.$digest();
        let current = projectManager.currentProject.getValue();

        // Assert
        expect(isReloadCalled).toBeTruthy();
        expect(current).toBeNull();
        expect(projectManager.projectCollection.getValue.length).toEqual(0);

    }));

    xit("close all projects", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
        // Arrange
        isReloadCalled = 1;
        projectManager.loadProject(new Models.Project({ id: 2, name: "Project 1" }));
        $rootScope.$digest();
        projectManager.loadProject(new Models.Project({ id: 2, name: "Project 2" }));
        $rootScope.$digest();

        // Act
        projectManager.closeProject(true);
        $rootScope.$digest();
        let current = projectManager.currentProject.getValue();

        // Assert
        expect(current).toBeNull();
        expect(projectManager.projectCollection.getValue.length).toEqual(0);

    }));

});