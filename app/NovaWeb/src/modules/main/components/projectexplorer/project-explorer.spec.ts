import "angular";
import "angular-mocks";
import { NotificationService} from "../../../core/notification";
import {LocalizationServiceMock} from "../../../core/localization";
import {IBPTreeController, BPTreeControllerMock, ITreeNode} from "../../../core/widgets/bp-tree/bp-tree.mock";
import {ProjectRepositoryMock} from "../../services/project-repository.mock";
import {ProjectManager, IProjectManager, Models, SubscriptionEnum} from "../../managers/project-manager";
import {ProjectExplorerController} from "./project-explorer"


describe("Project Explorer Test", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("notification", NotificationService);
        $provide.service("manager", ProjectManager);
    }));
    let explorer;
    beforeEach(inject((manager: IProjectManager) => {
        explorer = new ProjectExplorerController(manager);
        explorer.tree = new BPTreeControllerMock();
    }));

    describe("Load project: ", () => {
        it("load project", inject(($rootScope: ng.IRootScopeService, manager: IProjectManager) => {
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
        

    });

    describe("Remove project:", () => {
        //TODO
    });


});