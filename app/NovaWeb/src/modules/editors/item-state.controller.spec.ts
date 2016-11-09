import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";

import "../main";
import { Models } from "../main/models";
import {LocalizationServiceMock} from "../core/localization/localization.mock";
import {MessageServiceMock} from "../core/messages/message.mock";
import {NavigationServiceMock} from "../core/navigation/navigation.svc.mock";
import {IItemInfoService} from "../core/navigation/item-info.svc";
import {ItemInfoServiceMock} from "../core/navigation/item-info.svc.mock";
import {IArtifactManager} from "../managers/artifact-manager/artifact-manager";
import {IProjectManager} from "../managers/project-manager/project-manager";
import {IStatefulArtifact} from "../managers/artifact-manager/artifact";
import {ArtifactManagerMock} from "../managers/artifact-manager/artifact-manager.mock";
import {IStatefulArtifactFactory} from "../managers/artifact-manager/artifact/artifact.factory";
import {StatefulArtifactFactoryMock} from "../managers/artifact-manager/artifact/artifact.factory.mock";
import {ItemStateController} from "./item-state.controller";
import {IMessageService} from "../core/messages/message.svc";
import {ILocalizationService} from "../core/localization/localizationService";
import {INavigationService} from "../core/navigation/navigation.svc";
import {MessageType, Message} from "../core/messages/message";

