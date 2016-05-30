import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../core/localization";
import {EventManager, EventSubscriber} from "../../core/event-manager";
import {ProjectRepositoryMock} from "../services/project-repository.mock";
import {ProjectManager, Models, SubscriptionEnum} from "../managers/project-manager";


describe("Project Manager Test", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("eventManager", EventManager);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
    }));

    describe("Load projects: ", () => {
        it("Single project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();
            //Act
            let project = projectManager.CurrentProject;

            //Asserts
            expect(project).toBeDefined();
            expect(project.id).toEqual(1);
            expect(project.name).toEqual("Project 1");
        }));

        it("Multiple projects", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1);
            $rootScope.$digest();

            projectManager.notify(SubscriptionEnum.ProjectLoad, 2);
            $rootScope.$digest();

            //Act
            let current = projectManager.CurrentProject;
            let projects = projectManager.ProjectCollection;

            //Asserts
            expect(projects).toEqual(jasmine.any(Array));
            expect(projects.length).toEqual(2);
            expect(current).toEqual(projects[0]);
        }));

        it("Load project children", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange

            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            let project: Models.IProject = projectManager.CurrentProject;

            projectManager.notify(SubscriptionEnum.ProjectChildrenLoad, project.id, 10);
            $rootScope.$digest();

            //Act
            let artifact = projectManager.CurrentProject.artifacts[0];

            //Asserts
            expect(artifact).toBeDefined();
            expect(artifact.artifacts).toEqual(jasmine.any(Array));
            expect(artifact.artifacts.length).toEqual(5);
            expect(artifact.artifacts[0].id).toEqual(1000);
        }));
        it("Load project children. Project not found", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let error;
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            projectManager["eventManager"].attach(EventSubscriber.Main, "exception", function (ex) {
                error = ex;
            })

            //Act
            projectManager.notify(SubscriptionEnum.ProjectChildrenLoad, 5, 5);
            $rootScope.$digest();


            //Asserts
            expect(error.message).toBe("Project_NotFound");
        }));
        it("Load project children. Artifact not found", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let error;
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            projectManager["eventManager"].attach(EventSubscriber.Main, "exception", function (ex) {
                error = ex;
            })

            //Act
            projectManager.notify(SubscriptionEnum.ProjectChildrenLoad, 1, 5);
            $rootScope.$digest();


            //Asserts
            expect(error.message).toBe("Artifact_NotFound");
        }));

    });

    describe("Current Project: ", () => {
        it("Current project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changedProject;
            let func = function (project: Models.IProject) {
                changedProject = project;
            };

            projectManager.subscribe(SubscriptionEnum.ProjectChanged, func);

            //Act
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            //Asserts
            expect(changedProject).toEqual(projectManager.CurrentProject);
        }));

        it("Set Current project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changedProject;
            
            projectManager.subscribe(SubscriptionEnum.ProjectChanged, function (project: Models.IProject) {
                changedProject = project;
            });
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act

            let newProject = projectManager.ProjectCollection[0];

            projectManager.CurrentProject = newProject;
            //Asserts
            expect(changedProject).toEqual(newProject);
        }));

        it("Set current artifact", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changed;
            let func = function (artifact: Models.IArtifact) {
                changed = artifact;
            };

            projectManager.subscribe(SubscriptionEnum.ArtifactChanged, func);

            //Act
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            //Asserts
            expect(changed).toEqual(projectManager.CurrentArtifact);
        }));

        it("Current artifact has changed", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changed;
            projectManager.subscribe(SubscriptionEnum.ArtifactChanged, function (artifact: Models.IArtifact) {
                changed = artifact;
            });
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            //Act
            let projectid = projectManager.CurrentProject.id;
            let artifact = projectManager.ProjectCollection[0].artifacts[2];
            projectManager.CurrentArtifact = artifact;
            //Asserts

            expect(changed).toEqual(artifact);
            expect(projectid).toEqual(projectManager.CurrentProject.id);

        }));
        it("Current artifact hasn't changed", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changed;
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();

            //Act
            projectManager.CurrentArtifact = projectManager.CurrentProject.artifacts[0];

            projectManager.subscribe(SubscriptionEnum.ArtifactChanged, function (artifact: Models.IArtifact) {
                changed = artifact;
            });
            projectManager.CurrentArtifact = projectManager.ProjectCollection[0].artifacts[0];
            //Asserts

            expect(projectManager.CurrentArtifact).toBeDefined();
            expect(changed).toBeUndefined();
        }));

        it("Current artifact has changed for multiple project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changed;
            projectManager.subscribe(SubscriptionEnum.ArtifactChanged, function (artifact: Models.IArtifact) {
                changed = artifact;
            });

            //Act
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            $rootScope.$digest();
            projectManager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            $rootScope.$digest();
            projectManager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            let projectid = projectManager.CurrentProject.id;
            let artifact = projectManager.ProjectCollection[1].artifacts[1];
            projectManager.CurrentArtifact = artifact;

            //Asserts
            expect(changed).toEqual(artifact);
            expect(projectManager.CurrentProject.id).toEqual(2);

        }));

    });

    describe("Delete Project: ", () => {
        it("Delete current project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changedProject;
            let func = function (project: Models.IProject) {
                changedProject = project;
            };

            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act
            projectManager.notify(SubscriptionEnum.ProjectClose );

            //Asserts
            expect(projectManager.ProjectCollection.length).toBe(2);
            expect(projectManager.CurrentProject.id).toBe(2);
        }));
        it("Delete all projects", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changedProject;
            let func = function (project: Models.IProject) {
                changedProject = project;
            };
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act
            projectManager.notify(SubscriptionEnum.ProjectClose, true);

            //Asserts
            expect(projectManager.ProjectCollection.length).toBe(0);
            expect(projectManager.CurrentProject).toBeNull();
        }));
    });

    describe("Select Artifact ", () => {
        it("Select Artifact successful", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act

            let artifact = projectManager.selectArtifact(22);

            //Asserts
            expect(artifact).toBeDefined();
            expect(artifact.projectId).toBeDefined(2);
        }));
        it("Select Artifact unsuccessful", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.notify(SubscriptionEnum.ProjectLoad, 1, "Project 1");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 2, "Project 2");
            projectManager.notify(SubscriptionEnum.ProjectLoad, 3, "Project 3");
            $rootScope.$digest();

            //Act

            let artifact = projectManager.selectArtifact(305);

            //Asserts
            expect(artifact).toBeUndefined();
        }));
    });

});