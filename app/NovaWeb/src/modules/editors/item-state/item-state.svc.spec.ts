import * as angular from "angular";
import "angular-mocks";
import "angular-ui-router";
import "../../main";
import {NavigationServiceMock} from "../../core/navigation/navigation.service.mock";
import {IItemInfoService, IItemInfoResult} from "../../core/itemInfo/itemInfo.service";
import {ItemInfoServiceMock} from "../../core/itemInfo/itemInfo.service.mock";
import {INavigationService} from "../../core/navigation/navigation.service";
import {IItemStateService} from "./item-state.svc";
import {HttpStatusCode} from "../../core/httpInterceptor/http-status-code";
import {ILoadingOverlayService} from "../../core/loadingOverlay/loadingOverlay.service";
import {LoadingOverlayServiceMock} from "../../core/loadingOverlay/loadingOverlay.service.mock";
import {IMessageService} from "../../main/components/messages/message.svc";
import {MessageServiceMock} from "../../main/components/messages/message.mock";

describe("Item State Service tests", () => {
    let $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        $state: ng.ui.IStateService,
        messageService: IMessageService,
        navigationService: INavigationService,
        itemInfoService: IItemInfoService,
        itemStateService: IItemStateService,
        loadingOverlayService: ILoadingOverlayService;

    beforeEach(angular.mock.module("ui.router"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("itemInfoService", ItemInfoServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
    }));

    beforeEach(inject((
        _$timeout_: ng.ITimeoutService,
        _$q_: ng.IQService,
        _$state_: ng.ui.IStateService,
        _navigationService_: INavigationService,
        _messageService_: IMessageService,
        _itemInfoService_: IItemInfoService,
        _itemStateService_: IItemStateService,
        _loadingOverlayService_: ILoadingOverlayService) => {

        $timeout = _$timeout_;
        $q = _$q_;
        $state = _$state_;
        loadingOverlayService = _loadingOverlayService_;
        messageService = _messageService_;
        navigationService = _navigationService_;
        itemInfoService = _itemInfoService_;
        itemStateService = _itemStateService_;

        $state.current.name = "main.item";
    }));

    it("should contain a ItemStateService", () => {
        expect(itemStateService).toBeDefined();
    });

    it("returns a success promise for a regular artifact", () => {
        // arrange
        const itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => $q.resolve({
            id: 123,
            projectId: 1
        }));

        // act
        let result: IItemInfoResult;
        itemStateService.getItemInfoResult(123).then(res => {
            result = res;
        });
        $timeout.flush();

        // assert
        expect(result).toBeDefined();
    });

    it("returns a rejected promise for a deleted project", () => {
        // arrange
        const itemInfoProjectSpy = spyOn(itemInfoService, "isProject").and.callFake(() => true);
        const loadingOverlaySpy = spyOn(loadingOverlayService, "beginLoading");
        const messageSpy = spyOn(messageService, "addError");
        const itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => $q.resolve({
            id: 123,
            isDeleted: true
        }));

        // act
        let result: IItemInfoResult;
        let error;
        itemStateService.getItemInfoResult(123).then(res => {
            result = res;
        }).catch(err => {
            error = err;
        });
        $timeout.flush();

        // assert
        expect(result).not.toBeDefined();
        expect(error).toBeDefined();
        expect(error.statusCode).toBe(HttpStatusCode.NotFound);
        expect(loadingOverlaySpy).toHaveBeenCalled();
        expect(messageSpy).toHaveBeenCalled();
    });

    it("navigates to main if the initial state is empty", () => {
        // arrange
        $state.current.name = "";
        const navigationSpy = spyOn(navigationService, "navigateToMain");
        const itemInfoSpy = spyOn(itemInfoService, "get").and.callFake(() => $q.reject({statusCode: HttpStatusCode.NotFound}));

        // act
        itemStateService.getItemInfoResult(123);
        $timeout.flush();

        // assert
        expect(navigationSpy).toHaveBeenCalled();
    });

    it("navigates to main if the provided artifact id is invalid", () => {
        // arrange
        $state.current.name = "";
        const navigationSpy = spyOn(navigationService, "navigateToMain");

        // act
        itemStateService.getItemInfoResult(null);
        $timeout.flush();

        // assert
        expect(navigationSpy).toHaveBeenCalled();
    });
});
