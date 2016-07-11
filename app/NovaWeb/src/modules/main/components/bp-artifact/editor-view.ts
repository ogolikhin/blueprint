import { Models} from "../..";


export interface IArtifactEditor {
    //create(artifact: Models.IArtifact, metadata: Models.IProjectMeta);
}


class FieldType implements AngularFormly.IFieldConfigurationObject {
    public key: string;
    public type: string;
    public defaultValue: any;
    public predefinedType: Models.PropertyTypePredefined;
    public propertyType: Models.IPropertyType;
    public templateOptions: AngularFormly.ITemplateOptions;
    public expressionProperties: any;

    public get group(): string {
        if (["name", "type", "createdby","createdon", "lasteditedby","laseditedon"].indexOf(this.key) >= 0)
            return "system";
        else if (this.propertyType.isRichText) {
            return "tabbed"
        }
        return "custom";
    }


    constructor(name: string, value: any, propertyType?: Models.IPropertyType) {
        this.key = name;
        this.templateOptions = {};
        this.expressionProperties = {};
        this.propertyType = propertyType;
            //if (typeof name === "string") {
        //    this.name = name;
        //    if (!this.predefinedType) {
        //        this.predefinedType = Models.PropertyTypePredefined[name];
        //    }
        //} else {
        //    this.id = <number>name;
        //    this.name = Models.PropertyTypePredefined[predefinedType] || `property_${name}`;
        //}

        this.create();
        this.defaultValue = value !== null ? value : this.defaultValue;
    }
    private create(): void {
        this.templateOptions = {
            label: this.propertyType.name,
            required: this.propertyType.isRequired,
            disabled: this.propertyType.disabled
        };
//        this.data = this.propertyType;
        switch (this.propertyType.primitiveType) {
            case Models.PrimitiveType.Text:
                this.type = this.propertyType.isRichText ? "tinymceInline" : (this.propertyType.isMultipleAllowed ? "textarea" : "input");
                this.defaultValue = this.propertyType.stringDefaultValue;
                //field.templateOptions.minlength;
                //field.templateOptions.maxlength;
                break;
            case Models.PrimitiveType.Date:
                this.type = "input";
                this.templateOptions.type = "date";
                this.defaultValue = this.propertyType.dateDefaultValue;
                //field.templateOptions.min = type.minDate;
                //field.templateOptions.max = type.maxDate;
                break;
            case Models.PrimitiveType.Number:
                this.type = "input";
                this.templateOptions.type = "number";
                this.defaultValue = this.propertyType.decimalDefaultValue;
                this.templateOptions.min = this.propertyType.minNumber;
                this.templateOptions.max = this.propertyType.maxNumber;
                break;
            case Models.PrimitiveType.Choice:
                this.type = "select";
                this.defaultValue = (this.propertyType.defaultValidValueIndex || 0).toString();
                if (this.propertyType.validValues) {
                    this.templateOptions.options = this.propertyType.validValues.map(function (it, index) {
                        return <AngularFormly.ISelectOption>{ value: index.toString(), name: it };
                    });
                }
                break;
            default:
                return undefined;
        }
    }


}


export class ArtifactEditor implements IArtifactEditor {
    private _artifact: Models.IArtifact;
    private _project: Models.IProject;
    private artifactType: Models.IItemType;
    private _model: any = {};
    private _fields: FieldType[] = [];

    constructor(artifact: Models.IArtifact, propertyTypes: Models.IPropertyType[]) {

        if (!artifact || !propertyTypes || propertyTypes.length=== 0) {
            throw new Error("#Project_NotFound");
        }
        this._artifact = artifact;
        let field: FieldType;
        propertyTypes.forEach((it: Models.IPropertyType) => {
            this._fields.push(field = new FieldType(it.key, artifact[it.key], it));
            this._model[field.key] = field.defaultValue;
        });
        //if (artifact.predefinedType === Models.ItemTypePredefined.Project) {
        //    this.artifactType = <Models.IItemType>{
        //        id: -1,
        //        name: Models.ItemTypePredefined[Models.ItemTypePredefined.Project],
        //        baseType: Models.ItemTypePredefined.Project
        //    }
        //} else {
        //    this.artifactType = project.meta.artifactTypes.filter((it: Models.IItemType) => {
        //        return it.id === artifact.itemTypeId;
        //    })[0];
        //}


    }

