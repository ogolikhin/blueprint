export interface IBaseValidation  {
    hasValueIfRequred(isRequired: boolean, newValue: any, oldValue: any); 
}

export class BaseValidation implements IBaseValidation {
    public hasValueIfRequred(isRequired: boolean, newValue: any, oldValue: any) {
        return isRequired ? 
                    !!newValue || 
                    !!oldValue 
                    : true;
    }
}