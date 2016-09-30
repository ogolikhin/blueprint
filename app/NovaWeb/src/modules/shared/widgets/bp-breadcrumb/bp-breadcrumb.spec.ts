import * as angular from "angular";
import {BPBreadcrumbController} from "./bp-breadcrumb";
import {IBreadcrumbLink} from "./breadcrumb-link";

describe("BPBreadcrumbComponent", () => {
    let $compile: ng.ICompileService;
    let $scope: ng.IScope;

    beforeEach(angular.mock.module("bp.widgets.breadcrumb"));

    beforeEach(inject((_$compile_: ng.ICompileService, _$rootScope_: ng.IRootScopeService) => {
        $compile = _$compile_;
        $scope = _$rootScope_.$new();
    }));

    afterEach(() => {
        $compile = null;
        $scope = null;
    });

    it("correctly initializes the bound properties and events", () => {
        // arrange
        const template = `<bp-breadcrumb></bp-breadcrumb>`;

        // act
        const controller = <BPBreadcrumbController>$compile(template)($scope).controller("bpBreadcrumb");

        // assert
        expect(controller.links).toEqual([]);
        expect(controller.onNavigate).toBeUndefined();
    });

    it("correctly binds properties and events", () => {
        // arrange
        const template = `<bp-breadcrumb links="[{id: 0, name: 'test0', isEnabled: true}]" on-navigate="navigateTo(link)"></bp-breadcrumb>`;

        // act
        const controller = <BPBreadcrumbController>$compile(template)($scope).controller("bpBreadcrumb");

        // assert
        expect(controller.links).toEqual([{id: 0, name: "test0", isEnabled: true}]);
        expect(angular.isFunction(controller.onNavigate)).toEqual(true);
    });

    it("correctly disposes the bound properties and events", () => {
        // arrange
        const template = `<bp-breadcrumb></bp-breadcrumb>`;
        const controller = <BPBreadcrumbController>$compile(template)($scope).controller("bpBreadcrumb");
        controller.links = [];
        controller.onNavigate = (parameter) => {};

        // act
        controller.dispose();

        // assert
        expect(controller.links).toBeUndefined();
        expect(controller.onNavigate).toBeUndefined();
    });

    it("ignores navigation to disabled links", () => {
        // arrange
        const disabledLink = { id: 0, name: "disabled link", isEnabled: false };
        const template = `<bp-breadcrumb links="links" on-navigate="navigateTo(link)"></bp-breadcrumb>`;
        const component = $compile(template)($scope);
        const controller = <BPBreadcrumbController>component.controller("bpBreadcrumb");

        $scope["links"] = [disabledLink];
        $scope["navigateTo"] = (link: IBreadcrumbLink) => {};
        $scope.$digest();

        const onNavigateSpy = spyOn(controller, "onNavigate").and.callThrough();
        const navigateToSpy = spyOn($scope, "navigateTo");

        // act
        component.find("a").click();
        
        // assert
        expect(onNavigateSpy).not.toHaveBeenCalled();
        expect(navigateToSpy).not.toHaveBeenCalled();
    });

    it("navigates to enabled links", () => {
        // arrange
        const enabledLink = { id: 0, name: "enabled link", isEnabled: true };
        const template = `<bp-breadcrumb links="links" on-navigate="navigateTo(link)"></bp-breadcrumb>`;
        const component = $compile(template)($scope);
        const controller = <BPBreadcrumbController>component.controller("bpBreadcrumb");

        $scope["links"] = [enabledLink];
        $scope["navigateTo"] = (link: IBreadcrumbLink) => {};
        $scope.$digest();

        const onNavigateSpy = spyOn(controller, "onNavigate").and.callThrough();
        const navigateToSpy = spyOn($scope, "navigateTo");

        // act
        component.find("a").click();
        
        // assert
        expect(onNavigateSpy).toHaveBeenCalledWith({ link: enabledLink });
        expect(navigateToSpy).toHaveBeenCalledWith(enabledLink);
    });
});