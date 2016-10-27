import "../../";
import * as angular from "angular";
import "angular-mocks";
describe("Filter: Moment Date", () => {
    let momentFilter;

    // Load the module
    beforeEach(angular.mock.module("app.main"));

    // Provide any mocks needed
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {

    }));

    // Inject in angular constructs otherwise,
    //  you would need to inject these into each test
    beforeEach(inject(($filter: ng.IFilterService) => {
        momentFilter = $filter('momentDate')
    }));

    it('should exist', () => {
        expect(!!momentFilter).toBe(true);
    });

    it('should parse date', () => {
        const tempDate = new Date("October 11 1980");

        expect(momentFilter(tempDate)).toBe('1980-10-11T00:00:00-04:00');
        expect(momentFilter(tempDate, 'MMMM DD, YYYY')).toBe("October 11, 1980")
    })

});
