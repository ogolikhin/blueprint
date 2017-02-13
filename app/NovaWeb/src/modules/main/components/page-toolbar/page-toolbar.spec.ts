import {StatefulArtifactFactoryMock} from "../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {CreateArtifactService, ICreateArtifactService} from "../projectControls/create-artifact.svc";
import {JobsServiceMock} from "../../../editorsModule/jobs/jobs.service.mock";
import "angular";
import "angular-mocks";
import "angular-ui-router";
import "rx";
import {PageToolbarController} from "./page-toolbar";
import {IDialogService} from "../../../shared";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {INavigationService} from "../../../commonModule/navigation/navigation.service";
import {NavigationServiceMock} from "../../../commonModule/navigation/navigation.service.mock";
import {LoadingOverlayService} from "../../../commonModule/loadingOverlay/loadingOverlay.service";
import {IProjectManager} from "../../../managers/project-manager/project-manager";
import {ISession} from "../../../shell/login/session.svc";
import {AuthSvcMock, ModalServiceMock} from "../../../shell/login/mocks.spec";
import {UnpublishedArtifactsServiceMock} from "../../../editorsModule/unpublished/unpublished.service.mock";
import {ArtifactServiceMock} from "../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {IArtifactService} from "../../../managers/artifact-manager/artifact/artifact.svc";
import {IMessageService} from "../messages/message.svc";
import {SelectionManagerMock} from "../../../managers/selection-manager/selection-manager.mock";
import {SessionSvcMock} from "../../../shell/login/session.svc.mock";

