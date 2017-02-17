import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "./index";
import {ComponentTest} from "../util/component.test";
import {AppController} from "./app.component";
import {INavigationService} from "../commonModule/navigation/navigation.service";
import {NavigationServiceMock} from "../commonModule/navigation/navigation.service.mock";
import {UnpublishedArtifactsServiceMock} from "../editorsModule/unpublished/unpublished.service.mock";
import {IUser} from "./login/auth.svc";
import {SelectionManagerMock} from "../managers/selection-manager/selection-manager.mock";

describe("Component AppComponent", () => {
    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("session", SessionSvcMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("settings", SettingsMock);
        $provide.service("$window", WindowMock);
        $provide.service("dialogService", () => ({}));
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
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
            const user: IUser = vm.currentUser;

            //Assert
            expect(user).toBeDefined();
            expect(user.id).toEqual(1);
            expect(user.displayName).toEqual("MyCurrentUser");

        }));
    });

    describe("isDatabaseUser", () => {
        it("should return if the user is database or Windows/SAML", inject((session: SessionSvcMock) => {

            //Arrange
            const vm: AppController = componentTest.createComponent({});

            //Act
            const isDB: boolean = vm.isDatabaseUser();

            //Assert
            expect(isDB).toEqual(true);

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
            const expectedOptions: string = "toolbar = no, location = no, directories = no, status = no, menubar = no, titlebar = no, scrollbars = yes, resizable = yes, copyhistory = no, width = 1300, height = 800, top = 160, left = 150";
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

    public currentUser: IUser = {
        id: 1,
        displayName: "MyCurrentUser",
        login: "user",
        isFallbackAllowed: false,
        isSso: false,
        source: 0,
        licenseType: 3
    };

    public logout() {
        return this.$q.when([]);
    }

    public ensureAuthenticated() {
        return this.$q.when([]);
    }
}
