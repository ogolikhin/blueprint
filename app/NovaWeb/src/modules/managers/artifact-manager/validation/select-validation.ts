import {IBaseValidation, BaseValidation} from "./base-validation";
import {Models} from "../../../main/models";

export interface ISelectValidation extends IBaseValidation {
    checkValue(value: Models.IChoicePropertyValue, isValidated: boolean, validValues: any[]);
    isValid(isRequired: boolean, value: Models.IChoicePropertyValue, isValidated: boolean, validValues: any[]);
}

export class SelectValidation extends BaseValidation implements ISelectValidation {
    
    public hasValueIfRequired(
            isRequired: boolean, value: Models.IChoicePropertyValue) {
        if (!isRequired) {
            return true;
        }
        if (!value) {
            return false;
        }
        if (_.isNumber(value) ) {
            return value > 0;
        }
        return  _.isArray(value.validValues) && value.validValues.length === 1 || !!value.customValue;
    }

    public checkValue(value: Models.IChoicePropertyValue, isValidated: boolean, validValues: any[]) {
        if (!value) {
            return true;
        }

        if (isValidated) {
            if (value.customValue) {
                return false;
            }
            const selectedValues = _.map(value.validValues || [], it => it.id);
            const choiceValues = _.map(validValues || [], it => it.id);

           
            return selectedValues.length === 1 && _.find(choiceValues, it => it === selectedValues[0]);
        }
        return true; 
    }


    public isValid(isRequired: boolean, value: Models.IChoicePropertyValue, isValidated: boolean, validValues: number[]): boolean {
        return this.hasValueIfRequired(isRequired, value) &&
               this.checkValue(value, isValidated, validValues);
    }
}

export interface IMultiSelectValidation extends ISelectValidation {
}

export class MultiSelectValidation extends SelectValidation implements IMultiSelectValidation {
    public hasValueIfRequired(
            isRequired: boolean, value: Models.IChoicePropertyValue) {
        if (!isRequired) {
            return true;
        }
        if (!value) {
            return false;
        }
        if (_.isArray(value) ) {
            return value.length > 0;
        }
        return  _.isArray(value.validValues) && value.validValues.length > 0;
    }

    public checkValue(value: Models.IChoicePropertyValue, isValidated: boolean, validValues: any[]) {
        if (!value) {
            return true;
        }
        const choiceValues = _.map(validValues || [], it => it.id);

        if (isValidated && choiceValues.length) {
            const selectedValues = _.map(value.validValues || [], it => it.id);
            const intersect = _.intersection(choiceValues, selectedValues);
            return intersect.length > 0 && 
                   intersect.length <= choiceValues.length && 
                   selectedValues.length === intersect.length;
        }
        return false; 
    }

}
