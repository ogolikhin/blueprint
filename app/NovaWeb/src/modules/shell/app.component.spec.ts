import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "./index";
import {ComponentTest} from "../util/component.test";
import {AppController} from "./app.component";

describe("Component AppComponent", () => {
    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("session", SessionSvcMock);
        $provide.service("settings", SettingsMock);
        $provide.service("$window", WindowMock);
    }));

    var componentTest: ComponentTest<AppController>;

    beforeEach(() => {
        componentTest = new ComponentTest<AppController>("<app></app>", "app");
    });

    describe("get current user", () => {
        it("should return the session user", inject((session: SessionSvcMock) => {

            //Arrange
            var vm: AppController = componentTest.createComponent({});

            //Act
            var user = vm.currentUser;

            //Assert
            expect(user).toEqual("MyCurrentUser");

        }));
    });

    describe("logout", () => {
        it("should call session logout and refresh state", inject((session: SessionSvcMock, $state: ng.ui.IStateService, $q: ng.IQService) => {

            //Arrange
            var vm: AppController = componentTest.createComponent({});
            spyOn(session, "logout").and.callThrough();
            spyOn($state, "reload").and.callThrough();
            var event = componentTest.scope.$broadcast("dummyEvent");

            //Act
            vm.logout(event);
            componentTest.scope.$digest();

            //Assert
            expect(session.logout).toHaveBeenCalled();
            expect($state.reload).toHaveBeenCalled();
            expect(event.defaultPrevented).toBeTruthy();


        }));
    });

    describe("navigate to help url", () => {
        it("should open a window with correct params", inject(($window: WindowMock) => {

            //Arrange
            var vm: AppController = componentTest.createComponent({});
            spyOn($window, "open").and.callThrough();
            var event = componentTest.scope.$broadcast("dummyEvent");

            //Act
            vm.navigateToHelpUrl(event);

            //Assert
            /* tslint:disable */
            var expectedOptions: string = "toolbar = no, location = no, directories = no, status = no, menubar = no, titlebar = no, scrollbars = no, resizable = yes, copyhistory = no, width = 1300, height = 800, top = 160, left = 150";
            /* tslint:enable */
            expect($window.open).toHaveBeenCalledWith("http://HelpURL", "_blank", expectedOptions);
            expect(event.defaultPrevented).toBeTruthy();

        }));
    });
});

class WindowMock {
    public screenX = 50;
    public screenY = 60;
    public screenLeft = 50;
    public screenTop = 60;
    public outerWidth = 1500;
    public outerHeight = 1000;

    public open(url: string, title: string, windowFeatures: string) {
    }
}

class SettingsMock {
    get(key: string, defaultValue?: string): string {
        return "http://" + key;
    }
}

class SessionSvcMock {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public currentUser = "MyCurrentUser";

    public logout() {
        return this.$q.when([]);
    }
}
