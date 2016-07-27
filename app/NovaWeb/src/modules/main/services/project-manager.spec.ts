import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {ConfigValueHelper } from "../../core";
import {MessageService} from "../../shell/";
import {ProjectRepositoryMock} from "./project-repository.mock";
import {ProjectManager, Models} from "../";


describe("Project Manager Test", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("configValueHelper", ConfigValueHelper);
        $provide.service("messageService", MessageService);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
    }));
    beforeEach(inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
        $rootScope["config"] = {
            "settings": {
                "StorytellerMessageTimeout": `{ "Warning": 0, "Info": 3000, "Error": 0 }`
            }
        };
        projectManager.initialize();
    }));

    describe("Load projects: ", () => {
        it("Single project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();
            //Act
            let project = projectManager.currentProject.getValue();

            //Asserts
            expect(project).toBeDefined();
            expect(project.id).toEqual(1);
            expect(project.name).toEqual("Project 1");
        }));

        it("Multiple projects", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            projectManager.loadProject(new Models.Project({ id: 2, name: "Project 2" }));
            $rootScope.$digest();

            //Act
            let current = projectManager.currentProject.getValue();
            let projects = projectManager.projectCollection.getValue();

            //Asserts
            expect(projects).toEqual(jasmine.any(Array));
            expect(projects.length).toEqual(2);
            expect(current).toEqual(projects[0]);
        }));

        it("Load project children", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange

            projectManager.loadProject({ id: 1, name: "Project 1" } as Models.Project);
            $rootScope.$digest();

//            let project: Models.IProject = projectManager.currentProject.getValue();

            projectManager.loadArtifact({ id: 10 } as Models.IArtifact);
            $rootScope.$digest();

            //Act
            let artifact = projectManager.currentProject.getValue().artifacts[0];

            //Asserts
            expect(artifact).toBeDefined();
            expect(artifact.artifacts).toEqual(jasmine.any(Array));
            expect(artifact.artifacts.length).toEqual(5);
            expect(artifact.artifacts[0].id).toEqual(1000);
        }));
        it("Load project children. Null. Project not found", inject(($rootScope: ng.IRootScopeService,
            projectManager: ProjectManager, messageService: MessageService) => {
            // Arrange
            //let error;
            projectManager.loadProject(null as Models.IProject);
            $rootScope.$digest();

            //Act
            let messages = messageService.messages;

            //Asserts
            expect(messages).toEqual(jasmine.any(Array));
            expect(messages.length).toBe(1);
            expect(messages[0].messageText).toBe("Project_NotFound");

        }));
        it("Load project children. undefined. Artifact not found", inject(($rootScope: ng.IRootScopeService,
            projectManager: ProjectManager, messageService: MessageService) => {
            // Arrange
            projectManager.loadProject({ id: 1, name: "Project 1" } as Models.Project);
            projectManager.loadArtifact(undefined as Models.IArtifact);
            $rootScope.$digest();

            //Act
            let messages = messageService.messages;

            //Asserts
            expect(messages).toEqual(jasmine.any(Array));
            expect(messages.length).toBe(1);
            expect(messages[0].messageText).toBe("Artifact_NotFound");

        }));
        it("Load project children. Null. Artifact not found", inject(($rootScope: ng.IRootScopeService,
            projectManager: ProjectManager, messageService: MessageService) => {
            // Arrange
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            projectManager.loadArtifact(null as Models.IArtifact);
            $rootScope.$digest();

            //Act
            let messages = messageService.messages;

            //Asserts
            expect(messages).toEqual(jasmine.any(Array));
            expect(messages.length).toBe(0);

        }));
        it("Load project children. Undefined. Artifact not found", inject(($rootScope: ng.IRootScopeService,
            projectManager: ProjectManager, messageService: MessageService) => {
            // Arrange
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            projectManager.loadArtifact(undefined as Models.IArtifact);
            $rootScope.$digest();

            //Act
            let messages = messageService.messages;

            //Asserts
            expect(messages).toEqual(jasmine.any(Array));
            expect(messages.length).toBe(1);
            expect(messages[0].messageText).toBe("Artifact_NotFound");

        }));
        it("Load project children. Project not found", inject(($rootScope: ng.IRootScopeService,
            projectManager: ProjectManager, messageService: MessageService) => {
            // Arrange
            //let error;
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();
            projectManager.loadArtifact(ProjectRepositoryMock.createArtifact(88, 2));
            $rootScope.$digest();

            //Act
            let messages = messageService.messages;

            //Asserts
            expect(messages).toEqual(jasmine.any(Array));
            //expect(messages.length).toBe(1);
            //expect(messages[0].messageText).toBe("Artifact_NotFound");

        }));
        it("Load project children. Artifact not found", inject(($rootScope: ng.IRootScopeService,
            projectManager: ProjectManager, messageService: MessageService) => {
            // Arrange
            //let error;
            projectManager.loadProject({ id: 1, name: "Project 1" } as Models.IProject);
            $rootScope.$digest();
            projectManager.loadArtifact(ProjectRepositoryMock.createArtifact(999, 1));
            $rootScope.$digest();

            //Act
            let messages = messageService.messages;

            //Asserts
            expect(messages).toEqual(jasmine.any(Array));
            //expect(messages.length).toBe(1);
            //expect(messages[0].messageText).toBe("Artifact_NotFound");
        }));

    });

    describe("Current Project: ", () => {
        it("Current project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange

            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();

            //Act
            let first = projectManager.currentProject.getValue();

            //Asserts
            expect(first).toBeDefined();
            expect(first.id).toEqual(1);
            expect(first.name).toEqual("Project 1");
        }));

        it("Set Current project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changedProject;

            projectManager.currentProject.subscribe((project: Models.IProject) => {
                changedProject = project;
            });

            //Act
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();

            //Asserts
            expect(changedProject).toBeDefined();
            expect(changedProject.id).toEqual(1);
            expect(changedProject.name).toEqual("Project 1");
        }));

        it("Set Current artifact", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changedArtifact;

            projectManager.currentArtifact.subscribe((artifact: Models.IArtifact) => {
                changedArtifact = artifact;
            });

            //Act
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();

            //Asserts
            expect(changedArtifact).toBeDefined();
            expect(changedArtifact.id).toEqual(1);
            expect(changedArtifact.name).toEqual("Project 1");
            expect(changedArtifact).toEqual(projectManager.currentProject.getValue());
        }));



        it("Current artifact has changed", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changedArtifact: Models.IArtifact[] = [];

            projectManager.currentArtifact.subscribe((artifact: Models.IArtifact) => {
                changedArtifact.push(artifact);
            });

            //Act
            projectManager.loadProject({ id: 1, name: "Project 1" } as Models.Project);
            $rootScope.$digest();
            projectManager.loadArtifact(ProjectRepositoryMock.createArtifact(10, 1));
            $rootScope.$digest();

            //Asserts
            expect(changedArtifact).toBeDefined();
            expect(changedArtifact).toEqual(jasmine.any(Array));
            expect(changedArtifact.length).toBe(3);
            expect(changedArtifact[0]).toBeNull();
            expect(changedArtifact[1].id).toEqual(1);
            expect(changedArtifact[2].id).toEqual(10);

        }));
        it("Current artifact hasn't changed", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            let changedArtifact: Models.IArtifact[] = [];

            projectManager.currentArtifact.subscribe((artifact: Models.IArtifact) => {
                changedArtifact.push(artifact);
            });



            //Act
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();

            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();

            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();

            //Asserts

            
            expect(changedArtifact).toBeDefined();
            expect(changedArtifact).toEqual(jasmine.any(Array));
            expect(changedArtifact.length).toBe(2);
            expect(changedArtifact[0]).toBeNull();

        }));
    });

    describe("Delete Project: ", () => {
        it("Delete current project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();
            projectManager.loadProject(new Models.Project({ id: 2, name: "Project 2" }));
            $rootScope.$digest();
            projectManager.loadProject(new Models.Project({ id: 3, name: "Project 3" }));
            $rootScope.$digest();

            //Act
            let first = projectManager.currentProject.getValue();
            projectManager.closeProject();
            $rootScope.$digest();
            let second = projectManager.currentProject.getValue();
            let projects = projectManager.projectCollection.getValue();

            //Asserts
            expect(projects).toEqual(jasmine.any(Array));
            expect(projects.length).toBe(2);
            expect(first.id).toBe(3);
            expect(second.id).toBe(2);
        }));
        it("Delete all projects", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
            $rootScope.$digest();
            projectManager.loadProject(new Models.Project({ id: 2, name: "Project 2" }));
            $rootScope.$digest();
            projectManager.loadProject(new Models.Project({ id: 3, name: "Project 3" }));
            $rootScope.$digest();


            //Act
            projectManager.closeProject(true);
            $rootScope.$digest();
            let projects = projectManager.projectCollection.getValue();
            let artifact = projectManager.currentArtifact.getValue();
            let project = projectManager.currentProject.getValue();


            //Asserts
            expect(projects).toEqual(jasmine.any(Array));
            expect(projects.length).toBe(0);
            expect(artifact).toBeNull();
            expect(project).toBeNull();
        }));
    });
    describe("properties", () => {
        it("should a project be un-selected", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));

            //Act
            
            //Asserts
            expect(projectManager.isProjectSelected).toBeFalsy();
            expect(projectManager.isArtifactSelected).toBeFalsy();
        }));
        it("should project be selected", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            // Arrange
            projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));

            //Act
            $rootScope.$digest();
            
            //Asserts
            expect(projectManager.isProjectSelected).toBeTruthy();
            expect(projectManager.isArtifactSelected).toBeTruthy();

            projectManager.closeProject();
            $rootScope.$digest();
            expect(projectManager.isProjectSelected).toBeFalsy();
            expect(projectManager.isArtifactSelected).toBeFalsy();
        }));
    });

});