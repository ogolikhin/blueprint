import * as angular from "angular";
import "angular-mocks";
import {BPCollapsible} from "./bp-collapsible";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";

describe("BPCollapsible Directive", () => {
    var longElement: JQuery;

    beforeEach(angular.mock.module(($compileProvider: ng.ICompileProvider, $provide: ng.auto.IProvideService) => {
        $compileProvider.directive("bpCollapsible", BPCollapsible.factory());
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        let ContainterId = "discussion-scrollable-content";
        let collapsibleElementHtml = `<div class='collapsible' bp-collapsible='80' scrollable-container-id=${ContainterId} style='height:250px;'></div>`;
        longElement = angular.element(`<div class='scrollable-content'>${collapsibleElementHtml}</div>`);
        var scope = $rootScope.$new();
        let element = $compile(longElement)(scope);
        angular.element("body").append(element);
        scope.$digest();
        let perfectScrollbar = {};
        perfectScrollbar["update"] = () => {
        };
        (<any>window).PerfectScrollbar = perfectScrollbar;
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
            expect(longElement[0].children[0].classList.contains("collapsed")).toBe(true);
        }));

    it("clicking showmore and showless should remove and add collapsed class",
        inject(($timeout: ng.ITimeoutService) => {
            // Act, Arrange
            $timeout.flush();
            longElement[0].getElementsByClassName("show-more")[0].dispatchEvent(new Event("click", {"bubbles": true}));

            // Assert
            expect(longElement[0].classList.contains("collapsed")).toBe(false);
            longElement[0].getElementsByClassName("show-less")[0].dispatchEvent(new Event("click", {"bubbles": true}));
            expect(longElement[0].children[0].classList.contains("collapsed")).toBe(true);
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
