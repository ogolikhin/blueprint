import "../../main.module";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../../util/component.test";
import {BpSidebarLayoutCtrl} from "./bp-sidebar-layout";

describe("Component BpSidebarLayout", () => {

    beforeEach(angular.mock.module("app.main"));

    var directiveTest: ComponentTest<BpSidebarLayoutCtrl>;
    var layout = `
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
        it("should be visible by default", () => {

            //Arrange
            var vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});

            //Assert
            expect(vm.isLeftToggled).toBe(true);
            expect(vm.isRightToggled).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(true);
        });
        it("should toggle the left side correctly", () => {

            //Arrange
            var vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});

            //Act
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(false);
            expect(vm.isRightToggled).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(true);
        });
        it("should toggle the right side correctly", () => {

            //Arrange
            var vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});

            //Act
            vm.toggleRight(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(true);
            expect(vm.isRightToggled).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(false);
        });
        it("should toggle the both sides correctly", () => {

            //Arrange
            var vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});

            //Act
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();
            vm.toggleRight(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(false);
            expect(vm.isRightToggled).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(false);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(false);
        });
        it("should double toggle the left side correctly", () => {

            //Arrange
            var vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});

            //Act
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();
            vm.toggleLeft(directiveTest.scope.$broadcast("dummyEvent"));
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(true);
            expect(vm.isRightToggled).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(true);
        });
        it("should double-toggle the both sides correctly", () => {

            //Arrange
            var vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});

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
            expect(vm.isLeftToggled).toBe(true);
            expect(vm.isRightToggled).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(true);
        });
    });
});