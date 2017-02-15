import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {Models} from "../../../main";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";

export class PropertyEditorFilters {

    constructor(private localization: ILocalizationService) {

    }

    public getPropertyEditorFilters(itemTypePredefined: ItemTypePredefined): {[id: string]: boolean} {
        let propertyFilters: {[id: string]: boolean} = {};
        if (itemTypePredefined === ItemTypePredefined.PROShape) {
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
