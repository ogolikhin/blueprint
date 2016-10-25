import "../../";
import * as angular from "angular";
import "angular-mocks";
import {QuickSearchService} from './quickSearchService'

describe("Service: Quick Search", () => {
    let service;
    // Load the module
    beforeEach(angular.mock.module("app.main"));

    // Provide any mocks needed
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {

    }));

    // Inject in angular constructs otherwise,
    //  you would need to inject these into each test
    beforeEach(inject((quickSearchService) => {
        service = quickSearchService;
    }));

    it('should contain a QuickSearchService', () => {
        expect(service).toBeDefined();
    });

    it('search is only enabled if you have at least one open project', () => {
        //const mocProject;
        expect(service.canSearch()).toBe(true);
        service.projectManager.projectCollection.value = [{id:123}];
        expect(service.canSearch()).toBe(false);
    });

});
