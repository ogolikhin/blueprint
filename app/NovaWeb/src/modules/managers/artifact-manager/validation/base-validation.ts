export interface IBaseValidation  {
    hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any, isValidated?: boolean);
}

export class BaseValidation implements IBaseValidation {
    public hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any, isValidated: boolean = true) {
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
