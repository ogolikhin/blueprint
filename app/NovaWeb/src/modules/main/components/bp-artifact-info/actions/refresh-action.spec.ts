import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {RolePermissions} from "../../../models/enums";
import {IMetaDataService, IStatefulArtifact, IStatefulArtifactFactory, MetaDataService} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {LogMock} from "../../../../shell/log/server-logger.svc.mock";
import {ItemTypePredefined} from "../../../models/itemTypePredefined.enum";
import {IMainBreadcrumbService} from "../../bp-page-content/mainbreadcrumb.svc";
import {MainBreadcrumbServiceMock} from "../../bp-page-content/mainbreadcrumb.svc.mock";
import {ProjectExplorerServiceMock} from "../../bp-explorer/project-explorer.service.mock";
import {IProjectExplorerService} from "../../bp-explorer/project-explorer.service";
import {NavigationServiceMock} from "../../../../commonModule/navigation/navigation.service.mock";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {RefreshAction} from "./refresh-action";

describe("RefreshAction", () => {
    let $scope: ng.IScope;
    let $q: ng.IQService;
    let artifact: IStatefulArtifact;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectExplorerService", ProjectExplorerServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("navigationService", NavigationServiceMock);
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

    it("is disabled when artifact is dirty",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                navigationService: INavigationService,
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
                loadingOverlayService, metaDataService, navigationService, mainBreadcrumbService);

            // assert
            expect(refreshAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is Project",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                navigationService: INavigationService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            artifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const refreshAction = new RefreshAction(artifact, localization, projectExplorerService,
                loadingOverlayService, metaDataService, navigationService, mainBreadcrumbService);

            // assert
            expect(refreshAction.disabled).toBe(false);
        }));

    it("is disabled when artifact is Collections",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                navigationService: INavigationService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            artifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const refreshAction = new RefreshAction(artifact, localization, projectExplorerService,
                loadingOverlayService, metaDataService, navigationService, mainBreadcrumbService);

            // assert
            expect(refreshAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Baselines and Reviews",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                navigationService: INavigationService,
                mainBreadcrumbService: IMainBreadcrumbService) => {
            // arrange
            artifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.BaselinesAndReviews
                });

            // act
            const refreshAction = new RefreshAction(artifact, localization, projectExplorerService,
                loadingOverlayService, metaDataService, navigationService, mainBreadcrumbService);

            // assert
            expect(refreshAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is valid",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                projectExplorerService: IProjectExplorerService,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                navigationService: INavigationService,
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
                loadingOverlayService, metaDataService, navigationService, mainBreadcrumbService);

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
                           navigationService: INavigationService,
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
            refreshAction = new RefreshAction(artifact, localization, projectExplorerService, loadingOverlayService,
                metaDataService, navigationService, mainBreadcrumbService);
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
        let projectExplorerSpy: jasmine.Spy;
        let endLoadingSpy: jasmine.Spy;

        beforeEach(inject((statefulArtifactFactory: IStatefulArtifactFactory,
                           localization: ILocalizationService,
                           projectExplorerService: IProjectExplorerService,
                           loadingOverlayService: ILoadingOverlayService,
                           metaDataService: IMetaDataService,
                           navigationService: INavigationService,
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
            refreshAction = new RefreshAction(artifact, localization, projectExplorerService, loadingOverlayService,
                metaDataService, navigationService, mainBreadcrumbService);

            beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading").and.callThrough();
            projectExplorerSpy = spyOn(projectExplorerService, "refresh").and.callThrough();
            endLoadingSpy = spyOn(loadingOverlayService, "endLoading").and.callThrough();
        }));

        describe("and refresh succeeds", () => {
            beforeEach(() => {
                projectExplorerSpy.and.callFake(() => $q.resolve());

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
                expect(projectExplorerSpy).toHaveBeenCalled();
            });

            it("hides loading screen", () => {
                // assert
                expect(endLoadingSpy).toHaveBeenCalled();
            });
        });
    });
});
