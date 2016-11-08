import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {SaveAction} from "./save-action";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";
import {LoadingOverlayService, ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";

describe("SaveAction", () => {
    let $scope: ng.IScope;
    let $q: ng.IQService;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$q_: ng.IQService) => {
        $scope = $rootScope.$new();
        $q = _$q_;
    }));

    it("throws exception when localization is null",
        inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            const localization: ILocalizationService = null;
            let error: Error = null;

            // act
            try {
                new SaveAction(artifact, localization, messageService, loadingOverlayService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error).toEqual(new Error("Localization service not provided or is null"));
        }));

    it("throws exception when message service is null",
        inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
                localization: ILocalizationService,
                loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            const messageService: IMessageService = null;
            let error: Error = null;

            // act
            try {
                new SaveAction(artifact, localization, messageService, loadingOverlayService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error).toEqual(new Error("Message service not provided or is null"));
        }));

    it("throws exception when loadingOverlayService is null",
        inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
                localization: ILocalizationService,
                messageService: IMessageService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            const loadingOverlayService: ILoadingOverlayService = null;
            let error: Error = null;

            // act
            try {
                new SaveAction(artifact, localization, messageService, loadingOverlayService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error).toEqual(new Error("Loading overlay service not provided or is null"));
        }));

    it("is disabled when artifact is null",
        inject((localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = null;

            // act
            const saveAction = new SaveAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(saveAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is read-only",
        inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
                localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            artifact.artifactState.readonly = true;

            // act
            const saveAction = new SaveAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(saveAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is not dirty",
        inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
                localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });

            // act
            const saveAction = new SaveAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(saveAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Project",
        inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
                localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const saveAction = new SaveAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(saveAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collections",
        inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
                localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const saveAction = new SaveAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(saveAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is dirty",
        inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
                localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            artifact.artifactState.dirty = true;

            // act
            const saveAction = new SaveAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(saveAction.disabled).toBe(false);
        }));

    describe("when executed", () => {
        let saveAction: SaveAction;
        let saveSpy: jasmine.Spy;
        let addErrorSpy: jasmine.Spy;
        let beginLoadingSpy: jasmine.Spy;
        let endLoadingSpy: jasmine.Spy;

        beforeEach(inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
                           localization: ILocalizationService,
                           messageService: IMessageService,
                           loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            saveAction = new SaveAction(artifact, localization, messageService, loadingOverlayService);
            saveSpy = spyOn(artifact, "save");
            addErrorSpy = spyOn(messageService, "addError").and.callThrough();
            beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading").and.callThrough();
            endLoadingSpy = spyOn(loadingOverlayService, "endLoading").and.callThrough();
        }));

        describe("and save succeeds", () => {
            beforeEach(() => {
                // arrange
                saveSpy.and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                // act
                saveAction.execute();
                $scope.$digest();
            });

            it("shows loading screen", () => {
                // assert
                expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            });

            it("saves artifact", () => {
                // assert
                expect(saveSpy).toHaveBeenCalledTimes(1);
            });

            it("doesn't add error message to message service", () => {
                // assert
                expect(addErrorSpy).not.toHaveBeenCalled();
            });

            it("hides loading screen", () => {
                // assert
                expect(endLoadingSpy).toHaveBeenCalledTimes(1);
            });
        });

        describe("and save fails", () => {
            beforeEach(() => {
                // arrange
                saveSpy.and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject(new Error(""));
                    return deferred.promise;
                });

                // act
                saveAction.execute();
                $scope.$digest();
            });

            it("shows loading screen", () => {
                // assert
                expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            });

            it("saves artifact", () => {
                // assert
                expect(saveSpy).toHaveBeenCalledTimes(1);
            });

            it("does add error message to message service", () => {
                // assert
                expect(addErrorSpy).toHaveBeenCalled();
            });

            it("hides loading screen", () => {
                // assert
                expect(endLoadingSpy).toHaveBeenCalledTimes(1);
            });
        });

        describe("and save throws exception", () => {
            beforeEach(() => {
                // arrange
                saveSpy.and.callFake(() => {
                    throw new Error("Test error");
                });

                // act
                try {
                    saveAction.execute();
                } catch (error) {
                    expect(error).not.toBeNull();
                }
            });

            it("shows loading screen", () => {
                // assert
                expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            });

            it("saves artifact", () => {
                // assert
                expect(saveSpy).toHaveBeenCalledTimes(1);
            });

            it("adds error message to message service", () => {
                // assert
                expect(addErrorSpy).toHaveBeenCalledWith(new Error("Test error"));
            });

            it("hides loading screen", () => {
                // assert
                expect(endLoadingSpy).toHaveBeenCalledTimes(1);
            });
        });
    });
});
