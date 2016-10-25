import "../../";
import * as angular from "angular";
import "angular-mocks";
import {QuickSearchController} from './quickSearchController';

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
            $invalid: false
        }
    }));

    it('should exist', () => {
        expect(controller).toBeDefined();
    });

    it('should only show error if submitted', () => {
        expect(controller.hasError()).toBe(false);
        controller.form.$invalid = true;
        controller.form.$submitted = false;
        expect(controller.hasError()).toBe(false);
        controller.form.$invalid = true;
        controller.form.$submitted = true;
        expect(controller.hasError()).toBe(true);
    });

    it('can opens the modal when valid', () => {
        expect(controller.openModal()).not.toBe(null);
        controller.form.$invalid = true;
        expect(controller.openModal()).toBe(null);
    });
});
