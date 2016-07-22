import {Models} from "../../../";
import {Helper} from "../../../../core";
import {IMessageService} from "../../../../shell/";
import {IArtifactService} from "../../../services/";
import {tinymceMentionsData} from "../../../../util/tinymce-mentions.mock"; //TODO: added just for testing

export interface IEditorContext {
    artifact?: Models.IArtifact;
    project?: Models.IProject;
    type?: Models.IItemType;
    propertyTypes?: Models.IPropertyType[];
}

export interface IGroupFields {
    [key: string]: AngularFormly.IFieldConfigurationObject[];
}


export class BpBaseEditor {
    public static $inject: [string] = ["messageService"];

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: IPropertyEditor;
    public context: IEditorContext;

    public readOnly: boolean = false;

    constructor (public messageService: IMessageService) {
        this.editor = new PropertyEditor(); 
    }

    public $onChanges(obj: any) {
        try {
            this.fields = [];
            this.model = {};
            if (this.onLoading(obj)) {
                this.onLoad(this.context);
            }
        } catch (ex) {
            this.messageService.addError(ex.message);
        }
    }

    public $onDestroy() {
        if (this.editor) {
            this.editor.destroy();
        }
        delete this.editor;
        delete this.context;
        delete this.fields;
        delete this.model;
    }


    public isReadOnly($viewValue, $modelValue, scope): boolean {
        return this.readOnly;
    };

    public onPropertyChange($viewValue, $modelValue, scope) {
    };

    public onLoading(obj: any): boolean  {
        this.context = obj.context ? obj.context.currentValue : null;
        return !!(this.context && this.context.artifact && this.context.propertyTypes);
    }

    public onLoad(context: IEditorContext)
    {
        this.onUpdate(context);
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        if (!angular.isArray(this.fields)) {
            this.fields = [];
        }
        this.fields.push(field);
    }

    public onUpdate(context: IEditorContext) {
        let fieldContexts = context.propertyTypes.map((it: Models.IPropertyType) => {
            return new PropertyContext(it);
        });

        this.editor.load(context.artifact, fieldContexts);
        this.model = this.editor.getModel();
        this.editor.getFields().forEach((it: AngularFormly.IFieldConfigurationObject) => {
            //add property change handler to each field
            angular.extend(it.templateOptions, {
                onChange: this.onPropertyChange.bind(this)
            });
            //angular.extend(it.expressionProperties, {
            //    "templateOptions.disabled": this.isReadOnly.bind(this)
            //});

            this.onFieldUpdate(it);

        });
    }
}


export enum LookupEnum {
    System = 0,
    Custom = 1,
    Special = 2,
}

export interface IPropertyEditor {
    load(artifact: Models.IArtifact, properties: PropertyContext[]);
    getFields(): AngularFormly.IFieldConfigurationObject[];
    getModel(): any;
    destroy(): void;
}

export class PropertyContext implements Models.IPropertyType {
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
    public fieldPropertyName: string;
    public modelPropertyName: string | number;
    public lookup: LookupEnum;
    public group: string; 

