import * as angular from "angular";
import "angular-mocks";
import {BPTooltip} from "./bp-tooltip";
import "lodash";

describe("Directive BP-Tooltip", () => {
    angular.module("bp.widgets.tooltip", [])
        .directive("bpTooltip", BPTooltip.factory());

    const template = `<div><div bp-tooltip="{{tooltipContent}}"
                                bp-tooltip-truncated="{{isTruncated}}"
                                bp-tooltip-limit="{{limit}}"
                                style="{{tooltipStyle}}">{{tooltipTrigger}}</div></div>`;
    const templateNested = `<div><div bp-tooltip="{{tooltipContent}}"
                                      bp-tooltip-truncated="{{isTruncated}}"
                                      style="{{tooltipStyle}}"><span>{{tooltipTrigger}}</span></div></div>`;
    const template2xNested = `<div><div bp-tooltip="{{tooltipContent}}"
                                        bp-tooltip-truncated="{{isTruncated}}"><div>
                                        <div style="{{tooltipStyle}}">{{tooltipTrigger}}</div>
                                        </div></div></div>`;

    const tooltipTrigger = `<div><div bp-tooltip="Tooltip's content">Tooltip trigger</div></div>`;

    beforeEach(angular.mock.module("bp.widgets.tooltip"));

    afterEach(function () {
        angular.element("body").empty();
    });

    it("shows the tooltip on mouseover on the trigger",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const tooltipContent = "Tooltip's content";
                const scope = $rootScope.$new();
                scope["tooltipContent"] = tooltipContent;
                scope["tooltipTrigger"] = "Tooltip trigger";
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent);
            }
        )
    );

    it("shows a truncated tooltip (set limit)",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const tooltipContent = "Tooltip's content";
                const limit = 4;
                const scope = $rootScope.$new();
                scope["tooltipContent"] = tooltipContent;
                scope["tooltipTrigger"] = "Tooltip trigger";
                scope["limit"] = limit;
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent.slice(0, limit) + "…");
            }
        )
    );

    it("shows a truncated tooltip (default limit)",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const defaultLimit = 250;
                const randomString = [];
                for (let i = 0; i < defaultLimit + 50; i++) {
                    randomString[i] = _.random(48, 122);
                }
                const tooltipContent = String.fromCharCode(...randomString);
                const scope = $rootScope.$new();
                scope["tooltipContent"] = tooltipContent;
                scope["tooltipTrigger"] = "Tooltip trigger";
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent.slice(0, defaultLimit) + "…");
            }
        )
    );

    it("shows the whole tooltip if the limit is bigger then the tooltip's length",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const tooltipContent = "Tooltip's content";
                const limit = 40;
                const scope = $rootScope.$new();
                scope["tooltipContent"] = tooltipContent;
                scope["tooltipTrigger"] = "Tooltip trigger";
                scope["limit"] = limit;
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent);
            }
        )
    );

    it("does not show the tooltip on mouseover on the trigger if text is not truncated",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const scope = $rootScope.$new();
                scope["tooltipContent"] = "Tooltip's content";
                scope["tooltipTrigger"] = "Tooltip trigger";
                scope["isTruncated"] = true;
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeNull();
            }
        )
    );

    it("shows the tooltip on mouseover on the trigger if text is truncated",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const tooltipContent = "Tooltip's content";
                const scope = $rootScope.$new();
                scope["tooltipContent"] = tooltipContent;
                scope["tooltipTrigger"] = "Tooltip trigger with looooong text";
                scope["isTruncated"] = true;
                scope["tooltipStyle"] = "text-overflow: ellipsis; width: 50px; overflow: hidden; white-space: nowrap;";
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent);
            }
        )
    );

    it("shows the tooltip on mouseover on the trigger if text is truncated in first (and only) child",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const tooltipContent = "Tooltip's content";
                const scope = $rootScope.$new();
                scope["tooltipContent"] = tooltipContent;
                scope["tooltipTrigger"] = "Tooltip trigger with looooong text";
                scope["isTruncated"] = true;
                scope["tooltipStyle"] = "text-overflow: ellipsis; width: 50px; overflow: hidden; white-space: nowrap;";
                const element = $compile(templateNested)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent);
            }
        )
    );

    it("does not show the tooltip on mouseover on the trigger if text is truncated in grandchild",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const scope = $rootScope.$new();
                scope["tooltipContent"] = "Tooltip's content";
                scope["tooltipTrigger"] = "Tooltip trigger with looooong text";
                scope["isTruncated"] = true;
                scope["tooltipStyle"] = "text-overflow: ellipsis; width: 50px; overflow: hidden; white-space: nowrap;";
                const element = $compile(template2xNested)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeNull();
            }
        )
    );

    it("removes the tooltip on mouseout from the trigger",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const tooltipContent = "Tooltip's content";
                const scope = $rootScope.$new();
                scope["tooltipContent"] = tooltipContent;
                scope["tooltipTrigger"] = "Tooltip trigger";
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltipOver = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                trigger.dispatchEvent(new Event("mouseout", {"bubbles": true}));
                const tooltipOut = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltipOver).toBeDefined();
                expect(tooltipOver.textContent).toBe(tooltipContent);
                expect(tooltipOut).toBeNull();
            }
        )
    );

    it("hides the tooltip on mousedown on the trigger",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const tooltipContent = "Tooltip's content";
                const scope = $rootScope.$new();
                scope["tooltipContent"] = tooltipContent;
                scope["tooltipTrigger"] = "Tooltip trigger";
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                trigger.dispatchEvent(new Event("mousedown", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).not.toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent);
            }
        )
    );

    it("changes the tooltip text dynamically", function (done) {
            inject(
                ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                    // Arrange
                    const updatedContent = "Updated tooltip's content";
                    const scope = $rootScope.$new();
                    scope["tooltipContent"] = "Tooltip's content";
                    scope["tooltipTrigger"] = "Tooltip trigger";
                    const element = $compile(template)(scope);
                    angular.element("body").append(element);
                    scope.$digest();

                    // Act
                    $rootScope.$apply();
                    const trigger = <HTMLElement>element[0].firstChild;
                    trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                    const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                    trigger.setAttribute("bp-tooltip", updatedContent);

                    // Assert
                    expect(tooltip).toBeDefined();
                    expect(tooltip.classList).toContain("show");

                    setTimeout(function () {
                        expect(tooltip.textContent).toBe(updatedContent);
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
                const scope = $rootScope.$new();
                scope["tooltipContent"] = "Tooltip's content";
                scope["tooltipTrigger"] = "Tooltip trigger";
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                document.body.style.width = "400px";
                document.body.style.height = "400px";
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                const ev = document.createEvent("MouseEvent");
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
                const scope = $rootScope.$new();
                scope["tooltipContent"] = "Tooltip's content";
                scope["tooltipTrigger"] = "Tooltip trigger";
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                document.body.style.width = "400px";
                document.body.style.height = "400px";
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                const ev = document.createEvent("MouseEvent");
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

    it("moves the tooltip to the bottom if too close to the top edge",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const scope = $rootScope.$new();
                scope["tooltipContent"] = _.pad("", 300, "ABCDEFGHI "); // generates long content for the tooltip
                scope["tooltipTrigger"] = "Tooltip trigger";
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                document.body.style.width = "300px";
                document.body.style.height = "400px";
                scope.$digest();

                // Act
                $rootScope.$apply();
                const trigger = <HTMLElement>element[0].firstChild;
                trigger.dispatchEvent(new Event("mouseover", {"bubbles": true}));
                const tooltip = <HTMLElement>document.body.querySelector("div.bp-tooltip");

                const ev = document.createEvent("MouseEvent");
                ev.initMouseEvent(
                    "mousemove",
                    true /* bubble */, true /* cancelable */,
                    window, null,
                    0, 0, /* screen coordinates */
                    document.body.clientWidth * 0.25, 100, /* client coordinates */
                    false, false, false, false, /* modifier keys */
                    null, null
                );
                trigger.dispatchEvent(ev);

                // Assert
                expect(tooltip.classList).toContain("bp-tooltip-top-tip");
                expect(tooltip.classList).not.toContain("bp-tooltip-bottom-tip");
            }
        )
    );
});
