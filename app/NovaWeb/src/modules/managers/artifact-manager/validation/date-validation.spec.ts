import "angular";
import "angular-mocks";
import {ValidationService, IValidationService} from "./validation.svc";
import {LocalizationService} from "../../../core/localization/localizationService";

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
            expect(result).toBeTruthy();
        });
    });

    describe("maxDate -", () => {

        it("isValidated true - ignore validation", () => {
            // act
            const result = validationService.dateValidation.maxDate(null, null, false);

            // assert
            expect(result).toBeTruthy();
        });

        it("less than max date - success", () => {
            // arrange
            const date = new Date(2016, 11, 24);
            const maxDate = new Date(2016, 11, 25);
            // act
            const result = validationService.dateValidation.maxDate(date, maxDate, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("exact max date - success", () => {
            // arrange
            const date = new Date(2016, 11, 25);
            const maxDate = new Date(2016, 11, 25);
            // act
            const result = validationService.dateValidation.maxDate(date, maxDate, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("more than max date - fails", () => {
            // arrange
            const date = new Date(2016, 11, 25);
            const maxDate = new Date(2016, 11, 1);
            // act
            const result = validationService.dateValidation.maxDate(date, maxDate, true);

            // assert
            expect(result).toBeFalsy();
        });

        
    });

    describe("minDate -", () => {


        it("isValidated true - ignore validation", () => {
            // act
            const result = validationService.dateValidation.minDate(null, null, false);

            // assert
            expect(result).toBeTruthy();
        });

        it("more than min date - success", () => {
            // arrange
            const date = new Date(2016, 11, 25);
            const minDate = new Date(2016, 11, 24);
            // act
            const result = validationService.dateValidation.minDate(date, minDate, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("exact min date - success", () => {
            // arrange
            const date = new Date(2016, 11, 25);
            const minDate = new Date(2016, 11, 25);
            // act
            const result = validationService.dateValidation.minDate(date, minDate, true);

            // assert
            expect(result).toBeTruthy();
        });

        it("less than min date - fails", () => {
            // arrange
            const date = new Date(2016, 11, 1);
            const minDate = new Date(2016, 11, 25);
            // act
            const result = validationService.dateValidation.minDate(date, minDate, true);

            // assert
            expect(result).toBeFalsy();
        });

    });
    
    describe("minDateSQL -", () => {

        it("exact min sql date - success", () => {
            // arrange
            const date = new  Date(1753, 0, 1); 
            // act
            const result = validationService.dateValidation.minDateSQL(date);

            // assert
            expect(result).toBeTruthy();
        });
        it("less than min sql date - fails", () => {
            // arrange
            const date = new  Date(1752, 11, 31); 
            // act
            const result = validationService.dateValidation.minDateSQL(date);

            // assert
            expect(result).toBeFalsy();
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
            expect(result).toBeTruthy();
        });

        it("minDateSQL - fails", () => {
            // arrange
            createValidationSpies(false, true, true, true, true);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });

        it("wrongFormat - fails", () => {
            // arrange
            createValidationSpies(true, false, true, true, true);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });

        it("minDate - fails", () => {
            // arrange
            createValidationSpies(true, true, false, true, true);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });

        it("maxDate - fails", () => {
            // arrange
            createValidationSpies(true, true, true, false, true);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });

        it("hasValueIfRequired - fails", () => {
            // arrange
            createValidationSpies(true, true, true, true, false);
            // act
            const result = validationService.dateValidation.isValid(null, null, null, null, null);
            // assert
            expect(result).toBeFalsy();
        });
    });
});
