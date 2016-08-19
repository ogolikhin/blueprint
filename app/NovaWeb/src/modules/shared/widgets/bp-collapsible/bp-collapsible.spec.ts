import { BPCollapsible } from "./bp-collapsible";
import "angular";
import "angular-mocks";

describe("BPCollapsible Directive", () => {
    var longElement: JQuery;

    beforeEach(angular.mock.module(($compileProvider: ng.ICompileProvider) => {
        $compileProvider.directive("bpCollapsible", BPCollapsible.factory());
    }));

    beforeEach(inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        longElement = angular.element("<div class='collapsible' bp-collapsible='80' style='height:250px;'></div>");
        var scope = $rootScope.$new();
        let element = $compile(longElement)(scope);
        angular.element("body").append(element);
        scope.$digest();
    }));

    afterEach(function () {
        angular.element("body").empty();
    });

    it("can create the attribute",
        inject(($timeout: ng.ITimeoutService) => {
            // Act, Arrange
            $timeout.flush();

            // Assert
            let collapsible = document.body.getElementsByClassName("collapsible")[0].hasAttribute("bp-collapsible");
            expect(collapsible).toBe(true);
        }));

    it("collapsed class should be added to element",
        inject(($timeout: ng.ITimeoutService) => {
            // Act, Arrange
            $timeout.flush();

            // Assert
            expect(longElement[0].classList.contains("collapsed")).toBe(true);
        }));

    it("clicking showmore and showless should remove and add collapsed class",
        inject(($timeout: ng.ITimeoutService) => {
            // Act, Arrange
            $timeout.flush();
            longElement[0].children[0].dispatchEvent(new Event("click", { "bubbles": true }));

            // Assert
            expect(longElement[0].classList.contains("collapsed")).toBe(false);
            longElement[0].children[1].dispatchEvent(new Event("click", { "bubbles": true }));
            expect(longElement[0].classList.contains("collapsed")).toBe(true);
        }));

    it("collapsed class should not be added to element",
        inject(($timeout: ng.ITimeoutService) => {
            // Act, Arrange
            longElement[0].style.height = `${50}px`;
            $timeout.flush();

            // Assert
            expect(longElement[0].classList.contains("collapsed")).toBe(false);
        }));
});
