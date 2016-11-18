import {IValidationService} from "./validation.svc";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {INumberValidation, NumberValidation} from "./number-validation";
import {IDateValidation, DateValidation} from "./date-validation";
import {ISelectValidation, SelectValidation, IMultiSelectValidation, MultiSelectValidation} from "./select-validation";
import {ITextValidation, TextValidation, ITextRtfValidation, TextRtfValidation} from "./text-validation";
import {IUserPickerValidation, UserPickerValidation} from "./user-picker-validation";
import {BaseValidation} from "./base-validation";

export class ValidationServiceMock implements IValidationService {
    public numberValidation: INumberValidation;
    public dateValidation: IDateValidation;
    public selectValidation: ISelectValidation;
    public multiSelectValidation: IMultiSelectValidation;
    public textRtfValidation: ITextRtfValidation;
    public userPickerValidation: IUserPickerValidation;
    public textValidation: ITextValidation;

    constructor() {
        this.numberValidation = new NumberValidationMock();
        this.dateValidation = new DateValidationMock();
        this.selectValidation = new SelectValidationMock();
        this.multiSelectValidation = new MultiSelectValidationMock();
        this.textRtfValidation = new TextRtfValidationMock();
        this.userPickerValidation = new UserPickerValidationMock();
        this.textValidation = new TextValidationMock();
    }
}

class NumberValidationMock extends BaseValidation implements INumberValidation {

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

class DateValidationMock extends BaseValidation implements IDateValidation {
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
