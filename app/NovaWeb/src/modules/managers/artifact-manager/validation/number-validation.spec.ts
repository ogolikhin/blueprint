import "angular";
import "angular-mocks";
import {ValidationService, IValidationService} from "./validation.svc";
import {LocalizationService} from "../../../core/localization/localizationService";

describe("number validation tests - ", () => {
    let validationService: IValidationService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationService);
        $provide.service("validationService", ValidationService);
    }));

    beforeEach(inject((
        _validationService_: IValidationService) => {
        validationService = _validationService_;
    }));

    describe("decimal validation - ", () => {

        it("isValidated true - ignore validation", () => {
            // act
            const result = validationService.numberValidation.decimalPlaces(null, null, null, false);

            // assert
            expect(result).toBeTruthy();
        });

        it("success", () => {
            // arrange
            const value = 1.22;
            // act
            const result = validationService.numberValidation.decimalPlaces(value, value, 2, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("value contains no decimals - success", () => {
            // arrange
            const value = 122;
            // act
            const result = validationService.numberValidation.decimalPlaces(value, value, 0, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("value contains too many decimals - fails", () => {
            // arrange
            const value = 1.22;
            // act
            const result = validationService.numberValidation.decimalPlaces(value, value, 0, true);

            // assert
            expect(result).toBeFalsy();
        });

        it("more than specified decimal points - fails", () => {
            // arrange
            const value = 1.222;
            // act
            const result = validationService.numberValidation.decimalPlaces(value, value, 2, true);

            // assert
            expect(result).toBeFalsy();
        });
    });

    describe("wrongFormat - ", () => {

        it("isValidated true - ignore validation", () => {
            // act
            const result = validationService.numberValidation.wrongFormat(null, null, null, false);

            // assert
            expect(result).toBeTruthy();
        });
    });

    describe("isMax - ", () => {

        it("isValidated true - ignore validation", () => {
            // act
            const result = validationService.numberValidation.isMax(null, null, null, false);

            // assert
            expect(result).toBeTruthy();
        });

        it("less than max - success", () => {
            // arrange
            const value = 99;
            // act
            const result = validationService.numberValidation.isMax(value, value, 100, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("exact max - success", () => {
            // arrange
            const value = 100;
            // act
            const result = validationService.numberValidation.isMax(value, value, 100, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("more than max - fails", () => {
            // arrange
            const value = 101;
            // act
            const result = validationService.numberValidation.isMax(value, value, 100, true);

            // assert
            expect(result).toBeFalsy();
        });
    });

    describe("isMin - ", () => {

        it("isValidated true - ignore validation", () => {
            // act
            const result = validationService.numberValidation.isMin(null, null, null, false);

            // assert
            expect(result).toBeTruthy();
        });

        it("greater than min - success", () => {
            // arrange
            const value = 11;
            // act
            const result = validationService.numberValidation.isMin(value, value, 10, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("exact min - success", () => {
            // arrange
            const value = 10;
            // act
            const result = validationService.numberValidation.isMin(value, value, 10, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("less than min - fails", () => {
            // arrange
            const value = 9;
            // act
            const result = validationService.numberValidation.isMin(value, value, 10, true);

            // assert
            expect(result).toBeFalsy();
        });
    });

    describe("isValid - ", () => {
        function createValidationSpies(isValidDecimal, isValidWrongFormat, isValidIsMin, isValidIsMax, isValidRequired) {

            spyOn(validationService.numberValidation, "decimalPlaces").and.returnValue(isValidDecimal);
            spyOn(validationService.numberValidation, "wrongFormat").and.returnValue(isValidWrongFormat);
            spyOn(validationService.numberValidation, "isMin").and.returnValue(isValidIsMin);
            spyOn(validationService.numberValidation, "isMax").and.returnValue(isValidIsMax);
            spyOn(validationService.numberValidation, "hasValueIfRequired").and.returnValue(isValidRequired);
        }

        it("everything valid", () => {
            // arrange
            createValidationSpies(true, true, true, true, true);
            // act
            const result = validationService.numberValidation.isValid(null, null, null, null, null, null, null);
            // assert
            expect(result).toBeTruthy();
        });

        it("decimalPlaces - fails", () => {
            // arrange
            createValidationSpies(false, true, true, true, true);
            // act
            const result = validationService.numberValidation.isValid(null, null, null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });

        it("wrongFormat - fails", () => {
            // arrange
            createValidationSpies(true, false, true, true, true);
            // act
            const result = validationService.numberValidation.isValid(null, null, null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });

        it("isMin - fails", () => {
            // arrange
            createValidationSpies(true, true, false, true, true);
            // act
            const result = validationService.numberValidation.isValid(null, null, null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });

        it("isMax - fails", () => {
            // arrange
            createValidationSpies(true, true, true, false, true);
            // act
            const result = validationService.numberValidation.isValid(null, null, null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });
        
        it("hasValueIfRequired - fails", () => {
            // arrange
            createValidationSpies(true, true, true, true, false);
            // act
            const result = validationService.numberValidation.isValid(null, null, null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });
    });
});