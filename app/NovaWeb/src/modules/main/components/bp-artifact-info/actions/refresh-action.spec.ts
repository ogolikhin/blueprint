import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {RefreshAction} from "./refresh-action";
import {
    IStatefulArtifact,
    IStatefulArtifactFactory,
    IMetaDataService,
    MetaDataService
} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {ItemTypePredefined, RolePermissions} from "../../../models/enums";
import {LogMock} from "../../../../shell/log/server-logger.svc.mock";
import {LoadingOverlayService, ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {MainBreadcrumbServiceMock} from "../../bp-page-content/mainbreadcrumb.svc.mock";
import {MainBreadcrumbService, IMainBreadcrumbService} from "../../bp-page-content/mainbreadcrumb.svc";
import {ProjectExplorerServiceMock} from "../../bp-explorer/project-explorer.service.mock";
import {IProjectExplorerService} from "../../bp-explorer/project-explorer.service";

xdescribe("RefreshAction", () => {
    let $scope: ng.IScope;
    let $q: ng.IQService;
    let artifact: IStatefulArtifact;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectExplorerService", ProjectExplorerServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("metaDataService", MetaDataService);
        $provide.service("$log", LogMock);
        $provide.service("mainBreadcrumbService", MainBreadcrumbServiceMock);
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
        inject((projectExplorerService: IProjectExplorerService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            const localization: ILocalizationService = null;
            let error: Error = null;

            // act
            try {
                new RefreshAction(artifact, localization, projectExplorerService, loadingOverlayService, metaDataService, mainBreadcrumbService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error).toEqual(new Error("Localization service not provided or is null"));
        }));

    it("throws exception when projectExplorerService is null",
        inject((localization: ILocalizationService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            const projectExplorerService: IProjectExplorerService = null;
            let error: Error = null;

            // act
            try {
                new RefreshAction(artifact, localization, projectExplorerService, loadingOverlayService, metaDataService, mainBreadcrumbService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error).toEqual(new Error("Project manager not provided or is null"));
        }));

    it("throws exception when artifact is null",
        inject((localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            artifact = null;
            let error: Error = null;

            // act
            try {
                new RefreshAction(artifact, localization, projectExplorerService, loadingOverlayService, metaDataService, mainBreadcrumbService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error).toEqual(new Error("Artifact not provided or is null"));
        }));

    it("throws exception when loadingOverlayService is null",
        inject((localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            const loadingOverlayService: ILoadingOverlayService = null;
            let error: Error = null;

            // act
            try {
                new RefreshAction(artifact, localization, projectExplorerService, loadingOverlayService, metaDataService, mainBreadcrumbService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error).toEqual(new Error("Loading overlay service not provided or is null"));
        }));

    it("throws exception when metaDataService is null",
        inject((localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            const metaDataService: IMetaDataService = null;
            let error: Error = null;

            // act
            try {
                new RefreshAction(artifact, localization, projectExplorerService, loadingOverlayService, metaDataService, mainBreadcrumbService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error).toEqual(new Error("MetaData service not provided or is null"));
        }));

    it("is disabled when artifact is dirty",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
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
            const refreshAction = new RefreshAction(artifact, localization, projectExplorerService,
                loadingOverlayService, metaDataService, mainBreadcrumbService);

            // assert
            expect(refreshAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is Project",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            artifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const refreshAction = new RefreshAction(artifact, localization, projectExplorerService,
                loadingOverlayService, metaDataService, mainBreadcrumbService);

            // assert
            expect(refreshAction.disabled).toBe(false);
        }));

    it("is disabled when artifact is Collections",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            artifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const refreshAction = new RefreshAction(artifact, localization, projectExplorerService,
                loadingOverlayService, metaDataService, mainBreadcrumbService);

            // assert
            expect(refreshAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Baselines and Reviews",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            artifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.BaselinesAndReviews
                });

            // act
            const refreshAction = new RefreshAction(artifact, localization, projectExplorerService,
                loadingOverlayService, metaDataService, mainBreadcrumbService);

            // assert
            expect(refreshAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is valid",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
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
            const refreshAction = new RefreshAction(artifact, localization, projectExplorerService,
                loadingOverlayService, metaDataService, mainBreadcrumbService);

            // assert
            expect(refreshAction.disabled).toBe(false);
        }));

    describe("when executed", () => {
        let refreshAction: RefreshAction;
        let beginLoadingSpy: jasmine.Spy;
        let refreshSpy: jasmine.Spy;
        let projectRefreshSpy: jasmine.Spy;
        let endLoadingSpy: jasmine.Spy;
        let reloadBreadcrumbSpy: jasmine.Spy;

        beforeEach(inject((statefulArtifactFactory: IStatefulArtifactFactory,
                           localization: ILocalizationService,
                           projectExplorerService: IProjectExplorerService,
                           loadingOverlayService: ILoadingOverlayService,
                           metaDataService: IMetaDataService,
                           mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            artifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            refreshAction = new RefreshAction(artifact, localization, projectExplorerService, loadingOverlayService, metaDataService, mainBreadcrumbService);
            beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading").and.callThrough();
            refreshSpy = spyOn(artifact, "refresh");
            projectRefreshSpy = spyOn(projectExplorerService, "refresh").and.callThrough();
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

            // This is now done indirectly - project only refreshes if the artifact is currently selected
            // in selection manager.
            // it("calls project manager refresh", () => {
            //     // assert
            //     expect(projectRefreshSpy).toHaveBeenCalled();
            // });

            it("hides loading screen", () => {
                // assert
                expect(endLoadingSpy).toHaveBeenCalled();
            });
        });

        describe("and refresh succeeds", () => {
            beforeEach(inject((mainBreadcrumbService: IMainBreadcrumbService) => {
                refreshSpy.and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });
                reloadBreadcrumbSpy = spyOn(mainBreadcrumbService, "reloadBreadcrumbs");

                // act
                refreshAction.execute();
                $scope.$digest();
            }));

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

            it("reloads breadcrumb", () => {
                // assert
                expect(reloadBreadcrumbSpy).toHaveBeenCalled();
            });
        });
    });

    describe("when project refresh executed", () => {
        let refreshAction: RefreshAction;
        let beginLoadingSpy: jasmine.Spy;
        let projectRefreshSpy: jasmine.Spy;
        let endLoadingSpy: jasmine.Spy;

        beforeEach(inject((statefulArtifactFactory: IStatefulArtifactFactory,
                           localization: ILocalizationService,
                           projectExplorerService: IProjectExplorerService,
                           loadingOverlayService: ILoadingOverlayService,
                           metaDataService: IMetaDataService,
                           mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            artifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            refreshAction = new RefreshAction(artifact, localization, projectExplorerService, loadingOverlayService, metaDataService, mainBreadcrumbService);
            beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading").and.callThrough();
            projectRefreshSpy = spyOn(projectExplorerService, "refreshCurrent").and.callThrough();
            endLoadingSpy = spyOn(loadingOverlayService, "endLoading").and.callThrough();
        }));


        describe("and refresh succeeds", () => {
            beforeEach(() => {
                projectRefreshSpy.and.callFake(() => {
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

            it("calls refresh on Project", () => {
                // assert
                expect(projectRefreshSpy).toHaveBeenCalled();
            });

            it("hides loading screen", () => {
                // assert
                expect(endLoadingSpy).toHaveBeenCalled();
            });
        });
    });
});
