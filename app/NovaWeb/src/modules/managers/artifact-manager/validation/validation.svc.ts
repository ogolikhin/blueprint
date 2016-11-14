import {ILocalizationService} from "../../../core/localization/localizationService";

export interface IValidationService {
    numberValidation: INumberValidation;
    dateValidation: IDateValidation;
}

export interface INumberValidation {
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
    min(newValue: number, 
          oldValue: number, 
          _min: any, 
          localization: ILocalizationService, 
          isValidated: boolean): boolean;
    max(newValue: number, 
           oldValue: number,
           _max: any, 
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

export interface IDateValidation {
    minDate(newValue: string, 
                    oldValue: string, 
                    _minDate: any, 
                    localization: ILocalizationService, 
                    isValidated: boolean): boolean;
    maxDate(newValue: string, 
                    oldValue: string,
                    _maxDate: any, 
                    localization: ILocalizationService, 
                    isValidated: boolean): boolean;
    isValid(newValue: string, 
                oldValue: string,  
                localization: ILocalizationService, 
                _minDate: any,
                _maxDate: any,  
                isValidated: boolean,
                isRequired: boolean): boolean;
}

export class ValidationService implements IValidationService {
    public numberValidation: INumberValidation;
    public dateValidation: IDateValidation;

    constructor() {
        this.numberValidation = new NumberValidation();
        this.dateValidation = new DateValidation();
    }
}

class NumberValidation implements INumberValidation {
    
    public decimalPlaces(newValue: number, 
                                    oldValue: number,  
                                    decimalPlaces: number, 
                                    localization: ILocalizationService, 
                                    isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }
        let value = oldValue || newValue;
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
        let value = oldValue || newValue;
        return !value || angular.isNumber(localization.current.toNumber(value, isValidated ? decimalPlaces : null));
    }

    public max(newValue: number, 
                    oldValue: number,
                    _max: any, 
                    localization: ILocalizationService, 
                    isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }
        let max = localization.current.toNumber(_max);
        if (angular.isNumber(max)) {
            let value = localization.current.toNumber(oldValue || newValue);
            if (angular.isNumber(value)) {
                return value <= max;
            }
        }
        return true;
    }

    public min(newValue: number, 
                    oldValue: number, 
                    _min: any, 
                    localization: ILocalizationService, 
                    isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }
        let min = localization.current.toNumber(_min);
        if (angular.isNumber(min)) {
            let value = localization.current.toNumber(oldValue || newValue);
            if (angular.isNumber(value)) {
                return value >= min;
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
        return this.decimalPlaces(newValue, oldValue,  decimalPlaces, localization, isValidated) &&
                  this.wrongFormat(newValue, oldValue,  decimalPlaces, localization, isValidated) &&
                  this.min(newValue, oldValue,  _min, localization, isValidated) &&
                  this.max(newValue, oldValue, _max, localization, isValidated) &&
                  (isRequired ? (!!newValue || !!oldValue) : true);
    }
}

class DateValidation implements IDateValidation {
    public minDate(newValue: string, 
                    oldValue: string, 
                    _minDate: any, 
                    localization: ILocalizationService, 
                    isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        let date = localization.current.toDate(oldValue || newValue, true);
        let minDate = localization.current.toDate(_minDate, true);

        if (date && minDate) {
            return date.getTime() >= minDate.getTime();
        }
        return true;
    }

    public maxDate(newValue: string, 
                            oldValue: string,
                            _maxDate: any, 
                            localization: ILocalizationService, 
                            isValidated: boolean): boolean {
        if (!isValidated) {
            return true;
        }

        let date = localization.current.toDate(oldValue || newValue, true);
        let maxDate = localization.current.toDate(_maxDate, true);

        if (date && maxDate) {
            return date.getTime() <= maxDate.getTime();
        }

        return true;
    }

    public isValid(newValue: string, 
                            oldValue: string,  
                            localization: ILocalizationService, 
                            _minDate: any,
                            _maxDate: any,  
                            isValidated: boolean,
                            isRequired: boolean): boolean {
        return this.maxDate(newValue, oldValue, _maxDate, localization, isValidated) &&
                  this.minDate(newValue, oldValue, _minDate, localization, isValidated) &&
                  (isRequired ? (!!newValue || !!oldValue) : true);
    }

}