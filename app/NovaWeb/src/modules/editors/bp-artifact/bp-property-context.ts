import * as angular from "angular";
import {Helper} from "../../shared";
import {PropertyTypePredefined, PropertyLookupEnum} from "../../main/models/enums";
import {IPropertyType, IPropertyValue, IOption, PrimitiveType} from "../../main/models/models";

export class PropertyContext implements IPropertyType {
    public id: number;
    public versionId: number;
    public name: string;
    public primitiveType: PrimitiveType;
    public instancePropertyTypeId: number;
    public isRichText: boolean;
    public decimalDefaultValue: number;
    public userGroupDefaultValue: any[];
    public stringDefaultValue: string;
    public decimalPlaces: number;
    public maxNumber: number;
    public minNumber: number;
    public dateDefaultValue: string;
    public maxDate: string;
    public minDate: string;
    public isMultipleAllowed: boolean;
    public isRequired: boolean;
    public isValidated: boolean;
    public validValues: IOption[];
    public defaultValidValueId: number;
    public propertyTypePredefined: PropertyTypePredefined;
    public disabled: boolean;
    //extension
    public fieldPropertyName: string;
    public modelPropertyName: string | number;
    public lookup: PropertyLookupEnum;
    public isFresh?: boolean;

    public static createFromPropertyType(type: IPropertyType) {
        const propertyContext = new PropertyContext();
        angular.extend(propertyContext, type);
        propertyContext.init();
        return propertyContext;
    }

    public static createFromPropertyValue(propertyValue: IPropertyValue) {
        const propertyContext = new PropertyContext();
        propertyContext.id = propertyValue.propertyTypeId;
        propertyContext.propertyTypePredefined = propertyValue.propertyTypePredefined;
        propertyContext.name = propertyValue["name"];
        propertyContext.primitiveType = propertyValue["primitiveType"];
        propertyContext.isMultipleAllowed = propertyValue.isMultipleAllowed;
        if (propertyContext.primitiveType === PrimitiveType.Text) {
            propertyContext.isRichText = propertyValue.isRichText;
        }
        else if (propertyContext.primitiveType === PrimitiveType.Choice && propertyValue.value) {
            propertyContext.isMultipleAllowed = propertyValue.value["isMultipleAllowed"];
            if (propertyValue.value["validValues"]) {
                propertyContext.validValues = propertyValue.value["validValues"];
            }
        }
        propertyContext.init();
        return propertyContext;
    }

    private init() {
        let propertyTypeName: string = _.camelCase(String(PropertyTypePredefined[this.propertyTypePredefined]));
        if (this.isSystem(this.propertyTypePredefined)) {
            this.lookup = PropertyLookupEnum.System;
            this.fieldPropertyName = propertyTypeName;
            this.modelPropertyName = propertyTypeName;
        } else if (this.propertyTypePredefined === PropertyTypePredefined.CustomGroup) {
            this.lookup = PropertyLookupEnum.Custom;
            this.fieldPropertyName = `${PropertyLookupEnum[this.lookup]}_${this.id.toString()}`;
            this.modelPropertyName = this.id;
        } else {
            this.lookup = PropertyLookupEnum.Special;
            this.fieldPropertyName = propertyTypeName;
            this.modelPropertyName = this.propertyTypePredefined;
        }
    }

    private isSystem(type: PropertyTypePredefined): boolean {
        return [PropertyTypePredefined.Name,
                PropertyTypePredefined.ItemTypeId,
                PropertyTypePredefined.CreatedBy,
                PropertyTypePredefined.CreatedOn,
                PropertyTypePredefined.LastEditedBy,
                PropertyTypePredefined.LastEditedOn,
                PropertyTypePredefined.Description].indexOf(type) >= 0;
    }

}
