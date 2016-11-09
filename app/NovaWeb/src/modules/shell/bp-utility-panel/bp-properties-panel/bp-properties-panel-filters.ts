import {Models} from "../../../main";
import {ILocalizationService} from "../../../core/localization/localizationService";

export class PropertyEditorFilters {

    constructor(private localization: ILocalizationService) {

    }

    public getPropertyEditorFilters(itemTypePredefined: Models.ItemTypePredefined): {[id: string]: boolean} {
        let propertyFilters: {[id: string]: boolean} = {};
        if (itemTypePredefined === Models.ItemTypePredefined.PROShape) {
            propertyFilters[Models.PropertyTypePredefined.X] = true;
            propertyFilters[Models.PropertyTypePredefined.Y] = true;
            propertyFilters[Models.PropertyTypePredefined.Width] = true;
            propertyFilters[Models.PropertyTypePredefined.Height] = true;
            propertyFilters[Models.PropertyTypePredefined.Label] = true;
            propertyFilters[Models.PropertyTypePredefined.Name] = true;
        }

        return propertyFilters;
    }
}
