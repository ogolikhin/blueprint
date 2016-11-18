export interface IBaseValidation  {
    hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any);
}

export class BaseValidation implements IBaseValidation {
    public hasValueIfRequired(isRequired: boolean, newValue: any, oldValue: any) {
        return isRequired ?
                    !!newValue ||
                    !!oldValue
                    : true;
    }
}
