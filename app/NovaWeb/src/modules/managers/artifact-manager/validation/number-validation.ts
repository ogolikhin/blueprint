import { ILocalizationService } from "../../../core/localization/localizationService";
import { IBaseValidation, BaseValidation } from "./base-validation";

export interface INumberValidation extends IBaseValidation {
    decimalPlaces(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        localization: ILocalizationService,
        isValidated: boolean): boolean;
    wrongFormat(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        localization: ILocalizationService,
        isValidated: boolean): boolean;
    isMin(newValue: number,
        oldValue: number,
        min: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean;
    isMax(newValue: number,
        oldValue: number,
        max: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean;
    isValid(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        localization: ILocalizationService,
        _min: any,
        _max: any,
        isValidated: boolean,
        isRequired: boolean): boolean;
}

export class NumberValidation extends BaseValidation implements INumberValidation {

    public decimalPlaces(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }
        const value = oldValue || newValue;
        if (value) {
            let decimal = value.toString().split(localization.current.decimalSeparator);
            if (decimal.length === 2) {
                return decimal[1].length <= decimalPlaces;
            }
        }
        return true;
    }

    public wrongFormat(newValue: number,
        oldValue: number,
        decimalPlaces: number,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        const value = oldValue || newValue;
        return !value || angular.isNumber(localization.current.toNumber(value, isValidated ? decimalPlaces : null));
    }

    public isMax(newValue: number,
        oldValue: number,
        max: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }
        const maxNum = localization.current.toNumber(max);
        if (!_.isNull(maxNum)) {
            let value = localization.current.toNumber(oldValue || newValue);
            if (angular.isNumber(value)) {
                return value <= maxNum;
            }
        }
        return true;
    }

    public isMin(newValue: number,
        oldValue: number,
        min: any,
        localization: ILocalizationService,
        isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }
        const minNum = localization.current.toNumber(min);
        if (!_.isNull(minNum)) {
            let value = localization.current.toNumber(oldValue || newValue);
            if (angular.isNumber(value)) {
                return value >= minNum;
            }
        }
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
        return this.decimalPlaces(newValue, oldValue, decimalPlaces, localization, isValidated) &&
            this.wrongFormat(newValue, oldValue, decimalPlaces, localization, isValidated) &&
            this.isMin(newValue, oldValue, _min, localization, isValidated) &&
            this.isMax(newValue, oldValue, _max, localization, isValidated) &&
            super.hasValueIfRequred(isRequired, newValue, oldValue);
    }
}
