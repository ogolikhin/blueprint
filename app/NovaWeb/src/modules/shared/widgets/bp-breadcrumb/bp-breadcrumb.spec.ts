import * as angular from "angular";
import "angular-mocks";
import "angular-ui-router";
import ".";
import {BPBreadcrumbController} from "./bp-breadcrumb";
import {NavigationServiceMock} from "../../../commonModule/navigation/navigation.service.mock";

describe("BPBreadcrumbComponent", () => {
    let $compile: ng.ICompileService;
    let $scope: ng.IScope;

    beforeEach(angular.mock.module("ui.router"));
    beforeEach(angular.mock.module("bp.widgets.breadcrumb"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("navigationService", NavigationServiceMock);
    }));

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
        expect(controller.links).toBeUndefined();
    });

    it("correctly binds properties and events", () => {
        // arrange
        const template = `<bp-breadcrumb links="links"></bp-breadcrumb>`;
        const links = [{id: 0, name: "test0", isEnabled: true}];
        $scope["links"] = links;

        // act
        const controller = <BPBreadcrumbController>$compile(template)($scope).controller("bpBreadcrumb");

        // assert
        expect(controller.links).toEqual(links);
    });

    it("correctly disposes the bound properties and events", () => {
        // arrange
        const template = `<bp-breadcrumb></bp-breadcrumb>`;
        const controller = <BPBreadcrumbController>$compile(template)($scope).controller("bpBreadcrumb");
        controller.links = [];

        // act
        controller.$onDestroy();

        // assert
        expect(controller.links).toBeUndefined();
    });

    it("navigates to enabled links", inject(($state: ng.ui.IStateService, $timeout: ng.ITimeoutService) => {
        // arrange
        const enabledLink = {id: 0, name: "enabled link", isEnabled: true};
        const template = `<bp-breadcrumb links="links"></bp-breadcrumb>`;
        const component = $compile(template)($scope);
        const controller = <BPBreadcrumbController>component.controller("bpBreadcrumb");
        const stateSpy = spyOn($state, "go");

        $scope["links"] = [enabledLink];
        $scope.$digest();

        $state.current.name = "main";

        // act
        component.find("a").click();
        $timeout.flush();

        // assert
        expect(stateSpy).toHaveBeenCalled();
        expect(stateSpy).toHaveBeenCalledWith("main.item", {id: 0, version: undefined, path: undefined}, jasmine.any(Object));
    }));
});
