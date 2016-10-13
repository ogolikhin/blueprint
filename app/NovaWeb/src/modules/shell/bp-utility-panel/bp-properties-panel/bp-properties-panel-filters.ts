import {Models} from "../../../main";
import {ILocalizationService} from "../../../core";

export class PropertyEditorFilters {

    constructor(private localization: ILocalizationService) {

    }

    public getPropertyEditorFilters(itemTypePredefined: Models.ItemTypePredefined): {[id: string]: boolean} {
        let propertyFilters: {[id: string]: boolean} = {};
        if (itemTypePredefined === Models.ItemTypePredefined.PROShape) {
            propertyFilters[this.localization.get("Label_X")] = true;
            propertyFilters[this.localization.get("Label_Y")] = true;
            propertyFilters[this.localization.get("Label_Width")] = true;
            propertyFilters[this.localization.get("Label_Height")] = true;
            propertyFilters[this.localization.get("Label_Label")] = true;
            propertyFilters[this.localization.get("Label_Name")] = true;
            propertyFilters[this.localization.get("Label_Description")] = true;
        }

        return propertyFilters;
    }
}
