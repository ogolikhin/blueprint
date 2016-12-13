export interface IBaseValidation  {
    hasValueIfRequired(isRequired: boolean, value: any);
}

export class BaseValidation implements IBaseValidation {
    public hasValueIfRequired(isRequired: boolean, value: any) {
        if (isRequired) {
            return !!value;
        }
        return true;
    }
}
