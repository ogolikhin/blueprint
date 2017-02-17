import "angular";
import "angular-mocks";
import "rx/dist/rx.lite.js";
import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";
import {ItemInfoServiceMock} from "../../../commonModule/itemInfo/itemInfo.service.mock";
import {LoadingOverlayServiceMock} from "../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {MainBreadcrumbServiceMock} from "../bp-page-content/mainbreadcrumb.svc.mock";
import {MoveCopyArtifactInsertMethod} from "../dialogs/move-copy-artifact/move-copy-artifact";
import {MessageType} from "../messages/message";
import {MessageServiceMock} from "../messages/message.mock";
import {ItemTypePredefined} from "../../models/itemTypePredefined.enum";
import {AdminStoreModels, Models, TreeModels} from "../../models";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {StatefulArtifactMock} from "../../../managers/artifact-manager/artifact/artifact.mock";
import {MetaDataServiceMock} from "../../../managers/artifact-manager/metadata/metadata.svc.mock";
import {SelectionManagerMock} from "../../../managers/selection-manager/selection-manager.mock";
import {ProjectServiceStatusCode, IProjectService} from "../../../managers/project-manager/project-service";
import {ProjectServiceMock} from "../../../managers/project-manager/project-service.mock";
import {IProjectExplorerService, ProjectExplorerService} from "./project-explorer.service";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {ExplorerNodeVM} from "../../models/tree-node-vm-factory";

