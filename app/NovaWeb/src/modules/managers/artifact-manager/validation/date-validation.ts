import { ILocalizationService } from "../../../core/localization/localizationService";

export interface IDateValidation {
    minDate(newValue: string,
        oldValue: string,
        minDate: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean;
    maxDate(newValue: string,
        oldValue: string,
        maxDate: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean;
    isValid(newValue: string,
        oldValue: string,
        localization: ILocalizationService,
        minDate: any,
        maxDate: any,
        isValidated: boolean,
        isRequired: boolean): boolean;
}

export class DateValidation implements IDateValidation {
    public minDate(newValue: string,
        oldValue: string,
        _minDate: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        const date = localization.current.toDate(oldValue || newValue, true);
        const minDateValue = localization.current.toDate(_minDate, true);

        if (date && minDateValue) {
            return date.getTime() >= minDateValue.getTime();
        }
        return true;
    }

    public maxDate(newValue: string,
        oldValue: string,
        maxDate: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        const date = localization.current.toDate(oldValue || newValue, true);
        const maxDateValue = localization.current.toDate(maxDate, true);

        if (date && maxDateValue) {
            return date.getTime() <= maxDateValue.getTime();
        }

        return true;
    }

    public isValid(newValue: string,
        oldValue: string,
        localization: ILocalizationService,
        minDate: any,
        maxDate: any,
        isValidated: boolean,
        isRequired: boolean): boolean {
        return this.maxDate(newValue, oldValue, maxDate, localization, isValidated) &&
            this.minDate(newValue, oldValue, minDate, localization, isValidated) &&
            (isRequired ? (!!newValue || !!oldValue) : true);
    }

}