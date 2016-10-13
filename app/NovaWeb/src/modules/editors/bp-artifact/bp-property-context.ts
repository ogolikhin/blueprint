import * as angular from "angular";
import {Helper} from "../../shared";
import {Enums, Models} from "../../main";


export class PropertyContext implements Models.IPropertyType {
    public id: number;
    public versionId: number;
    public name: string;
    public primitiveType: Models.PrimitiveType;
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
    public validValues: Models.IOption[];
    public defaultValidValueId: number;
    public propertyTypePredefined: Models.PropertyTypePredefined;
    public disabled: boolean;
    //extension
    public fieldPropertyName: string;
    public modelPropertyName: string | number;
    public lookup: Enums.PropertyLookupEnum;


    constructor(type: Models.IPropertyType) {
        angular.extend(this, type);
        let propertyTypeName: string = Helper.toCamelCase(String(Models.PropertyTypePredefined[this.propertyTypePredefined]));
        if (this.isSystem(this.propertyTypePredefined)) {
            this.lookup = Enums.PropertyLookupEnum.System;
            this.fieldPropertyName = propertyTypeName;
            this.modelPropertyName = propertyTypeName;
        } else if (angular.isUndefined(this.propertyTypePredefined) && angular.isNumber(this.id)) {
            this.lookup = Enums.PropertyLookupEnum.Custom;
            this.fieldPropertyName = `${Enums.PropertyLookupEnum[this.lookup]}_${this.id.toString()}`;
            this.modelPropertyName = this.id;
        } else {
            this.lookup = Enums.PropertyLookupEnum.Special;
            this.fieldPropertyName = propertyTypeName;
            this.modelPropertyName = this.propertyTypePredefined;
        }
    }

    private isSystem(type: Models.PropertyTypePredefined): boolean {
        return [Models.PropertyTypePredefined.Name,
                Models.PropertyTypePredefined.ItemTypeId,
                Models.PropertyTypePredefined.CreatedBy,
                Models.PropertyTypePredefined.CreatedOn,
                Models.PropertyTypePredefined.LastEditedBy,
                Models.PropertyTypePredefined.LastEditedOn,
                Models.PropertyTypePredefined.Description].indexOf(type) >= 0;
    }

}

