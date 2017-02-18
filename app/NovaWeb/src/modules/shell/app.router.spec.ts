import "angular";
import "angular-mocks";
import "angular-ui-router";
import "rx/dist/rx.lite";
import {ISelectionManager} from "../managers/selection-manager/selection-manager";
import {SelectionManagerMock} from "../managers/selection-manager/selection-manager.mock";
import {MainStateController, LogoutStateController} from "./app.router";
import {INavigationService} from "../commonModule/navigation/navigation.service";
import {NavigationServiceMock} from "../commonModule/navigation/navigation.service.mock";
import {IClipboardService} from "../editorsModule/bp-process/services/clipboard.svc";
import {ISession} from "./login/session.svc";
import {IMessageService} from "../main/components/messages/message.svc";
import {MessageServiceMock} from "../main/components/messages/message.mock";
import {MessageType} from "../main/components/messages/message";
import {SessionSvcMock} from "./login/session.svc.mock";
import {IProjectExplorerService} from "../main/components/bp-explorer/project-explorer.service";

describe("AppRouter", () => {
    let $rootScope: ng.IRootScopeService,
        $window: ng.IWindowService,
        $state: angular.ui.IStateService,
        $log: ng.ILogService,
        selectionManager: ISelectionManager,
        messageService: IMessageService,
        navigationService: INavigationService,
        clipboardService: IClipboardService,
        projectExplorerService: IProjectExplorerService,
        session: ISession,
        isServerLicenseValid: boolean,
        ctrl: MainStateController,
        ctrlLogout: LogoutStateController;

    beforeEach(angular.mock.module("ui.router"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("isServerLicenseValid", Boolean);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("clipboardService", () => {
            return {
                clearData: () => null
            };
        });
        $provide.service("projectExplorerService", () => {
            return {
                removeAll: () => null
            };
        });
        $provide.service("session", SessionSvcMock);
    }));

    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$window_: ng.IWindowService,
                       _$state_: ng.ui.IStateService,
                       _$log_: ng.ILogService,
                       _selectionManager_: ISelectionManager,
                       _messageService_: IMessageService,
                       _navigationService_: INavigationService,
                       _clipboardService_: IClipboardService,
                       _projectExplorerService_: IProjectExplorerService,
                       _session_: ISession,
                       _isServerLicenseValid_: boolean) => {

        $rootScope = _$rootScope_;
        $window = _$window_;
        $state = _$state_;
        $log = _$log_;
        selectionManager = _selectionManager_;
        messageService = _messageService_;
        navigationService = _navigationService_;
        clipboardService = _clipboardService_;
        projectExplorerService = _projectExplorerService_;
        session = _session_;
        isServerLicenseValid = true;
    }));

    describe("$stateChangeSuccess", () => {
        beforeEach(() => {
            ctrl = new MainStateController($rootScope, $state, $log, selectionManager, isServerLicenseValid, messageService);
        });

        xit("should change title if navigating to an artifact", () => {
            // arrange
            $window.document.title = "Storyteller";
            const expectedTitle = "PR123: Artifact Name";
            const fromState = {name: "main"};
            const toState = {name: "main.item"};
            $state.$current.locals.globals["title"] = expectedTitle;

            // act
            $rootScope.$broadcast("$stateChangeSuccess", toState, null, fromState, null);

            // assert
            expect($window.document.title).toBe(expectedTitle);
        });

        xit("should set title to default value if navigating to main", () => {
            // arrange
            $window.document.title = "";
            const expectedTitle = "Storyteller";
            const fromState = {name: "main.item"};
            const toState = {name: "main"};

            // act
            $rootScope.$broadcast("$stateChangeSuccess", toState, null, fromState, null);

            // assert
            expect($window.document.title).toBe(expectedTitle);
        });

        it("should clear normal messages when changing state between artifacts", () => {
            // arrange
            const clearMessagesSpy = spyOn(messageService, "clearMessages").and.callThrough();
            const fromState = {name: "main"};
            const toState = {name: "main.item"};

            // act
            $rootScope.$broadcast("$stateChangeSuccess", toState, null, fromState, null);

            // assert
            expect(clearMessagesSpy).toHaveBeenCalled();
        });

        it("should clear normal and Deleted messages when main", () => {
            // arrange
            const clearMessagesSpy = spyOn(messageService, "clearMessages").and.callThrough();
            const fromState = {name: "main.item.details"};
            const toState = {name: "main"};

            // act
            $rootScope.$broadcast("$stateChangeSuccess", toState, null, fromState, null);

            // assert
            expect(clearMessagesSpy).toHaveBeenCalled();
            expect(clearMessagesSpy).toHaveBeenCalledWith(false, [MessageType.Deleted]);
        });

        it("should clear normal and persistent messages when logout", () => {
            // arrange
            const clearMessagesSpy = spyOn(messageService, "clearMessages").and.callThrough();
            const fromState = {name: "main.item.general"};
            const toState = {name: "logout"};

            // act
            $rootScope.$broadcast("$stateChangeSuccess", toState, null, fromState, null);

            // assert
            expect(clearMessagesSpy).toHaveBeenCalled();
            expect(clearMessagesSpy).toHaveBeenCalledWith(true);
        });

        it("should clear normal and persistent messages when error", () => {
            // arrange
            const clearMessagesSpy = spyOn(messageService, "clearMessages").and.callThrough();
            const fromState = {name: "main.item.general"};
            const toState = {name: "error"};

            // act
            $rootScope.$broadcast("$stateChangeSuccess", toState, null, fromState, null);

            // assert
            expect(clearMessagesSpy).toHaveBeenCalled();
            expect(clearMessagesSpy).toHaveBeenCalledWith(true);
        });
    });

    describe("Logout", () => {
        it("should call the correct methods", () => {
            // arrange
            const logoutSpy = spyOn(session, "logout").and.callFake(() => {
                return {
                    then: function(callback) { return callback(); }
                };
            });
            const navigateToMainSpy = spyOn(navigationService, "navigateToMain").and.callFake(() => {
                return {
                    finally: function(callback) { return callback(); }
                };
            });
            const removeAllSpy = spyOn(projectExplorerService, "removeAll");
            const clearDataSpy = spyOn(clipboardService, "clearData");
            ctrlLogout = new LogoutStateController($log, session, projectExplorerService, navigationService, clipboardService);

            // act

            // assert
            expect(logoutSpy).toHaveBeenCalled();
            expect(navigateToMainSpy).toHaveBeenCalled();
            expect(removeAllSpy).toHaveBeenCalled();
            expect(clearDataSpy).toHaveBeenCalled();
        });
    });
});
