import "../..";
import "angular";
import "angular-mocks";

describe("Directive BP-Tooltip", () => {
    let tooltipTrigger = `<div><div bp-tooltip="Tooltip's content">Tooltip trigger</div></div>`;

    beforeEach(angular.mock.module("app.core"));

    afterEach(function(){
        angular.element("body").empty();
    });

    it("removes the directive attribute and creates the tooltip inside the trigger",
        inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        let scope = $rootScope.$new();
        let element = $compile(tooltipTrigger)(scope);
        scope.$digest();

        // Act
        $rootScope.$apply();
        let trigger = <HTMLElement>element[0].firstChild;
        let tooltip = <HTMLElement>trigger.querySelector("div.bp-tooltip");

        // Assert
        expect(trigger.classList).toContain("bp-tooltip-trigger");
        expect(tooltip).toBeDefined();
    }));

    it("shows the tooltip on mouseover on the trigger",
        inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        let scope = $rootScope.$new();
        let element = $compile(tooltipTrigger)(scope);
        angular.element("body").append(element);
        scope.$digest();

        // Act
        $rootScope.$apply();
        let trigger = <HTMLElement>element[0].firstChild;
        let tooltip = <HTMLElement>trigger.querySelector("div.bp-tooltip");

        tooltip.dispatchEvent(new Event("mouseover", { "bubbles": true }));

        // Assert
        expect(tooltip.classList).toContain("show");
    }));

    it("hides the tooltip on mouseout from the trigger",
        inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        let scope = $rootScope.$new();
        let element = $compile(tooltipTrigger)(scope);
        angular.element("body").append(element);
        scope.$digest();

        // Act
        $rootScope.$apply();
        let trigger = <HTMLElement>element[0].firstChild;
        let tooltip = <HTMLElement>trigger.querySelector("div.bp-tooltip");

        tooltip.dispatchEvent(new Event("mouseover", { "bubbles": true }));
        tooltip.dispatchEvent(new Event("mouseout", { "bubbles": true }));

        // Assert
        expect(tooltip.classList).not.toContain("show");
    }));

    it("moves the tooltip according to mouse position (top left)",
        inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        let scope = $rootScope.$new();
        let element = $compile(tooltipTrigger)(scope);
        angular.element("body").append(element);
        document.body.style.width = "400px";
        document.body.style.height = "400px";
        scope.$digest();

        // Act
        $rootScope.$apply();
        let trigger = <HTMLElement>element[0].firstChild;
        let tooltip = <HTMLElement>trigger.querySelector("div.bp-tooltip");

        tooltip.dispatchEvent(new Event("mouseover", { "bubbles": true }));

        let ev = document.createEvent("MouseEvent");
        ev.initMouseEvent(
            "mousemove",
            true /* bubble */, true /* cancelable */,
            window, null,
            0, 0, /* screen coordinates */
            document.body.clientWidth * 0.25, 10, /* client coordinates */
            false, false, false, false, /* modifier keys */
            null, null
        );
        tooltip.dispatchEvent(ev);

        // Assert
        expect(tooltip.classList).toContain("bp-tooltip-left-tip");
        expect(tooltip.classList).not.toContain("bp-tooltip-right-tip");
        expect(tooltip.classList).toContain("bp-tooltip-top-tip");
        expect(tooltip.classList).not.toContain("bp-tooltip-bottom-tip");
    }));

    it("moves the tooltip according to mouse position (bottom right)",
        inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        let scope = $rootScope.$new();
        let element = $compile(tooltipTrigger)(scope);
        angular.element("body").append(element);
        document.body.style.width = "400px";
        document.body.style.height = "400px";
        scope.$digest();

        // Act
        $rootScope.$apply();
        let trigger = <HTMLElement>element[0].firstChild;
        let tooltip = <HTMLElement>trigger.querySelector("div.bp-tooltip");

        tooltip.dispatchEvent(new Event("mouseover", { "bubbles": true }));

        let ev = document.createEvent("MouseEvent");
        ev.initMouseEvent(
            "mousemove",
            true /* bubble */, true /* cancelable */,
            window, null,
            0, 0, /* screen coordinates */
            document.body.clientWidth * 0.75, document.body.clientHeight * 0.75, /* client coordinates */
            false, false, false, false, /* modifier keys */
            null, null
        );
        tooltip.dispatchEvent(ev);

        // Assert
        expect(tooltip.classList).not.toContain("bp-tooltip-left-tip");
        expect(tooltip.classList).toContain("bp-tooltip-right-tip");
        expect(tooltip.classList).not.toContain("bp-tooltip-top-tip");
        expect(tooltip.classList).toContain("bp-tooltip-bottom-tip");
    }));
/*
    it("creates the tooltip structure as a child of the BODY",
        inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        let scope = $rootScope.$new();
        let element = $compile(tooltipTrigger)(scope);
        angular.element("body").append(element);
        angular.element("body").append(element);
        scope.$digest();

        // Act
        $rootScope.$apply();
        let trigger = <HTMLElement>element[0].firstChild;
        let tooltip = <HTMLElement>trigger.querySelector("div.bp-tooltip");

        // Assert
        expect(tooltip).toBeDefined();
    }));
*/
});