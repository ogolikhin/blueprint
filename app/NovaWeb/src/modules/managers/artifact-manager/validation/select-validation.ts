import { IBaseValidation, BaseValidation } from "./base-validation";

export interface ISelectValidation extends IBaseValidation {
}

export class SelectValidation extends BaseValidation implements ISelectValidation {
    public hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any, isValidated: boolean = true) {
        if (!isValidated) {
            return true;
        }

        if (isRequired) { 
            return !(_.isUndefined(newValue) || _.isNull(newValue) || 
                        _.isUndefined(oldValue) || _.isNull(oldValue));
        } else {
            return true;                
        }
    }
}

export interface IMultiSelectValidation extends IBaseValidation {
}

export class MultiSelectValidation extends BaseValidation implements IMultiSelectValidation {
    public hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any, isValidated: boolean = true) {
        if (!isValidated) {
            return true;
        }

        if (isRequired) { 
            return (_.isObject(newValue) && !(_.isArray(newValue) && newValue.length === 0)) ||
                      (_.isObject(oldValue) && !(_.isArray(oldValue) && oldValue.length === 0)); 
        } else {
            return true;                
        }
    }
}
