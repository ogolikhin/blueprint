import * as angular from "angular";
import "angular-mocks";
import "rx";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {LoadingOverlayServiceMock} from "../../../../core/loading-overlay/loading-overlay.svc.mock";
import {PublishArtifactsAction} from "./publish-artifacts-action";
import {
    UnpublishedArtifactsService,
    IUnpublishedArtifactsService
} from "../../../../editors/unpublished/unpublished.svc";
import {IArtifact} from "../../../models/models";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {IDialogService} from "../../../../shared";

describe("PublishArtifactsAction", () => {
    let $q: ng.IQService;
    let $timeout: ng.ITimeoutService;
    let localization: ILocalizationService;
    let loadingOverlayService: ILoadingOverlayService;
    let messageService: IMessageService;
    let publishService: IUnpublishedArtifactsService;
    let dialogService: IDialogService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("publishService", UnpublishedArtifactsService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("dialogService", DialogServiceMock);
    }));

    beforeEach(inject((_$q_: ng.IQService,
                       _messageService_: IMessageService,
                       _localization_: ILocalizationService,
                       _publishService_: IUnpublishedArtifactsService,
                       _loadingOverlayService_: ILoadingOverlayService,
                       _$timeout_: ng.ITimeoutService,
                       _dialogService_: IDialogService) => {
        $q = _$q_;
        $timeout = _$timeout_;
        messageService = _messageService_;
        localization = _localization_;
        publishService = _publishService_;
        loadingOverlayService = _loadingOverlayService_;
        dialogService = _dialogService_;
        
    }));

    it("throws exception when localization is null", () => {
        expect(() => new PublishArtifactsAction(publishService, null, messageService, loadingOverlayService, dialogService))
            .toThrow(new Error("Localization service not provided or is null"));
    });

    it("is disabled when no artifacts are provided", () => {
        // arrange
        const artifacts = [];
        const action = new PublishArtifactsAction(publishService, localization, messageService, loadingOverlayService, dialogService);

        // act
        action.updateList(artifacts);

        // assert
        expect(action.disabled).toBe(true);
    });

    it("is enabled when artifacts are provided", () => {
        // arrange
        const artifacts = <IArtifact[]>[{id: 1}, {id: 2}];
        const action = new PublishArtifactsAction(publishService, localization, messageService, loadingOverlayService, dialogService);

        // act
        action.updateList(artifacts);

        // assert
        expect(action.disabled).toBe(false);
    });

    it("calls publish and shows message when successfully executed", () => {
        // arrange
        const artifacts = <IArtifact[]>[{id: 1}, {id: 2}];
        const action = new PublishArtifactsAction(publishService, localization, messageService, loadingOverlayService, dialogService);
        action.updateList(artifacts);

        const publishSpy = spyOn(publishService, "publishArtifacts").and.returnValue($q.resolve({
            artifacts: artifacts,
            projects: [{id: 1}]
        }));
        const messageSpy = spyOn(messageService, "addInfo");
        const overlaySpy = spyOn(loadingOverlayService, "endLoading");

        // act
        action.execute();
        $timeout.flush();

        // assert
        expect(publishSpy).toHaveBeenCalled();
        expect(messageSpy).toHaveBeenCalled();
        expect(overlaySpy).toHaveBeenCalled();
    });

    it("reloads unpublished artifacts and shows error when un-successfully executed", () => {
        // arrange
        const artifacts = <IArtifact[]>[{id: 1}, {id: 2}];
        const action = new PublishArtifactsAction(publishService, localization, messageService, loadingOverlayService, dialogService);
        action.updateList(artifacts);

        const publishSpy = spyOn(publishService, "publishArtifacts").and.returnValue($q.reject("error"));
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
