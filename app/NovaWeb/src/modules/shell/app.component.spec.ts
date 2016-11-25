﻿import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "./index";
import {ComponentTest} from "../util/component.test";
import {AppController} from "./app.component";
import {INavigationService} from "./../core/navigation/navigation.svc";
import {NavigationServiceMock} from "./../core/navigation/navigation.svc.mock";

describe("Component AppComponent", () => {
    beforeEach(angular.mock.module("app.shell"));


    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("session", SessionSvcMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("projectManager", () => ({
            removeAll: () => undefined
        }));
        $provide.service("selectionManager", () => ({
            clearAll: () => undefined
        }));
        $provide.service("settings", SettingsMock);
        $provide.service("$window", WindowMock);
        $provide.service("publishService", () => ({
            getUnpublishedArtifacts: () => { return {artifacts: []}; }
        }));
    }));

    let componentTest: ComponentTest<AppController>;

    beforeEach(() => {
        componentTest = new ComponentTest<AppController>("<app></app>", "app");
    });

    describe("get current user", () => {
        it("should return the session user", inject((session: SessionSvcMock) => {

            //Arrange
            const vm: AppController = componentTest.createComponent({});

            //Act
            const user = vm.currentUser;

            //Assert
            expect(user).toEqual("MyCurrentUser");

        }));
    });

    describe("logout", () => {
        it("should call session logout, navigate to main state and pop up login window",
            inject((session: SessionSvcMock, navigationService: INavigationService, $q: ng.IQService, $window: WindowMock) => {

            //Arrange
            const vm: AppController = componentTest.createComponent({});
            spyOn(navigationService, "navigateToLogout").and.callThrough();
            const event = componentTest.scope.$broadcast("dummyEvent");

            //Act
            vm.logout(event);
            componentTest.scope.$digest();

            //Assert
            expect(navigationService.navigateToLogout).toHaveBeenCalled();
            expect(event.defaultPrevented).toBeTruthy();


        }));
    });

    describe("navigate to help url", () => {
        it("should open a window with correct params", inject(($window: WindowMock) => {

            //Arrange
            const vm: AppController = componentTest.createComponent({});
            spyOn($window, "open").and.callThrough();
            const event = componentTest.scope.$broadcast("dummyEvent");

            //Act
            vm.navigateToHelpUrl(event);

            //Assert
            // tslint:disable-next-line: max-line-length
            const expectedOptions: string = "toolbar = no, location = no, directories = no, status = no, menubar = no, titlebar = no, scrollbars = no, resizable = yes, copyhistory = no, width = 1300, height = 800, top = 160, left = 150";
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

    public location: LocationMock = new LocationMock();

    public open(url: string, title: string, windowFeatures: string) {
        return undefined;
    }
}

class LocationMock {
    public reload() {
        return undefined;
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

    public ensureAuthenticated() {
        return this.$q.when([]);
    }

}
