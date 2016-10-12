import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {RefreshAction} from "./refresh-action";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService} from "../../../../core";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {IProjectManager, ProjectManager} from "../../../../managers/project-manager/project-manager";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../../core/loading-overlay";
import {LogMock} from "../../../../shell/log/server-logger.svc.mock";

describe("RefreshAction", () => {
    let $scope: ng.IScope;
    let $q: ng.IQService;
    let artifact: IStatefulArtifact;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectManager", ProjectManager);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("$log", LogMock);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$q_: ng.IQService) => {
        $scope = $rootScope.$new();
        $q = _$q_;
    }));

    beforeEach(inject((statefulArtifactFactory: IStatefulArtifactFactory) => {
        artifact = statefulArtifactFactory.createStatefulArtifact(
        {
                id: 1, 
                predefinedType: ItemTypePredefined.Actor 
        });
    }));

    it("throws exception when localization is null", 
        inject((
            projectManager: IProjectManager,
            statefulArtifactFactory: IStatefulArtifactFactory, 
            loadingOverlayService: ILoadingOverlayService
            ) => {
        // arrange
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new RefreshAction(artifact, localization, projectManager, loadingOverlayService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    }));

    it("throws exception when projectManager is null", 
        inject((
            localization: ILocalizationService,
            statefulArtifactFactory: IStatefulArtifactFactory, 
            loadingOverlayService: ILoadingOverlayService
            ) => {
        // arrange
        const projectManager: IProjectManager = null;
        let error: Error = null;

        // act
        try {
            new RefreshAction(artifact, localization, projectManager, loadingOverlayService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Project manager not provided or is null"));
    }));

    it("throws exception when artifact is null", 
        inject((
            localization: ILocalizationService,
            projectManager: IProjectManager,
            loadingOverlayService: ILoadingOverlayService,
            statefulArtifactFactory: IStatefulArtifactFactory
            ) => {
        // arrange
        artifact = null;
        let error: Error = null;

        // act
        try {
            new RefreshAction(artifact, localization, projectManager, loadingOverlayService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Artifact not provided or is null"));
    }));

    it("throws exception when loadingOverlayService is null", 
        inject((
            localization: ILocalizationService,
            projectManager: IProjectManager,
            statefulArtifactFactory: IStatefulArtifactFactory, 
            ) => {
        // arrange
        const loadingOverlayService: ILoadingOverlayService = null;
        let error: Error = null;

        // act
        try {
            new RefreshAction(artifact, localization, projectManager, loadingOverlayService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Loading overlay service not provided or is null"));
    }));

    it("is disabled when artifact is read-only", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService,
            projectManager: IProjectManager,
            loadingOverlayService: ILoadingOverlayService) => {
        // arrange
        artifact = statefulArtifactFactory.createStatefulArtifact({ id: 1 });
        artifact.artifactState.readonly = true;

        // act
        const refreshAction = new RefreshAction(artifact, localization, projectManager, loadingOverlayService);

        // assert
        expect(refreshAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is dirty", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService,
            projectManager: IProjectManager,
            loadingOverlayService: ILoadingOverlayService) => {
        // arrange
        artifact = statefulArtifactFactory.createStatefulArtifact(
            { 
                id: 1, 
                predefinedType: ItemTypePredefined.TextualRequirement, 
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit
            });
        artifact.artifactState.dirty = true;

        // act
        const refreshAction = new RefreshAction(artifact, localization, projectManager, loadingOverlayService);

        // assert
        expect(refreshAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is Project", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService,
            projectManager: IProjectManager,
            loadingOverlayService: ILoadingOverlayService) => {
        // arrange
        artifact = statefulArtifactFactory.createStatefulArtifact(
            {
                 id: 1, 
                 predefinedType: ItemTypePredefined.Project 
            });

        // act
        const refreshAction = new RefreshAction(artifact, localization, projectManager, loadingOverlayService);

        // assert
        expect(refreshAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is Collections", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService,
            projectManager: IProjectManager,
            loadingOverlayService: ILoadingOverlayService) => {
        // arrange
        artifact = statefulArtifactFactory.createStatefulArtifact(
            { 
                id: 1, 
                predefinedType: ItemTypePredefined.Collections 
            });

        // act
        const refreshAction = new RefreshAction(artifact, localization, projectManager, loadingOverlayService);

        // assert
        expect(refreshAction.disabled).toBe(true);
    }));

    it("is enabled when artifact is valid", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService,
            projectManager: IProjectManager,
            loadingOverlayService: ILoadingOverlayService) => {
        // arrange
        artifact = statefulArtifactFactory.createStatefulArtifact(
            { 
                id: 1, 
                predefinedType: ItemTypePredefined.TextualRequirement, 
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit
            });

        // act
        const refreshAction = new RefreshAction(artifact, localization, projectManager, loadingOverlayService);

        // assert
        expect(refreshAction.disabled).toBe(false);
    }));

    describe("when executed", () => {
        let refreshAction: RefreshAction;
        let beginLoadingSpy: jasmine.Spy;
        let refreshSpy: jasmine.Spy;
        let projectRefreshSpy: jasmine.Spy;
        let endLoadingSpy: jasmine.Spy;

        beforeEach(inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService,
            projectManager: IProjectManager,
            loadingOverlayService: ILoadingOverlayService
        ) => {
            // arrange
            artifact = statefulArtifactFactory.createStatefulArtifact(
            { 
                id: 1, 
                predefinedType: ItemTypePredefined.TextualRequirement, 
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit
            });
            refreshAction = new RefreshAction(artifact, localization, projectManager, loadingOverlayService);
            beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading").and.callThrough();
            refreshSpy = spyOn(artifact, "refresh");
            projectRefreshSpy = spyOn(projectManager, "refresh").and.callThrough();
            endLoadingSpy = spyOn(loadingOverlayService, "endLoading").and.callThrough();
        }));

        describe("and refresh fails", () => {
            beforeEach(() => {
                // arrange
                refreshSpy.and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject(null);
                    return deferred.promise;
                });

                // act
                refreshAction.execute();
                $scope.$digest();
            });

            it("shows loading screen", () => {
                // assert
                expect(beginLoadingSpy).toHaveBeenCalled();
            });

            it("calls refresh on artifact", () => {
                // assert
                expect(refreshSpy).toHaveBeenCalled();
            });

            it("calls project manager refresh", () => {
                // assert
                expect(projectRefreshSpy).toHaveBeenCalled();
            });

            it("hides loading screen", () => {
                // assert
                expect(endLoadingSpy).toHaveBeenCalled();
            });
        });

        describe("and refresh succeeds", () => {
            beforeEach(() => {
                refreshSpy.and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                // act
                refreshAction.execute();
                $scope.$digest();
            });

            it("shows loading screen", () => {
                // assert
                expect(beginLoadingSpy).toHaveBeenCalled();
            });

            it("calls refresh on artifact", () => {
                // assert
                expect(refreshSpy).toHaveBeenCalled();
            });

            it("hides loading screen", () => {
                // assert
                expect(endLoadingSpy).toHaveBeenCalled();
            });
        });
    });
});