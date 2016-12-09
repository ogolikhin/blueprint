import {IBaseValidation, BaseValidation} from "./base-validation";

export interface IUserPickerValidation extends IBaseValidation {
}

export class UserPickerValidation extends BaseValidation implements IUserPickerValidation {
    public hasValueIfRequired(isRequired: boolean, value: any) {
        
        if (isRequired) {
            if (!value || (_.isArray(value) && !value.length)) {
                return false;
            }
        }
        return true;
    }
}
