import * as angular from "angular";
import "angular-mocks";
import "rx";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {LoadingOverlayServiceMock} from "../../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {
    UnpublishedArtifactsService,
    IUnpublishedArtifactsService
} from "../../../../editorsModule/unpublished/unpublished.service";
import {IArtifact} from "../../../models/models";
import {DiscardArtifactsAction} from "./discard-artifacts-action";
import {IDialogService} from "../../../../shared/";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {IMessageService} from "../../messages/message.svc";
import {MessageServiceMock} from "../../messages/message.mock";
import {ProjectExplorerServiceMock} from "../../bp-explorer/project-explorer.service.mock";
import {IProjectExplorerService} from "../../bp-explorer/project-explorer.service";

describe("DiscardArtifactsAction", () => {
    let $q: ng.IQService;
    let $timeout: ng.ITimeoutService;
    let localization: ILocalizationService;
    let loadingOverlayService: ILoadingOverlayService;
    let messageService: IMessageService;
    let publishService: IUnpublishedArtifactsService;
    let projectExplorerService: IProjectExplorerService;
    let dialogService: IDialogService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("publishService", UnpublishedArtifactsService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("projectExplorerService", ProjectExplorerServiceMock);
        $provide.service("dialogService", DialogServiceMock);
    }));

    beforeEach(inject((_$q_: ng.IQService,
                       _messageService_: IMessageService,
                       _localization_: ILocalizationService,
                       _publishService_: IUnpublishedArtifactsService,
                       _loadingOverlayService_: ILoadingOverlayService,
                       _projectExplorerService_: IProjectExplorerService,
                       _$timeout_: ng.ITimeoutService,
                       _dialogService_: IDialogService) => {
        $q = _$q_;
        $timeout = _$timeout_;
        messageService = _messageService_;
        localization = _localization_;
        publishService = _publishService_;
        projectExplorerService = _projectExplorerService_;
        loadingOverlayService = _loadingOverlayService_;
        dialogService = _dialogService_;
    }));

    it("throws exception when localization is null", () => {
        expect(() => new DiscardArtifactsAction(publishService, null, messageService, loadingOverlayService, projectExplorerService, dialogService))
            .toThrow(new Error("Localization service not provided or is null"));
    });

    it("is disabled when no artifacts are provided", () => {
        // arrange
        const artifacts = [];
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectExplorerService, dialogService);

        // act
        action.updateList(artifacts);

        // assert
        expect(action.disabled).toBe(true);
    });

    it("is enabled when artifacts are provided", () => {
        // arrange
        const artifacts = <IArtifact[]>[{id: 1}, {id: 2}];
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectExplorerService, dialogService);

        // act
        action.updateList(artifacts);

        // assert
        expect(action.disabled).toBe(false);
    });

    it("calls discard and refreshes projects (if project loaded) when successfully executed", () => {
        // arrange
        const artifacts = <IArtifact[]>[{id: 1}, {id: 2}];
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectExplorerService, dialogService);
        action.updateList(artifacts);

        const discardSpy = spyOn(publishService, "discardArtifacts").and.returnValue($q.resolve({
            artifacts: artifacts,
            projects: [{id: 1}]
        }));
        const projectCollectionSpy = spyOn(projectExplorerService, "projects").and.returnValue([1]);
        const projectSpy = spyOn(projectExplorerService, "refreshAll");
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
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectExplorerService, dialogService);
        action.updateList(artifacts);

        const discardSpy = spyOn(publishService, "discardArtifacts").and.returnValue($q.resolve({
            artifacts: artifacts,
            projects: [{id: 1}]
        }));
        const projectCollectionSpy = spyOn(projectExplorerService, "projects").and.returnValue([]);
        const projectSpy = spyOn(projectExplorerService, "refreshAll");
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
        const action = new DiscardArtifactsAction(publishService, localization, messageService, loadingOverlayService, projectExplorerService, dialogService);
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
