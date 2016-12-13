import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {SelectionManagerMock} from "../selection-manager/selection-manager.mock";
import {ProjectManager, IProjectManager, IArtifactNode} from "./project-manager";
import {Models, AdminStoreModels, Enums, TreeModels} from "../../main/models";
import {IItemInfoResult} from "../../core/navigation/item-info.svc";
import {ItemInfoServiceMock} from "../../core/navigation/item-info.svc.mock";
import {MetaDataServiceMock} from "../artifact-manager/metadata/metadata.svc.mock";
import {StatefulArtifactFactoryMock} from "../artifact-manager/artifact/artifact.factory.mock";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {DialogServiceMock} from "../../shared/widgets/bp-dialog/bp-dialog.mock";
import {LoadingOverlayServiceMock} from "../../core/loading-overlay/loading-overlay.svc.mock";
import {MainBreadcrumbServiceMock} from "../../main/components/bp-page-content/mainbreadcrumb.svc.mock";
import {AnalyticsProviderMock} from "../../main/components/analytics/analyticsProvider.mock";
import {ProjectServiceMock} from "./project-service.mock";
import {ArtifactManagerMock} from "../../managers/artifact-manager/artifact-manager.mock";
import {StatefulArtifactMock} from "../../managers/artifact-manager/artifact/artifact.mock";
import {ProjectServiceStatusCode} from "./project-service";
import {HttpStatusCode} from "../../core/http/http-status-code";