    constructor(type: Models.IPropertyType, specialType?: string) {
        angular.extend(this, type);
        let propertyTypeName: string = Helper.camelCase(String(Models.PropertyTypePredefined[this.propertyTypePredefined]));
        if (this.isSystem(this.propertyTypePredefined)) {
            this.lookup = LookupEnum.System;
            this.fieldPropertyName = propertyTypeName;
            this.modelPropertyName = propertyTypeName;
        } else if (!specialType && angular.isDefined(this.propertyTypePredefined)) {
            this.lookup = LookupEnum.Custom;
            this.fieldPropertyName = `property_${this.id.toString()}`;
            this.modelPropertyName = this.id;
        } else {
            this.lookup = LookupEnum.Special;
            this.fieldPropertyName = `special_${this.id.toString()}`;
            this.modelPropertyName = this.id;
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

export class PropertyEditor implements IPropertyEditor {
    private _artifact: Models.IArtifact;

    private _fields: AngularFormly.IFieldConfigurationObject[];
    private _model: any = {};

    constructor() {}

    public load(artifact: Models.IArtifact, properties: PropertyContext[]) {

        this._fields = [];

        if (artifact && angular.isArray(properties)) {
            this._artifact = artifact;
            properties.forEach((it: PropertyContext) => {
                if (it.fieldPropertyName && it.modelPropertyName) {
                    let field = this.createPropertyField(it);
                    let value: any;

                    //Get property value 
                    if (it.lookup === LookupEnum.System) {
                        value = angular.isDefined(this._artifact[it.modelPropertyName]) ? this._artifact[it.modelPropertyName] : undefined;
                    } else if (it.lookup === LookupEnum.Custom && angular.isArray(this._artifact.customPropertyValues)) {
                        let propertyValue = this._artifact.customPropertyValues.filter((value) => {
                            return value.propertyTypeId === <number>it.modelPropertyName;
                        })[0];
                        value = propertyValue ? propertyValue.value : undefined;
                    } else if (it.lookup === LookupEnum.Special && angular.isArray(this._artifact.specificPropertyValues)) {
                        let propertyValue = this._artifact.customPropertyValues.filter((value) => {
                            return value.propertyTypeId === <number>it.modelPropertyName;
                        })[0];
                        value = propertyValue ? propertyValue.value : undefined;
                    }
                
                    //create internal model property value
                    if (angular.isDefined(value)) {
                        if (it.primitiveType === Models.PrimitiveType.Date) {
                            value = new Date(value);
                        } else if (it.primitiveType === Models.PrimitiveType.Choice) {
                            if (value.validValueIds) {
                                value = value.validValueIds[0];  // Temporary user only one value for single select
                            }
                            value = value.toString();

                        } else if (it.primitiveType === Models.PrimitiveType.User) {
                            //TODO: must be changed when  a field editor for this type of property is created
                            if (value.userGroups) {
                                value = value.map((val: Models.IUserGroup) => {
                                    return val.displayName;
                                })[0];
                            } else {
                                value = (value as Models.IUserGroup).displayName;
                            }
                            value = value.toString();
                        }
                        this._model[it.fieldPropertyName] = value;
                    }
                    this._fields.push(field);
                }
            });
        } 
    }

    public destroy() {
        delete this._fields;
        delete this._model;
        delete this._artifact;
    }

    public getFields(): AngularFormly.IFieldConfigurationObject[] {

        return this._fields || [];
    }

    public getModel(): any {
        return this._model || {};
    }

    private createPropertyField(context: PropertyContext): AngularFormly.IFieldConfigurationObject {

        let field: AngularFormly.IFieldConfigurationObject = {
            key: context.fieldPropertyName,
            data: context,
            templateOptions: {
                label: context.name,
                required: context.isRequired,
                disabled: context.disabled,
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
                    //TODO: added just for testing
                    if (true) { //here we need something to decide if the tinyMCE editor should have mentions
                        field.templateOptions["tinymceOption"].mentions = {
                            source: tinymceMentionsData,
                            delay: 100,
                            items: 5,
                            queryBy: "fullname",
                            insert: function (item) {
                                return `<a class="mceNonEditable" href="mailto:${item.emailaddress}" title="ID# ${item.id}">${item.fullname}</a>`;
                            }
                        };
                    }
                }
                break;
            case Models.PrimitiveType.Date:
                field.type = "frmlyDatepicker";
                field.templateOptions["datepickerOptions"] = {
                    maxDate: context.maxDate,
                    minDate: context.minDate
                };
                field.defaultValue = context.dateDefaultValue || new Date();
                break;
            case Models.PrimitiveType.Number:
                field.type = "frmlyNumber";
                field.defaultValue = context.decimalDefaultValue;
                if (angular.isNumber(context.minNumber)) {
                    field.templateOptions.min = context.minNumber;
                }
                if (angular.isNumber(context.maxNumber)) {
                    field.templateOptions.max = context.maxNumber;
                }
                if (angular.isNumber(context.decimalPlaces)) {
                    field.templateOptions["decimalPlaces"] = context.decimalPlaces;
                }
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
                field.type = "input"; // needs to be changed to user selection
                //if (angular.isNumber(context.defaultValidValueId)) {
                //    field.defaultValue = context.defaultValidValueId.toString();
                //}
                //field.templateOptions.options = [];
                //if (context.validValues && context.validValues.length) {
                //    field.templateOptions.options = context.validValues.map(function (it) {
                //        return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.value };
                //    });
                //}
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
