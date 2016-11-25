import {ILocalizationService} from "../../../core/localization/localizationService";
import {IBaseValidation, BaseValidation} from "./base-validation";

export interface IDateValidation extends IBaseValidation {

    wrongFormat(value: string): boolean;
    minSQLDate(value: string | Date): boolean;
    minDate(value: string | Date, minDate: Date, isValidated: boolean): boolean;
    maxDate(value: string | Date, maxDate: Date, isValidated: boolean): boolean;
    isValid(value: string | Date, minDate: Date, maxDate: Date, isValidated: boolean, isRequired: boolean): boolean;
}

export class DateValidation extends BaseValidation implements IDateValidation {

    constructor(private localization: ILocalizationService) {
        super();
    };

    private convert(value: string | Date): Date {
        if (_.isDate(value)) {
            return value;
        }
        let d = this.localization.current.toDate(value, true, this.localization.current.shortDateFormat);
        return d;
    }

    public wrongFormat(value: string | Date): boolean {
        if (_.isDate(value)) {
            return true;
        }
        return this.localization.current.isValidDate(value);
    }

    public minSQLDate(value: string | Date): boolean {
        const minsqldate = new Date(1753, 0, 1);
        return this.minDate(value, minsqldate, true);
    }

    public minDate(value: string | Date, minDate: Date, isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        const date = this.convert(value);

        if (date && minDate) {
            return date.getTime() >= minDate.getTime();
        }
        return true;
    }

    public maxDate(value: string | Date, maxDate: Date, isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        const date = this.convert(value);

        if (date && maxDate) {
            return date.getTime() <= maxDate.getTime();
        }

        return true;
    }

    public isValid(value: string | Date, minDate: Date, maxDate: Date, isValidated: boolean, isRequired: boolean): boolean {
        return this.wrongFormat(value) &&
               this.minSQLDate(value) &&
               this.minDate(value, minDate, isValidated) &&
               this.maxDate(value, maxDate, isValidated) &&
               super.hasValueIfRequired(isRequired, value, isValidated);
    }

}
