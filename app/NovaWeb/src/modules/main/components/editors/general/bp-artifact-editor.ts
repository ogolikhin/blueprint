
import { Models} from "../../..";

enum LookupEnum {
    ByName = 0,
    ById = 1,
    ByType = 2,
}
export interface IArtifactEditor {
    getFields(): AngularFormly.IFieldConfigurationObject[];
    getModel(): any;
}


export class FieldContext implements Models.IPropertyType {
    public id: number;
    public versionId: number;
    public name: string;
    public primitiveType: Models.PrimitiveType;
    public instancePropertyTypeId: number;
    public isRichText: boolean;
    public decimalDefaultValue: number;
    public dateDefaultValue: Date;
    public userGroupDefaultValue: any[];
    public stringDefaultValue: string;
    public decimalPlaces: number;
    public maxNumber: number;
    public minNumber: number;
    public maxDate: Date;
    public minDate: Date;
    public isMultipleAllowed: boolean;
    public isRequired: boolean;
    public isValidated: boolean;
    public validValues: Models.IOption[];
    public defaultValidValueId: number;
    public propertyTypePredefined: Models.PropertyTypePredefined;
    public disabled: boolean;
    //extension
    public group: string;
    public fieldPropertyName: string;
    public modelPropertyName: string | number;
    public lookup: LookupEnum;

    constructor(type: Models.IPropertyType, modelPropertyName?: string ) {
        angular.extend(this, type);

        let txtType: string = String(Models.PropertyTypePredefined[this.propertyTypePredefined]);
        if (modelPropertyName) {
            this.fieldPropertyName = `property_${modelPropertyName}`;
            this.modelPropertyName = modelPropertyName;
            this.lookup = LookupEnum.ByName;
        } else if (angular.isDefined(type.id)) {
            this.fieldPropertyName = `property_${this.id.toString()}`;
            this.modelPropertyName = this.id;
            this.lookup = LookupEnum.ById;
        } else if (angular.isDefined(type.propertyTypePredefined)) {
            this.fieldPropertyName = `property_${txtType.toLowerCase()}`;
            this.modelPropertyName = <number>this.propertyTypePredefined;
            this.lookup = LookupEnum.ByType;
        }

        if (["name", "itemtype", "createdby", "createdon", "lasteditedby", "lasteditedon"].indexOf(txtType.toLowerCase()) >= 0) {
            this.group = "system";
        } else if (type.isRichText) {
            this.group = "tabbed";
        } else {
            this.group = "custom";
        }
        //}
    }
}

export class PropertyEditor  {
    private _artifact: Models.IArtifact;
    private _project: Models.IProject;

    private _fields: AngularFormly.IFieldConfigurationObject[] = [];
    private _model: any = {};

    constructor(artifact: Models.IArtifact, filedContexts: FieldContext[]) {

        if (!artifact || !filedContexts) {
            return;
        }
        this._artifact = artifact;

//        let types = this.getArtifctPropertyTypes();


        filedContexts.forEach((it: FieldContext) => {
            if (it.fieldPropertyName && it.modelPropertyName) {
                let field = this.createPropertyField(it);
                let value: any;

                if (it.lookup === LookupEnum.ByName) {
                    value = angular.isDefined(this._artifact[it.modelPropertyName]) ? this._artifact[it.modelPropertyName] : undefined;
                } else { 
                    var propertyValue = this._artifact.propertyValues.filter((value) => {
                        return it.lookup === LookupEnum.ById ?
                            value.propertyTypeId === <number>it.modelPropertyName :
                            value.propertyTypePredefined === <number>it.modelPropertyName;
                    })[0];
                    value = propertyValue ? propertyValue.value : undefined;
                    }
                if (angular.isDefined(value)) {
                    this._model[it.fieldPropertyName] = it.primitiveType === Models.PrimitiveType.Choice ? value.toString() : value;
                    }
                this._fields.push(field);
                }
        });
    }

    public destroy() {
        delete this._fields;
        delete this._model;
        delete this._artifact;
        delete this._project;
    }

    public getFields(): AngularFormly.IFieldConfigurationObject[] {

        return this._fields;
    }

    public getModel(): any {
        return this._model || {};
    }

    //private getArtifctPropertyTypes(): Models.IPropertyType[] {
    //    let properties: FieldContext[] = [];

    //    let _artifactType: Models.IItemType;
    //    //add custom properties
    //    if (this._artifact.predefinedType === Models.ItemTypePredefined.Project) {
    //        _artifactType = <Models.IItemType>{
    //            id: Models.ItemTypePredefined.Project,
    //            name: Models.ItemTypePredefined[Models.ItemTypePredefined.Project],
    //            baseType: Models.ItemTypePredefined.Project,
    //            customPropertyTypeIds: []
    //        };
    //    } else {
    //        _artifactType = this._project.meta.artifactTypes.filter((it: Models.IItemType) => {
    //            return it.id === this._artifact.itemTypeId;
    //        })[0];
    //    }
        
        
    //    //add system properties  
    //    properties.push(new FieldContext(<Models.IPropertyType>{
    //        name: "Name",
    //        propertyTypePredefined: Models.PropertyTypePredefined.Name,
    //        primitiveType: Models.PrimitiveType.Text,
    //        isRequired: true
    //    }, "name"));

