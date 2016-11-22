import {IBaseValidation, BaseValidation} from "./base-validation";

export interface IUserPickerValidation extends IBaseValidation {
}

export class UserPickerValidation extends BaseValidation implements IUserPickerValidation {
    public hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any, isValidated: boolean = true) {
        if (!isValidated) {
            return true;
        }

        if (isRequired) {
            return (_.isObject(newValue) && !( _.isArray(newValue) && newValue.length === 0)) ||
                (_.isObject(oldValue) && !(_.isArray(oldValue) && oldValue.length === 0));
        } else {
            return true;
        }
    }
}
