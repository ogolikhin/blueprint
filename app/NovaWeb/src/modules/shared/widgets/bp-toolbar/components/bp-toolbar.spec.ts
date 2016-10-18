import * as angular from "angular";
import "angular-mocks";
import "../";
import {BPToolbarController} from "./bp-toolbar";
import {BPButtonAction} from "../actions";

describe("BPToolbar", () => {
    let $compile: ng.ICompileService;
    let $scope: ng.IScope;

    beforeEach(angular.mock.module("bp.widgets.toolbar"));

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
        const template = `<bp-toolbar-2></bp-toolbar-2>`;

        // act
        const controller = <BPToolbarController>$compile(template)($scope).controller("bpToolbar2");

        // assert
        expect(controller.actions).toEqual([]);
    });

    it("correctly binds properties and events", () => {
        // arrange
        const template = `<bp-toolbar-2 actions="actions"></bp-toolbar-2>`;
        const actions = [
            new BPButtonAction(() => {
            }, () => true, "test1", "test1", "test1"),
            new BPButtonAction(() => {
            }, () => true, "test2", "test2", "test2")
        ];
        $scope["actions"] = actions;
        // act
        const controller = <BPToolbarController>$compile(template)($scope).controller("bpToolbar2");

        // assert
        expect(controller.actions).toEqual(actions);
    });
});
