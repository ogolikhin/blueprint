import { IBaseValidation, BaseValidation } from "./base-validation";

export interface ISelectValidation extends IBaseValidation {
}

export class SelectValidation extends BaseValidation implements ISelectValidation {
}

export interface IMultiSelectValidation extends IBaseValidation {
}

export class MultiSelectValidation extends BaseValidation implements IMultiSelectValidation {
    public hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any) {
        return isRequired ?
                    angular.isArray(newValue) && newValue.length !== 0 ||
                    angular.isArray(oldValue) && oldValue.length !== 0
                    : true;
    }
}
