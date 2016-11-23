import {Helper} from "../../../shared";
import {IBaseValidation, BaseValidation} from "./base-validation";

export interface ISystemValidation extends IBaseValidation {
    validateName(nameValue: string);
}

export class SystemValidation extends BaseValidation implements ISystemValidation {
    public validateName(nameValue: string) {
        if (!_.isString(nameValue) || nameValue.trim().length === 0 ) {
            return false;
        } else {
            return true;
        }
    }
}