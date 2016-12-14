﻿import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {SelectionManagerMock} from "../selection-manager/selection-manager.mock";
import {ProjectManager, IProjectManager, IArtifactNode} from "./project-manager";
import {Models, AdminStoreModels, Enums, TreeModels} from "../../main/models";
import {IItemInfoResult} from "../../core/navigation/item-info.svc";
import {ItemInfoServiceMock} from "../../core/navigation/item-info.svc.mock";
import {MetaDataServiceMock} from "../artifact-manager/metadata/metadata.svc.mock";
import {StatefulArtifactFactoryMock} from "../artifact-manager/artifact/artifact.factory.mock";
import {MessageType} from "../../core/messages/message";
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
import {MoveCopyArtifactInsertMethod} from "../../main/components/dialogs/move-copy-artifact/move-copy-artifact";

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
        } as AdminStoreModels.IInstanceItem;
        const statefulArtifact = statefulArtifactFactory.createStatefulArtifact(project);
        statefulArtifact.children = [artifact];
        let projectNode = factory.createStatefulArtifactNodeVM(statefulArtifact, true);

        projectManager.projectCollection.getValue().unshift(projectNode);
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
            expect(projectManager.projectCollection.getValue().length).toEqual(2);
            expect(projectManager.projectCollection.getValue()[0].model.id).toEqual(11);
            expect(projectManager.projectCollection.getValue()[1].model.id).toEqual(10);
        })));
     });

     describe("get descendants to be deleted", () => {
        it("success", (inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, projectService: ProjectServiceMock,
        statefulArtifactFactory: StatefulArtifactFactoryMock) => {
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
            const statefulArtifact = statefulArtifactFactory.createStatefulArtifact(artifact);
            
            //Act
            let error: Error;
            let result: Models.IArtifactWithProject[];
            projectManager.getDescendantsToBeDeleted(statefulArtifact)
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
        it("single success", (inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager) => {
            // Arrange
            
            //Act
            projectManager.remove(10);
            $rootScope.$digest();

            //Asserts
            expect(projectManager.projectCollection.getValue().length).toEqual(0);
        })));

        it("all success", (inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager, projectService: ProjectServiceMock,
        selectionManager: SelectionManagerMock, statefulArtifactFactory: StatefulArtifactFactoryMock, artifactManager: ArtifactManagerMock) => {
            // Arrange
            let factory = new TreeModels.TreeNodeVMFactory(projectService, artifactManager, statefulArtifactFactory);
            const project = {
                id: 12,
                name: "oldName 2",
                parentFolderId: undefined,
                type: AdminStoreModels.InstanceItemType.Project,
                hasChildren: true,
                projectId: 12,
                itemTypeId: Enums.ItemTypePredefined.Project,
                prefix: "PR",
                itemTypeName: "Project",
                predefinedType: Enums.ItemTypePredefined.Project
            } as AdminStoreModels.IInstanceItem;
            const statefulArtifact = statefulArtifactFactory.createStatefulArtifact(project);
            let projectNode = factory.createStatefulArtifactNodeVM(statefulArtifact, true);

            projectManager.projectCollection.getValue().unshift(projectNode);
            
            //Act
            projectManager.removeAll();
            $rootScope.$digest();

            //Asserts
            expect(projectManager.projectCollection.getValue().length).toEqual(0);
        })));
     });

     describe("open project with dialog", () => {
        it("success", (inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, dialogService: DialogServiceMock) => {
            // Arrange
            spyOn(dialogService, "open").and.callFake(() => {
                return $q.resolve(11);
            });
            
            //Act
            projectManager.openProjectWithDialog();
            $rootScope.$digest();

            //Asserts
            expect(projectManager.projectCollection.getValue().length).toEqual(2);
            expect(projectManager.projectCollection.getValue()[0].model.id).toEqual(11);
            expect(projectManager.projectCollection.getValue()[1].model.id).toEqual(10);
        })));
     });

     describe("refresh project", () => {
        it("all projects success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
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
            projectManager.refreshAll().catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectManager.projectCollection.getValue().length).toEqual(1);
            let project = projectManager.projectCollection.getValue()[0];
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("newName");
            expect((<IArtifactNode>project).children.length).toEqual(1);
            expect((<IArtifactNode>project).children[0].model.id).toEqual(20);
            expect((<IArtifactNode>project).children[0].model.name).toEqual("artifact");
            expect((<IArtifactNode>project).children[0].children.length).toEqual(1);
            expect((<IArtifactNode>project).children[0].children[0].model.id).toEqual(25);
            expect((<IArtifactNode>project).children[0].children[0].model.name).toEqual("new artifact");
        }));

        it("single project success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
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
            projectManager.refresh(10).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectManager.projectCollection.getValue().length).toEqual(1);
            let project = projectManager.projectCollection.getValue()[0];
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("newName");
            expect((<IArtifactNode>project).children.length).toEqual(1);
            expect((<IArtifactNode>project).children[0].model.id).toEqual(20);
            expect((<IArtifactNode>project).children[0].model.name).toEqual("artifact");
            expect((<IArtifactNode>project).children[0].children.length).toEqual(1);
            expect((<IArtifactNode>project).children[0].children[0].model.id).toEqual(25);
            expect((<IArtifactNode>project).children[0].children[0].model.name).toEqual("new artifact");
        }));

        it("single project selected artifact not found load parent success", 
        inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                if (expandToArtifactId === 20) {
                    return $q.reject({statusCode: HttpStatusCode.NotFound, errorCode: ProjectServiceStatusCode.ResourceNotFound});
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
            projectManager.refresh(10).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectManager.projectCollection.getValue().length).toEqual(1);
            let project = projectManager.projectCollection.getValue()[0];
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("newName");
            expect((<IArtifactNode>project).children.length).toEqual(1);
            expect((<IArtifactNode>project).children[0].model.id).toEqual(25);
            expect((<IArtifactNode>project).children[0].model.name).toEqual("new artifact");
        }));

        it("single project selected artifact not found project not found failure", 
        inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock, selectionManager: SelectionManagerMock, dialogService: DialogServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                return $q.reject({statusCode: HttpStatusCode.NotFound, errorCode: ProjectServiceStatusCode.ResourceNotFound});
            });
            const artifact = new StatefulArtifactMock($q);
            artifact.id = 10;
            artifact.projectId = 10;
            artifact.parentId = 10;
            selectionManager.setArtifact(artifact);
           
            //Act
            let error: Error;
            projectManager.refresh(10).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectManager.projectCollection.getValue().length).toEqual(0);
            expect(dialogService.alerts.length).toEqual(1);
            expect(dialogService.alerts[0]).toEqual("Refresh_Project_NotFound");
        }));

        it("single project selected artifact other error failure", 
        inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock, messageService: MessageServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                return $q.reject({statusCode: HttpStatusCode.ServerError, message: "error message"});
            });
           
            //Act
            projectManager.refresh(10);
            $rootScope.$digest();

            //Asserts
            expect(projectManager.projectCollection.getValue().length).toEqual(1);
            let project = projectManager.projectCollection.getValue()[0];
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("oldName");
            expect((<IArtifactNode>project).children).toBeUndefined();
            expect(messageService.messages[0].messageType).toEqual(MessageType.Error);
            expect(messageService.messages[0].messageText).toEqual("error message");
        }));

        it("single project selected artifact not found load parent other error failure", 
        inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock, messageService: MessageServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                if (expandToArtifactId === 20) {
                    return $q.reject({statusCode: HttpStatusCode.NotFound, errorCode: ProjectServiceStatusCode.ResourceNotFound});
                } else {
                    return $q.reject({statusCode: HttpStatusCode.ServerError, message: "error message"});
                }
            });
           
            //Act
            let error: Error;
            projectManager.refresh(10).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectManager.projectCollection.getValue().length).toEqual(1);
            let project = projectManager.projectCollection.getValue()[0];
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("oldName");
            expect((<IArtifactNode>project).children).toBeUndefined();
            expect(messageService.messages[0].messageType).toEqual(MessageType.Error);
            expect(messageService.messages[0].messageText).toEqual("error message");
        }));

        it("single project selected artifact not found load project success", 
        inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            spyOn(projectService, "getProjectTree").and.callFake((projectId, expandToArtifactId) => {
                    return $q.reject({statusCode: HttpStatusCode.NotFound, errorCode: ProjectServiceStatusCode.ResourceNotFound});
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
            projectManager.refresh(10).catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(projectManager.projectCollection.getValue().length).toEqual(1);
            let project = projectManager.projectCollection.getValue()[0];
            expect(project.model.id).toEqual(10);
            expect(project.model.name).toEqual("newName");
            expect((<IArtifactNode>project).children.length).toEqual(1);
            expect((<IArtifactNode>project).children[0].model.id).toEqual(20);
            expect((<IArtifactNode>project).children[0].model.name).toEqual("artifact");
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

        it("inside method success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 20,
                orderIndex: 10
            };

            //Act
            let error: Error;
            let result: number;
            projectManager.calculateOrderIndex(MoveCopyArtifactInsertMethod.Inside, artifact)
            .then((res) => {
                result = res;
            })
            .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result).toBeUndefined();
        }));

        it("below method between success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 20,
                orderIndex: 10
            };

            //Act
            let error: Error;
            let result: number;
            projectManager.calculateOrderIndex(MoveCopyArtifactInsertMethod.Below, artifact)
            .then((res) => {
                result = res;
            })
            .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result).toEqual(12.5);
        }));

        it("above method top success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 20,
                orderIndex: 10
            };

            //Act
            let error: Error;
            let result: number;
            projectManager.calculateOrderIndex(MoveCopyArtifactInsertMethod.Above, artifact)
            .then((res) => {
                result = res;
            })
            .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result).toEqual(5);
        }));

        it("below method bottom success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 21,
                orderIndex: 15
            };

            //Act
            let error: Error;
            let result: number;
            projectManager.calculateOrderIndex(MoveCopyArtifactInsertMethod.Below, artifact)
            .then((res) => {
                result = res;
            })
            .catch((err) => error = err);
            $rootScope.$digest();

            //Asserts
            expect(error).toBeUndefined();
            expect(result).toEqual(25);
        }));

        it("above method between success", inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, projectManager: IProjectManager, 
        projectService: ProjectServiceMock, itemInfoService: ItemInfoServiceMock) => {
            // Arrange
            const artifact = <Models.IArtifact>{
                id: 21,
                orderIndex: 15
            };

            //Act
            let error: Error;
            let result: number;
            projectManager.calculateOrderIndex(MoveCopyArtifactInsertMethod.Above, artifact)
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
