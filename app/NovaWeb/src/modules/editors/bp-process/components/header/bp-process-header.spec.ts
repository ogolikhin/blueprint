import * as angular from "angular";
import "angular-mocks";
import {BpProcessHeaderController} from "./bp-process-header";
import {IBreadcrumbLink} from "../../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {IBreadcrumbService} from "../../services/breadcrumb.svc";
import {INavigationService} from "../../../../core/navigation";
import {NavigationServiceMock} from "../../../../core/navigation/navigation.svc.mock";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {WindowManager} from "../../../../main";
import {CommunicationManager} from "../../";
import {
    ArtifactManager,
    ArtifactService,
    MetaDataService,
    StatefulArtifactFactory,
    ArtifactAttachmentsService,
    ArtifactRelationshipsService
} from "../../../../managers/artifact-manager";
import {LoadingOverlayService} from "../../../../core/loading-overlay";
import {BreadcrumbServiceMock} from "../../services/breadcrumb.svc.mock";
import {SelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {SessionSvcMock} from "../../../../shell/login/mocks.spec";
import {WindowResize} from "../../../../core/services/window-resize";
import {ProjectManager} from "../../../../managers/project-manager/project-manager";
import {ProjectService} from "../../../../managers/project-manager/project-service";

describe("BpProcessHeader", () => {
    let $rootScope: ng.IRootScopeService;
    let $q: ng.IQService;
    let $compile: ng.ICompileService;
    let controller: BpProcessHeaderController;
    let breadcrumbService: IBreadcrumbService;
    let navigationService: INavigationService;

    beforeEach(angular.mock.module("bp.editors.process", ($provide: ng.auto.IProvideService) => {
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("windowManager", WindowManager);
        $provide.service("windowResize", WindowResize);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("breadcrumbService", BreadcrumbServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("session", SessionSvcMock);
        $provide.service("artifactService", ArtifactService);
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
        $provide.service("artifactRelationships", ArtifactRelationshipsService);
        $provide.service("projectManager", ProjectManager);
        $provide.service("projectService", ProjectService);
    }));

    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$compile_: ng.ICompileService,
                       _$q_: ng.IQService,
                       _breadcrumbService_: IBreadcrumbService,
                       _navigationService_: INavigationService) => {
        $rootScope = _$rootScope_;
        $q = _$q_;
        $compile = _$compile_;
        breadcrumbService = _breadcrumbService_;
        navigationService = _navigationService_;
    }));

    xit("correctly initializes breadcrumb", () => {
        // arrange
        const deferred = $q.defer();
        deferred.resolve([
            // should have isEnabled = false since no link
            {id: 0, name: "link0"},
            {id: 1, name: "link1", link: "http//link1"},
            // should have isEnabled = false since last link
            {id: 2, name: "link2", link: "http://link2"}
        ]);
        spyOn(breadcrumbService, "getReferences").and.returnValue(deferred.promise);

        const template = "<bp-process-header></bp-process-header>";
        const element = $compile(template)($rootScope.$new());
        controller = element.controller("bpProcessHeader");

        const link0 = <IBreadcrumbLink>{id: 0, name: "link0", isEnabled: false};
        const link1 = <IBreadcrumbLink>{id: 1, name: "link1", isEnabled: true};
        const link2 = <IBreadcrumbLink>{id: 2, name: "link2", isEnabled: false};

        // act
        $rootScope.$digest();

        // assert
        expect(controller.breadcrumbLinks).not.toBeNull();
        expect(controller.breadcrumbLinks.length).toBe(3);
        expect(controller.breadcrumbLinks[0]).toEqual(link0);
        expect(controller.breadcrumbLinks[1]).toEqual(link1);
        expect(controller.breadcrumbLinks[2]).toEqual(link2);
    });

    describe("navigateTo method", () => {
        beforeEach(() => {
            const template = "<bp-process-header></bp-process-header>";
            const element = $compile(template)($rootScope.$new());
            controller = element.controller("bpProcessHeader");
        });

        afterEach(() => {
            controller = null;
        });

        it("doesn't navigate if link is null", () => {
            // arrange
            const navigateBackSpy = spyOn(navigationService, "navigateBack");
            controller.breadcrumbLinks = [];

            // act
            controller.navigateTo(null);

            // assert
            expect(navigateBackSpy).not.toHaveBeenCalled();
        });

        it("doesn't navigate to link that's not part of the breadcrumb", () => {
            // arrange
            const link = <IBreadcrumbLink>{id: 0, name: "enabled link", isEnabled: true};
            const navigateBackSpy = spyOn(navigationService, "navigateBack");
            controller.breadcrumbLinks = [];

            // act
            controller.navigateTo(link);

            // assert
            expect(navigateBackSpy).not.toHaveBeenCalled();
        });

        it("does not navigate to disabled link", () => {
            // arrange
            const link = <IBreadcrumbLink>{id: 0, name: "disabled link", isEnabled: false};
            const navigateBackSpy = spyOn(navigationService, "navigateBack");
            controller.breadcrumbLinks = [link];

            // act
            controller.navigateTo(link);

            // assert
            expect(navigateBackSpy).not.toHaveBeenCalled();
        });

        it("navigates to enabled link", () => {
            // arrange
            const link = <IBreadcrumbLink>{id: 0, name: "enabled link", isEnabled: true};
            const navigateBackSpy = spyOn(navigationService, "navigateBack");
            controller.breadcrumbLinks = [link];

            // act
            controller.navigateTo(link);

            // assert
            expect(navigateBackSpy).toHaveBeenCalledWith(0);
        });
    });
});
