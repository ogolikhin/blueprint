import "angular";
import "angular-mocks";
import {Helper} from "../../../core/utils/helper";
import {NotificationService} from "../../../core/notification";
import {ProjectManager, IProjectManager, Models, SubscriptionEnum} from "../../managers/project-manager";
import {ProjectExplorerController} from "./project-explorer"
import {BPTreeControllerMock} from "../../../core/widgets/bp-tree/bp-tree.mock";
import {LocalizationServiceMock} from "../../../core/localization";
import {ProjectRepositoryMock} from "../../services/project-repository.mock";


describe("Project Explorer Test", () => {
    let explorer: ProjectExplorerController;
    let isReloadCalled: boolean;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("notification", NotificationService);
        $provide.service("manager", ProjectManager);
    }));

    beforeEach(inject((manager: IProjectManager) => {
        explorer = new ProjectExplorerController(manager);
        explorer.tree = new BPTreeControllerMock();
        explorer.tree.reload = function (data: any[], id?: number) {
            isReloadCalled = true;
        }
    }));

    
    it("check property map", inject(($rootScope: ng.IRootScopeService, manager: IProjectManager) => {
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

    it("Load project", inject(($rootScope: ng.IRootScopeService, manager: IProjectManager) => {
        // Arrange
        isReloadCalled = false;

        let current = manager.CurrentProject;
        manager["loadProject"](1, "Project 1");
                        
        // Act
        $rootScope.$digest();

        // Assert
        expect(isReloadCalled).toBeTruthy();
        expect(manager.CurrentProject).toBeDefined();
        expect(manager.CurrentProject["loaded"]).toBeTruthy();
        expect(manager.CurrentProject["open"]).toBeTruthy();

    }));
    it("Load project children succsessful", inject(($rootScope: ng.IRootScopeService, manager: IProjectManager) => {
        // Arrange
        isReloadCalled = false;
        manager["loadProject"](1, "Project 1");    
        $rootScope.$digest();
        let currentArtifact = manager.CurrentProject.artifacts[0];

        // Act
        manager["loadProjectChildren"](1, 10);    
        $rootScope.$digest();

        let current = manager.CurrentProject;
        // Assert
        //let dsAfter = explorer.tree["_datasource"];

        expect(isReloadCalled).toBeTruthy();
        expect(manager.CurrentProject.artifacts[0]["loaded"]).toBeTruthy();
        expect(manager.CurrentProject.artifacts[0]["open"]).toBeTruthy();
        expect(manager.CurrentProject.artifacts[0].artifacts).toEqual(jasmine.any(Array));
    }));
        
    it("close current project", inject(($rootScope: ng.IRootScopeService, manager: IProjectManager) => {
        // Arrange
        isReloadCalled = false;
        manager["loadProject"](1, "Project 1");    
        $rootScope.$digest();

        let count = manager.ProjectCollection.length;
        // Act
        manager.notify(SubscriptionEnum.ProjectClose);
        $rootScope.$digest();

        // Assert
        expect(isReloadCalled).toBeTruthy();
        expect(manager.CurrentProject).toBeNull();
        expect(manager.ProjectCollection.length).toEqual(0);

    }));

    it("close all projects", inject(($rootScope: ng.IRootScopeService, manager: IProjectManager) => {
        // Arrange
        isReloadCalled = false;
        manager["loadProject"](1, "Project 1");
        $rootScope.$digest();
        manager["loadProject"](2, "Project 2");
        $rootScope.$digest();
        manager["loadProject"](3, "Project 3");
        $rootScope.$digest();


        // Act
        manager.notify(SubscriptionEnum.ProjectClose, true);
        $rootScope.$digest();

        // Assert
        expect(isReloadCalled).toBeTruthy();
        expect(manager.ProjectCollection.length).toEqual(0);

    }));

});