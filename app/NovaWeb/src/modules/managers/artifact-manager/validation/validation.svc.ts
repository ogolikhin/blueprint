import {ILocalizationService} from "../../../core/localization/localizationService";
import {INumberValidation, NumberValidation} from "./number-validation";
import {IDateValidation, DateValidation} from "./date-validation";

export interface IValidationService {
    numberValidation: INumberValidation;
    dateValidation: IDateValidation;
}

export class ValidationService implements IValidationService {
    public numberValidation: INumberValidation;
    public dateValidation: IDateValidation;

    constructor() {
        this.numberValidation = new NumberValidation();
        this.dateValidation = new DateValidation();
    }
}