    public getFields(): Models.IArtifactDetailFields {
        let fields: Models.IArtifactDetailFields = <Models.IArtifactDetailFields>{
            systemFields: [],
            customFields: [],
            noteFields: []
        };

        this.createFields().forEach((it: FieldType) => {
            if ("system" === it.group) {
                fields.systemFields.push(it);
            }
            else if ("tabbed" === it.group) {
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

    private getPropertyType(id: number | string): Models.IPropertyType {
        let _propertyType: Models.IPropertyType;
        if (typeof id === "number") {
            _propertyType = this._project.meta.propertyTypes.filter((it: Models.IPropertyType) => {
                return it.id === id;
            })[0];
        } else {
            let t = Models.PropertyTypePredefined[id];
            switch (t) {
                case Models.PropertyTypePredefined.name:
                    _propertyType = <Models.IPropertyType>{
                        name: "Name",
                        primitiveType: Models.PrimitiveType.Text,
                        disabled: true
                    }
                    break;
                case Models.PropertyTypePredefined.createdby:
                    _propertyType = <Models.IPropertyType>{
                        name: "Created by",
                        primitiveType: Models.PrimitiveType.Text,
                        disabled: true
                    }
                    break;
                case Models.PropertyTypePredefined.createdon:
                    _propertyType = <Models.IPropertyType>{
                        name: "Created on",
                        primitiveType: Models.PrimitiveType.Date,
                        disabled: true
                    }
                    break;
                case Models.PropertyTypePredefined.lasteditedby:
                    _propertyType = <Models.IPropertyType>{
                        name: "Created by",
                        primitiveType: Models.PrimitiveType.Text,
                        disabled: true
                    }
                    break;
                case Models.PropertyTypePredefined.lasteditedon:
                    _propertyType = <Models.IPropertyType>{
                        name: "Last edited by",
                        primitiveType: Models.PrimitiveType.Date,
                        disabled: true
                    }
                    break;
                case Models.PropertyTypePredefined.createdby:
                    _propertyType = <Models.IPropertyType>{
                        name: "Last edited on",
                        primitiveType: Models.PrimitiveType.Text,
                        disabled: true
                    }
                    break;
                case Models.PropertyTypePredefined.description:
                    _propertyType = <Models.IPropertyType>{
                        name: "Description",
                        primitiveType: Models.PrimitiveType.Text,
                        disabled: true
                    }
                    break;
            }
        }
        return _propertyType;
    }

    private createFields(): FieldType[] {
        let fields: FieldType[] = [];
        let field: FieldType;
        let key: any;

        for (key in this._artifact) {
            switch (key.toLowerCase()) {

                case "properyvalues":
                    <Models.IPropertyValue>this._artifact[key].forEach((it: Models.IPropertyValue) => {
                        let propertyType = this.getPropertyType(it.propertyTypeId);
                        let predefined = Models.PropertyTypePredefined[key];
                        fields.push(field = new FieldType(predefined, it.value, propertyType));
                        this._model[field.key] = field.defaultValue;
                    });
                    break;

                default:
//                    fields.push(new FieldType(key, artifact[key], Models.PropertyTypePredefined[key]))
                    let propertyType = this.getPropertyType(key);
                    if (propertyType) {
                        fields.push(field = new FieldType(key, this._artifact[key], propertyType));
                        this._model[field.key] = field.defaultValue;
                    }
                    break;


            }
        };
        return fields;
    }


//    public getArtifactPropertyFileds(artifact: Models.IArtifact): Models.IArtifactDetailFields {
//        try {
////            let t = Models.PropertyTypePredefined[Models.PropertyTypePredefined.almintegrationsettings];

//            let field: AngularFormly.IFieldConfigurationObject;

//            fields.systemFields.push(this.createField("name", <Models.IPropertyType>{
//                id: -1,
//                name: "Name",
//                primitiveType: Models.PrimitiveType.Text,
//                isRequired: true
//            }));
//            fields.systemFields.push(field = this.createField("typeId", <Models.IPropertyType>{
//                id: -1,
//                name: "Type",
//                primitiveType: Models.PrimitiveType.Choice,
//                isRequired: true
//            }));

//            field.templateOptions.options = artifactType ? [<AngularFormly.ISelectOption>{ value: artifactType.baseType.toString(), name: artifactType.name }] : [];
//            field.templateOptions.options = field.templateOptions.options.concat(project.meta.artifactTypes.filter((it: Models.IItemType) => {
//                return (artifactType && (artifactType.baseType === it.baseType));
//            }).map(function (it) {
//                return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.name };
//            }));

//            field.expressionProperties = {
//                "templateOptions.disabled": "to.options.length < 2",
//            };

//            fields.systemFields.push(this.createField("createdBy", <Models.IPropertyType>{
//                name: "Created by",
//                primitiveType: Models.PrimitiveType.Text,
//                disabled: true
//            }));
//            fields.systemFields.push(this.createField("createdOn", <Models.IPropertyType>{
//                name: "Created on",
//                primitiveType: Models.PrimitiveType.Date,
//                disabled: true
//            }));

//            fields.systemFields.push(this.createField("lastEditedBy", <Models.IPropertyType>{
//                name: "Last edited by",
//                primitiveType: Models.PrimitiveType.Text,
//                disabled: true
//            }));
//            fields.systemFields.push(this.createField("lastEditedOn", <Models.IPropertyType>{
//                name: "Last edited on",
//                primitiveType: Models.PrimitiveType.Date,
//                disabled: true
//            }));

//            fields.noteFields.push(this.createField("description", <Models.IPropertyType>{
//                name: "Description",
//                primitiveType: Models.PrimitiveType.Text,
//                isRichText: true
//            }));

//            if (artifactType) {
//                project.meta.propertyTypes.map((it: Models.IPropertyType) => {
//                    if ((artifactType.customPropertyTypeIds || []).indexOf(it.id) >= 0) {
//                        field = this.createField(`property_${it}`, it);
//                        if (field) {
//                            if (it.isRichText) {
//                                fields.noteFields.push(field);
//                            } else {
//                                fields.customFields.push(field);
//                            }
//                        }
//                    }
//                });
//            }


//            return fields;


//        } catch (ex) {
//            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
//        }

//    }





}

