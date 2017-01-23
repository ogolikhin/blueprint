import * as angular from "angular";
import "angular-mocks";
import "lodash";
import "rx/dist/rx.lite";
import {TabSliderComponent} from "./tabSlider";
import {TabSliderController} from "./tabSlider.controller";
import {WindowManager} from "../../../main/services/window-manager";
import {WindowResize} from "../../../core/services/windowResize";

describe("TabSliderComponent", () => {
    angular.module("bp.widgets.tabSlider", [])
        .component("tabSlider", new TabSliderComponent());

    beforeEach(angular.mock.module("bp.widgets.tabSlider", ($provide: ng.auto.IProvideService) => {
        $provide.service("windowManager", WindowManager);
        $provide.service("windowResize", WindowResize);
    }));

    it("Values are bound", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        const element = `<tab-slider
                                slide-selector="li"
                                invalid-class="invalid"
                                active-class="active"
                                transition-delay="500"
                                responsive="true"
                                slide-select="setActive()" />`;

        // Act
        const controller = $compile(element)($rootScope.$new()).controller("tabSlider") as TabSliderController;

        // Assert
        expect(controller.slideSelector).toEqual("li");
        expect(controller.invalidClass).toEqual("invalid");
        expect(controller.activeClass).toEqual("active");
        expect(controller.transitionDelay).toEqual(500);
        expect(controller.responsive).toBeTruthy();
        expect(_.isFunction(controller.slideSelect)).toBeTruthy();
    }));

    it("Defaults values are applied", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        const element = `<tab-slider />`;

        // Act
        const controller = $compile(element)($rootScope.$new()).controller("tabSlider") as TabSliderController;

        // Assert
        expect(controller.slideSelector).toEqual("li");
        expect(controller.invalidClass).toEqual("invalid");
        expect(controller.activeClass).toEqual("active");
        expect(controller.transitionDelay).toEqual(500);
        expect(controller.responsive).toBeFalsy();
        expect(_.isFunction(controller.slideSelect)).toBeFalsy();
    }));
});
