import { IBaseValidation, BaseValidation } from "./base-validation";

export interface IUserPickerValidation extends IBaseValidation {
}

export class UserPickerValidation extends BaseValidation implements IUserPickerValidation {
    public hasValueIfRequred(isRequired: boolean, newValue: any, oldValue: any) {
        return isRequired ? 
                    angular.isArray(newValue) && newValue.length !== 0 ||  
                    angular.isArray(oldValue) && oldValue.length !== 0   
                    : true;
    }
}