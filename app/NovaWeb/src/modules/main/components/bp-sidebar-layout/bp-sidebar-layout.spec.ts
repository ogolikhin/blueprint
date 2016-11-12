﻿import "../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import {Enums} from "../../models";
import {ComponentTest} from "../../../util/component.test";
import {BpSidebarLayoutCtrl} from "./bp-sidebar-layout";

describe("Component BpSidebarLayout", () => {

    beforeEach(angular.mock.module("app.main"));

    let directiveTest: ComponentTest<BpSidebarLayoutCtrl>;
    const layout = `
        <bp-sidebar-layout left-panel-title="My Left Panel Title" right-panel-title="My Right Panel Title">
            <bp-sidebar-layout-content-left>My Left Panel Content</bp-sidebar-layout-content-left>
            <bp-sidebar-layout-content-center>My Center Content</bp-sidebar-layout-content-center>
            <bp-sidebar-layout-content-right>My Right Panel Content</bp-sidebar-layout-content-right>
        </bp-sidebar-layout>
    `;

    beforeEach(() => {
        directiveTest = new ComponentTest<BpSidebarLayoutCtrl>(layout, "bp-sidebar-layout");
    });

    describe("the component is created", () => {
        it("should be hidden by default", () => {

            //Arrange
            const vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});

            //Assert
            expect(vm.isLeftToggled).toBe(false);
            expect(vm.isRightToggled).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(false);
        });
        it("should toggle the left side correctly", () => {

            //Arrange
            const vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});
            vm.togglePanel = () => {
                vm.isLeftToggled = !vm.isLeftToggled;
            };
            const event = directiveTest.scope.$broadcast("dummyEvent");

            //Act
            vm.toggleLeft(event);
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(true);
            expect(vm.isRightToggled).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(false);
            expect(event.defaultPrevented).toBeTruthy();
        });
        it("should toggle the right side correctly", () => {

            //Arrange
            const vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});
            vm.togglePanel = () => {
                vm.isRightToggled = !vm.isRightToggled;
            };

            const event = directiveTest.scope.$broadcast("dummyEvent");

            //Act
            vm.toggleRight(event);
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(false);
            expect(vm.isRightToggled).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(true);
            expect(event.defaultPrevented).toBeTruthy();
        });
        it("should toggle the both sides correctly", () => {

            //Arrange
            const vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});
            vm.togglePanel = (arg) => {
                if (arg.id === Enums.ILayoutPanel.Left) {
                    vm.isLeftToggled = !vm.isLeftToggled;
                }
                if (arg.id === Enums.ILayoutPanel.Right) {
                    vm.isRightToggled = !vm.isRightToggled;
                }
            };

            //Act
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();
            vm.toggleRight(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(true);
            expect(vm.isRightToggled).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(true);
        });
        it("should double toggle the left side correctly", () => {

            //Arrange
            const vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});
            vm.togglePanel = () => {
                vm.isLeftToggled = !vm.isLeftToggled;
            };

            //Act
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(false);
            expect(vm.isRightToggled).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(false);
        });
        it("should double-toggle the both sides correctly", () => {

            //Arrange
            const vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});
            vm.togglePanel = (arg) => {
                if (arg.id === Enums.ILayoutPanel.Left) {
                    vm.isLeftToggled = !vm.isLeftToggled;
                }
                if (arg.id === Enums.ILayoutPanel.Right) {
                    vm.isRightToggled = !vm.isRightToggled;
                }
            };

            //Act
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(false);
            expect(vm.isRightToggled).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(false);
        });
    });
});
