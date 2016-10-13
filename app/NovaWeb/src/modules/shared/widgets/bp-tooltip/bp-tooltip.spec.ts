import "../..";
import * as angular from "angular";
import "angular-mocks";

describe("Directive BP-Tooltip", () => {
    let tooltipTrigger = `<div><div bp-tooltip="Tooltip's content">Tooltip trigger</div></div>`;
    let tooltipTriggerNotTruncated = `<div><div bp-tooltip="Tooltip's content" bp-tooltip-truncated="true">Tooltip trigger</div></div>`;
    let tooltipTriggerTruncated = `<div><div bp-tooltip="Tooltip's content" bp-tooltip-truncated="true"
                                        style="text-overflow: ellipsis; width: 5px;">Tooltip trigger</div></div>`;

    beforeEach(angular.mock.module("app.shared"));

    afterEach(function () {
        angular.element("body").empty();
    });

    it("shows the tooltip on mouseover on the trigger",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                let scope = $rootScope.$new();
                let element = $compile(tooltipTrigger)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                let trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                let tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toContain("Tooltip's content");
            }
        )
    );

    it("does not show the tooltip on mouseover on the trigger if text is not truncated",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                let scope = $rootScope.$new();
                let element = $compile(tooltipTriggerNotTruncated)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                let trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                let tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeNull();
            }
        )
    );

    it("shows the tooltip on mouseover on the trigger if text is truncated",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                let scope = $rootScope.$new();
                let element = $compile(tooltipTriggerTruncated)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                let trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                let tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toContain("Tooltip's content");
            }
        )
    );

    it("removes the tooltip on mouseout from the trigger",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                let scope = $rootScope.$new();
                let element = $compile(tooltipTrigger)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                let trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                let tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                trigger.dispatchEvent(new Event("mouseout", {"bubbles": true}));
                tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeNull();
            }
        )
    );

    it("hides the tooltip on mousedown on the trigger",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                let scope = $rootScope.$new();
                let element = $compile(tooltipTrigger)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                let trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                let tooltip = <HTMLElement>trigger.querySelector("div.bp-tooltip");

                trigger.dispatchEvent(new Event("mousedown", {"bubbles": true}));
                tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).not.toContain("show");
                expect(tooltip.textContent).toContain("Tooltip's content");
            }
        )
    );

    it("changes the tooltip text dynamically", function (done) {
            inject(
                ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                    // Arrange
                    let scope = $rootScope.$new();
                    let element = $compile(tooltipTrigger)(scope);
                    angular.element("body").append(element);
                    scope.$digest();

                    // Act
                    $rootScope.$apply();
                    let trigger = <HTMLElement>element[0].firstChild;
                    trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                    let tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                    trigger.setAttribute("bp-tooltip", "Updated tooltip's content");

                    // Assert
                    expect(tooltip).toBeDefined();
                    expect(tooltip.classList).toContain("show");

                    setTimeout(function () {
                        expect(tooltip.textContent).toContain("Updated tooltip's content");
                        done();
                    }, 100);
                }
            );
        }
    );

    it("moves the tooltip according to mouse position (top left)",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
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
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                let tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

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
                trigger.dispatchEvent(ev);

                // Assert
                expect(tooltip.classList).toContain("bp-tooltip-left-tip");
                expect(tooltip.classList).not.toContain("bp-tooltip-right-tip");
                expect(tooltip.classList).toContain("bp-tooltip-top-tip");
                expect(tooltip.classList).not.toContain("bp-tooltip-bottom-tip");
            }
        )
    );

    it("moves the tooltip according to mouse position (bottom right)",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
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
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                let tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

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
                trigger.dispatchEvent(ev);

                // Assert
                expect(tooltip.classList).not.toContain("bp-tooltip-left-tip");
                expect(tooltip.classList).toContain("bp-tooltip-right-tip");
                expect(tooltip.classList).not.toContain("bp-tooltip-top-tip");
                expect(tooltip.classList).toContain("bp-tooltip-bottom-tip");
            }
        )
    );
});
