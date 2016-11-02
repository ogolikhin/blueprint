import "../../";
import * as angular from "angular";
import "angular-mocks";
import {QuickSearchModalController} from "./quickSearchModalController";
import {QuickSearchService} from "./quickSearchService";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";


describe("Controller: Quick Search Modal", () => {
    let controller;
    const uibModalInstance = {
        close: () => {/*mock*/
        }, dismiss: () => {/*mock*/
        }
    };
    // Load the module
    beforeEach(angular.mock.module("app.main"));

    // Provide any mocks needed
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));

    // Inject in angular constructs otherwise,
    //  you would need to inject these into each test
    beforeEach(inject(($controller: ng.IControllerService,
                       $log: ng.ILogService) => {
        controller = $controller(QuickSearchModalController, {
            QuickSearchService,
            $log,
            $uibModalInstance: uibModalInstance
        });
        controller.form = {
            $submitted: false,
            $invalid: false
        }
    }));

    it('should exist', () => {
        expect(controller).toBeDefined();
    });

    it('can only search if a term is valid', () => {
        expect(controller.search('New')).not.toBe(null);
        controller.form.$invalid = true;
        expect(controller.search()).toBe(null);
    });

});
