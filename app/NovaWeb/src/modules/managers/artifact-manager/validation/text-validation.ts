import {Helper} from "../../../shared";
import {IBaseValidation, BaseValidation} from "./base-validation";

export interface ITextValidation extends IBaseValidation {
}

export interface ITextRtfValidation extends IBaseValidation {
}

export class TextValidation extends BaseValidation implements ITextValidation {
}

export class TextRtfValidation extends BaseValidation implements ITextRtfValidation {
    public hasValueIfRequired(isRequired: boolean, value: any) {
        
        if (isRequired) {
            return Helper.tagsContainText(value);
        } 
        return true;
    }
}
