import * as angular from "angular";
import "angular-mocks";
import {BPAvatarController, BPAvatarComponent} from "./bp-avatar";

describe("BPAvatarComponent", () => {
    angular.module("bp.widgets.avatar", [])
        .component("bpAvatar", new BPAvatarComponent());

    let scope: ng.IScope;
    const template = `<bp-avatar 
            is-guest="isGuest" 
            user-id="userId" 
            user-name="{{userName}}" />`;

    beforeEach(angular.mock.module("bp.widgets.avatar"));

    it("should be visible by default", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        scope = $rootScope.$new();
        scope["userName"] = "admin";
        scope["userId"] = 1;

        // Act
        const element: ng.IAugmentedJQuery = $compile(template)(scope);
        const controller = element.controller("bpAvatar") as BPAvatarController;
        scope.$digest();

        //Assert
        expect(controller.initials).toBe("A");
        expect(controller.icon).toBe("/svc/adminstore/users/1/icon");
        expect(controller.guest).toBe(false);
        expect(element[0].querySelectorAll(".avatar__img").length).toBe(1);
        expect(element[0].querySelectorAll(".avatar__initials").length).toBe(1);
        expect(element[0].querySelectorAll(".avatar__initials--guest").length).toBe(0);
    }));

    it("should display 2 initials for two names", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        scope = $rootScope.$new();
        scope["userName"] = "Admin Person";
        scope["userId"] = 1;

        // Act
        const element: ng.IAugmentedJQuery = $compile(template)(scope);
        const controller = element.controller("bpAvatar") as BPAvatarController;

        //Assert
        expect(controller.initials).toBe("AP");
    }));

    it("should display 2 initials for more than 2 names", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        scope = $rootScope.$new();
        scope["userName"] = "Admin Middle Name Person";
        scope["userId"] = 1;

        // Act
        const element: ng.IAugmentedJQuery = $compile(template)(scope);
        const controller = element.controller("bpAvatar") as BPAvatarController;

        //Assert
        expect(controller.initials).toBe("AP");
    }));

    it("should not have image for guest user", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        scope = $rootScope.$new();
        scope["isGuest"] = true;

        // Act
        const element: ng.IAugmentedJQuery = $compile(template)(scope);
        const controller = element.controller("bpAvatar") as BPAvatarController;
        scope.$digest();

        //Assert
        expect(controller.guest).toBe(true);
        expect(controller.initials).toBe("");
        expect(element[0].querySelectorAll(".avatar__img").length).toBe(0);
        expect(element[0].querySelectorAll(".avatar__initials").length).toBe(1);
        expect(element[0].querySelectorAll(".avatar__initials--guest").length).toBe(1);
    }));
});
