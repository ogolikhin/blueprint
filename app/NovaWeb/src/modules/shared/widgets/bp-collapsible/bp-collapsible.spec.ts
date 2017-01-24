import * as angular from "angular";
import "angular-mocks";
import {BPCollapsible} from "./bp-collapsible";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";

describe("BPCollapsible Directive", () => {
    beforeEach(angular.mock.module(($compileProvider: ng.ICompileProvider, $provide: ng.auto.IProvideService) => {
        $compileProvider.directive("bpCollapsible", BPCollapsible.instance());
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        const collapsibleElementHtml = `<div class='collapsible' bp-collapsible='80' style='height:250px;'></div>`;
        const longElement = angular.element(`<div class='scrollable-content'>${collapsibleElementHtml}</div>`);
        const scope = $rootScope.$new();
        const element = $compile(longElement)(scope);
        angular.element("body").append(element);
        scope.$digest();
    }));

    afterEach(function () {
        angular.element("body").empty();
    });

    it("can create the attribute",
        inject(($timeout: ng.ITimeoutService) => {
            // Act, Arrange
            const collapsible = document.body.querySelector(".collapsible");
            $timeout.flush();

            // Assert
            expect(collapsible.hasAttribute("bp-collapsible")).toBe(true);
        }));

    it("collapsed class should be added to element",
        inject(($timeout: ng.ITimeoutService) => {
            // Act, Arrange
            const collapsible = document.body.querySelector(".collapsible");
            $timeout.flush();

            // Assert
            expect(collapsible.classList.contains("collapsible__collapsed")).toBe(true);
            expect(collapsible.classList.contains("collapsible__expanded")).toBe(false);
        }));

    it("clicking showmore and showless should remove and add collapsed class",
        inject(($timeout: ng.ITimeoutService) => {
            // Act, Arrange
            const collapsible = document.body.querySelector(".collapsible");
            $timeout.flush();

            // Assert
            document.body.querySelector(".collapsible__show-more .collapsible__button").dispatchEvent(new Event("click", {"bubbles": true}));
            expect(collapsible.classList.contains("collapsible__collapsed")).toBe(false);
            expect(collapsible.classList.contains("collapsible__expanded")).toBe(true);

            document.body.querySelector(".collapsible__show-less .collapsible__button").dispatchEvent(new Event("click", {"bubbles": true}));
            expect(collapsible.classList.contains("collapsible__collapsed")).toBe(true);
            expect(collapsible.classList.contains("collapsible__expanded")).toBe(false);
        }));

    it("collapsed class should not be added to element",
        inject(($timeout: ng.ITimeoutService) => {
            // Act, Arrange
            const collapsible = document.body.querySelector(".collapsible") as HTMLElement;
            collapsible.style.height = `${50}px`;
            $timeout.flush();

            // Assert
            expect(collapsible.classList.contains("collapsible__collapsed")).toBe(false);
            expect(collapsible.classList.contains("collapsible__expanded")).toBe(true);
        }));
});