xdescribe("ProjectExplorerService", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("metadataService", MetaDataServiceMock);
        $provide.service("itemInfoService", ItemInfoServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("projectService", ProjectServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("mainbreadcrumbService", MainBreadcrumbServiceMock);
        $provide.service("projectExplorerService", ProjectExplorerService);
    }));

    const starterProject = {
        id: 10,
        name: "oldName",
        parentFolderId: undefined,
        type: AdminStoreModels.InstanceItemType.Project,
        hasChildren: true,
        projectId: 10,
        itemTypeId: ItemTypePredefined.Project,
        prefix: "PR",
        itemTypeName: "Project",
        predefinedType: ItemTypePredefined.Project
    } as AdminStoreModels.IInstanceItem;

    let projectExplorerService: IProjectExplorerService;
    let selectionManager: ISelectionManager;
    let projectService: IProjectService;

    beforeEach(inject(($q: ng.IQService,
                       $compile: ng.ICompileService,
                       $rootScope: ng.IRootScopeService,
                       _projectExplorerService_: IProjectExplorerService,
                       _selectionManager_: ISelectionManager,
                       _projectService_: IProjectService) => {

        projectExplorerService = _projectExplorerService_;
        selectionManager = _selectionManager_;
        projectService = _projectService_;

        const artifact = new StatefulArtifactMock($q);
        artifact.id = 20;
        artifact.projectId = 10;
        artifact.parentId = 10;
        selectionManager.setArtifact(artifact);
        // projectManager.initialize();

        const factory = new TreeModels.TreeNodeVMFactory(projectService);
        const projectNode = factory.createExplorerNodeVM(starterProject, true);
        projectExplorerService.projects.unshift(projectNode);
    }));

    describe("add project", () => {
        it("single project success", (inject(($rootScope: ng.IRootScopeService) => {
            // Arrange

            //Act
            let error: Error;
            projectExplorerService.add(11).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectExplorerService.projects.length).toEqual(2);
            expect(projectExplorerService.projects[0].model.id).toEqual(11);
            expect(projectExplorerService.projects[1].model.id).toEqual(10);
        })));

        it("single project metadata service error failure", (inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                                                                     metadataService: MetaDataServiceMock, messageService: MessageServiceMock) => {
            // Arrange
            spyOn(metadataService, "get").and.callFake(() => {
                return $q.reject("error text");
            });

            //Act
            let error: Error;
            projectExplorerService.add(11).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeDefined();
            expect(messageService.messages.length).toEqual(1);
            expect(messageService.messages[0].messageType).toEqual(MessageType.Error);
            expect(messageService.messages[0].messageText).toEqual("error text");
        })));
    });

    describe("dispose", () => {
        it("success", (inject(($rootScope: ng.IRootScopeService) => {
            // Arrange

            //Act
            // FIXME:
            // projectExplorerService.dispose();
            $rootScope.$digest();

            //Asserts
            expect(projectExplorerService.projects.length).toEqual(0);
        })));
    });

    describe("get selected project id", () => {
        it("success", (inject(($rootScope: ng.IRootScopeService) => {
            // Arrange

            //Act
            // FIXME:
            // let result: number = projectExplorerService.getSelectedProjectId();

            //Asserts
            // expect(result).toEqual(10);
        })));
    });

    describe("open project and expand to node", () => {
        it("success", (inject(($q: ng.IQService, $rootScope: ng.IRootScopeService) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake(() => {
                return $q.resolve([<Models.IArtifact>{
                    id: 20,
                    name: "artifact",
                    hasChildren: true,
                    children: [<Models.IArtifact>{
                        id: 25,
                        name: "new artifact"
                    }]

                }]);
            });
            spyOn(projectService, "getProject").and.callFake(() => {
                return $q.resolve(<AdminStoreModels.IInstanceItem>{
                    id: 10,
                    name: "newName"
                });
            });

            //Act
            let error: Error;
            projectExplorerService.openProjectAndExpandToNode(10, 25).catch((err) => error = err);
            $rootScope.$digest();
            const project = projectExplorerService.projects[0] as ExplorerNodeVM;

            //Asserts
            expect(error).toBeUndefined();
            expect(projectExplorerService.projects.length).toEqual(1);
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("newName");
            expect(project.children.length).toEqual(1);
            expect(project.children[0].model.id).toEqual(20);
            expect(project.children[0].model.name).toEqual("artifact");
            expect(project.children[0].children.length).toEqual(1);
            expect(project.children[0].children[0].model.id).toEqual(25);
            expect(project.children[0].children[0].model.name).toEqual("new artifact");
        })));
    });

    describe("get descendants to be deleted", () => {
        it("success", (inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectService: ProjectServiceMock) => {
            // Arrange
            spyOn(projectService, "getProject").and.callFake(() => {
                return $q.resolve(<AdminStoreModels.IInstanceItem>{
                    id: 10,
                    name: "newName"
                });
            });
            spyOn(projectService, "getArtifacts").and.callFake(() => {
                return $q.resolve([<Models.IArtifact>{
                    id: 20,
                    name: "artifact"
                }]);
            });
            const artifact = <Models.IArtifact>{
                id: 25,
                projectId: 10
            };

            //Act
            let error: Error;
            let result: Models.IArtifactWithProject[];
            projectExplorerService.getDescendantsToBeDeleted(artifact)
                .then((res: Models.IArtifactWithProject[]) => result = res)
                .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result.length).toEqual(1);
            expect(result[0].id).toEqual(20);
            expect(result[0].name).toEqual("artifact");
            expect(result[0].projectName).toEqual("newName");
        })));
    });

    describe("remove project", () => {
        it("single success", (inject(($rootScope: ng.IRootScopeService) => {
            // Arrange

            //Act
            projectExplorerService.remove(10);
            $rootScope.$digest();

            //Asserts
            expect(projectExplorerService.projects.length).toEqual(0);
        })));

        it("all success", (inject(($rootScope: ng.IRootScopeService,
                                   projectService: ProjectServiceMock,
                                   selectionManager: SelectionManagerMock) => {
            // Arrange
            let factory = new TreeModels.TreeNodeVMFactory(projectService);
            const project = {
                id: 12,
                name: "oldName 2",
                parentFolderId: undefined,
                type: AdminStoreModels.InstanceItemType.Project,
                hasChildren: true,
                projectId: 12,
                itemTypeId: ItemTypePredefined.Project,
                prefix: "PR",
                itemTypeName: "Project",
                predefinedType: ItemTypePredefined.Project
            } as AdminStoreModels.IInstanceItem;
            let projectNode = factory.createExplorerNodeVM(project, true);

            projectExplorerService.projects.unshift(projectNode);

            //Act
            projectExplorerService.removeAll();
            $rootScope.$digest();

            //Asserts
            expect(projectExplorerService.projects.length).toEqual(0);
        })));
    });

    describe("open project with dialog", () => {
        it("success", (inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, dialogService: DialogServiceMock) => {
            // Arrange
            const project = {
                id: 11,
                name: "oldName 11",
                parentFolderId: undefined,
                type: AdminStoreModels.InstanceItemType.Project,
                hasChildren: true,
                projectId: 11,
                itemTypeId: ItemTypePredefined.Project,
                prefix: "PR",
                itemTypeName: "Project",
                predefinedType: ItemTypePredefined.Project
            } as AdminStoreModels.IInstanceItem;
            spyOn(dialogService, "open").and.callFake(() => {
                return $q.resolve(
                    project
                );
            });

            //Act
            projectExplorerService.openProjectWithDialog();
            $rootScope.$digest();

            //Asserts
            expect(projectExplorerService.projects.length).toEqual(2);
            expect(projectExplorerService.projects[0].model.id).toEqual(project.id);
            expect(projectExplorerService.projects[1].model.id).toEqual(starterProject.id);
        })));
    });

    describe("open project", () => {
        it("success", (inject(($q: ng.IQService, $rootScope: ng.IRootScopeService) => {
            // Act
            projectExplorerService.openProject(starterProject);
            $rootScope.$digest();

            // Assert
            expect(projectExplorerService.projects.length).toEqual(1);
            expect(projectExplorerService.projects[0].model.id).toEqual(starterProject.id);
        })));
    });

    describe("refresh project", () => {
        it("all projects success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                                           projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake(() => {
                return $q.resolve([<Models.IArtifact>{
                    id: 20,
                    name: "artifact",
                    hasChildren: true,
                    children: [<Models.IArtifact>{
                        id: 25,
                        name: "new artifact"
                    }]

                }]);
            });
            spyOn(projectService, "getProject").and.callFake(() => {
                return $q.resolve(<AdminStoreModels.IInstanceItem>{
                    id: 10,
                    name: "newName"
                });
            });

            //Act
            let error: Error;
            projectExplorerService.refreshAll().catch((err) => error = err);
            $rootScope.$digest();
            const project = projectExplorerService.projects[0];

            //Asserts
            expect(error).toBeUndefined();
            expect(projectExplorerService.projects.length).toEqual(1);
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("newName");
            expect(project.children.length).toEqual(1);
            expect(project.children[0].model.id).toEqual(20);
            expect(project.children[0].model.name).toEqual("artifact");
            expect(project.children[0].children.length).toEqual(1);
            expect(project.children[0].children[0].model.id).toEqual(25);
            expect(project.children[0].children[0].model.name).toEqual("new artifact");
        }));

        it("single project success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                                             projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake(() => {
                return $q.resolve([<Models.IArtifact>{
                    id: 20,
                    name: "artifact",
                    hasChildren: true,
                    children: [<Models.IArtifact>{
                        id: 25,
                        name: "new artifact"
                    }]

                }]);
            });
            spyOn(projectService, "getProject").and.callFake(() => {
                return $q.resolve(<AdminStoreModels.IInstanceItem>{
                    id: 10,
                    name: "newName"
                });
            });

            //Act
            let error: Error;
            projectExplorerService.refresh(10).catch((err) => error = err);
            $rootScope.$digest();
            const project = projectExplorerService.projects[0];

            //Asserts
            expect(error).toBeUndefined();
            expect(projectExplorerService.projects.length).toEqual(1);
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("newName");
            expect(project.children.length).toEqual(1);
            expect(project.children[0].model.id).toEqual(20);
            expect(project.children[0].model.name).toEqual("artifact");
            expect(project.children[0].children.length).toEqual(1);
            expect(project.children[0].children[0].model.id).toEqual(25);
            expect(project.children[0].children[0].model.name).toEqual("new artifact");
        }));

        it("single project failure", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                                             projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake(() => {
                return $q.resolve([<Models.IArtifact>{
                    id: 20,
                    name: "artifact",
                    hasChildren: true,
                    children: [<Models.IArtifact>{
                        id: 25,
                        name: "new artifact"
                    }]

                }]);
            });
            spyOn(projectService, "getProject").and.callFake(() => {
                return $q.reject();
            });

            //Act
            let rejected: boolean;
            projectExplorerService.refresh(10).catch(() => rejected = true);
            $rootScope.$digest();
            const project = projectExplorerService.projects[0];

            //Asserts
            expect(projectExplorerService.projects.length).toEqual(1);
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("oldName");
            expect(project.children).toBeUndefined();
            expect(rejected).toEqual(true);
        }));

        it("single project selected artifact not found artifact deleted success",
            inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                    projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock, selectionManager: SelectionManagerMock,
                    messageService: MessageServiceMock) => {
                // Arrange
                spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    return $q.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: ProjectServiceStatusCode.ResourceNotFound
                    });
                });
                spyOn(projectService, "getProject").and.callFake(() => {
                    return $q.resolve(<AdminStoreModels.IInstanceItem>{
                        id: 10,
                        name: "newName"
                    });
                });
                spyOn(projectService, "getArtifacts").and.callFake(() => {
                    return $q.resolve([<Models.IArtifact>{
                        id: 20,
                        name: "artifact"
                    }]);
                });
                const artifact = new StatefulArtifactMock($q);
                artifact.id = 20;
                artifact.projectId = 10;
                artifact.parentId = null;
                selectionManager.setArtifact(artifact);

                //Act
                let error: Error;
                projectExplorerService.refresh(10).catch((err) => error = err);
                $rootScope.$digest();
                const project = projectExplorerService.projects[0];

                //Asserts
                expect(error).toBeUndefined();
                expect(projectExplorerService.projects.length).toEqual(1);
                expect(messageService.messages.length).toEqual(1);
                expect(messageService.messages[0].messageType).toEqual(MessageType.Info);
                expect(messageService.messages[0].messageText).toEqual("Refresh_Artifact_Deleted");
                expect(project.model.id).toEqual(10);
                expect(project.model.name).toEqual("newName");
                expect(project.children.length).toEqual(1);
                expect(project.children[0].model.id).toEqual(20);
                expect(project.children[0].model.name).toEqual("artifact");
            }));

        it("single project selected artifact not found artifact deleted failure",
            inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                    projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock, selectionManager: SelectionManagerMock,
                    messageService: MessageServiceMock, metadataService: MetaDataServiceMock, dialogService: DialogServiceMock) => {
                // Arrange
                spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    return $q.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: ProjectServiceStatusCode.ResourceNotFound
                    });
                });
                spyOn(projectService, "getProject").and.callFake(() => {
                    return $q.resolve(<AdminStoreModels.IInstanceItem>{
                        id: 10,
                        name: "newName"
                    });
                });
                spyOn(projectService, "getArtifacts").and.callFake(() => {
                    return $q.resolve([<Models.IArtifact>{
                        id: 20,
                        name: "artifact"
                    }]);
                });
                spyOn(metadataService, "get").and.callFake(() => {
                    return $q.reject("error text");
                });
                const artifact = new StatefulArtifactMock($q);
                artifact.id = 20;
                artifact.projectId = 10;
                artifact.parentId = null;
                selectionManager.setArtifact(artifact);

                //Act
                let error: Error;
                projectExplorerService.refresh(10).catch((err) => error = err);
                $rootScope.$digest();

                //Asserts
                expect(error).toBeUndefined();
                expect(projectExplorerService.projects.length).toEqual(0);
                expect(dialogService.alerts.length).toEqual(1);
                expect(dialogService.alerts[0]).toEqual("Refresh_Project_NotFound");
                expect(messageService.messages.length).toEqual(1);
                expect(messageService.messages[0].messageType).toEqual(MessageType.Info);
                expect(messageService.messages[0].messageText).toEqual("Refresh_Artifact_Deleted");
            }));


        it("single project no selection success", inject(($q: ng.IQService,
                                                          $rootScope: ng.IRootScopeService,
                                                          projectService: ProjectServiceMock,
                                                          itemInfoService: ItemInfoServiceMock,
                                                          selectionManager: SelectionManagerMock) => {
            // Arrange
            selectionManager.setArtifact(null);

            spyOn(projectService, "getProjectTree").and.callFake(() => {
                return $q.resolve([<Models.IArtifact>{
                    id: 20,
                    name: "artifact",
                    hasChildren: true
                }]);
            });
            spyOn(projectService, "getProject").and.callFake(() => {
                return $q.resolve(<AdminStoreModels.IInstanceItem>{
                    id: 10,
                    name: "newName"
                });
            });

            //Act
            let error: Error;
            projectExplorerService.refresh(10).catch((err) => error = err);
            $rootScope.$digest();
            const project = projectExplorerService.projects[0];

            //Asserts
            expect(error).toBeUndefined();
            expect(projectExplorerService.projects.length).toEqual(1);
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("newName");
            expect(project.children.length).toEqual(1);
            expect(project.children[0].model.id).toEqual(20);
            expect(project.children[0].model.name).toEqual("artifact");
        }));

        it("single project selected artifact not found load parent success",
            inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                    projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
                // Arrange
                spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    if (expandToArtifactId === 20) {
                        return $q.reject({
                            statusCode: HttpStatusCode.NotFound,
                            errorCode: ProjectServiceStatusCode.ResourceNotFound
                        });
                    } else {
                        return $q.resolve([<Models.IArtifact>{
                            id: 25,
                            name: "new artifact"
                        }]);
                    }
                });
                spyOn(projectService, "getProject").and.callFake(() => {
                    return $q.resolve(<AdminStoreModels.IInstanceItem>{
                        id: 10,
                        name: "newName"
                    });
                });

                //Act
                let error: Error;
                projectExplorerService.refresh(10).catch((err) => error = err);
                $rootScope.$digest();
                const project = projectExplorerService.projects[0];

                //Asserts
                expect(error).toBeUndefined();
                expect(projectExplorerService.projects.length).toEqual(1);
                expect(project.model.id).toEqual(10);
                expect(project.model.name).toEqual("newName");
                expect(project.children.length).toEqual(1);
                expect(project.children[0].model.id).toEqual(25);
                expect(project.children[0].model.name).toEqual("new artifact");
            }));

        it("single project selected artifact not found load parent failure",
            inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                    projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock, metadataService: MessageServiceMock) => {
                // Arrange
                spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    if (expandToArtifactId === 20) {
                        return $q.reject({
                            statusCode: HttpStatusCode.NotFound,
                            errorCode: ProjectServiceStatusCode.ResourceNotFound
                        });
                    } else {
                        return $q.resolve([<Models.IArtifact>{
                            id: 25,
                            name: "new artifact"
                        }]);
                    }
                });
                spyOn(metadataService, "get").and.callFake(() => {
                    return $q.reject("error text");
                });

                //Act
                let rejected: boolean;
                projectExplorerService.refresh(10).catch(() => rejected = true);
                $rootScope.$digest();

                //Asserts
                expect(projectExplorerService.projects.length).toEqual(1);
                let project = projectExplorerService.projects[0];
                expect(project.model.id).toEqual(10);
                expect(project.model.name).toEqual("oldName");
                expect(rejected).toEqual(true);
            }));

        it("single project selected artifact not found project not found failure",
            inject(($q: ng.IQService,
                    $rootScope: ng.IRootScopeService,
                    projectService: ProjectServiceMock,
                    itemInfoService: ItemInfoServiceMock,
                    selectionManager: SelectionManagerMock,
                    dialogService: DialogServiceMock) => {
                // Arrange
                spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    return $q.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: ProjectServiceStatusCode.ResourceNotFound
                    });
                });
                const artifact = new StatefulArtifactMock($q);
                artifact.id = 10;
                artifact.projectId = 10;
                artifact.parentId = 10;
                selectionManager.setArtifact(artifact);

                //Act
                let error: Error;
                projectExplorerService.refresh(10).catch((err) => error = err);
                $rootScope.$digest();

                //Asserts
                expect(error).toBeUndefined();
                expect(projectExplorerService.projects.length).toEqual(0);
                expect(dialogService.alerts.length).toEqual(1);
                expect(dialogService.alerts[0]).toEqual("Refresh_Project_NotFound");
            }));

        it("single project selected artifact other error failure",
            inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                    projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock, messageService: MessageServiceMock) => {
                // Arrange
                spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    return $q.reject({statusCode: HttpStatusCode.ServerError, message: "error message"});
                });

                //Act
                projectExplorerService.refresh(10);
                $rootScope.$digest();
                const project = projectExplorerService.projects[0];

                //Asserts
                expect(projectExplorerService.projects.length).toEqual(1);
                expect(project.model.id).toEqual(10);
                expect(project.model.name).toEqual("oldName");
                expect(project.children).toBeUndefined();
                expect(messageService.messages[0].messageType).toEqual(MessageType.Error);
                expect(messageService.messages[0].messageText).toEqual("error message");
            }));

        it("single project selected artifact not found load parent other error failure",
            inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                    projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock, messageService: MessageServiceMock) => {
                // Arrange
                spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    if (expandToArtifactId === 20) {
                        return $q.reject({
                            statusCode: HttpStatusCode.NotFound,
                            errorCode: ProjectServiceStatusCode.ResourceNotFound
                        });
                    } else {
                        return $q.reject({statusCode: HttpStatusCode.ServerError, message: "error message"});
                    }
                });

                //Act
                let error: Error;
                projectExplorerService.refresh(10).catch((err) => error = err);
                $rootScope.$digest();
                const project = projectExplorerService.projects[0];

                //Asserts
                expect(error).toBeUndefined();
                expect(projectExplorerService.projects.length).toEqual(1);
                expect(project.model.id).toEqual(10);
                expect(project.model.name).toEqual("oldName");
                expect(project.children).toBeUndefined();
                expect(messageService.messages[0].messageType).toEqual(MessageType.Error);
                expect(messageService.messages[0].messageText).toEqual("error message");
            }));

        it("single project selected artifact not found load project success",
            inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                    projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
                // Arrange
                spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    return $q.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: ProjectServiceStatusCode.ResourceNotFound
                    });
                });
                spyOn(projectService, "getProject").and.callFake(() => {
                    return $q.resolve(<AdminStoreModels.IInstanceItem>{
                        id: 10,
                        name: "newName"
                    });
                });
                spyOn(projectService, "getArtifacts").and.callFake(() => {
                    return $q.resolve([<Models.IArtifact>{
                        id: 20,
                        name: "artifact"
                    }]);
                });

                //Act
                let error: Error;
                projectExplorerService.refresh(10).catch((err) => error = err);
                $rootScope.$digest();
                const project = projectExplorerService.projects[0];

                //Asserts
                expect(error).toBeUndefined();
                expect(projectExplorerService.projects.length).toEqual(1);
                expect(project.model.id).toEqual(10);
                expect(project.model.name).toEqual("newName");
                expect(project.children.length).toEqual(1);
                expect(project.children[0].model.id).toEqual(20);
                expect(project.children[0].model.name).toEqual("artifact");
            }));
    });

    describe("calculate order index", () => {
        beforeEach(inject(($q: ng.IQService, projectService: ProjectServiceMock) => {
            spyOn(projectService, "getArtifacts").and.callFake(() => {
                return $q.resolve([<Models.IArtifact>{
                    id: 20,
                    name: "artifact",
                    orderIndex: 10
                },
                    <Models.IArtifact>{
                        id: 21,
                        name: "another artifact",
                        orderIndex: 15
                    }]);
            });
        }));

        it("inside method success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                                            projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 20,
                orderIndex: 10
            };

            //Act
            let error: Error;
            let result: number;
            projectExplorerService.calculateOrderIndex(MoveCopyArtifactInsertMethod.Inside, artifact)
                .then((res) => {
                    result = res;
                })
                .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result).toBeUndefined();
        }));

        it("below method between success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                                                   projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 20,
                orderIndex: 10
            };

            //Act
            let error: Error;
            let result: number;
            projectExplorerService.calculateOrderIndex(MoveCopyArtifactInsertMethod.Below, artifact)
                .then((res) => {
                    result = res;
                })
                .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result).toEqual(12.5);
        }));

        it("above method top success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                                               projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 20,
                orderIndex: 10
            };

            //Act
            let error: Error;
            let result: number;
            projectExplorerService.calculateOrderIndex(MoveCopyArtifactInsertMethod.Above, artifact)
                .then((res) => {
                    result = res;
                })
                .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result).toEqual(5);
        }));

        it("below method bottom success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                                                  projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 21,
                orderIndex: 15
            };

            //Act
            let error: Error;
            let result: number;
            projectExplorerService.calculateOrderIndex(MoveCopyArtifactInsertMethod.Below, artifact)
                .then((res) => {
                    result = res;
                })
                .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result).toEqual(25);
        }));

        it("above method between success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService,
                                                   projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 21,
                orderIndex: 15
            };

            //Act
            let error: Error;
            let result: number;
            projectExplorerService.calculateOrderIndex(MoveCopyArtifactInsertMethod.Above, artifact)
                .then((res) => {
                    result = res;
                })
                .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result).toEqual(12.5);
        }));
    });
});
