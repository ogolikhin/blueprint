import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "../../main";
import {Models} from "../../main/models";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {NavigationServiceMock} from "../../core/navigation/navigation.svc.mock";
import {IItemInfoService, IItemInfoResult} from "../../core/navigation/item-info.svc";
import {ItemInfoServiceMock} from "../../core/navigation/item-info.svc.mock";
import {IArtifactManager} from "../../managers/artifact-manager/artifact-manager";
import {IProjectManager} from "../../managers/project-manager/project-manager";
import {IStatefulArtifact} from "../../managers/artifact-manager/artifact/artifact";
import {ArtifactManagerMock} from "../../managers/artifact-manager/artifact-manager.mock";
import {ProjectManagerMock} from "../../managers/project-manager/project-manager.mock";
import {IStatefulArtifactFactory} from "../../managers/artifact-manager/artifact/artifact.factory";
import {StatefulArtifactFactoryMock} from "../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ItemStateController} from "./item-state.controller";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {MessageType, Message} from "../../core/messages/message";

describe("Item State Controller tests", () => {
    let $stateParams: ng.ui.IStateParamsService,
        $timeout: ng.ITimeoutService,
        $rootScope: ng.IRootScopeService,
        $q: ng.IQService,
        artifactManager: IArtifactManager,
        projectManager: IProjectManager,
        localization: ILocalizationService,
        messageService: IMessageService,
        navigationService: INavigationService,
        itemInfoService: IItemInfoService,
        statefulArtifactFactory: IStatefulArtifactFactory,
        ctrl: ItemStateController;

    beforeEach(angular.mock.module("ui.router"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("projectManager", ProjectManagerMock);
        $provide.service("itemInfoService", ItemInfoServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((
        _$stateParams_: ng.ui.IStateParamsService,
        _$timeout_: ng.ITimeoutService,
        _$rootScope_: ng.IRootScopeService,
        _$q_: ng.IQService,
        _artifactManager_: IArtifactManager,
        _projectManager_: IProjectManager,
        _localization_: ILocalizationService,
        _messageService_: IMessageService,
        _navigationService_: INavigationService,
        _itemInfoService_: IItemInfoService,
        _statefulArtifactFactory_: IStatefulArtifactFactory) => {

        $stateParams = _$stateParams_;
        $timeout = _$timeout_;
        $rootScope = _$rootScope_;
        $q = _$q_;
        artifactManager = _artifactManager_;
        projectManager = _projectManager_;
        localization = _localization_;
        messageService = _messageService_;
        navigationService = _navigationService_;
        itemInfoService = _itemInfoService_;
        statefulArtifactFactory = _statefulArtifactFactory_;
    }));

    beforeEach(() => {
        artifactManager.selection.setExplorerArtifact = (artifact) => null;
        artifactManager.selection.setArtifact = (artifact) => null;
    });

    function getItemStateController(itemInfo: IItemInfoResult, version?: string): ItemStateController {
        if (version) {
            $stateParams["version"] = version;
        }

        return new ItemStateController(
            $stateParams,
            artifactManager,
            projectManager,
            messageService,
            localization,
            navigationService,
            itemInfoService,
            statefulArtifactFactory,
            $timeout,
            itemInfo);
    }

    it("respond to url", inject(($state: ng.ui.IStateService) => {
        expect($state.href("main.item", {id: 1})).toEqual("#/main/1");
    }));

    it("clears locked messages", () => {
        // arrange
        const itemInfo = {
            id: 10
        } as IItemInfoResult;
        const deleteMessageSpy = spyOn(messageService, "deleteMessageById");
        const message = new Message(MessageType.Deleted, "test");
        message.id = 1;
        messageService.addMessage(message);

        // act
        ctrl = getItemStateController(itemInfo);
        $rootScope.$digest();

        // assert
        expect(deleteMessageSpy).toHaveBeenCalled();
        expect(deleteMessageSpy).toHaveBeenCalledWith(message.id);
    });

    describe("when not in artifact manager", () => {
        let artifactId, artifactManagerSpy, itemInfoSpy;

        beforeEach(() => {
            artifactId = 10;
            artifactManagerSpy = spyOn(artifactManager, "get").and.returnValue(null);
        });

        afterEach(() => {
            artifactManagerSpy = undefined;
            itemInfoSpy = undefined;
        });

        describe("state changes to artifact", () => {
            let isArtifactSpy;

            beforeEach(() => {
                isArtifactSpy = spyOn(itemInfoService, "isArtifact").and.callFake(() => true);
            });

            afterEach(() => {
                isArtifactSpy = undefined;
            });

            it("diagram", () => {
                // arrange
                const expectedEditor = "diagram";
                const itemInfo = {
                    id: artifactId,
                    projectId: 11,
                    predefinedType: Models.ItemTypePredefined.GenericDiagram
                } as IItemInfoResult;

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                expect(ctrl.activeEditor).toBe(expectedEditor);
            });

            it("glossary", () => {
                // arrange
                const expectedEditor = "glossary";
                const itemInfo = {
                    id: artifactId,
                    projectId: 11,
                    predefinedType: Models.ItemTypePredefined.Glossary
                } as IItemInfoResult;

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                expect(ctrl.activeEditor).toBe(expectedEditor);
            });

            it("general", () => {
                // arrange
                const expectedEditor = "general";
                const itemInfo = {
                    id: artifactId,
                    projectId: 11,
                    predefinedType: Models.ItemTypePredefined.Project
                } as IItemInfoResult;

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                expect(ctrl.activeEditor).toBe(expectedEditor);
            });

            it("collection", () => {
                // arrange
                const expectedEditor = "collection";
                const itemInfo = {
                    id: artifactId,
                    projectId: 11,
                    predefinedType: Models.ItemTypePredefined.ArtifactCollection
                } as IItemInfoResult;

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                expect(ctrl.activeEditor).toBe(expectedEditor);
            });

            it("process", () => {
                // arrange
                const expectedEditor = "process";
                const itemInfo = {
                    id: artifactId,
                    projectId: 11,
                    predefinedType: Models.ItemTypePredefined.Process
                } as IItemInfoResult;

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                expect(ctrl.activeEditor).toBe(expectedEditor);
            });

            it("details", () => {
                // arrange
                const expectedEditor = "details";
                const itemInfo = {
                    id: artifactId,
                    projectId: 11,
                    predefinedType: Models.ItemTypePredefined.Actor
                } as IItemInfoResult;

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                expect(ctrl.activeEditor).toBe(expectedEditor);
            });
        });

        describe("state changes to non-artifact", () => {
            it("should redirect to artifact", () => {
                // arrange
                const artifactId = 10;
                const itemInfo = {
                    id: artifactId,
                    subArtifactId: 123
                } as any as IItemInfoResult;

                const isSubArtifactSpy = spyOn(itemInfoService, "isSubArtifact").and.callFake(() => true);
                const navigationSpy = spyOn(navigationService, "navigateTo");

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                expect(navigationSpy).toHaveBeenCalled();
                expect(navigationSpy).toHaveBeenCalledWith({id: artifactId, redirect: true});
            });

            it("should navigate to a project", () => {
                // arrange
                const artifactId = 10;
                const isProjectSpy = spyOn(itemInfoService, "isProject").and.callFake(() => true);
                const itemInfo = {
                    id: artifactId,
                    projectId: artifactId
                } as any as IItemInfoResult;
                const navigationSpy = spyOn(navigationService, "navigateTo");
                const projectManagerSpy = spyOn(projectManager, "openProject").and.callFake(() => $q.resolve());
                const reloadNavigationSpy = spyOn(navigationService, "reloadCurrentState");

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                expect(navigationSpy).not.toHaveBeenCalled();
                expect(isProjectSpy).toHaveBeenCalled();
                expect(projectManagerSpy).toHaveBeenCalled();
                expect(projectManagerSpy).toHaveBeenCalledWith(artifactId);
                expect(reloadNavigationSpy).not.toHaveBeenCalled();
            });
        });

        describe("artifact is deleted", () => {
            it("should show a historical version of artifact", () => {
                // arrange
                const artifactId = 10;
                const itemInfo = {
                    id: artifactId,
                    predefinedType: Models.ItemTypePredefined.Actor,
                    isDeleted: true,
                    deletedByUser: {}
                } as any as IItemInfoResult;

                const isArtifactSpy = spyOn(itemInfoService, "isArtifact").and.callFake(() => true);
                const navigationSpy = spyOn(navigationService, "navigateTo");
                const selectionSpy = spyOn(artifactManager.selection, "setExplorerArtifact");

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                const selectedArtifact: IStatefulArtifact = artifactManager.selection.setExplorerArtifact["calls"].argsFor(0)[0];
                expect(navigationSpy).not.toHaveBeenCalled();
                expect(selectionSpy).toHaveBeenCalled();
                expect(selectedArtifact.artifactState.historical).toBe(true);
                expect(selectedArtifact.artifactState.deleted).toBe(true);
            });

            it("should show a historical version if deleted artifact is an Artifact Collection", () => {
                // arrange
                const artifactId = 10;
                const isArtifactSpy = spyOn(itemInfoService, "isArtifact").and.callFake(() => true);
                const itemInfo = {
                    id: artifactId,
                    predefinedType: Models.ItemTypePredefined.ArtifactCollection,
                    isDeleted: true,
                    deletedByUser: {}
                } as any as IItemInfoResult;
                const navigationSpy = spyOn(navigationService, "navigateTo");
                const selectionSpy = spyOn(artifactManager.selection, "setExplorerArtifact");

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                const selectedArtifact: IStatefulArtifact = artifactManager.selection.setExplorerArtifact["calls"].argsFor(0)[0];
                expect(ctrl.activeEditor).toBe("collection");
                expect(navigationSpy).not.toHaveBeenCalled();
                expect(selectionSpy).toHaveBeenCalled();
                expect(selectedArtifact.artifactState.historical).toBe(true);
                expect(selectedArtifact.artifactState.deleted).toBe(true);
            });

            it("should redirect to a historical version if deleted artifact is a Collection Folder", () => {
                // arrange
                const artifactId = 10;
                const isArtifactSpy = spyOn(itemInfoService, "isArtifact").and.callFake(() => true);
                const itemInfo = {
                    id: artifactId,
                    projectId: 1,
                    parentId: 3,
                    predefinedType: Models.ItemTypePredefined.CollectionFolder,
                    isDeleted: true,
                    deletedByUser: {}
                } as any as IItemInfoResult;
                const navigationSpy = spyOn(navigationService, "navigateTo");
                const selectionSpy = spyOn(artifactManager.selection, "setExplorerArtifact");

                // act
                ctrl = getItemStateController(itemInfo);
                $timeout.flush();

                // assert
                const selectedArtifact: IStatefulArtifact = artifactManager.selection.setExplorerArtifact["calls"].argsFor(0)[0];
                expect(ctrl.activeEditor).toBe("details");
                expect(navigationSpy).not.toHaveBeenCalled();
                expect(selectionSpy).toHaveBeenCalled();
                expect(selectedArtifact.artifactState.historical).toBe(true);
                expect(selectedArtifact.artifactState.deleted).toBe(true);
            });
        });

        describe("historical artifact", () => {
            it("should navigate to a historical version of artifact", () => {
                // arrange
                const artifactId = 10;
                const version = 5;
                const isArtifactSpy = spyOn(itemInfoService, "isArtifact").and.callFake(() => true);
                const itemInfo = {
                    id: artifactId,
                    predefinedType: Models.ItemTypePredefined.Actor,
                    versionCount: 20
                } as any as IItemInfoResult;
                const navigationSpy = spyOn(navigationService, "navigateTo");
                const selectionSpy = spyOn(artifactManager.selection, "setExplorerArtifact");

                // act
                ctrl = getItemStateController(itemInfo, version.toString());
                $timeout.flush();

                // assert
                const selectedArtifact: IStatefulArtifact = artifactManager.selection.setExplorerArtifact["calls"].argsFor(0)[0];
                expect(navigationSpy).not.toHaveBeenCalled();
                expect(selectionSpy).toHaveBeenCalled();
                expect(selectedArtifact.artifactState.historical).toBe(true);
                expect(selectedArtifact.getEffectiveVersion()).toBe(5);
            });

            it("should navigate to main and display error because version greater than version number", () => {
                // arrange
                const artifactId = 10;
                const version = 25;
                const isArtifactSpy = spyOn(itemInfoService, "isArtifact").and.callFake(() => true);
                const itemInfo = {
                    id: artifactId,
                    predefinedType: Models.ItemTypePredefined.Actor,
                    versionCount: 20
                } as any as IItemInfoResult;
                const navigationSpy = spyOn(navigationService, "navigateTo");
                const navigateToMainSpy = spyOn(navigationService, "navigateToMain");
                const messageSpy = spyOn(messageService, "addError").and.callFake(message => void (0));
                const selectionSpy = spyOn(artifactManager.selection, "setExplorerArtifact");

                // act
                ctrl = getItemStateController(itemInfo, version.toString());
                $timeout.flush();

                // assert
                const selectedArtifact: IStatefulArtifact = artifactManager.selection.setExplorerArtifact["calls"].argsFor(0)[0];
                expect(navigationSpy).not.toHaveBeenCalled();
                expect(navigateToMainSpy).toHaveBeenCalled();
                expect(selectionSpy).not.toHaveBeenCalled();
                expect(messageSpy).toHaveBeenCalled();
                expect(selectedArtifact).toBeUndefined();
            });
        });
    });

    describe("when in artifact manager", () => {
        let artifactId: number,
            statefulArtifact,
            artifactManagerSpy: jasmine.Spy;

        beforeEach(() => {
            artifactId = 10;
            statefulArtifact = {
                id: artifactId,
                predefinedType: Models.ItemTypePredefined.Process,
                artifactState: {
                    deleted: false
                },
                unload: () => void(0),
                errorObservable: () => {
                    return {
                        subscribeOnNext: () => void(0)
                    };
                }
            };
            artifactManagerSpy = spyOn(artifactManager, "get").and.returnValue(statefulArtifact);
        });

        afterEach(() => {
            artifactManagerSpy = undefined;
        });

        it("should unload existing artifact and go to main.item.process state", () => {
            // arrange
            const itemInfo = {
                id: artifactId,
                predefinedType: Models.ItemTypePredefined.Actor
            } as any as IItemInfoResult;
            const unloadSpy = spyOn(statefulArtifact, "unload");

            // act
            ctrl = getItemStateController(itemInfo);
            $timeout.flush();

            // assert
            expect(unloadSpy).toHaveBeenCalled();
        });

        it("should not use artifact from artifact manager if it's deleted", () => {
            // arrange
            statefulArtifact.artifactState.deleted = true;
            const itemInfo = {
                id: artifactId,
                predefinedType: Models.ItemTypePredefined.Actor,
                isDeleted: true,
                deletedByUser: {}
            } as any as IItemInfoResult;
            const unloadSpy = spyOn(statefulArtifact, "unload");

            // act
            ctrl = getItemStateController(itemInfo);
            $timeout.flush();

            // assert
            expect(unloadSpy).not.toHaveBeenCalled();
        });
    });
});
