import * as angular from "angular";
import "angular-mocks";
import {BpProcessHeaderController} from "./bp-process-header";
import {IBreadcrumbLink} from "../../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {IBreadcrumbService} from "../../services/breadcrumb.svc";
import {NavigationServiceMock} from "../../../../commonModule/navigation/navigation.service.mock";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {WindowManager} from "../../../../main";
import {CommunicationManager} from "../../";
import {
    ArtifactService,
    MetaDataService,
    ArtifactAttachmentsService,
    ArtifactRelationshipsService
} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {BreadcrumbServiceMock} from "../../services/breadcrumb.svc.mock";
import {SelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {WindowResize} from "../../../../commonModule/services/windowResize";
import {ProjectManager} from "../../../../managers/project-manager/project-manager";
import {ProjectService} from "../../../../managers/project-manager/project-service";
import {MainBreadcrumbServiceMock} from "../../../../main/components/bp-page-content/mainbreadcrumb.svc.mock";
import {ItemInfoService} from "../../../../commonModule/itemInfo/itemInfo.service";
import {LoadingOverlayServiceMock} from "../../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {CollectionServiceMock} from "../../../collection/collection.service.mock";
import {MessageServiceMock} from "../../../../main/components/messages/message.mock";
import {SessionSvcMock} from "../../../../shell/login/session.svc.mock";
import {IAnalyticsService, AnalyticsServiceMock} from "../../../../main/components/analytics";

describe("BpProcessHeader", () => {
    let $rootScope: ng.IRootScopeService;
    let $q: ng.IQService;
    let $compile: ng.ICompileService;
    let controller: BpProcessHeaderController;
    let localization: LocalizationServiceMock;
    let breadcrumbService: IBreadcrumbService;
    let analyticsService: IAnalyticsService;

    beforeEach(angular.mock.module("bp.editors.process", ($provide: ng.auto.IProvideService) => {
            $provide.service("selectionManager", SelectionManager);
            $provide.service("localization", LocalizationServiceMock);
            $provide.service("messageService", MessageServiceMock);
            $provide.service("dialogService", DialogServiceMock);
            $provide.service("windowManager", WindowManager);
            $provide.service("windowResize", WindowResize);
            $provide.service("communicationManager", CommunicationManager);
            $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
            $provide.service("navigationService", NavigationServiceMock);
            $provide.service("breadcrumbService", BreadcrumbServiceMock);
            $provide.service("metadataService", MetaDataService);
            $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
            $provide.service("session", SessionSvcMock);
            $provide.service("artifactService", ArtifactService);
            $provide.service("artifactAttachments", ArtifactAttachmentsService);
            $provide.service("artifactRelationships", ArtifactRelationshipsService);
            $provide.service("itemInfoService", ItemInfoService);
            $provide.service("projectManager", ProjectManager);
            $provide.service("projectService", ProjectService);
            $provide.service("mainbreadcrumbService", MainBreadcrumbServiceMock);
            $provide.service("collectionService", CollectionServiceMock);
            $provide.service("analyticsService", AnalyticsServiceMock);
        }));

    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$compile_: ng.ICompileService,
                       _$q_: ng.IQService,
                       _localization_: LocalizationServiceMock,
                       _breadcrumbService_: IBreadcrumbService) => {
        $rootScope = _$rootScope_;
        $q = _$q_;
        $compile = _$compile_;
        localization = _localization_;
        breadcrumbService = _breadcrumbService_;
    }));

    it("correctly initializes breadcrumb", () => {
        // arrange
        const deferred = $q.defer();
        deferred.resolve([
            {id: 0, name: "link0", version: undefined, accessible: false},
            {id: 1, name: "link1", version: undefined, accessible: true},
            {id: 2, name: "link2", version: undefined, accessible: true}
        ]);
        spyOn(breadcrumbService, "getReferences").and.returnValue(deferred.promise);

        const template = "<bp-process-header></bp-process-header>";
        const element = $compile(template)($rootScope.$new());
        controller = element.controller("bpProcessHeader");

        const link0 = <IBreadcrumbLink>{
            id: 0,
            name: localization.get("ST_Breadcrumb_InaccessibleArtifact"),
            version: undefined,
            isEnabled: false
        };
        const link1 = <IBreadcrumbLink>{
            id: 1,
            name: "link1",
            version: undefined,
            isEnabled: true
        };

        // act
        $rootScope.$digest();

        // assert
        expect(controller.breadcrumbLinks).not.toBeNull();
        expect(controller.breadcrumbLinks.length).toBe(2);
        expect(controller.breadcrumbLinks[0]).toEqual(link0);
        expect(controller.breadcrumbLinks[1]).toEqual(link1);
    });
});
