import { Models} from "../../../main";

export class PropertyEditorFilters {

    public static getPropertyEditorFilters(itemTypePredefined: Models.ItemTypePredefined): Models.PropertyTypePredefined[]{
        let propertyTypes: Models.PropertyTypePredefined[] = [];
        if (itemTypePredefined === Models.ItemTypePredefined.PROShape)
        {
            propertyTypes.push(Models.PropertyTypePredefined.X);
            propertyTypes.push(Models.PropertyTypePredefined.Y);
            propertyTypes.push(Models.PropertyTypePredefined.Width);
            propertyTypes.push(Models.PropertyTypePredefined.Height);
            propertyTypes.push(Models.PropertyTypePredefined.Label);
            propertyTypes.push(Models.PropertyTypePredefined.Name);            
            propertyTypes.push(Models.PropertyTypePredefined.Description);
        }
        return propertyTypes;
    }

}