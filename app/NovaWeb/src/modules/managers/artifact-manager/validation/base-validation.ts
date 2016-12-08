export interface IBaseValidation  {
    hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any, isValidated?: boolean, itAllowsCustomValues?: boolean);
}

export class BaseValidation implements IBaseValidation {
    public hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any, isValidated: boolean = true) {
        if (isRequired) {
            return !!newValue || !!oldValue;
        }
        return true;
    }
}