describe("Item State Controller tests", () => {
    let $state: angular.ui.IStateService,
        $rootScope: ng.IRootScopeService,
        $q: ng.IQService,
        artifactManager: IArtifactManager,
        projectManager: IProjectManager,
        localization,
        messageService: IMessageService,
        navigationService,
        itemInfoService,
        statefulArtifactFactory,
        ctrl: ItemStateController,
        stateSpy;

    beforeEach(angular.mock.module("ui.router"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("itemInfoService", ItemInfoServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((
        _$state_: ng.ui.IStateService,
        _$rootScope_: ng.IRootScopeService,
        _$q_: ng.IQService,
        _artifactManager_: IArtifactManager,
        _projectManager_: IProjectManager,
        _localization_: ILocalizationService,
        _messageService_: IMessageService,
        _navigationService_: INavigationService,
        _itemInfoService_: IItemInfoService,
        _statefulArtifactFactory_: IStatefulArtifactFactory) => {

        $state = _$state_;
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
        stateSpy = spyOn($state, "go");
    });

    function getItemStateController(id: string): ItemStateController {
        $state.params["id"] = id;

        return new ItemStateController(
            $state,
            artifactManager,
            projectManager,
            messageService,
            localization,
            navigationService,
            itemInfoService,
            statefulArtifactFactory);
    }

    afterEach(() => {
        delete $state.params["id"];
    });

    it("respond to url", () => {
        expect($state.href("main.item", { id: 1 })).toEqual("#/main/1");
    });

    it("clears locked messages", () => {
        // arrange
        const artifactId = 10;
        const deleteMessageSpy = spyOn(messageService, "deleteMessageById");
        const message = new Message(MessageType.Deleted, "test");
        message.id = 1;
        messageService.addMessage(message);

        // act
        ctrl = getItemStateController(artifactId.toString());
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
                const expectedState = "main.item.diagram";
                itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({id: artifactId, projectId: 11, predefinedType: Models.ItemTypePredefined.GenericDiagram});
                    return deferred.promise;
                });

                // act
                ctrl = getItemStateController(artifactId.toString());
                $rootScope.$digest();

                // assert
                expect(itemInfoSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalledWith(expectedState, {id: artifactId, version: undefined}, {reload: expectedState});
            });

            it("glossary", () => {
                // arrange
                const expectedState = "main.item.glossary";
                itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({id: artifactId, projectId: 11, predefinedType: Models.ItemTypePredefined.Glossary});
                    return deferred.promise;
                });

                // act
                ctrl = getItemStateController(artifactId.toString());
                $rootScope.$digest();

                // assert
                expect(itemInfoSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalledWith(expectedState, {id: artifactId, version: undefined}, {reload: expectedState});
            });

            it("general", () => {
                // arrange
                const expectedState = "main.item.general";
                itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({id: artifactId, projectId: 11, predefinedType: Models.ItemTypePredefined.Project});
                    return deferred.promise;
                });

                // act
                ctrl = getItemStateController(artifactId.toString());
                $rootScope.$digest();

                // assert
                expect(itemInfoSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalledWith(expectedState, {id: artifactId, version: undefined}, {reload: expectedState});
            });

            it("collection", () => {
                // arrange
                const expectedState = "main.item.collection";
                itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({id: artifactId, projectId: 11, predefinedType: Models.ItemTypePredefined.ArtifactCollection});
                    return deferred.promise;
                });

                // act
                ctrl = getItemStateController(artifactId.toString());
                $rootScope.$digest();

                // assert
                expect(itemInfoSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalledWith(expectedState, {id: artifactId, version: undefined}, {reload: expectedState});
            });

            it("process", () => {
                // arrange
                const expectedState = "main.item.process";
                itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({id: artifactId, projectId: 11, predefinedType: Models.ItemTypePredefined.Process});
                    return deferred.promise;
                });

                // act
                ctrl = getItemStateController(artifactId.toString());
                $rootScope.$digest();

                // assert
                expect(itemInfoSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalledWith(expectedState, {id: artifactId, version: undefined}, {reload: expectedState});
            });

            it("details", () => {
                // arrange
                const expectedState = "main.item.details";
                itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({id: artifactId, projectId: 11, predefinedType: Models.ItemTypePredefined.Actor});
                    return deferred.promise;
                });

                // act
                ctrl = getItemStateController(artifactId.toString());
                $rootScope.$digest();

                // assert
                expect(itemInfoSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalled();
                expect(stateSpy).toHaveBeenCalledWith(expectedState, {id: artifactId, version: undefined}, {reload: expectedState});
            });
        });

        describe("state changes to non-artifact", () => {
            it("should redirect to artifact", () => {
                // arrange
                const artifactId = 10;
                const subArtifactId = 123;
                const isSubArtifactSpy = spyOn(itemInfoService, "isSubArtifact").and.callFake(() => true);
                const itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({id: artifactId, subArtifactId: subArtifactId});
                    return deferred.promise;
                });
                const navigationSpy = spyOn(navigationService, "navigateTo");

                // act
                ctrl = getItemStateController(subArtifactId.toString());
                $rootScope.$digest();

                // assert
                expect(navigationSpy).toHaveBeenCalled();
                expect(navigationSpy).toHaveBeenCalledWith({id: 10, redirect: true});
            });

            it("should not navigate to a project, should navigate to Main", () => {
                // arrange
                const artifactId = 10;
                const isSubArtifactSpy = spyOn(itemInfoService, "isProject").and.callFake(() => true);
                const itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({id: artifactId, projectId: artifactId});
                    return deferred.promise;
                });
                const navigationSpy = spyOn(navigationService, "navigateTo");
                const mainNavigationSpy = spyOn(navigationService, "navigateToMain");
                const messageSpy = spyOn(messageService, "addError");

                // act
                ctrl = getItemStateController(artifactId.toString());
                $rootScope.$digest();

                // assert
                expect(navigationSpy).not.toHaveBeenCalled();
                expect(mainNavigationSpy).toHaveBeenCalled();
                expect(messageSpy).toHaveBeenCalled();
            });
        });

        describe("artifact is deleted", () => {
            it("should redirect to a historical version of artifact and add a message", () => {
                // arrange
                const artifactId = 10;
                const isArtifactSpy = spyOn(itemInfoService, "isArtifact").and.callFake(() => true);
                const itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({
                        id: artifactId,
                        predefinedType: Models.ItemTypePredefined.Actor,
                        isDeleted: true,
                        deletedByUser: {}
                    });
                    return deferred.promise;
                });
                const navigationSpy = spyOn(navigationService, "navigateTo");
                const messageSpy = spyOn(messageService, "addMessage").and.callFake(message => void(0));
                const selectionSpy = spyOn(artifactManager.selection, "setExplorerArtifact");

                // act
                ctrl = getItemStateController(artifactId.toString());
                $rootScope.$digest();

                // assert
                const selectedArtifact: IStatefulArtifact = artifactManager.selection.setExplorerArtifact["calls"].argsFor(0)[0];
                expect(stateSpy).toHaveBeenCalled();
                expect(navigationSpy).not.toHaveBeenCalled();
                expect(messageSpy).toHaveBeenCalled();
                expect(selectionSpy).toHaveBeenCalled();
                expect(selectedArtifact.artifactState.historical).toBe(true);
                expect(selectedArtifact.artifactState.deleted).toBe(true);
            });

        });

        describe("artifact is not found", () => {
            it("should redirect to main state and show a not found message", () => {
                // arrange
                const artifactId = 10;
                const itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject({
                        message: "Item (Id:${artifactId}) is not found.",
                        statusCode: 404
                    });
                    return deferred.promise;
                });
                const navigationSpy = spyOn(navigationService, "navigateToMain");
                const messageSpy = spyOn(messageService, "addError").and.callFake(message => void(0));

                // act
                ctrl = getItemStateController(artifactId.toString());
                $rootScope.$digest();

                // assert
                expect(navigationSpy).toHaveBeenCalled();
                expect(messageSpy).toHaveBeenCalled();
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
            const unloadSpy = spyOn(statefulArtifact, "unload");

            // act
            ctrl = getItemStateController(artifactId.toString());
            $rootScope.$digest();

            // assert
            expect(unloadSpy).toHaveBeenCalled();
            expect(stateSpy).toHaveBeenCalled();
            expect(stateSpy).toHaveBeenCalledWith("main.item.process", {id: artifactId, version: undefined}, {reload: "main.item.process"});
        });

        it("should not use artifact from artifact manager if it's deleted", () => {
            // arrange
            statefulArtifact.artifactState.deleted = true;
            const unloadSpy = spyOn(statefulArtifact, "unload");
            const itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve({
                        id: artifactId,
                        predefinedType: Models.ItemTypePredefined.Actor,
                        isDeleted: true,
                        deletedByUser: {}
                    });
                    return deferred.promise;
                });

            // act
            ctrl = getItemStateController(artifactId.toString());
            $rootScope.$digest();

            // assert
            expect(unloadSpy).not.toHaveBeenCalled();
            expect(itemInfoSpy).toHaveBeenCalled();
        });
    });
});
