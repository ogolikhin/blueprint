import {ILocalizationService} from "../../../core/localization/localizationService";
import {INumberValidation, NumberValidation} from "./number-validation";
import {IDateValidation, DateValidation} from "./date-validation";
import {ISelectValidation, SelectValidation, IMultiSelectValidation, MultiSelectValidation} from "./select-validation";
import {ITextValidation, TextValidation, ITextRtfValidation, TextRtfValidation} from "./text-validation";
import {IUserPickerValidation, UserPickerValidation} from "./user-picker-validation";
import {IPropertyDescriptorBuilder, IPropertyDescriptor} from "../../../editors/configuration/property-descriptor-builder";

export interface IValidationService {
    numberValidation: INumberValidation;
    dateValidation: IDateValidation;
    selectValidation: ISelectValidation;
    multiSelectValidation: IMultiSelectValidation;
    textRtfValidation: ITextRtfValidation;
    userPickerValidation: IUserPickerValidation;
    textValidation: ITextValidation;
}

export class ValidationService implements IValidationService {
    public static $inject = [
        "propertyDescriptorBuilder"
    ];
    public numberValidation: INumberValidation;
    public dateValidation: IDateValidation;
    public selectValidation: ISelectValidation;
    public multiSelectValidation: IMultiSelectValidation;
    public textRtfValidation: ITextRtfValidation;
    public userPickerValidation: IUserPickerValidation;
    public textValidation: ITextValidation;

    constructor(private propertyDescriptor: IPropertyDescriptorBuilder) {
        this.numberValidation = new NumberValidation();
        this.dateValidation = new DateValidation();
        this.selectValidation = new SelectValidation();
        this.multiSelectValidation = new MultiSelectValidation();
        this.textRtfValidation = new TextRtfValidation(); 
        this.userPickerValidation = new UserPickerValidation();
        this.textValidation = new TextValidation();
    }

}