describe("Page Toolbar:", () => {
    let _$q: ng.IQService;
    let _$state: ng.ui.IStateService;
    let $scope: ng.IScope;
    let toolbarCtrl: PageToolbarController;
    let artifact: any;
    let stateSpy: any;
    let createArtifactService: ICreateArtifactService;

    beforeEach(angular.mock.module("ui.router"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {

        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", () => {
            return {
                open: {}
            };
        });
        $provide.service("projectManager", () => {
            return {
                remove: (projectId: number) => {
                    return;
                },
                removeAll: () => {
                    return;
                },
                refreshAll: () => {
                    return;
                },
                getSelectedProjectId: {},
                projectCollection: {
                    getValue: () => {
                        return;
                    }
                }
            };
        });
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
        $provide.service("messageService", () => {
            return {
                addInfo: {}
            };
        });
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("auth", AuthSvcMock);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("session", SessionSvcMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("jobsService", JobsServiceMock);
        $provide.service("createArtifactService", CreateArtifactService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));


    beforeEach(inject(($q: ng.IQService,
                       $state: ng.ui.IStateService,
                       $timeout: ng.ITimeoutService,
                       $rootScope: ng.IRootScopeService,
                       localization: LocalizationServiceMock,
                       dialogService: IDialogService,
                       projectManager: IProjectManager,
                       selectionManager: SelectionManagerMock,
                       publishService: UnpublishedArtifactsServiceMock,
                       messageService: IMessageService,
                       navigationService: NavigationServiceMock,
                       artifactService: IArtifactService,
                       loadingOverlayService: LoadingOverlayService,
                       jobsService: JobsServiceMock,
                       createArtifactService: ICreateArtifactService,
                       session: ISession) => {
        $scope = $rootScope.$new();
        _$q = $q;
        _$state = $state;

        //artifact = statefulArtifactFactory.createStatefulArtifact({id: 1, projectId: 1});
        artifact = {
            projectId: 1,
            autosave: () => { return _$q.resolve(); },
            refresh: () => { return; },
            artifactState : {
                unlock: () => {return; }
            },
            discard: () => { return; }
        };
        toolbarCtrl = new PageToolbarController($q,
        _$state,
        $timeout,
        localization,
        dialogService,
        projectManager,
        selectionManager,
        publishService,
        messageService,
        navigationService,
        artifactService,
        loadingOverlayService,
        null,
        createArtifactService);
        spyOn(selectionManager, "autosave").and.callFake(() => { return $q.resolve(); });

    }));

    describe("refresh all ->", () => {

        it("refresh successful: project is opened",
            inject((projectManager: IProjectManager) => {
            // Arrange
            spyOn(projectManager.projectCollection, "getValue").and.returnValue([{}]);

            const refreshAllSpy = spyOn(projectManager, "refreshAll").and.callFake(() => { return _$q.resolve(); });

            // Act
            toolbarCtrl.refreshAll();
            $scope.$digest();

            // Assert
            expect(refreshAllSpy).toHaveBeenCalled();
        }));

        it("refresh successful: no opened project, but artifact is selected",
            inject((selectionManager: ISelectionManager, projectManager: IProjectManager) => {
            // Arrange
            const refreshAllSpy = spyOn(artifact, "refresh").and.callFake(() => { return _$q.resolve(); });
            spyOn(projectManager.projectCollection, "getValue").and.returnValue([]);
            spyOn(selectionManager, "getArtifact").and.returnValue(artifact);

            // Act
            toolbarCtrl.refreshAll();
            $scope.$digest();

            // Assert
            expect(refreshAllSpy).toHaveBeenCalled();
        }));

        it("refresh unsuccessful: no opened project or selected artifact",
            inject((selectionManager: ISelectionManager, projectManager: IProjectManager) => {
            // Arrange
            const refreshArtifactSpy = spyOn(artifact, "refresh").and.callFake(() => { return _$q.resolve(); });
            const refreshAllSpy = spyOn(projectManager, "refreshAll").and.callFake(() => { return _$q.resolve(); });

            spyOn(projectManager.projectCollection, "getValue").and.returnValue([]);
            spyOn(selectionManager, "getArtifact").and.returnValue(undefined);

            // Act
            toolbarCtrl.refreshAll();
            $scope.$digest();

            // Assert
            expect(refreshAllSpy).not.toHaveBeenCalled();
            expect(refreshArtifactSpy).not.toHaveBeenCalled();
        }));

    });
    describe("publish all ->", () => {
        beforeEach(inject((projectManager: IProjectManager, selectionManager: ISelectionManager) => {
            spyOn(projectManager, "getSelectedProjectId").and.returnValue(1);
            spyOn(selectionManager, "getArtifact").and.returnValue(artifact);
            stateSpy = spyOn(_$state, "go");
        }));

        it("publish successful",
            inject((
                publishService: UnpublishedArtifactsServiceMock,
                dialogService: IDialogService,
                messageService: IMessageService
                ) => {
            // Arrange
            spyOn(publishService, "getUnpublishedArtifacts").and.callFake(() => {
                return _$q.resolve({
                        artifacts: [{}, {}]
                });
            });
            spyOn(messageService, "addInfo").and.callFake(() => {; });
            const refreshSpy = spyOn(artifact, "refresh").and.callFake(() => {return _$q.resolve(); });
            const confirmSpy = spyOn(dialogService, "open").and.callFake(() => {return _$q.resolve(); });
            const publishSpy = spyOn(publishService, "publishAll").and.callFake(() => {return _$q.resolve(); });

            // Act
            toolbarCtrl.publishAll();
            $scope.$digest();

            // Assert
            expect(confirmSpy).toHaveBeenCalled();
            expect(publishSpy).toHaveBeenCalled();
            expect(refreshSpy).toHaveBeenCalled();
        }));
        it("publish successful: nothing to publish",
            inject((
                publishService: UnpublishedArtifactsServiceMock,
                dialogService: IDialogService,
                messageService: IMessageService
                ) => {
            // Arrange
            spyOn(publishService, "getUnpublishedArtifacts").and.callFake(() => {
                return _$q.resolve({artifacts: []});
            });
            const messageSpy = spyOn(messageService, "addInfo").and.callFake(() => {; });
            const confirmSpy = spyOn(dialogService, "open").and.callFake(() => {return _$q.resolve(); });
            const publishAll = spyOn(publishService, "publishAll").and.callFake(() => {return _$q.resolve(); });

            // Act
            toolbarCtrl.publishAll();
            $scope.$digest();

            // Assert
            expect(messageSpy).toHaveBeenCalled();
            expect(confirmSpy).not.toHaveBeenCalled();
            expect(publishAll).not.toHaveBeenCalled();
        }));

    });

    describe("discard all ->", () => {
        beforeEach(inject((projectManager: IProjectManager, selectionManager: ISelectionManager) => {
            spyOn(projectManager, "getSelectedProjectId").and.returnValue(1);
            spyOn(selectionManager, "getArtifact").and.returnValue(artifact);
            spyOn(projectManager.projectCollection, "getValue").and.returnValue([{}]);
        }));

        it("discard successful",
            inject((
                publishService: UnpublishedArtifactsServiceMock,
                projectManager: IProjectManager,
                selectionManager: ISelectionManager,
                dialogService: IDialogService,
                messageService: IMessageService
                ) => {
            // Arrange
            spyOn(publishService, "getUnpublishedArtifacts").and.callFake(() => {
                return _$q.resolve({artifacts: [{}, {}]});
            });

            const confirmSpy = spyOn(dialogService, "open").and.callFake(() => {return _$q.resolve(); });
            const discardSpy = spyOn(publishService, "discardAll").and.callFake(() => {return _$q.resolve(); });
            spyOn(messageService, "addInfo").and.callFake((msg) => {
                expect(msg).toEqual("Discard_All_Success_Message");
            });

            // Act
            toolbarCtrl.discardAll();
            $scope.$digest();

            // Assert
            expect(confirmSpy).toHaveBeenCalled();
            expect(discardSpy).toHaveBeenCalled();
        }));
        it("discard -> confirmation canceled",
            inject((
                publishService: UnpublishedArtifactsServiceMock,
                projectManager: IProjectManager,
                selectionManager: ISelectionManager,
                dialogService: IDialogService,
                messageService: IMessageService
                ) => {
            // Arrange
            spyOn(publishService, "getUnpublishedArtifacts").and.callFake(() => {
                return _$q.resolve({artifacts: [{}, {}]});
            });

            const confirmSpy = spyOn(dialogService, "open").and.callFake(() => {return _$q.reject(); });
            const discardSpy = spyOn(publishService, "discardAll").and.callFake(() => {return _$q.resolve(); });


            // Act
            toolbarCtrl.discardAll();
            $scope.$digest();

            // Assert
            expect(confirmSpy).toHaveBeenCalled();
            expect(discardSpy).not.toHaveBeenCalled();
        }));

        it("discard -> nothing to discard",
            inject((
                publishService: UnpublishedArtifactsServiceMock,
                projectManager: IProjectManager,
                selectionManager: ISelectionManager,
                dialogService: IDialogService,
                messageService: IMessageService
                ) => {
            // Arrange
            spyOn(publishService, "getUnpublishedArtifacts").and.callFake(() => {
                return _$q.resolve({artifacts: []});
            });

            const confirmSpy = spyOn(dialogService, "open").and.callFake(() => {return _$q.reject(); });
            const discardSpy = spyOn(publishService, "discardAll").and.callFake(() => {return _$q.resolve(); });
            spyOn(messageService, "addInfo").and.callFake((msg) => {
                expect(msg).toEqual("Discard_All_No_Unpublished_Changes");
            });

            // Act
            toolbarCtrl.discardAll();
            $scope.$digest();

            // Assert
            expect(confirmSpy).not.toHaveBeenCalled();
            expect(discardSpy).not.toHaveBeenCalled();
        }));
    });

    describe("close project->", () => {

        it("does nothing, no artifact selected", inject((navigationService: INavigationService,
                                                         selectionManager: ISelectionManager,
                                                         projectManager: IProjectManager) => {
            // Arrange
            const evt = {
                preventDefault: () => {
                    return;
                },
                currentTarget: {
                    id: "projectclose"
                }
            };

            spyOn(selectionManager, "getArtifact").and.returnValue(undefined);
            const navigateToSpy = spyOn(navigationService, "navigateTo");
            const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
            const removeProjectSpy = spyOn(projectManager, "remove");

            // Act
            toolbarCtrl.closeProject();
            $scope.$digest();

            // Assert
            expect(navigateToSpy).not.toHaveBeenCalled();
            expect(navigateToMainSpy).not.toHaveBeenCalled();
            expect(removeProjectSpy).not.toHaveBeenCalled();
        }));

        it("navigates to main state, one project is opened and selected artifact belongs to the project", inject((navigationService: INavigationService,
                                                                                                                  selectionManager: ISelectionManager,
                                                                                                                  projectManager: IProjectManager) => {
            // Arrange
            const evt = {
                preventDefault: () => {
                    return;
                },
                currentTarget: {
                    id: "projectclose"
                }
            };
            const openedProjects = [{model: {id: 1}}];

            spyOn(projectManager.projectCollection, "getValue").and.returnValue(openedProjects);
            const selectionSpy = spyOn(selectionManager, "getArtifact").and.returnValue(artifact);

            const navigateToSpy = spyOn(navigationService, "navigateTo");
            const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
            const removeProjectSpy = spyOn(projectManager, "remove").and.callFake(() => openedProjects.pop());
            const clearStickyMessagesSpy = spyOn(toolbarCtrl, "clearStickyMessages");

            // Act
            toolbarCtrl.closeProject();
            $scope.$digest();

            // Assert
            expect(navigateToSpy).not.toHaveBeenCalled();
            expect(navigateToMainSpy).toHaveBeenCalled();
            expect(removeProjectSpy).toHaveBeenCalled();
            expect(clearStickyMessagesSpy).toHaveBeenCalled();
        }));

        it("navigates to project, selected artifact does not belong to the project", inject((navigationService: INavigationService,
                                                                                             selectionManager: ISelectionManager,
                                                                                             projectManager: IProjectManager,
                                                                                             $rootScope: ng.IRootScopeService) => {
            // Arrange
            const evt = {
                preventDefault: () => {
                    return;
                },
                currentTarget: {
                    id: "projectclose"
                }
            };
            const openedProjects = [{model: {id: 1}}];
            artifact.projectId = 555;

            spyOn(projectManager.projectCollection, "getValue").and.returnValue(openedProjects);
            const selectionSpy = spyOn(selectionManager, "getArtifact").and.returnValue(artifact);
            const navigateToSpy = spyOn(navigationService, "navigateTo");
            const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
            const removeProjectSpy = spyOn(projectManager, "remove");
            const clearStickyMessagesSpy = spyOn(toolbarCtrl, "clearStickyMessages");

            // Act
            toolbarCtrl.closeProject();
            $scope.$digest();

            // Assert
            expect(selectionSpy).toHaveBeenCalled();
            expect(navigateToSpy).toHaveBeenCalledWith({id: 1});
            expect(navigateToMainSpy).not.toHaveBeenCalled();
            expect(removeProjectSpy).not.toHaveBeenCalled();
            expect(clearStickyMessagesSpy).toHaveBeenCalled();
        }));

        it("navigates to project, selected artifact belongs to the project, but more than one project is opened",
            inject((navigationService: INavigationService,
                    selectionManager: ISelectionManager,
                    projectManager: IProjectManager) => {
                // Arrange
                const evt = {
                    preventDefault: () => {
                        return;
                    },
                    currentTarget: {
                        id: "projectclose"
                    }
                };
                const openedProjects = [{model: {id: 2}}, {model: {id: 1}}];

                spyOn(projectManager.projectCollection, "getValue").and.returnValue(openedProjects);
                spyOn(selectionManager, "getArtifact").and.returnValue(artifact);


                const navigateToSpy = spyOn(navigationService, "navigateTo");
                const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
                const removeProjectSpy = spyOn(projectManager, "remove").and.callFake(() => openedProjects.pop());
                const clearStickyMessagesSpy = spyOn(toolbarCtrl, "clearStickyMessages");

                // Act
                toolbarCtrl.closeProject();
                $scope.$digest();

                // Assert
                expect(navigateToSpy).toHaveBeenCalledWith({id: 2});
                expect(navigateToMainSpy).not.toHaveBeenCalled();
                expect(removeProjectSpy).toHaveBeenCalled();
                expect(clearStickyMessagesSpy).toHaveBeenCalled();
            }));
    });

    describe("close all projects->", () => {
        it("navigates to main",
            inject((navigationService: INavigationService,
                    selectionManager: ISelectionManager,
                    projectManager: IProjectManager) => {
                // Arrange
                const evt = {
                    preventDefault: () => {
                        return;
                    },
                    currentTarget: {
                        id: "projectcloseall"
                    }
                };
                const openedProjects = [{model: {id: 2}}, {model: {id: 1}}];


                spyOn(projectManager.projectCollection, "getValue").and.returnValue(openedProjects);
                spyOn(selectionManager, "getArtifact").and.returnValue(artifact);

                const navigateToSpy = spyOn(navigationService, "navigateTo");
                const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
                const removeAllProjectSpy = spyOn(projectManager, "removeAll").and.callFake(() => {
                    return;
                });
                const clearAllSpy = spyOn(selectionManager, "clearAll");
                const clearStickyMessagesSpy = spyOn(toolbarCtrl, "clearStickyMessages");

                // Act
                toolbarCtrl.closeAllProjects();
                $scope.$digest();

                // Assert
                expect(navigateToSpy).not.toHaveBeenCalled();
                expect(navigateToMainSpy).toHaveBeenCalled();
                expect(removeAllProjectSpy).toHaveBeenCalled();
                expect(clearStickyMessagesSpy).toHaveBeenCalled();
            }));
    });
});
