import "angular";
import "angular-mocks";
import { NotificationService} from "../../core/notification";
import {ProjectRepositoryMock} from "../services/project-repository.mock";
import {ProjectManager, Models, SubscriptionEnum} from "../managers/project-manager";


describe("Project Manager Test", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("notification", NotificationService);
        $provide.service("manager", ProjectManager);
    }));

    describe("Load projects: ", () => {
        it("Load single project", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            manager.notify(SubscriptionEnum.ProjectLoad, 1);
            $rootScope.$digest();
            //Act
            let project = manager.CurrentProject;

            //Asserts
            expect(project).toBeDefined();
            expect(project.id).toEqual(1);
        }));

        it("multiple project", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            manager.notify(SubscriptionEnum.ProjectLoad, 1);
            $rootScope.$digest();

            manager.notify(SubscriptionEnum.ProjectLoad, 2);
            $rootScope.$digest();

            //Act
            let current = manager.CurrentProject;
            let projects = manager.ProjectCollection;

            //Asserts
            expect(projects).toEqual(jasmine.any(Array));
            expect(projects.length).toEqual(2);
            expect(current).toEqual(projects[0]);
        }));

        it("project children", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange

            manager.notify(SubscriptionEnum.ProjectLoad, 1);
            $rootScope.$digest();

            let project: Models.IProject = manager.CurrentProject;

            manager.notify(SubscriptionEnum.ProjectChildrenLoad, project.id, 1);

            //Act
            project = manager.CurrentProject;

            //Asserts
            expect(project.artifacts).toBeDefined();
            expect(project.artifacts).toEqual(jasmine.any(Array));
            expect(project.artifacts.length).toEqual(1);
            expect(project.artifacts[0].id).toEqual(1);
        }));




    });

    describe("Load projects: ", () => {

    });
});