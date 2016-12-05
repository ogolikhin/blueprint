export interface IBaseValidation  {
    hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any, isValidated?: boolean, itAllowsCustomValues?: boolean);
}

export class BaseValidation implements IBaseValidation {
    public hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any,
                              isValidated: boolean = true, itAllowsCustomValues: boolean = false) {
        if (!isValidated) {
            return true;
        }

        if (isRequired) {
            return !!newValue || !!oldValue;
        } else {
            return true;
        }
    }
}
