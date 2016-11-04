import "../../";
import * as angular from "angular";
import "angular-mocks";
import {QuickSearchController, IQuickSearchController} from './quickSearchController';

describe("Controller: Quick Search", () => {
    let controller;
    // Load the module
    beforeEach(angular.mock.module("app.main"));

    // Provide any mocks needed
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {

    }));

    // Inject in angular constructs otherwise,
    //  you would need to inject these into each test
    beforeEach(inject(($controller: ng.IControllerService) => {
        controller = $controller(QuickSearchController);
        controller.form = {
            $submitted: false,
            $invalid: false,
            $error: {},
            $setPristine: function() {
                //do nothing
            }
        };
    }));

    it("should exist", () => {
        expect(controller).toBeDefined();
    });

    it("should only not show errors if $invalid and not $submitted", () => {
        expect(controller.hasError()).toBe(false);
        controller.form.$invalid = true;
        controller.form.$submitted = false;
        expect(controller.hasError()).toBe(false);
    });

    it("should show errors if $invalid and $submitted", () => {
        expect(controller.hasError()).toBe(false);
        controller.form.$invalid = true;
        controller.form.$submitted = true;
        expect(controller.hasError()).toBe(true);
    });

    it("should not show errors if $invalid and $submitted but $error is 'required' defined", () => {
        expect(controller.hasError()).toBe(false);
        controller.form.$invalid = true;
        controller.form.$submitted = true;
        controller.form.$error.required = {};
        expect(controller.hasError()).toBe(false);
    });

    it("keypress calls $setPristine", () => {
        const spy = spyOn(controller.form, "$setPristine");
        controller.onKeyPress({keyCode: 1});

        expect(spy).toHaveBeenCalled();
    });

    it("keypress enter, does not call $setPristine", () => {
        const spy = spyOn(controller.form, "$setPristine");
        controller.onKeyPress({keyCode: 13});

        expect(spy).not.toHaveBeenCalled();
    });

    it("keydown delete, does not call $setPristine", () => {
        const spy = spyOn(controller.form, "$setPristine");
        controller.onKeyDown({keyCode: 46});

        expect(spy).toHaveBeenCalled();
    });

    it("keydown backspace, does not call $setPristine", () => {
        const spy = spyOn(controller.form, "$setPristine");
        controller.onKeyDown({keyCode: 8});

        expect(spy).toHaveBeenCalled();
    });

    it("can opens the modal when valid", () => {
        expect(controller.openModal()).not.toBe(null);
        controller.form.$invalid = true;
        expect(controller.openModal()).toBe(null);
    });
});
