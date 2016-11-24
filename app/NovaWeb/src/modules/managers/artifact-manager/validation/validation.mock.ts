import {IValidationService} from "./validation.svc";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {INumberValidation, NumberValidation} from "./number-validation";
import {IDateValidation, DateValidation} from "./date-validation";
import {ISelectValidation, SelectValidation, IMultiSelectValidation, MultiSelectValidation} from "./select-validation";
import {ITextValidation, TextValidation, ITextRtfValidation, TextRtfValidation} from "./text-validation";
import {IUserPickerValidation, UserPickerValidation} from "./user-picker-validation";
import {BaseValidation} from "./base-validation";
import {ISystemValidation, SystemValidation} from "./system-validation";

export class ValidationServiceMock implements IValidationService {
    public numberValidation: INumberValidation;
    public dateValidation: IDateValidation;
    public selectValidation: ISelectValidation;
    public multiSelectValidation: IMultiSelectValidation;
    public textRtfValidation: ITextRtfValidation;
    public userPickerValidation: IUserPickerValidation;
    public textValidation: ITextValidation;
    public systemValidation: ISystemValidation;

    public static $inject = [ "localization"];

    constructor(private localization: ILocalizationService) {
        this.numberValidation = new NumberValidationMock(localization);
        this.dateValidation = new DateValidationMock(localization);
        this.selectValidation = new SelectValidationMock();
        this.multiSelectValidation = new MultiSelectValidationMock();
        this.textRtfValidation = new TextRtfValidationMock();
        this.userPickerValidation = new UserPickerValidationMock();
        this.textValidation = new TextValidationMock();
        this.systemValidation = new SystemValidationMock();
    }
}

class NumberValidationMock extends BaseValidation implements INumberValidation {

    constructor(private localization: ILocalizationService) {
        super();
    };

    public decimalPlaces(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        isValidated: boolean): boolean {
        return true;
    }

    public wrongFormat(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        isValidated: boolean): boolean {
        return true;
    }

    public isMax(newValue: number,
        oldValue: number,
        _max: any,
        isValidated: boolean): boolean {
        return true;
    }

    public isMin(newValue: number,
        oldValue: number,
        _min: any,
        isValidated: boolean): boolean {
        return true;
    }

    public isValid(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        _min: any,
        _max: any,
        isValidated: boolean,
        isRequired: boolean): boolean {
        return true;
    }
}

class DateValidationMock extends BaseValidation implements IDateValidation {

    constructor(private localization: ILocalizationService) {
        super();
    };

    public wrongFormat(newValue): boolean {
        return true;
    }
    public minSQLDate(newValue): boolean {
        return true;
    }
    public minDate(newValue: string,
        _minDate: any,
        isValidated: boolean): boolean {
        return true;
    }

    public maxDate(newValue: string,
        _maxDate: any,
        isValidated: boolean): boolean {
        return true;
    }

    public isValid(newValue: string,
        _minDate: any,
        _maxDate: any,
        isValidated: boolean,
        isRequired: boolean): boolean {
        return true;
    }
}

class SelectValidationMock extends BaseValidation implements ISelectValidation {
    hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any) {
        return true;
    }
}

class MultiSelectValidationMock extends BaseValidation implements IMultiSelectValidation {
    hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any) {
        return true;
    }
}

class TextRtfValidationMock extends BaseValidation implements ITextRtfValidation {
    hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any) {
        return true;
    }
}

class UserPickerValidationMock extends BaseValidation implements IUserPickerValidation {
    hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any) {
        return true;
    }
}

class TextValidationMock extends BaseValidation implements ITextValidation {
    hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any) {
        return true;
    }
}

class SystemValidationMock extends BaseValidation implements ISystemValidation {
    validateName(nameValue: string) {
        return true;
    }
}
