import "angular";
import "angular-mocks";
import {EventManager} from "../../../core/event-manager";
import {ProjectManager, IProjectManager, SubscriptionEnum} from "../../managers/project-manager";
import {ProjectExplorerController} from "./project-explorer";
import {BPTreeControllerMock} from "../../../core/widgets/bp-tree/bp-tree.mock";
import {LocalizationServiceMock} from "../../../core/localization.mock";
import {ProjectRepositoryMock} from "../../services/project-repository.mock";


describe("Project Explorer Test", () => {
    let explorer: ProjectExplorerController;
    let isReloadCalled: boolean;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("eventManager", EventManager);
        $provide.service("projectManager", ProjectManager);
    }));

    beforeEach(inject((projectManager: IProjectManager) => {
        explorer = new ProjectExplorerController(projectManager);
        explorer.tree = new BPTreeControllerMock();
        explorer.tree.reload = function (data: any[], id?: number) {
            isReloadCalled = true;
        };
    }));

    
    it("check property map", inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager) => {
        // Arrange
        // Act
        // Assert

        expect(explorer.propertyMap).toBeDefined();
        expect(explorer.propertyMap["id"]).toEqual("id");
        expect(explorer.propertyMap["typeId"]).toEqual("type");
        expect(explorer.propertyMap["name"]).toEqual("name");
        expect(explorer.propertyMap["hasChildren"]).toEqual("hasChildren");
        expect(explorer.propertyMap["artifacts"]).toEqual("children");

    }));

    it("Load project", inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager) => {
        // Arrange
        isReloadCalled = false;

        projectManager["loadProject"](1, "Project 1");
                        
        // Act
        $rootScope.$digest();

        // Assert
        expect(isReloadCalled).toBeTruthy();
        expect(projectManager.CurrentProject).toBeDefined();
        expect(projectManager.CurrentProject["loaded"]).toBeTruthy();
        expect(projectManager.CurrentProject["open"]).toBeTruthy();

    }));
    it("Load project children succsessful", inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager) => {
        // Arrange
        isReloadCalled = false;
        projectManager["loadProject"]({ id: 1, name:"Project 1", artifacts:[]});    
        $rootScope.$digest();

        // Act
        projectManager["loadProjectChildren"](1, 10);    
        $rootScope.$digest();

        // Assert
        //let dsAfter = explorer.tree["_datasource"];

        expect(isReloadCalled).toBeTruthy();
        expect(projectManager.CurrentProject.artifacts[0]["loaded"]).toBeTruthy();
        expect(projectManager.CurrentProject.artifacts[0]["open"]).toBeTruthy();
        expect(projectManager.CurrentProject.artifacts[0].artifacts).toEqual(jasmine.any(Array));
    }));
        
    it("close current project", inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager) => {
        // Arrange
        isReloadCalled = false;
        projectManager["loadProject"](1, "Project 1");    
        $rootScope.$digest();

        // Act
        projectManager.notify(SubscriptionEnum.ProjectClose);
        $rootScope.$digest();

        // Assert
        expect(isReloadCalled).toBeTruthy();
        expect(projectManager.CurrentProject).toBeNull();
        expect(projectManager.ProjectCollection.length).toEqual(0);

    }));

    it("close all projects", inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager) => {
        // Arrange
        isReloadCalled = false;
        projectManager["loadProject"](1, "Project 1");
        $rootScope.$digest();
        projectManager["loadProject"](2, "Project 2");
        $rootScope.$digest();
        projectManager["loadProject"](3, "Project 3");
        $rootScope.$digest();


        // Act
        projectManager.notify(SubscriptionEnum.ProjectClose, true);
        $rootScope.$digest();

        // Assert
        expect(isReloadCalled).toBeTruthy();
        expect(projectManager.ProjectCollection.length).toEqual(0);

    }));

});