import * as angular from "angular";
import "angular-mocks";
import "lodash";
import "rx/dist/rx.lite";
import {TabSliderComponent} from "./tabSlider";
import {TabSliderController} from "./tabSlider.controller";
import {IWindowManager, WindowManager} from "../../../main/services/window-manager";
import {WindowResize} from "../../../core/services/windowResize";

describe("TabSliderController", () => {
    angular.module("bp.widgets.tabSlider", [])
        .component("tabSlider", new TabSliderComponent());

    let controller: TabSliderController;
    let scope: ng.IScope;
    let element: ng.IAugmentedJQuery;
    // the following setup is to make sure that PhantomJS renders the slides the way they are supposed to
    const template = `
<tab-slider slide-selector=".slide" style="display:block;width:100px;">
    <div class="container" style="overflow:hidden;white-space:nowrap;">
        <div class="slide slide1" style="display:inline-block;width:40px;">1</div>
        <div class="slide slide2" style="display:inline-block;width:40px;">2</div>
        <div class="slide slide3" style="display:inline-block;width:40px;">3</div>
        <div class="slide slide4" style="display:inline-block;width:40px;">4</div>
        <div class="slide slide5" style="display:inline-block;width:40px;">5</div>
    </div>
</tab-slider>`;

    beforeEach(angular.mock.module("bp.widgets.tabSlider", ($provide: ng.auto.IProvideService) => {
        $provide.service("windowManager", WindowManager);
        $provide.service("windowResize", WindowResize);
    }));

    afterEach(() => {
        angular.element("body").empty();
    });

    describe("Component lifecylcle methods", () => {
        beforeEach(inject(($rootScope: ng.IRootScopeService,
                           $templateCache: ng.ITemplateCacheService,
                           $compile: ng.ICompileService,
                           $timeout: ng.ITimeoutService,
                           $q: ng.IQService,
                           windowManager: IWindowManager) => {
            scope = $rootScope.$new();
            element = angular.element(`<tab-slider />`);
            controller = new TabSliderController(scope, element, $templateCache, $compile, $timeout, $q, windowManager);
        }));

        it("$onChanges, calls setupSlides and recalculate", () => {
            // Arrange
            spyOn(controller, "setupSlides");
            spyOn(controller, "recalculate").and.callFake(() => {
                return {
                    then: () => {
                        return;
                    }
                };
            });

            // Act
            controller.$onChanges();
            scope.$apply();

            // Assert
            expect(controller["setupSlides"]).toHaveBeenCalled();
            expect(controller["recalculate"]).toHaveBeenCalled();
        });

        it("$postLink, calls setupContainer and recalculate", () => {
            // Arrange
            spyOn(controller, "setupContainer");
            spyOn(controller, "recalculate");

            // Act
            controller.$postLink();

            // Assert
            expect(controller["setupContainer"]).toHaveBeenCalled();
            expect(controller["recalculate"]).toHaveBeenCalled();
        });
    });

    describe("Public methods", () => {
        it("nextSlide and previousSlide change the current slide index correctly",
            inject(($rootScope: ng.IRootScopeService, $compile: ng.ICompileService) => {
                // Arrange
                scope = $rootScope.$new();
                element = angular.element(template);
                controller = $compile(element)(scope).controller("tabSlider") as TabSliderController;

                // Act
                scope.$apply();

                // Assert
                expect(controller["scrollIndex"]).toEqual(0);
                controller.nextSlide();
                expect(controller["scrollIndex"]).toEqual(1);
                controller.nextSlide();
                controller.nextSlide();
                controller.nextSlide();
                expect(controller["scrollIndex"]).toEqual(4);
                controller.nextSlide();
                expect(controller["scrollIndex"]).toEqual(4);
                controller.previousSlide();
                expect(controller["scrollIndex"]).toEqual(3);
                controller.previousSlide();
                controller.previousSlide();
                controller.previousSlide();
                expect(controller["scrollIndex"]).toEqual(0);
                controller.previousSlide();
                expect(controller["scrollIndex"]).toEqual(0);
            }));

        it("showButtonPrev and showButtonNext are false when don't need to show buttons",
            inject(($rootScope: ng.IRootScopeService, $compile: ng.ICompileService) => {
                // Arrange
                scope = $rootScope.$new();
                element = angular.element(template.replace("width:100px;", ""));
                controller = $compile(element)(scope).controller("tabSlider") as TabSliderController;

                // Act
                scope.$apply();

                // Assert
                expect(controller["scrollIndex"]).toEqual(0);
                expect(controller.showButtonPrev()).toBeFalsy();
                expect(controller.showButtonNext()).toBeFalsy();
            }));

        it("showButtonPrev is false and showButtonNext is true when we show buttons but we are on first tab",
            inject(($rootScope: ng.IRootScopeService, $compile: ng.ICompileService) => {
                // Arrange
                scope = $rootScope.$new();
                element = angular.element(template);
                angular.element("body").append(element);
                controller = $compile(element)(scope).controller("tabSlider") as TabSliderController;

                // Act
                scope.$apply();

                // Assert
                expect(controller["scrollIndex"]).toEqual(0);
                expect(controller.showButtonPrev()).toBeFalsy();
                expect(controller.showButtonNext()).toBeTruthy();
            }));

        it("showButtonPrev and showButtonNext are true when we show buttons and we are not on first tab",
            inject(($rootScope: ng.IRootScopeService, $compile: ng.ICompileService) => {
                // Arrange
                scope = $rootScope.$new();
                element = angular.element(template);
                angular.element("body").append(element);
                controller = $compile(element)(scope).controller("tabSlider") as TabSliderController;

                // Act
                scope.$apply();
                controller.nextSlide();

                // Assert
                expect(controller["scrollIndex"]).toEqual(1);
                expect(controller.showButtonPrev()).toBeTruthy();
                expect(controller.showButtonNext()).toBeTruthy();
            }));

        it("showButtonPrev is true and showButtonNext is false when we show buttons and we are on last tab",
            inject(($rootScope: ng.IRootScopeService, $compile: ng.ICompileService) => {
                // Arrange
                scope = $rootScope.$new();
                element = angular.element(template);
                angular.element("body").append(element);
                controller = $compile(element)(scope).controller("tabSlider") as TabSliderController;

                // Act
                scope.$apply();
                controller.nextSlide();
                controller.nextSlide();
                controller.nextSlide();
                controller.nextSlide();

                // Assert
                expect(controller["scrollIndex"]).toEqual(4);
                expect(controller.showButtonPrev()).toBeTruthy();
                expect(controller.showButtonNext()).toBeFalsy();
            }));
    });

    describe("ensureActiveVisible ", () => {
        it("should not call setFirstVisible as the active slide is already visible",
            inject(($rootScope: ng.IRootScopeService, $compile: ng.ICompileService, $timeout: ng.ITimeoutService) => {
                // Arrange
                scope = $rootScope.$new();
                // makes the first slide the active one
                const customTemplate = template.replace("slide slide1", "slide slide1 active");
                element = angular.element(customTemplate);
                angular.element("body").append(element);
                controller = $compile(element)(scope).controller("tabSlider") as TabSliderController;
                spyOn(controller, "ensureActiveVisible").and.callThrough();
                spyOn(controller, "isSlideHidden").and.callThrough();
                spyOn(controller, "setFirstVisible");

                // Act
                scope.$apply();
                $timeout.flush();

                // Assert
                expect(controller["ensureActiveVisible"]).toHaveBeenCalled();
                expect(controller["isSlideHidden"]).toHaveBeenCalledWith(0);
                expect(controller["setFirstVisible"]).not.toHaveBeenCalled();
            }));

        it("should call setFirstVisible as the active slide is not visible",
            inject(($rootScope: ng.IRootScopeService, $compile: ng.ICompileService, $timeout: ng.ITimeoutService) => {
                // Arrange
                scope = $rootScope.$new();
                // makes the last slide the active one
                let customTemplate = template.replace("slide slide5", "slide slide5 active");
                // adds a select callback function
                scope["onSelect"] = (index: number) => {
                    const active = document.body.querySelector(".container").children[index];
                    active.classList.add("active");
                };
                customTemplate = customTemplate.replace("<tab-slider", "<tab-slider slide-select=\"onSelect\"");
                element = angular.element(customTemplate);
                angular.element("body").append(element);
                controller = $compile(element)(scope).controller("tabSlider") as TabSliderController;
                spyOn(controller, "ensureActiveVisible").and.callThrough();
                spyOn(controller, "isSlideHidden").and.callThrough();
                spyOn(controller, "setFirstVisible").and.callThrough();
                const currentActive = document.body.querySelector(".container .active");

                // Act
                scope.$apply();
                $timeout.flush();

                // Assert
                expect(controller["ensureActiveVisible"]).toHaveBeenCalled();
                expect(controller["isSlideHidden"]).toHaveBeenCalledWith(4);
                expect(controller["setFirstVisible"]).toHaveBeenCalled();
                expect(document.body.querySelector(".container .active")).not.toBe(currentActive);
            }));
    });
});
