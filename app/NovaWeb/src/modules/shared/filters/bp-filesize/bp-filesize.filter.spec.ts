import "angular";
import "angular-mocks";
import "../";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";

describe("The test filter", () => {
    let $filter;

    beforeEach(angular.mock.module("bp.filters"));
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));
    beforeEach(inject((_$filter_: ng.IFilterService) => {
        $filter = _$filter_;
    }));


    it("should show bytes, if input is invalid", () => {
        // Arrange
        const input = "invalid";

        // Act
        const result = $filter("bpFilesize")(input);

        // Assert
        expect(result).toEqual("0 Filesize_Bytes");
    });
    
    it("should show bytes", () => {
        // Arrange
        const input = 100;

        // Act
        const result = $filter("bpFilesize")(input);

        // Assert
        expect(result).toEqual("100 Filesize_Bytes");
    });

    it("should show KB", () => {
        // Arrange
        const input = 10000;

        // Act
        const result = $filter("bpFilesize")(input);

        // Assert
        expect(result).toEqual("9.77 Filesize_KB");
    });

    it("should show MB", () => {
        // Arrange
        const input = 10000000;

        // Act
        const result = $filter("bpFilesize")(input);

        // Assert
        expect(result).toEqual("9.54 Filesize_MB");
    });

    it("should show GB", () => {
        // Arrange
        const input = 10000000000;

        // Act
        const result = $filter("bpFilesize")(input);

        // Assert
        expect(result).toEqual("9.31 Filesize_GB");
    });

    it("should show TB", () => {
        // Arrange
        const input = 10000000000000;

        // Act
        const result = $filter("bpFilesize")(input);

        // Assert
        expect(result).toEqual("9.09 Filesize_TB");
    });
});