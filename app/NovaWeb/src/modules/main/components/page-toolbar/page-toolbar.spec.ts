import "angular";
import "angular-mocks";
import {PageToolbarController} from "./page-toolbar";
import {IDialogService} from "../../../shared";
import {IMessageService} from "../../../core/messages/message.svc";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {NavigationServiceMock} from "../../../core/navigation/navigation.svc.mock";
import {LoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {IProjectManager} from "../../../managers/project-manager/project-manager";
import {ArtifactManagerMock} from "../../../managers/artifact-manager/artifact-manager.mock";
import {IArtifactManager} from "../../../managers/artifact-manager/artifact-manager";
import {PublishServiceMock} from "../../../managers/artifact-manager/publish.svc/publish.svc.mock";
import {IAnalyticsProvider, AnalyticsProvider} from "../analytics/analyticsProvider";


describe("Application toolbar:", () => {
    let _$q: ng.IQService;
    let $scope: ng.IScope;
    let toolbarCtrl: PageToolbarController;
    let artifact: any;


    //beforeEach(angular.mock.module("bp.components"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {

        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", () => {
            return {};
        });
        $provide.service("projectManager", () => {
            return {
                remove: (projectId: number) => {
                    return;
                },
                removeAll: () => {
                    return;
                },
                projectCollection: {
                    getValue: () => {
                        return;
                    }
                }
            };
        });
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("publishService", PublishServiceMock);
        $provide.service("messageService", () => {
            return {};
        });
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.provider("analytics", AnalyticsProvider);

    }));


    beforeEach(inject(($q: ng.IQService,
                       $rootScope: ng.IRootScopeService,
                       localization: LocalizationServiceMock,
                       dialogService: IDialogService,
                       projectManager: IProjectManager,
                       artifactManager: ArtifactManagerMock,
                       publishService: PublishServiceMock,
                       messageService: IMessageService,
                       navigationService: NavigationServiceMock,
                       loadingOverlayService: LoadingOverlayService,
                       analytics: IAnalyticsProvider) => {
        $scope = $rootScope.$new();
        _$q = $q;

        //artifact = statefulArtifactFactory.createStatefulArtifact({id: 1, projectId: 1});
        artifact = {
            projectId: 1,
            autosave: () => {
                let deferred = _$q.defer();
                deferred.resolve();
                return deferred.promise;
            }
        };
        toolbarCtrl = new PageToolbarController($q, localization,
            dialogService, projectManager, artifactManager, publishService,
            messageService, navigationService, loadingOverlayService, analytics);
        artifactManager.selection = {
            getArtifact: () => {
                return;
            },
            clearAll: () => {
                return;
            }
        } as ISelectionManager;
    }));
    describe("close project->", () => {

        it("does nothing, no artifact selected", inject((navigationService: INavigationService,
                                                         artifactManager: IArtifactManager,
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
            spyOn(artifactManager.selection, "getArtifact").and.returnValue(undefined);
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
                                                                                                                  artifactManager: IArtifactManager,
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
            const selectionSpy = spyOn(artifactManager.selection, "getArtifact").and.returnValue(artifact);

            const navigateToSpy = spyOn(navigationService, "navigateTo");
            const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
            const removeProjectSpy = spyOn(projectManager, "remove").and.callFake(() => openedProjects.pop());
            const clearLockedMessagesSpy = spyOn(toolbarCtrl, "clearLockedMessages");

            // Act
            toolbarCtrl.closeProject();
            $scope.$digest();

            // Assert
            expect(navigateToSpy).not.toHaveBeenCalled();
            expect(navigateToMainSpy).toHaveBeenCalled();
            expect(removeProjectSpy).toHaveBeenCalled();
            expect(clearLockedMessagesSpy).toHaveBeenCalled();
        }));

        it("navigates to project, selected artifact does not belong to the project", inject((navigationService: INavigationService,
                                                                                             artifactManager: IArtifactManager,
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
            const selectionSpy = spyOn(artifactManager.selection, "getArtifact").and.returnValue(artifact);


            const navigateToSpy = spyOn(navigationService, "navigateTo");
            const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
            const removeProjectSpy = spyOn(projectManager, "remove");
            const clearAllSpy = spyOn(artifactManager.selection, "clearAll");
            const clearLockedMessagesSpy = spyOn(toolbarCtrl, "clearLockedMessages");

            // Act
            toolbarCtrl.closeProject();
            $scope.$digest();

            // Assert
            expect(selectionSpy).toHaveBeenCalled();
            expect(clearAllSpy).toHaveBeenCalled();
            expect(navigateToSpy).toHaveBeenCalledWith({id: 1});
            expect(navigateToMainSpy).not.toHaveBeenCalled();
            expect(removeProjectSpy).not.toHaveBeenCalled();
            expect(clearLockedMessagesSpy).toHaveBeenCalled();
        }));

        it("navigates to project, selected artifact belongs to the project, but more than one project is opened",
            inject((navigationService: INavigationService,
                    artifactManager: IArtifactManager,
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
                spyOn(artifactManager.selection, "getArtifact").and.returnValue(artifact);


                const navigateToSpy = spyOn(navigationService, "navigateTo");
                const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
                const removeProjectSpy = spyOn(projectManager, "remove").and.callFake(() => openedProjects.pop());
                const clearAllSpy = spyOn(artifactManager.selection, "clearAll");
                const clearLockedMessagesSpy = spyOn(toolbarCtrl, "clearLockedMessages");

                // Act
                toolbarCtrl.closeProject();
                $scope.$digest();

                // Assert
                expect(clearAllSpy).toHaveBeenCalled();
                expect(navigateToSpy).toHaveBeenCalledWith({id: 2});
                expect(navigateToMainSpy).not.toHaveBeenCalled();
                expect(removeProjectSpy).toHaveBeenCalled();
                expect(clearLockedMessagesSpy).toHaveBeenCalled();
            }));
    });

    describe("close all projects->", () => {
        it("navigates to main",
            inject((navigationService: INavigationService,
                    artifactManager: IArtifactManager,
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
                spyOn(artifactManager.selection, "getArtifact").and.returnValue(artifact);

                const navigateToSpy = spyOn(navigationService, "navigateTo");
                const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
                const removeAllProjectSpy = spyOn(projectManager, "removeAll").and.callFake(() => {
                    return;
                });
                const clearAllSpy = spyOn(artifactManager.selection, "clearAll");
                const clearLockedMessagesSpy = spyOn(toolbarCtrl, "clearLockedMessages");

                // Act
                toolbarCtrl.closeAllProjects();
                $scope.$digest();

                // Assert
                expect(navigateToSpy).not.toHaveBeenCalled();
                expect(navigateToMainSpy).toHaveBeenCalled();
                expect(removeAllProjectSpy).toHaveBeenCalled();
                expect(clearLockedMessagesSpy).toHaveBeenCalled();
            }));
    });

});
