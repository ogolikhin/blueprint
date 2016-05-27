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

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("notification", NotificationService);
        $provide.service("manager", ProjectManager);
    }));

    beforeEach(inject((manager: IProjectManager) => {
        explorer = new ProjectExplorerController(manager);
        explorer.tree = new BPTreeControllerMock();
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
        let dsBefore = explorer.tree["_datasource"].splice();
                        
        // Act
        manager.notify(SubscriptionEnum.ProjectLoaded);
        $rootScope.$digest();

        // Assert
        let dsAfter = explorer.tree["_datasource"];

        expect(dsBefore).toEqual(jasmine.any(Array));
        expect(dsAfter).toEqual(jasmine.any(Array));
        expect(dsAfter.length).toBeGreaterThan(dsBefore.length);

    }));
    it("Load project children succsessful", inject(($rootScope: ng.IRootScopeService, manager: IProjectManager) => {
        // Arrange
        let dsBefore = explorer.tree["_datasource"].splice();
            
        // Act
        // Act
        manager.notify(SubscriptionEnum.ProjectLoaded, jasmine.any(Object));
        manager.notify(SubscriptionEnum.ProjectChildrenLoaded, jasmine.any(Object));
        $rootScope.$digest();

        // Assert
        let dsAfter = explorer.tree["_datasource"];

        expect(dsBefore).toEqual(jasmine.any(Array));
        expect(dsAfter).toEqual(jasmine.any(Array));
        expect(dsAfter[0].children.length).toEqual(5);
        expect(Helper.toFlat(dsAfter).length).toEqual(15);

    }));
        
    it("delete project succsessful", inject(($rootScope: ng.IRootScopeService, manager: IProjectManager) => {
        // Arrange
        manager.notify(SubscriptionEnum.ProjectLoaded, jasmine.any(Object));
        let dsBefore = explorer.tree["_datasource"].splice();

        // Act
        manager.notify(SubscriptionEnum.ProjectClosed, [{ id: 1 }]);
        $rootScope.$digest();

        // Assert
        let dsAfter = explorer.tree["_datasource"];

        expect(dsBefore).toEqual(jasmine.any(Array));
        expect(dsAfter).toEqual(jasmine.any(Array));
        expect(dsAfter.length).toEqual(9);

    }));

    it("delete project unsuccsessful", inject(($rootScope: ng.IRootScopeService, manager: IProjectManager) => {
        // Arrange
        manager.notify(SubscriptionEnum.ProjectLoaded, jasmine.any(Object));
        let dsBefore = explorer.tree["_datasource"].splice();

        // Act
        manager.notify(SubscriptionEnum.ProjectClosed, [ { id: 11 }]);
        $rootScope.$digest();

        // Assert
        let dsAfter = explorer.tree["_datasource"];

        expect(dsBefore).toEqual(jasmine.any(Array));
        expect(dsAfter).toEqual(jasmine.any(Array));
        expect(dsAfter.length).toEqual(10);

    }));

});