import "angular";
import "angular-mocks";
import {ValidationService, IValidationService} from "./validation.svc";
import {LocalizationService} from "../../../core/localization/localizationService";

describe("select validation tests - ", () => {
    let validationService: IValidationService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationService);
        $provide.service("validationService", ValidationService);
    }));

    beforeEach(inject((
        _validationService_: IValidationService) => {
        validationService = _validationService_;
    }));

    describe("selectValidation", () => {

        describe("hasValueIfRequired -", () => {

            it("returns true when it does not need to be validated", () => {
                // act
                const result = validationService.selectValidation.hasValueIfRequired(null, null);

                // assert
                expect(result).toBe(true);
            });

            it("returns true when required and values are valid", () => {
                const value = {customValue: "test"};
                // act
                const result = validationService.selectValidation.hasValueIfRequired(true, value);

                // assert
                expect(result).toBe(true);
            });
            it("returns false when required and values are null", () => {
                const value = null;
                // act
                const result = validationService.selectValidation.hasValueIfRequired(true, value);

                // assert
                expect(result).toBe(false);
            });

            it("returns true when required and CUSTOM values are valid", () => {
                const value = {"customValue": "test"};
                // act
                const result = validationService.selectValidation.hasValueIfRequired(true, value);

                // assert
                expect(result).toBe(true);
            });
            it("returns false when required and CUSTOM values are null", () => {
                const value = null;
                // act
                const result = validationService.selectValidation.hasValueIfRequired(true, value);

                // assert
                expect(result).toBe(false);
            });

        });

    });

    describe("multiSelectValidation", () => {

        describe("hasValueIfRequired -", () => {

            it("returns true when it does not need to be validated", () => {
                // act
                const result = validationService.multiSelectValidation.hasValueIfRequired(null, null);

                // assert
                expect(result).toBe(true);
            });

            it("returns true when required and values are valid", () => {
                const value = [12];
                // act
                const result = validationService.multiSelectValidation.hasValueIfRequired(true, value);

                // assert
                expect(result).toBe(true);
            });

            it("returns false when required and value is empty", () => {
                const value = [];
                // act
                const result = validationService.multiSelectValidation.hasValueIfRequired(true, value);

                // assert
                expect(result).toBe(false);
            });

            it("returns false when required and values are null", () => {
                const value = null;
                // act
                const result = validationService.multiSelectValidation.hasValueIfRequired(true, value);

                // assert
                expect(result).toBe(false);
            });

        });
    });
});
