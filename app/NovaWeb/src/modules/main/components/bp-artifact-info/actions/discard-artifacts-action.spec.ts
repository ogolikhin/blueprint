import * as angular from "angular";
import "angular-mocks";
import "rx";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {LoadingOverlayServiceMock} from "../../../../core/loading-overlay/loading-overlay.svc.mock";
import {
    UnpublishedArtifactsService,
    IUnpublishedArtifactsService
} from "../../../../editors/unpublished/unpublished.svc";
import {IArtifact} from "../../../models/models";
import {DiscardArtifactsAction} from "./discard-artifacts-action";
import {ProjectManagerMock} from "../../../../managers/project-manager/project-manager.mock";
import {IProjectManager} from "../../../../managers/project-manager/project-manager";

describe("DiscardArtifactsAction", () => {
    let $q: ng.IQService;
    let $timeout: ng.ITimeoutService;
    let localization: ILocalizationService;
    let loadingOverlayService: ILoadingOverlayService;
    let messageService: IMessageService;
    let publishService: IUnpublishedArtifactsService;
    let projectManager: IProjectManager;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("publishService", UnpublishedArtifactsService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("projectManager", ProjectManagerMock);
    }));

    beforeEach(inject((_$q_: ng.IQService,
                       _messageService_: IMessageService,
                       _localization_: ILocalizationService,
                       _publishService_: IUnpublishedArtifactsService,
                       _loadingOverlayService_: ILoadingOverlayService,
                       _projectManager_: IProjectManager,
                       _$timeout_: ng.ITimeoutService) => {
        $q = _$q_;
        $timeout = _$timeout_;
        messageService = _messageService_;
        localization = _localization_;
        publishService = _publishService_;
        projectManager = _projectManager_;
        loadingOverlayService = _loadingOverlayService_;
    }));

    it("throws exception when localization is null", () => {
        expect(() => new DiscardArtifactsAction(publishService, null, messageService, loadingOverlayService, projectManager))
            .toThrow(new Error("Localization service not provided or is null"));
    });

    it("is disabled when no artifacts are provided", () => {
        // arrange
        const artifacts = [];
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectManager);

        // act
        action.updateList(artifacts);

        // assert
        expect(action.disabled).toBe(true);
    });

    it("is enabled when artifacts are provided", () => {
        // arrange
        const artifacts = <IArtifact[]>[{id: 1}, {id: 2}];
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectManager);

        // act
        action.updateList(artifacts);

        // assert
        expect(action.disabled).toBe(false);
    });

    it("calls discard and refreshes projects (if project loaded) when successfully executed", () => {
        // arrange
        const artifacts = <IArtifact[]>[{id: 1}, {id: 2}];
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectManager);
        action.updateList(artifacts);

        const discardSpy = spyOn(publishService, "discardArtifacts").and.returnValue($q.resolve({
            artifacts: artifacts,
            projects: [{id: 1}]
        }));
        const projectCollectionSpy = spyOn(projectManager.projectCollection, "getValue").and.returnValue([1]);
        const projectSpy = spyOn(projectManager, "refreshAll");
        const messageSpy = spyOn(messageService, "addInfo");
        const overlaySpy = spyOn(loadingOverlayService, "endLoading");

        // act
        action.execute();
        $timeout.flush();

        // assert
        expect(discardSpy).toHaveBeenCalled();
        expect(projectSpy).toHaveBeenCalled();
        expect(messageSpy).toHaveBeenCalled();
        expect(overlaySpy).toHaveBeenCalled();
    });

    it("calls discard and doesn't refresh projects (if no project loaded) when successfully executed", () => {
        // arrange
        const artifacts = <IArtifact[]>[{id: 1}, {id: 2}];
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectManager);
        action.updateList(artifacts);

        const discardSpy = spyOn(publishService, "discardArtifacts").and.returnValue($q.resolve({
            artifacts: artifacts,
            projects: [{id: 1}]
        }));
        const projectCollectionSpy = spyOn(projectManager.projectCollection, "getValue").and.returnValue([]);
        const projectSpy = spyOn(projectManager, "refreshAll");
        const messageSpy = spyOn(messageService, "addInfo");
        const overlaySpy = spyOn(loadingOverlayService, "endLoading");

        // act
        action.execute();
        $timeout.flush();

        // assert
        expect(discardSpy).toHaveBeenCalled();
        expect(projectSpy).not.toHaveBeenCalled();
        expect(messageSpy).toHaveBeenCalled();
        expect(overlaySpy).toHaveBeenCalled();
    });

    it("reloads unpublished artifacts and shows error when un-successfully executed", () => {
        // arrange
        const artifacts = <IArtifact[]>[{id: 1}, {id: 2}];
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectManager);
        action.updateList(artifacts);

        const publishSpy = spyOn(publishService, "discardArtifacts").and.returnValue($q.reject("error"));
        const unpublishedSpy = spyOn(publishService, "getUnpublishedArtifacts");
        const errorMessageSpy = spyOn(messageService, "addError");
        const overlaySpy = spyOn(loadingOverlayService, "endLoading");

        // act
        action.execute();
        $timeout.flush();

        // assert
        expect(publishSpy).toHaveBeenCalled();
        expect(unpublishedSpy).toHaveBeenCalled();
        expect(errorMessageSpy).toHaveBeenCalled();
        expect(overlaySpy).toHaveBeenCalled();
    });
});
