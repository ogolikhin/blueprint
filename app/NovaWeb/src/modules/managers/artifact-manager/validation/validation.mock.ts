import { IValidationService } from "./validation.svc";
import { INumberValidation } from "./number-validation";
import { IDateValidation } from "./date-validation";
import { ILocalizationService } from "../../../core/localization/localizationService";

export class ValidationServiceMock implements IValidationService {
    public numberValidation: INumberValidation;
    public dateValidation: IDateValidation;

    constructor() {
        this.numberValidation = new NumberValidationMock();
        this.dateValidation = new DateValidationMock();
    }
}

class NumberValidationMock implements INumberValidation {

    public decimalPlaces(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        return true;
    }

    public wrongFormat(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        return true;
    }

    public isMax(newValue: number,
        oldValue: number,
        _max: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        return true;
    }

    public isMin(newValue: number,
        oldValue: number,
        _min: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        return true;
    }

    public isValid(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        localization: ILocalizationService,
        _min: any,
        _max: any,
        isValidated: boolean,
        isRequired: boolean): boolean {
        return true;
    }
}

class DateValidationMock implements IDateValidation {
    public minDate(newValue: string,
        oldValue: string,
        _minDate: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        return true;
    }

    public maxDate(newValue: string,
        oldValue: string,
        _maxDate: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        return true;
    }

    public isValid(newValue: string,
        oldValue: string,
        localization: ILocalizationService,
        _minDate: any,
        _maxDate: any,
        isValidated: boolean,
        isRequired: boolean): boolean {
        return true;
    }
}