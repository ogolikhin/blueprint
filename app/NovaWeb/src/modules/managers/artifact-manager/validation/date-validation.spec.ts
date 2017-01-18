import "angular";
import "angular-mocks";
import {ValidationService, IValidationService} from "./validation.svc";
import {LocalizationService} from "../../../core/localization/localization.service";

describe("date validation tests - ", () => {
    let validationService: IValidationService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationService);
        $provide.service("validationService", ValidationService);
    }));

    beforeEach(inject((
        _validationService_: IValidationService) => {
        validationService = _validationService_;
    }));


    describe("wrongFormat -", () => {

        it("null value - success", () => {
            const date = "";
            // act
            const result = validationService.dateValidation.wrongFormat(date);

            // assert
            expect(result).toBe(true);
        });
    });

    describe("maxDate -", () => {

        it("returns true when it does not need to be validated", () => {
            // act
            const result = validationService.dateValidation.maxDate(null, null, false);

            // assert
            expect(result).toBe(true);
        });

        it("null date - success", () => {
            // act
            const result = validationService.dateValidation.maxDate(null, null, true);

            // assert
            expect(result).toBe(true);
        });

        it("less than max date - success", () => {
            // arrange
            const date = new Date(2016, 11, 24);
            const maxDate = new Date(2016, 11, 25);
            // act
            const result = validationService.dateValidation.maxDate(date, maxDate, true);

            // assert
            expect(result).toBe(true);
        });

        it("exact max date - success", () => {
            // arrange
            const date = new Date(2016, 11, 25);
            const maxDate = new Date(2016, 11, 25);
            // act
            const result = validationService.dateValidation.maxDate(date, maxDate, true);

            // assert
            expect(result).toBe(true);
        });

        it("more than max date - fails", () => {
            // arrange
            const date = new Date(2016, 11, 25);
            const maxDate = new Date(2016, 11, 1);
            // act
            const result = validationService.dateValidation.maxDate(date, maxDate, true);

            // assert
            expect(result).toBe(false);
        });


    });

    describe("minDate -", () => {

        it("returns true when it does not need to be validated", () => {
            // act
            const result = validationService.dateValidation.minDate(null, null, false);

            // assert
            expect(result).toBe(true);
        });

        it("null date - success", () => {
            // act
            const result = validationService.dateValidation.minDate(null, null, true);

            // assert
            expect(result).toBe(true);
        });

        it("more than min date - success", () => {
            // arrange
            const date = new Date(2016, 11, 25);
            const minDate = new Date(2016, 11, 24);
            // act
            const result = validationService.dateValidation.minDate(date, minDate, true);

            // assert
            expect(result).toBe(true);
        });

        it("exact min date - success", () => {
            // arrange
            const date = new Date(2016, 11, 25);
            const minDate = new Date(2016, 11, 25);
            // act
            const result = validationService.dateValidation.minDate(date, minDate, true);

            // assert
            expect(result).toBe(true);
        });

        it("less than min date - fails", () => {
            // arrange
            const date = new Date(2016, 11, 1);
            const minDate = new Date(2016, 11, 25);
            // act
            const result = validationService.dateValidation.minDate(date, minDate, true);

            // assert
            expect(result).toBe(false);
        });

    });

    describe("minDateSQL -", () => {

        it("null date - success", () => {
            // act
            const result = validationService.dateValidation.minDateSQL(null);

            // assert
            expect(result).toBe(true);
        });

        it("exact min sql date - success", () => {
            // arrange
            const date = new  Date(1753, 0, 1);
            // act
            const result = validationService.dateValidation.minDateSQL(date);

            // assert
            expect(result).toBe(true);
        });
        it("less than min sql date - fails", () => {
            // arrange
            const date = new  Date(1752, 11, 31);
            // act
            const result = validationService.dateValidation.minDateSQL(date);

            // assert
            expect(result).toBe(false);
        });
    });

    describe("isValid - ", () => {
        function createValidationSpies(isValidMinDateSQL, isValidWrongFormat, isValidMinDate, isValidMaxDte, isValidRequired) {

            spyOn(validationService.dateValidation, "minDateSQL").and.returnValue(isValidMinDateSQL);
            spyOn(validationService.dateValidation, "wrongFormat").and.returnValue(isValidWrongFormat);
            spyOn(validationService.dateValidation, "minDate").and.returnValue(isValidMinDate);
            spyOn(validationService.dateValidation, "maxDate").and.returnValue(isValidMaxDte);
            spyOn(validationService.dateValidation, "hasValueIfRequired").and.returnValue(isValidRequired);
        }

        it("everything valid", () => {
            // arrange
            createValidationSpies(true, true, true, true, true);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBe(true);
        });

        it("minDateSQL - fails", () => {
            // arrange
            createValidationSpies(false, true, true, true, true);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBe(false);
        });

        it("wrongFormat - fails", () => {
            // arrange
            createValidationSpies(true, false, true, true, true);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBe(false);
        });

        it("minDate - fails", () => {
            // arrange
            createValidationSpies(true, true, false, true, true);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBe(false);
        });

        it("maxDate - fails", () => {
            // arrange
            createValidationSpies(true, true, true, false, true);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBe(false);
        });

        it("hasValueIfRequired - fails", () => {
            // arrange
            createValidationSpies(true, true, true, true, false);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBe(false);
        });
    });
});
