import "../../";
import * as angular from "angular";
import "angular-mocks";

describe("Filter: Moment Date Time", () => {
    let momentFilter;

    // Load the module
    beforeEach(angular.mock.module("app.main"));

    // Provide any mocks needed
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        return;
    }));

    // Inject in angular constructs otherwise,
    //  you would need to inject these into each test
    beforeEach(inject(($filter: ng.IFilterService) => {
        momentFilter = $filter("momentDateTime");
    }));

    it("should exist", () => {
        expect(!!momentFilter).toBe(true);
    });

    it("should parse date", () => {
        const tempDate = new Date("1980-12-11T09:45:05-05:00");

        expect(momentFilter(tempDate)).toBe("December 11, 1980 9:45:05 am");
        expect(momentFilter(tempDate, "YYYY-MM-DD HH:mm")).toBe("1980-12-11 09:45");
    });
});
