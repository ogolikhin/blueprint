import { Models} from "../../..";


export interface IArtifactEditor {
    getFields(): Models.IArtifactDetailFields;
    getModel(): any;
}



export class ArtifactEditor implements IArtifactEditor {
    private _artifact: Models.IArtifact;
    private _project: Models.IProject;

    private fields: AngularFormly.IFieldConfigurationObject[] = [];
    private _model: any = {};

    constructor(artifact: Models.IArtifact, project: Models.IProject) {

        if (!artifact || !project) {
            return;
        }
        this._artifact = artifact;
        this._project = project;
        this.getArtifctPropertyTypes().forEach((it: Models.IPropertyType) => {
            this.fields.push(this.createField(it));
        });
    }


    public getFields(): Models.IArtifactDetailFields {
        let fields: Models.IArtifactDetailFields = <Models.IArtifactDetailFields>{
            systemFields: [],
            customFields: [],
            noteFields: []
        };

        this.fields.forEach((it: AngularFormly.IFieldConfigurationObject) => {
            //this._model[it.key] = this._atrifact[]

            if ("system" === it.data["group"]) {
                fields.systemFields.push(it);
            }
            else if ("tabbed" === it.data["group"]) {
                fields.noteFields.push(it);
            } else {
                fields.customFields.push(it);
            }
        });

        return fields;
    }

    public getModel(): any {
        return this._model;
    }


    private createField(propertyType: Models.IPropertyType, name?: string): AngularFormly.IFieldConfigurationObject {
        let field: AngularFormly.IFieldConfigurationObject = {
            key: function () {
                if (name) {
                    return name;
                }
                if (angular.isDefined(propertyType.id))
                    return `property_${propertyType.id}`;
                else if (angular.isDefined(propertyType.propertyTypePredefined)) {
                    return `property_${String(Models.PropertyTypePredefined[propertyType.propertyTypePredefined]).toLowerCase()}`;
                }
                return undefined;
            } (),
            templateOptions: {
                label: propertyType.name,
                required: propertyType.isRequired,
                disabled: propertyType.disabled
            },
            expressionProperties: {},
            data: {
                group: function () {
                    var t = String(Models.PropertyTypePredefined[propertyType.propertyTypePredefined]).toLowerCase();
                    if (["name", "itemtype", "createdby", "createdon", "lasteditedby", "lasteditedon"].indexOf(t) >= 0)
                        return "system";
                    else if (propertyType.isRichText) {
                        return "tabbed"
                    }
                    return "custom";
                } (),
                propertyType : propertyType
            }
        }
        //        this.data = this.propertyType;
        switch (propertyType.primitiveType) {
            case Models.PrimitiveType.Text:
                field.type = propertyType.isRichText ? "tinymceInline" : (propertyType.isMultipleAllowed ? "textarea" : "input");
                field.defaultValue = propertyType.stringDefaultValue;
                //field.templateOptions.minlength;
                //field.templateOptions.maxlength;
                break;
            case Models.PrimitiveType.Date:
                field.type = "datepicker";
                field.templateOptions.type = "text";
                field.templateOptions["datepickerPopup"] = "dd-MMMM-yyyy";
                field.defaultValue = propertyType.dateDefaultValue || new Date();
                field.templateOptions["datepickerOptions"] = {
                    maxDate: propertyType.maxDate,
                    minDate: propertyType.minDate
                };
                //field.templateOptions.max = type.maxDate;
                break;
            case Models.PrimitiveType.Number:
                field.type = "input";
                field.templateOptions.type = "number";
                field.defaultValue = propertyType.decimalDefaultValue;
                field.templateOptions.min = propertyType.minNumber;
                field.templateOptions.max = propertyType.maxNumber;
                break;
            case Models.PrimitiveType.Choice:
                field.type = "select";
                field.defaultValue = (propertyType.defaultValidValueIndex || 0).toString();
                if (propertyType.validValues) {
                    field.templateOptions.options = propertyType.validValues.map(function (it, index) {
                        return <AngularFormly.ISelectOption>{ value: index.toString(), name: it };
                    });
                }
                break;
        }



        return field;


            

    } 


    private getArtifctPropertyTypes(): Models.IPropertyType[] {
        let properties: Models.IPropertyType[] = [];
        
        let _artifactType: Models.IItemType;
        //add custom properties
        if (this._artifact.predefinedType === Models.ItemTypePredefined.Project) {
            _artifactType = <Models.IItemType>{
                name: Models.ItemTypePredefined[Models.ItemTypePredefined.Project],
                baseType: Models.ItemTypePredefined.Project,
                customPropertyTypeIds: []
            }
        } else {
            _artifactType = this._project.meta.artifactTypes.filter((it: Models.IItemType) => {
                return it.id === this._artifact.itemTypeId;
            })[0];
        }
        
        
        //add system properties  
        properties.push(<Models.IPropertyType>{
            name: "Name",
            propertyTypePredefined: Models.PropertyTypePredefined.Name,
            primitiveType: Models.PrimitiveType.Text,
            isRequired: true
        });
        properties.push(<Models.IPropertyType>{
            name: "Type",
            propertyTypePredefined: Models.PropertyTypePredefined.ItemType,
            primitiveType: Models.PrimitiveType.Choice,
            validValues: function (meta: Models.IProjectMeta) {
                if (_artifactType.baseType === Models.ItemTypePredefined.Project)
                    return [_artifactType];
                return meta.artifactTypes.filter((it: Models.IItemType) => {
                    return (_artifactType && (_artifactType.baseType === it.baseType));
                })
            } (this._project.meta).map(function (it) {
                return it.name;
            }),
            isRequired: true
        });

        properties.push(<Models.IPropertyType>{
            name: "Created by",
            propertyTypePredefined: Models.PropertyTypePredefined.CreatedBy,
            primitiveType: Models.PrimitiveType.Text,
            disabled: true
        });
        properties.push(<Models.IPropertyType>{
            name: "Created on",
            propertyTypePredefined: Models.PropertyTypePredefined.CreatedOn,
            primitiveType: Models.PrimitiveType.Date,
            isRequired: true,
            maxDate: new Date("2016-07-31"),
            minDate: new Date("2016-07-01")
            //disabled: true
        });
        properties.push(<Models.IPropertyType>{
            name: "Last edited by",
            propertyTypePredefined: Models.PropertyTypePredefined.LastEditedBy,
            primitiveType: Models.PrimitiveType.Text,
            disabled: true
        });
        properties.push(<Models.IPropertyType>{
            name: "Last edited on",
            propertyTypePredefined: Models.PropertyTypePredefined.LastEditedOn,
            primitiveType: Models.PrimitiveType.Date,
            disabled: true
        });
        properties.push(<Models.IPropertyType>{
            name: "Description",
            propertyTypePredefined: Models.PropertyTypePredefined.Description,
            primitiveType: Models.PrimitiveType.Text,
            isRichText: true
        });
        //custom properties
        this._project.meta.propertyTypes.forEach((it: Models.IPropertyType) => {
            if (_artifactType.customPropertyTypeIds.indexOf(it.id) >= 0) {
                properties.push(it);
            }
        });
        return properties;
    }

}