describe("Project Manager Test", () => {

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("metadataService", MetaDataServiceMock);
        $provide.service("itemInfoService", ItemInfoServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("projectManager", ProjectManager);
        $provide.service("projectService", ProjectServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("mainbreadcrumbService", MainBreadcrumbServiceMock);
        $provide.service("analytics", AnalyticsProviderMock);
    }));
    beforeEach(inject(($q: ng.IQService, $compile: ng.ICompileService, $rootScope: ng.IRootScopeService, projectManager: ProjectManager,
        selectionManager: SelectionManagerMock, statefulArtifactFactory: StatefulArtifactFactoryMock, artifactManager: ArtifactManagerMock,
        projectService: ProjectServiceMock) => {
        artifactManager.selection = selectionManager;
        const artifact = new StatefulArtifactMock($q);
        artifact.id = 20;
        artifact.projectId = 10;
        artifact.parentId = 10;
        selectionManager.setArtifact(artifact);
        projectManager.initialize();
        let factory = new TreeModels.TreeNodeVMFactory(projectService, artifactManager, statefulArtifactFactory);

        const project = {
                id: 10,
                name: "oldName",
                parentFolderId: undefined,
                type: AdminStoreModels.InstanceItemType.Project,
                hasChildren: true,
                projectId: 10,
                itemTypeId: Enums.ItemTypePredefined.Project,
                prefix: "PR",
                itemTypeName: "Project",
                predefinedType: Enums.ItemTypePredefined.Project
                //permissions: projectInfo.permissions
        } as AdminStoreModels.IInstanceItem;
        const statefulArtifact = statefulArtifactFactory.createStatefulArtifact(project);
        let projectNode = factory.createStatefulArtifactNodeVM(statefulArtifact, true);

        projectManager.projectCollection.getValue().unshift(projectNode);
        projectManager.projectCollection.getValue().unshift(factory.createStatefulArtifactNodeVM(artifact));
    }));

    describe("add project", () => {
        it("single project success", (inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager) => {
            // Arrange
            
            //Act
            let error: Error;
            projectManager.add(11).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectManager.projectCollection.getValue().length === 3);
            expect(projectManager.projectCollection.getValue()[0].model.id === 10);
            expect(projectManager.projectCollection.getValue()[1].model.id === 20);
            expect(projectManager.projectCollection.getValue()[2].model.id === 11);
        })));
     });

     describe("refresh project", () => {
        it("single project success", (inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake(() => {
                    return $q.resolve([<Models.IArtifact>{
                            id: 10,
                            name: "newName"
                    }]);
            });

            //Act
            let error: Error;
            projectManager.refresh(10).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectManager.projectCollection.getValue().length === 2);
            expect(projectManager.projectCollection.getValue()[0].model.id === 10);
            expect(projectManager.projectCollection.getValue()[0].model.name === "newName");
        })));

        it("single project selected artifact not found", (inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    if (expandToArtifactId === 20) {
                            return $q.reject({statusCode: HttpStatusCode.NotFound, errorCode: ProjectServiceStatusCode.ResourceNotFound});
                    } else {
                            return $q.resolve([<Models.IArtifact>{
                            id: 10,
                            name: "newName"
                    }]);
                    }
                
            });
           
            //Act
            let error: Error;
            projectManager.refresh(10).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectManager.projectCollection.getValue().length === 2);
            expect(projectManager.projectCollection.getValue()[0].model.id === 10);
            expect(projectManager.projectCollection.getValue()[0].model.name === "newName");
        })));
     });


    /*describe("Load projects: ", () => {
        it("Single project", (() => {inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager, selectionManager: SelectionManager) => {
            // Arrange
            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
            $rootScope.$digest();
            //Act
            let project = selectionManager.selection.artifact;

            //Asserts
            expect(project).toBeDefined();
            expect(project.id).toEqual(1);
            expect(project.name).toEqual("Project 1");
        }));

        it("Multiple projects", (() => {inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager, selectionManager: SelectionManager) => {
            // Arrange
            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
            projectManager.loadProject(new Models.Project({id: 2, name: "Project 2"}));
            $rootScope.$digest();

            //Act
            let current = selectionManager.selection.artifact;
            let projects = projectManager.projectCollection.getValue();

            //Asserts
            expect(projects).toEqual(jasmine.any(Array));
            expect(projects.length).toEqual(2);
            expect(current).toEqual(projects[0]);
        }));

        it("Load project children", (() => {inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager, selectionManager: SelectionManager) => {
            // Arrange

            projectManager.loadProject({id: 1, name: "Project 1"} as Models.Project);
            $rootScope.$digest();

            projectManager.loadArtifact({id: 10} as Models.IArtifact);
            $rootScope.$digest();

            //Act
            let artifact = selectionManager.selection.artifact;

            //Asserts
            expect(artifact).toBeDefined();
            expect(artifact.artifacts).toEqual(jasmine.any(Array));
            expect(artifact.artifacts.length).toEqual(5);
            expect(artifact.artifacts[0].id).toEqual(1000);
        }));
        it("Load project children. Null. Project not found", (() => {inject(($rootScope: ng.IRootScopeService,
            projectManager: IProjectManager, messageService: MessageService) => {
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
        it("Load project children. undefined. Artifact not found", (() => {inject(($rootScope: ng.IRootScopeService,
            projectManager: IProjectManager, messageService: MessageService) => {
            // Arrange
            projectManager.loadProject({id: 1, name: "Project 1"} as Models.Project);
            projectManager.loadArtifact(undefined as Models.IArtifact);
            $rootScope.$digest();

            //Act
            let messages = messageService.messages;

            //Asserts
            expect(messages).toEqual(jasmine.any(Array));
            expect(messages.length).toBe(1);
            expect(messages[0].messageText).toBe("Artifact_NotFound");

        }));
        it("Load project children. Null. Artifact not found", (() => {inject(($rootScope: ng.IRootScopeService,
            projectManager: IProjectManager, messageService: MessageService) => {
            // Arrange
            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
            projectManager.loadArtifact(null as Models.IArtifact);
            $rootScope.$digest();

            //Act
            let messages = messageService.messages;

            //Asserts
            expect(messages).toEqual(jasmine.any(Array));
            expect(messages.length).toBe(0);

        }));
        it("Load project children. Undefined. Artifact not found", (() => {inject(($rootScope: ng.IRootScopeService,
            projectManager: IProjectManager, messageService: MessageService) => {
            // Arrange
            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
            projectManager.loadArtifact(undefined as Models.IArtifact);
            $rootScope.$digest();

            //Act
            let messages = messageService.messages;

            //Asserts
            expect(messages).toEqual(jasmine.any(Array));
            expect(messages.length).toBe(1);
            expect(messages[0].messageText).toBe("Artifact_NotFound");

        }));
        it("Load project children. Project not found", (() => {inject(($rootScope: ng.IRootScopeService,
            projectManager: IProjectManager, messageService: MessageService) => {
            // Arrange
            //let error;
            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
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
        it("Load project children. Artifact not found", (() => {inject(($rootScope: ng.IRootScopeService,
            projectManager: IProjectManager, messageService: MessageService) => {
            // Arrange
            //let error;
            projectManager.loadProject({id: 1, name: "Project 1"} as Models.IProject);
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

    });*/

    /*describe("Current Artifact: ", () => {
        it("Current artifact has changed", (() => {//
            inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager, selectionManager: SelectionManagerMock) => {
            // Arrange
            let changedArtifact: Models.IArtifact[] = [];

            selectionManager.selectedArtifactObservable.subscribe((artifact: Models.IArtifact) => {
                changedArtifact.push(artifact);
            });

            //Act
            projectManager.loadProject({id: 1, name: "Project 1"} as Models.Project);
            $rootScope.$digest();
            projectManager.loadArtifact(ProjectRepositoryMock.createArtifact(10, 1));
            $rootScope.$digest();

            //Asserts
            expect(changedArtifact).toBeDefined();
            expect(changedArtifact).toEqual(jasmine.any(Array));
            expect(changedArtifact.length).toBe(2);
            expect(changedArtifact[0]).toBeDefined();
            expect(changedArtifact[0].id).toEqual(1);
            expect(changedArtifact[1].id).toEqual(10);

        }));
        it("Current artifact hasn't changed", (() => {//
            inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager, selectionManager: SelectionManagerMock) => {
            // Arrange
            let changedArtifact: Models.IArtifact[] = [];

            selectionManager.selectedArtifactObservable.subscribe((artifact: Models.IArtifact) => {
                changedArtifact.push(artifact);
            });

            //Act
            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
            $rootScope.$digest();

            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
            $rootScope.$digest();

            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
            $rootScope.$digest();

            //Asserts


            expect(changedArtifact).toBeDefined();
            expect(changedArtifact).toEqual(jasmine.any(Array));
            expect(changedArtifact.length).toBe(1);
            expect(changedArtifact[0].name).toBe("Project 1");

        }));
    });

    describe("Delete Project: ", () => {
        it("Delete current project", (() => {//
            inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager, selectionManager: SelectionManagerMock) => {
            // Arrange
            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
            $rootScope.$digest();
            projectManager.loadProject(new Models.Project({id: 2, name: "Project 2"}));
            $rootScope.$digest();
            projectManager.loadProject(new Models.Project({id: 3, name: "Project 3"}));
            $rootScope.$digest();

            //Act
            let first = selectionManager.selection.artifact;
            projectManager.closeProject();
            $rootScope.$digest();
            let second = selectionManager.selection.artifact;
            let projects = projectManager.projectCollection.getValue();

            //Asserts
            expect(projects).toEqual(jasmine.any(Array));
            expect(projects.length).toBe(2);
            expect(first.id).toBe(3);
            expect(second.id).toBe(2);
        }));
        it("Delete all projects", (() => {inject(($rootScope: ng.IRootScopeService, 
                projectManager: IProjectManager, selectionManager: SelectionManagerMock) => {
            // Arrange
            projectManager.loadProject(new Models.Project({id: 1, name: "Project 1"}));
            $rootScope.$digest();
            projectManager.loadProject(new Models.Project({id: 2, name: "Project 2"}));
            $rootScope.$digest();
            projectManager.loadProject(new Models.Project({id: 3, name: "Project 3"}));
            $rootScope.$digest();


            //Act
            projectManager.closeProject(true);
            $rootScope.$digest();
            let projects = projectManager.projectCollection.getValue();
            let artifact = selectionManager.selection.artifact;


            //Asserts
            expect(projects).toEqual(jasmine.any(Array));
            expect(projects.length).toBe(0);
            expect(artifact).toBeNull();
        }));
    });*/

});
