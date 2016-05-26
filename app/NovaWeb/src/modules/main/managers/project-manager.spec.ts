import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../core/localization";
import {NotificationService} from "../../core/notification";
import {ProjectRepositoryMock} from "../services/project-repository";
import {ProjectManager, Models, SubscriptionEnum} from "../managers/project-manager";


describe("Project Manager Test", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("notification", NotificationService);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("manager", ProjectManager);
    }));

    describe("Load projects: ", () => {
        it("Single project", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();
            //Act
            let project = manager.CurrentProject;

            //Asserts
            expect(project).toBeDefined();
            expect(project.id).toEqual(1);
            expect(project.name).toEqual("Project 1");
        }));

        it("Multiple projects", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
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

        it("Load project children", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange

            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            let project: Models.IProject = manager.CurrentProject;

            manager.notify(SubscriptionEnum.ProjectChildrenLoad, project.id, 101);
            $rootScope.$digest();

            //Act
            project = manager.CurrentProject;

            //Asserts
            expect(project.artifacts).toBeDefined();
            expect(project.artifacts).toEqual(jasmine.any(Array));
            expect(project.artifacts.length).toEqual(5);
            expect(project.artifacts[0].id).toEqual(100);
        }));
        it("Load project children. Project not found", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let error;
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            manager["notification"].attach("main", "exception", function (ex) {
                error = ex;
            })

            //Act
            manager.notify(SubscriptionEnum.ProjectChildrenLoad, 5, 5);
            $rootScope.$digest();


            //Asserts
            expect(error.message).toBe("Project_NotFound");
        }));
        it("Load project children. Artifact not found", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let error;
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            manager["notification"].attach("main", "exception", function (ex) {
                error = ex;
            })

            //Act
            manager.notify(SubscriptionEnum.ProjectChildrenLoad, 1, 5);
            $rootScope.$digest();


            //Asserts
            expect(error.message).toBe("Artifact_NotFound");
        }));

    });

    describe("Current Project: ", () => {
        it("Current project", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let changedProject;
            let func = function (project: Models.IProject) {
                changedProject = project;
            };

            manager.subscribe(SubscriptionEnum.CurrentProjectChanged, func);

            //Act
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            //Asserts
            expect(changedProject).toEqual(manager.CurrentProject);
        }));

        it("Set Current project", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let changedProject;
            
            manager.subscribe(SubscriptionEnum.CurrentProjectChanged, function (project: Models.IProject) {
                changedProject = project;
            });
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            manager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            manager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act

            let newProject = manager.ProjectCollection[0];

            manager.CurrentProject = newProject;
            //Asserts
            expect(changedProject).toEqual(newProject);
        }));

        it("Set current artifact", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let changed;
            let func = function (artifact: Models.IArtifact) {
                changed = artifact;
            };

            manager.subscribe(SubscriptionEnum.CurrentArtifactChanged, func);

            //Act
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            //Asserts
            expect(changed).toEqual(manager.CurrentArtifact);
        }));

        it("Current artifact has changed", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let changed;
            manager.subscribe(SubscriptionEnum.CurrentArtifactChanged, function (artifact: Models.IArtifact) {
                changed = artifact;
            });
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            //Act
            let projectid = manager.CurrentProject.id;
            let artifact = manager.ProjectCollection[0].artifacts[2];
            manager.CurrentArtifact = artifact;
            //Asserts

            expect(changed).toEqual(artifact);
            expect(projectid).toEqual(manager.CurrentProject.id);

        }));
        it("Current artifact hasn't changed", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let changed;
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            //Act
            manager.CurrentArtifact = manager.CurrentProject.artifacts[0];

            manager.subscribe(SubscriptionEnum.CurrentArtifactChanged, function (artifact: Models.IArtifact) {
                changed = artifact;
            });
            manager.CurrentArtifact = manager.ProjectCollection[0].artifacts[0];
            //Asserts

            expect(manager.CurrentArtifact).toBeDefined();
            expect(changed).toBeUndefined();
        }));

        it("Current artifact has changed for multiple project", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let changed;
            manager.subscribe(SubscriptionEnum.CurrentArtifactChanged, function (artifact: Models.IArtifact) {
                changed = artifact;
            });

            //Act
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();
            manager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            $rootScope.$digest();
            manager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            let projectid = manager.CurrentProject.id;
            let artifact = manager.ProjectCollection[1].artifacts[1];
            manager.CurrentArtifact = artifact;

            //Asserts
            expect(changed).toEqual(artifact);
            expect(manager.CurrentProject.id).toEqual(2);

        }));

    });

    describe("Delete Project: ", () => {
        it("Delete current project", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let changedProject;
            let func = function (project: Models.IProject) {
                changedProject = project;
            };

            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            manager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            manager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act
            manager.notify(SubscriptionEnum.ProjectClose );

            //Asserts
            expect(manager.ProjectCollection.length).toBe(2);
            expect(manager.CurrentProject.id).toBe(2);
        }));
        it("Delete all projects", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            let changedProject;
            let func = function (project: Models.IProject) {
                changedProject = project;
            };
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            manager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            manager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act
            manager.notify(SubscriptionEnum.ProjectClose, true);

            //Asserts
            expect(manager.ProjectCollection.length).toBe(0);
            expect(manager.CurrentProject).toBeNull();
        }));
    });

    describe("Select Artifact ", () => {
        it("Select Artifact successful", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            manager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            manager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act

            let artifact = manager.selectArtifact(202);

            //Asserts
            expect(artifact).toBeDefined();
            expect(artifact.projectId).toBeDefined(2);
        }));
        it("Select Artifact unsuccessful", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            // Arrange
            manager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            manager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            manager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act

            let artifact = manager.selectArtifact(305);

            //Asserts
            expect(artifact).toBeUndefined();
        }));
    });

});