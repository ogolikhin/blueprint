import "angular";
import "angular-mocks";
import {ValidationService, IValidationService} from "./validation.svc";
import {LocalizationService} from "../../../core/localization/localization.service";

describe("user picker validation tests - ", () => {
    let validationService: IValidationService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationService);
        $provide.service("validationService", ValidationService);
    }));

    beforeEach(inject((
        _validationService_: IValidationService) => {
        validationService = _validationService_;
    }));

    describe("hasValueIfRequired -", () => {

        it("returns true when it does not need to be validated", () => {
            // act
            const result = validationService.userPickerValidation.hasValueIfRequired(null, null);

            // assert
            expect(result).toBe(true);
        });

        it("returns true when required and values are valid", () => {
            const value = ["test"];
            // act
            const result = validationService.userPickerValidation.hasValueIfRequired(true, value);

            // assert
            expect(result).toBe(true);
        });

        it("returns false when required and values are empty", () => {
            const value = [];
            // act
            const result = validationService.userPickerValidation.hasValueIfRequired(true, value);

            // assert
            expect(result).toBe(false);
        });

        it("returns false when required and values are null", () => {
            const value = null;
            // act
            const result = validationService.userPickerValidation.hasValueIfRequired(true, value);

            // assert
            expect(result).toBe(false);
        });

    });
});
