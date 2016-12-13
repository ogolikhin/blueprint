import {ILocalizationService} from "../../../core/localization/localizationService";
import {IBaseValidation, BaseValidation} from "./base-validation";

export interface INumberValidation extends IBaseValidation {
    decimalPlaces(newValue: number,
                  oldValue: number,
                  decimalPlaces: number,
                  isValidated: boolean): boolean;
    wrongFormat(newValue: number,
                oldValue: number,
                decimalPlaces: number,
                isValidated: boolean): boolean;
    isMin(newValue: number,
          oldValue: number,
          min: any,
          isValidated: boolean): boolean;
    isMax(newValue: number,
          oldValue: number,
          max: any,
          isValidated: boolean): boolean;
    isValid(newValue: number,
            oldValue: number,
            decimalPlaces: number,
            _min: any,
            _max: any,
            isValidated: boolean,
            isRequired: boolean): boolean;
}

export class NumberValidation extends BaseValidation implements INumberValidation {

    constructor(private localization: ILocalizationService) {
        super();
    };

    public decimalPlaces(newValue: number,
                         oldValue: number,
                         decimalPlaces: number,
                         isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }
        const value = oldValue || newValue;
        if (value) {
            let decimal = value.toString().split(this.localization.current.decimalSeparator);
            if (decimal.length === 2) {
                return decimal[1].length <= decimalPlaces;
            }
        }
        return true;
    }

    public wrongFormat(newValue: number,
                       oldValue: number,
                       decimalPlaces: number,
                       isValidated: boolean): boolean {
                  
        const value = oldValue || newValue;
        const isNumber = angular.isNumber(this.localization.current.toNumber(value, isValidated ? decimalPlaces : null));
        
        return !value || isNumber;
    }

    public isMax(newValue: number,
                 oldValue: number,
                 max: any,
                 isValidated: boolean): boolean {
        if (!isValidated || _.isUndefined(max)) {
            return true;
        }
        const maxNum = this.localization.current.toNumber(max);
        if (!_.isNull(maxNum)) {
            let value = this.localization.current.toNumber(oldValue || newValue);
            if (angular.isNumber(value)) {
                return value <= maxNum;
            }
        }
        return true;
    }

    public isMin(newValue: number,
                 oldValue: number,
                 min: any,
                 isValidated: boolean): boolean {
        if (!isValidated || _.isUndefined(min)) {
            return true;
        }
        const minNum = this.localization.current.toNumber(min);
        if (!_.isNull(minNum)) {
            let value = this.localization.current.toNumber(oldValue || newValue);
            if (angular.isNumber(value)) {
                return value >= minNum;
            }
        }
        return true;
    }

    public isValid(newValue: number,
                   oldValue: number,
                   decimalPlaces: number,
                   _min: any,
                   _max: any,
                   isValidated: boolean,
                   isRequired: boolean): boolean {
        return this.hasValueIfRequired(isRequired, newValue) &&
               this.decimalPlaces(newValue, oldValue, decimalPlaces, isValidated) &&
               this.wrongFormat(newValue, oldValue, decimalPlaces, isValidated) &&
               this.isMin(newValue, oldValue, _min, isValidated) &&
            this.isMax(newValue, oldValue, _max, isValidated);
    }
}
