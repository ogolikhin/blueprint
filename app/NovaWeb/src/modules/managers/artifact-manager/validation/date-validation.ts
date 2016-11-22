import {ILocalizationService} from "../../../core/localization/localizationService";
import {IBaseValidation, BaseValidation} from "./base-validation";

export interface IDateValidation extends IBaseValidation {
    
    wrongFormat(newValue: string): boolean;
    minSQLDate(newValue: string): boolean;
    minDate(newValue: string,
            minDate: any,
            isValidated: boolean): boolean;
    maxDate(newValue: string,
            maxDate: any,
            isValidated: boolean): boolean;
    isValid(newValue: string,
            minDate: any,
            maxDate: any,
            isValidated: boolean,
            isRequired: boolean): boolean;
}

export class DateValidation extends BaseValidation implements IDateValidation {

    constructor(private localization: ILocalizationService) {
        super();
    };

    public wrongFormat(newValue: string): boolean {
            return this.localization.current.isValidDate(newValue);
        }
    public minSQLDate(newValue: string): boolean {
        const minsql = new Date(1753, 1, 1);  
        if (newValue) {
            return this.minDate(newValue, minsql, true);
        }
        return true;

    }
    public minDate(newValue: string,
                   _minDate: any,
                   isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        const date = this.localization.current.toDate(newValue, true);
        const minDateValue = this.localization.current.toDate(_minDate, true);

        if (date && minDateValue) {
            return date.getTime() >= minDateValue.getTime();
        }
        return true;
    }

    public maxDate(newValue: string,
                   maxDate: any,
                   isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        const date = this.localization.current.toDate(newValue, true);
        const maxDateValue = this.localization.current.toDate(maxDate, true);

        if (date && maxDateValue) {
            return date.getTime() <= maxDateValue.getTime();
        }

        return true;
    }

    public isValid(newValue: string,
        minDate: any,
        maxDate: any,
        isValidated: boolean,
        isRequired: boolean): boolean {
        return this.wrongFormat(newValue) &&
               this.minSQLDate(newValue) &&
               this.minDate(newValue, maxDate, isValidated) &&
               this.maxDate(newValue, maxDate, isValidated) &&
               super.hasValueIfRequired(isRequired, newValue, isValidated);
    }

}
