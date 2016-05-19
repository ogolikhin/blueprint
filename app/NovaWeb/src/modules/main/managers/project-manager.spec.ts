import "angular";
import "angular-mocks";
//import {Models} from "../services/project-repository";
import {ProjectRepositoryMock} from "../services/project-repository.mock";
import {IProjectNotification, ProjectNotification, SubscriptionEnum} from "../services/project-notification";
import {ProjectManager, Models} from "../managers/project-manager";


describe("Project Manager Test", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectNotification", ProjectNotification);
        $provide.service("manager", ProjectManager);
    }));

    describe("Load projects: ", () => {
        it("single project", inject(($rootScope: ng.IRootScopeService,manager: ProjectManager, projectNotification: IProjectNotification) => {
            // Arrange
            projectNotification.notify(SubscriptionEnum.ProjectLoad, 1);
            $rootScope.$digest();
            //Act
            let project = manager.CurrentProject;

            //Asserts
            expect(project).toBeDefined();
            expect(project.id).toEqual(1);
        }));

        it("multiple project", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager, projectNotification: IProjectNotification) => {
            // Arrange
            projectNotification.notify(SubscriptionEnum.ProjectLoad, 1);
            $rootScope.$digest();

            projectNotification.notify(SubscriptionEnum.ProjectLoad, 2);
            $rootScope.$digest();

            //Act
            let current = manager.CurrentProject;
            let projects = manager.ProjectCollection;

            //Asserts
            expect(projects).toEqual(jasmine.any(Array));
            expect(projects.length).toEqual(2);
            expect(current).toEqual(projects[1]);
        }));

        it("project children", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager, projectNotification: IProjectNotification) => {
            // Arrange

            projectNotification.notify(SubscriptionEnum.ProjectLoad, 1);
            $rootScope.$digest();

            let project: Models.IProject = manager.CurrentProject;

            projectNotification.notify(SubscriptionEnum.ProjectChildrenLoad, project.id, 1);

            //Act
            project = manager.CurrentProject;

            //Asserts
            expect(project.children).toBeDefined();
            expect(project.children).toEqual(jasmine.any(Array));
            expect(project.children.length).toEqual(1);
            expect(project.children[0].id).toEqual(1);
        }));




    });
});