    //    properties.push(new FieldContext(<Models.IPropertyType>{
    //        name: "Type",
    //        propertyTypePredefined: Models.PropertyTypePredefined.ItemType,
    //        primitiveType: Models.PrimitiveType.Choice,
    //        validValues: function (meta: Models.IProjectMeta) {
    //            if (_artifactType.baseType === Models.ItemTypePredefined.Project) {
    //                return [_artifactType];
    //            }
    //            return meta.artifactTypes.filter((it: Models.IItemType) => {
    //                return (_artifactType && (_artifactType.baseType === it.baseType));
    //            });
    //        } (this._project.meta).map(function (it) {
    //            return <Models.IOption>{
    //                id: it.id,
    //                value: it.name
    //            };
    //        }),
    //        isRequired: true
    //    }, "itemTypeId"));

    //    properties.push(new FieldContext(<Models.IPropertyType>{
    //        name: "Created by",
    //        propertyTypePredefined: Models.PropertyTypePredefined.CreatedBy,
    //        primitiveType: Models.PrimitiveType.Text,
    //        disabled: true
    //    }));
    //    properties.push(new FieldContext(<Models.IPropertyType>{
    //        name: "Created on",
    //        propertyTypePredefined: Models.PropertyTypePredefined.CreatedOn,
    //        primitiveType: Models.PrimitiveType.Date,
    //        // the following are test values, using DateJS
    //        maxDate: new Date(moment(new Date()).add(15, "days").format("YYYY-MM-DD")),
    //        minDate: new Date(moment(new Date()).add(-15, "days").format("YYYY-MM-DD")),
    //        isRequired: true
    //        //disabled: true
    //    }));
    //    properties.push(new FieldContext(<Models.IPropertyType>{
    //        name: "Last edited by",
    //        propertyTypePredefined: Models.PropertyTypePredefined.LastEditedBy,
    //        primitiveType: Models.PrimitiveType.Text,
    //        disabled: true
    //    }));
    //    properties.push(new FieldContext(<Models.IPropertyType>{
    //        name: "Last edited on",
    //        propertyTypePredefined: Models.PropertyTypePredefined.LastEditedOn,
    //        primitiveType: Models.PrimitiveType.Date,
    //        disabled: true
    //    }));
    //    properties.push(new FieldContext(<Models.IPropertyType>{
    //        name: "Description",
    //        propertyTypePredefined: Models.PropertyTypePredefined.Description,
    //        primitiveType: Models.PrimitiveType.Text,
    //        isRichText: true
    //    }));
    //    //custom properties
    //    this._project.meta.propertyTypes.forEach((it: Models.IPropertyType) => {
    //        if (_artifactType.customPropertyTypeIds.indexOf(it.id) >= 0) {
    //            properties.push(new FieldContext(it));
    //        }
    //    });
    //    return properties;
    //}

    private createPropertyField(context: FieldContext): AngularFormly.IFieldConfigurationObject {
        
        let field: AngularFormly.IFieldConfigurationObject = {
            key: context.fieldPropertyName,
            data: context,
            templateOptions: {
                label: context.name,
                required: context.isRequired,
                disabled: context.disabled
            },
            expressionProperties: {}
        };
        //        this.data = this.propertyType;
        switch (context.primitiveType) {
            case Models.PrimitiveType.Text:
                field.type = context.isRichText ? "frmlyInlineTinymce" : (context.isMultipleAllowed ? "textarea" : "input");
                field.defaultValue = context.stringDefaultValue;
                if (context.isRichText) {
                    field.templateOptions["tinymceOption"] = {
                        //fixed_toolbar_container: ".form-tinymce-toolbar." + context.fieldPropertyName
                    };
                }
                break;
            case Models.PrimitiveType.Date:
                field.type = "datepicker";
                field.templateOptions["datepickerOptions"] = {
                    maxDate: context.maxDate,
                    minDate: context.minDate
                };
                field.defaultValue = context.dateDefaultValue || new Date();
                break;
            case Models.PrimitiveType.Number:
                field.type = "frmlyNumber";
                if (angular.isNumber(context.defaultValidValueId)) {
                    field.defaultValue = context.decimalDefaultValue;
                }
                field.templateOptions.min = context.minNumber;
                field.templateOptions.max = context.maxNumber;
                break;
            case Models.PrimitiveType.Choice:
                field.type = "select";
                if (angular.isNumber(context.defaultValidValueId)) {
                    field.defaultValue = context.defaultValidValueId.toString();
                }
                field.templateOptions.options = [];
                if (context.validValues && context.validValues.length) {
                    field.templateOptions.options = context.validValues.map(function (it) {
                        return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.value };
                    });
                }
                break;
            case Models.PrimitiveType.User:
                field.type = "select"; // needs to be changed to user selection
                if (angular.isNumber(context.defaultValidValueId)) {
                    field.defaultValue = context.defaultValidValueId.toString();
                }
                field.templateOptions.options = [];
                if (context.validValues && context.validValues.length) {
                    field.templateOptions.options = context.validValues.map(function (it) {
                        return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.value };
                    });
                }
                break;
            default:
            //case Models.PrimitiveType.Image:
                field.type = "input"; // needs to be changed to image editor
                field.defaultValue = (context.defaultValidValueId || 0).toString();
                field.templateOptions.options = [];
                if (context.validValues) {
                    field.templateOptions.options = context.validValues.map(function (it) {
                        return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.value };
                    });
                }
                break;
        }

        return field;


            

    } 



}

