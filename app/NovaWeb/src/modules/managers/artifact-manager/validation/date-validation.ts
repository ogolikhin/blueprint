import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IBaseValidation, BaseValidation} from "./base-validation";

export interface IDateValidation extends IBaseValidation {
    baseSQLDate: Date;
    wrongFormat(value: string): boolean;
    minDateSQL(value: string | Date): boolean;
    minDate(value: string | Date, minDate: Date, isValidated: boolean): boolean;
    maxDate(value: string | Date, maxDate: Date, isValidated: boolean): boolean;
    isValid(value: string | Date, minDate: Date, maxDate: Date, isValidated: boolean, isRequired: boolean): boolean;
}

export class DateValidation extends BaseValidation implements IDateValidation {
    public baseSQLDate = new Date(1753, 0, 1);

    constructor(private localization: ILocalizationService) {
        super();
    };

    private convert(value: string | Date): Date {
        if (_.isDate(value)) {
            return value;
        }
        return this.localization.current.toDate(value, true, this.localization.current.shortDateFormat);
    }

    public wrongFormat(value: string | Date): boolean {

        if (!value) {
            return true;
        }

        if (_.isDate(value)) {
            return true;
        }
        return this.localization.current.isValidDate(value);
    }

    public minDateSQL(value: string | Date): boolean {
        return this.minDate(value, this.baseSQLDate, true);
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
        return this.hasValueIfRequired(isRequired, value) &&
               this.wrongFormat(value) &&
               this.minDateSQL(value) &&
               this.minDate(value, minDate, isValidated) &&
               this.maxDate(value, maxDate, isValidated);
    }

}
