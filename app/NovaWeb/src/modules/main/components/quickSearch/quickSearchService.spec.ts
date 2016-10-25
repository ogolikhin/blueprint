import "../../";
import * as angular from "angular";
import "angular-mocks";
describe("Controller: Quick Search", () => {
    // Load the module
    beforeEach(angular.mock.module("app.main"));

    // Provide any mocks needed
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {

    }));

    // Inject in angular constructs otherwise,
    //  you would need to inject these into each test
    beforeEach(inject(() => {

    }));

    it('should exist', () => {

    });

});
