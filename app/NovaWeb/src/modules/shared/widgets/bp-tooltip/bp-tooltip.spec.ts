import "./";
import "angular-mocks";
import "lodash";
import {SettingsServiceMock} from "../../../commonModule/configuration/settings.mock";

interface ITooltipParams {
    tooltipContent: string;
    tooltipTrigger: string;
    isTruncated?: boolean;
    limit?: number;
    tooltipStyle?: string;
}

describe("Directive BP-Tooltip", () => {
    const tooltipContent = "Tooltip's content";
    let params: ITooltipParams = {
        tooltipContent: tooltipContent,
        tooltipTrigger: "Tooltip trigger"
    };
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

    function triggerTooltip(elem: ng.IAugmentedJQuery, eventName: string = "mouseover"): HTMLElement {
        const trigger = <HTMLElement>elem[0].firstChild;
        trigger.dispatchEvent(new Event(eventName, {"bubbles": true}));
        return <HTMLElement>document.body.querySelector("div.bp-tooltip");
    }

    beforeEach(angular.mock.module("bp.widgets.tooltip"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("settings", SettingsServiceMock);
        params = {
            tooltipContent: tooltipContent,
            tooltipTrigger: "Tooltip trigger"
        };
    }));

    afterEach(function () {
        angular.element("body").empty();
    });

    it("shows the tooltip on mouseover on the trigger",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const scope = $rootScope.$new();
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

                // Assert
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent);
            }
        )
    );

    it("shows a truncated tooltip (default limit set in app settings)",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, settings: SettingsServiceMock) => {
                // Arrange
                const defaultLimit = 250;
                const getNumberSpy = spyOn(settings, "getNumber").and.callFake(() => {
                    return defaultLimit;
                });

                const randomString = [];
                for (let i = 0; i < defaultLimit + 50; i++) {
                    randomString[i] = _.random(48, 122);
                }
                const customTooltipContent = String.fromCharCode(...randomString);
                const scope = $rootScope.$new();
                params.tooltipContent = customTooltipContent;
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

                // Assert
                expect(getNumberSpy).toHaveBeenCalled();
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(customTooltipContent.slice(0, defaultLimit) + "…");
            }
        )
    );

    it("shows a truncated tooltip (limit set on element overrides app settings)",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, settings: SettingsServiceMock) => {
                // Arrange
                const defaultLimit = 250;
                const getNumberSpy = spyOn(settings, "getNumber").and.callFake(() => {
                    return defaultLimit;
                });

                const limit = 4;
                const scope = $rootScope.$new();
                params.limit = limit;
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

                // Assert
                expect(getNumberSpy).toHaveBeenCalled();
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent.slice(0, limit) + "…");
            }
        )
    );

    it("disables the tooltip if limit = 0 (limit set in app settings)",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, settings: SettingsServiceMock) => {
                // Arrange
                const getNumberSpy = spyOn(settings, "getNumber").and.callFake(() => {
                    return 0;
                });

                const scope = $rootScope.$new();
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

                // Assert
                expect(getNumberSpy).toHaveBeenCalled();
                expect(tooltip).toBeNull();
            }
        )
    );

    it("disables the tooltip if app settings limit = 0 but element limit > 0",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, settings: SettingsServiceMock) => {
                // Arrange
                const getNumberSpy = spyOn(settings, "getNumber").and.callFake(() => {
                    return 0;
                });

                const limit = 4;
                const scope = $rootScope.$new();
                params.limit = limit;
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

                // Assert
                expect(getNumberSpy).toHaveBeenCalled();
                expect(tooltip).toBeNull();
            }
        )
    );

    it("shows the whole tooltip if the limit is bigger then the tooltip's length",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const limit = 40;
                const scope = $rootScope.$new();
                params.limit = limit;
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

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
                params.isTruncated = true;
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

                // Assert
                expect(tooltip).toBeNull();
            }
        )
    );

    it("shows the tooltip on mouseover on the trigger if text is truncated",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const scope = $rootScope.$new();
                params.tooltipTrigger = "Tooltip trigger with looooong text";
                params.tooltipStyle = "text-overflow: ellipsis; width: 50px; overflow: hidden; white-space: nowrap;";
                params.isTruncated = true;
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

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
                params.tooltipTrigger = "Tooltip trigger with looooong text";
                params.tooltipStyle = "text-overflow: ellipsis; width: 50px; overflow: hidden; white-space: nowrap;";
                params.isTruncated = true;
                _.extend(scope, params);
                const element = $compile(templateNested)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

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
                params.tooltipTrigger = "Tooltip trigger with looooong text";
                params.tooltipStyle = "text-overflow: ellipsis; width: 50px; overflow: hidden; white-space: nowrap;";
                params.isTruncated = true;
                _.extend(scope, params);
                const element = $compile(template2xNested)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);

                // Assert
                expect(tooltip).toBeNull();
            }
        )
    );

    it("removes the tooltip on mouseout from the trigger",
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                // Arrange
                const scope = $rootScope.$new();
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltipOver = triggerTooltip(element);
                const tooltipOut = triggerTooltip(element, "mouseout");

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
                const scope = $rootScope.$new();
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                scope.$digest();

                // Act 1
                $rootScope.$apply();
                let tooltip = triggerTooltip(element);

                // Assert 1
                expect(tooltip).toBeDefined();
                expect(tooltip.classList).toContain("show");
                expect(tooltip.textContent).toBe(tooltipContent);

                // Act 2
                tooltip = triggerTooltip(element, "mousedown");

                // Assert 2
                expect(tooltip.classList).not.toContain("show");
            }
        )
    );

    it("changes the tooltip text dynamically", function (done) {
            inject(
                ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                    // Arrange
                    const updatedContent = "Updated tooltip's content";
                    const scope = $rootScope.$new();
                    _.extend(scope, params);
                    const element = $compile(template)(scope);
                    angular.element("body").append(element);
                    scope.$digest();

                    // Act 1
                    $rootScope.$apply();
                    const tooltip = triggerTooltip(element);

                    // Assert 1
                    expect(tooltip).toBeDefined();
                    expect(tooltip.classList).toContain("show");

                    // Act 2
                    const trigger = <HTMLElement>element[0].firstChild;
                    trigger.setAttribute("bp-tooltip", updatedContent);

                    // Assert 2
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
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                document.body.style.width = "400px";
                document.body.style.height = "400px";
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);
                const trigger = <HTMLElement>element[0].firstChild;

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
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                document.body.style.width = "400px";
                document.body.style.height = "400px";
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);
                const trigger = <HTMLElement>element[0].firstChild;

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
                params.tooltipContent = _.pad("", 300, "ABCDEFGHI "); // generates long content for the tooltip
                _.extend(scope, params);
                const element = $compile(template)(scope);
                angular.element("body").append(element);
                document.body.style.width = "300px";
                document.body.style.height = "400px";
                scope.$digest();

                // Act
                $rootScope.$apply();
                const tooltip = triggerTooltip(element);
                const trigger = <HTMLElement>element[0].firstChild;

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
