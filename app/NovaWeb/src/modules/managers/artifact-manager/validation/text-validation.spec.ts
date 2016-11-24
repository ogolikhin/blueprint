import "angular";
import "angular-mocks";
import {ValidationService, IValidationService} from "./validation.svc";
import {LocalizationService} from "../../../core/localization/localizationService";

describe("text validation tests - ", () => {
    let validationService: IValidationService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationService);
        $provide.service("validationService", ValidationService);
    }));

    beforeEach(inject((
        _validationService_: IValidationService) => {
        validationService = _validationService_;
    }));

    describe("text rtf validation - ", () => {

        it("isValidated true - ignore validation", () => {
            // act
            const result = validationService.textRtfValidation.hasValueIfRequired(null, null, null, false);

            // assert
            expect(result).toBeTruthy();
        });

        it("isRequired true - valid values - success", () => {
            // act
            const result = validationService.textRtfValidation.hasValueIfRequired(true, "abc", "abc", true);

            // assert
            expect(result).toBeTruthy();
        });

        it("isRequired true - null values - fails", () => {
            // act
            const result = validationService.textRtfValidation.hasValueIfRequired(true, null, null, true);

            // assert
            expect(result).toBeFalsy();
        });

        it("isRequired false - null values - success", () => {
            // act
            const result = validationService.textRtfValidation.hasValueIfRequired(false, null, null, true);

            // assert
            expect(result).toBeTruthy();
        });
    });
});