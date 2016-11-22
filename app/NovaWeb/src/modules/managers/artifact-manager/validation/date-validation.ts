import {ILocalizationService} from "../../../core/localization/localizationService";
import {IBaseValidation, BaseValidation} from "./base-validation";

export interface IDateValidation extends IBaseValidation {
    
    wrongFormat(newValue: string | Date): boolean;
    minDate(newValue: string,
            oldValue: string,
            minDate: any,
            isValidated: boolean): boolean;
    maxDate(newValue: string,
            oldValue: string,
            maxDate: any,
            isValidated: boolean): boolean;
    isValid(newValue: string,
            oldValue: string,
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

    public minDate(newValue: string,
                   oldValue: string,
                   _minDate: any,
                   isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        const date = this.localization.current.toDate(oldValue || newValue, true);
        const minDateValue = this.localization.current.toDate(_minDate, true);

        if (date && minDateValue) {
            return date.getTime() >= minDateValue.getTime();
        }
        return true;
    }

    public maxDate(newValue: string,
                   oldValue: string,
                   maxDate: any,
                   isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        const date = this.localization.current.toDate(oldValue || newValue, true);
        const maxDateValue = this.localization.current.toDate(maxDate, true);

        if (date && maxDateValue) {
            return date.getTime() <= maxDateValue.getTime();
        }

        return true;
    }

    public isValid(newValue: string,
        oldValue: string,
        minDate: any,
        maxDate: any,
        isValidated: boolean,
        isRequired: boolean): boolean {
        return this.wrongFormat(newValue) &&
            this.maxDate(newValue, oldValue, maxDate, isValidated) &&
            this.minDate(newValue, oldValue, minDate, isValidated) &&
            super.hasValueIfRequired(isRequired, newValue, oldValue, isValidated);
    }

}
