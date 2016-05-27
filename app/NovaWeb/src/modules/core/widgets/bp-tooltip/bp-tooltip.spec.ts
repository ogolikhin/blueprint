import "../..";
import "angular";
import "angular-mocks";

describe("Directive BP-Tooltip", () => {
    let tooltipTrigger = `<div><div bp-tooltip="Tooltip's content">Tooltip trigger</div></div>`;
    let zIndexedTooltipTrigger = `<div style="z-index: 10"><div bp-tooltip="Tooltip's content" sty>Tooltip trigger</div></div>`;

    beforeEach(angular.mock.module("app.core"));

    it("removes the directive attribute and creates the tooltip inside the trigger", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        let scope = $rootScope.$new();
        let element = $compile(tooltipTrigger)(scope);
        scope.$digest();

        // Act
        $rootScope.$apply();
        let trigger = <HTMLElement>element[0].firstChild;
        let tooltip = trigger.querySelector("div.bp-tooltip");
        // Assert
        expect(trigger.classList).toContain("bp-tooltip-trigger");
        expect(tooltip).toBeDefined();
    }));

    /*
    it("creates the tooltip structure as a child of the BODY", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        let scope = $rootScope.$new();
        let element = $compile(zIndexedTooltipTrigger)(scope);
        scope.$digest();

        // Act
        $rootScope.$apply();
        let trigger = <HTMLElement>element[0].querySelector("div.bp-tooltip-trigger");
        console.log(trigger);
        let tooltip = trigger.querySelector("div.bp-tooltip");
        // Assert
        expect(tooltip).toBeDefined();
    }));


*/
});