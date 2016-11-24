import {ILocalizationService} from "../../../core/localization/localizationService";
import {INumberValidation, NumberValidation} from "./number-validation";
import {IDateValidation, DateValidation} from "./date-validation";
import {ISelectValidation, SelectValidation, IMultiSelectValidation, MultiSelectValidation} from "./select-validation";
import {ITextValidation, TextValidation, ITextRtfValidation, TextRtfValidation} from "./text-validation";
import {IUserPickerValidation, UserPickerValidation} from "./user-picker-validation";
import {ISystemValidation, SystemValidation} from "./system-validation";

export interface IValidationService {
    numberValidation: INumberValidation;
    dateValidation: IDateValidation;
    selectValidation: ISelectValidation;
    multiSelectValidation: IMultiSelectValidation;
    textRtfValidation: ITextRtfValidation;
    userPickerValidation: IUserPickerValidation;
    textValidation: ITextValidation;
    systemValidation: ISystemValidation;
}

export class ValidationService implements IValidationService {
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
        this.numberValidation = new NumberValidation(localization);
        this.dateValidation = new DateValidation(localization);
        this.selectValidation = new SelectValidation();
        this.multiSelectValidation = new MultiSelectValidation();
        this.textRtfValidation = new TextRtfValidation(); 
        this.userPickerValidation = new UserPickerValidation();
        this.textValidation = new TextValidation();
        this.systemValidation = new SystemValidation();
    }

}
