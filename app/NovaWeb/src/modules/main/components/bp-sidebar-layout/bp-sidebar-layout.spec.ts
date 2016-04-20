import "../../main.module";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../../util/component.test";
import {BpSidebarLayoutCtrl} from "./bp-sidebar-layout";

describe("Component BpSidebarLayout", () => {

    beforeEach(angular.mock.module('app.main'));

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

    describe("the component is initialized", () => {
        it("should be visible by default", () => {

            //Arrange
            var vm: BpSidebarLayoutCtrl = directiveTest.createComponent({});
            directiveTest.scope.$digest();

            //Assert
            expect(vm.isLeftToggled).toBe(true);
            expect(vm.isRightToggled).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("left-panel-visible")).toBe(true);
            expect(directiveTest.element.find(".bp-sidebar-wrapper").hasClass("right-panel-visible")).toBe(true);
        });
    });
});