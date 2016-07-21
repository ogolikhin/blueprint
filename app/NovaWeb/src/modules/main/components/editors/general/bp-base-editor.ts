import {Models} from "../../../";
import {IMessageService} from "../../../../shell/";
import {IArtifactService} from "../../../services/";
import {tinymceMentionsData} from "../../../../util/tinymce-mentions.mock"; //TODO: added just for testing

export interface IEditorContext {
    artifact?: Models.IArtifact;
    project?: Models.IProject;
    type?: Models.IItemType;
    propertyTypes?: Models.IPropertyType[];
}

export class BpBaseEditor {
    public static $inject: [string] = [
        "messageService",
        "artifactService"
    ];

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: IPropertyEditor;
    public context: IEditorContext;

    public readOnly: boolean = false;

    constructor(
        public messageService: IMessageService,
        public artifactService: IArtifactService) {
    }

    public $onChanges(changesObj) {
        try {
            this.context = changesObj.context ? changesObj.context.currentValue : null;
            this.fields = null;
            this.model = null;
            if (this.context && this.context.artifact && this.context.propertyTypes) {
                this.contextLoading(this.context);
            }
        } catch (ex) {
            this.messageService.addError(ex.message);
        }
    }

    public contextLoading(context: IEditorContext)
    {
        this.contextLoaded(context);
    }

    public onPropertyChange($viewValue, $modelValue, scope) {
    };

    public isReadOnly($viewValue, $modelValue, scope): boolean {
        return this.readOnly;
    };

    public contextLoaded(context: IEditorContext) {
        let fieldContexts = context.propertyTypes.map((it: Models.IPropertyType) => {
            switch (it.propertyTypePredefined) {
                case Models.PropertyTypePredefined.Name:
                    return new FieldContext(it, "name");
                case Models.PropertyTypePredefined.ItemType:
                    return new FieldContext(it, "itemTypeId");
                default:
                    return new FieldContext(it);
            }
        });


        this.editor = new PropertyEditor(context.artifact, fieldContexts);
        this.model = this.editor.getModel();
        this.fields = this.editor.getFields().map((it: AngularFormly.IFieldConfigurationObject) => {
            //add property change handler to each field
            angular.extend(it.templateOptions, {
                onChange: this.onPropertyChange.bind(this)
            });
            angular.extend(it.expressionProperties, {
                "templateOptions.disabled": this.isReadOnly.bind(this)
            });
            return it;
        });
    }
}


enum LookupEnum {
    ByName = 0,
    ById = 1,
    ByType = 2,
}

export interface IPropertyEditor {
    getFields(): AngularFormly.IFieldConfigurationObject[];
    getModel(): any;
    destroy(): void;
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
    public isSystem: boolean;
    public fieldPropertyName: string;
    public modelPropertyName: string | number;
    public lookup: LookupEnum;

    constructor(type: Models.IPropertyType, modelPropertyName?: string) {
        angular.extend(this, type);

        let txtType: string = String(Models.PropertyTypePredefined[this.propertyTypePredefined]);
        if (modelPropertyName) {
            this.lookup = LookupEnum.ByName;
            this.fieldPropertyName = modelPropertyName;
            this.modelPropertyName = modelPropertyName;
        } else if (angular.isDefined(type.propertyTypePredefined)) {
            this.lookup = LookupEnum.ByType;
            this.fieldPropertyName = txtType.toLowerCase();
            this.modelPropertyName = <number>this.propertyTypePredefined;
        } else {
            this.lookup = LookupEnum.ById;
            this.fieldPropertyName = `property_${this.id.toString()}`;
            this.modelPropertyName = this.id;
        }
        this.isSystem = ["name", "itemtype", "createdby", "createdon", "lasteditedby", "lasteditedon"].indexOf(txtType.toLowerCase()) >= 0;
        //}
    }


}

export class PropertyEditor implements IPropertyEditor {
    private _artifact: Models.IArtifact;
    private _project: Models.IProject;

    private _fields: AngularFormly.IFieldConfigurationObject[];
    private _model: any = {};

    constructor(artifact: Models.IArtifact, filedContexts: FieldContext[]) {

        if (!artifact || !filedContexts) {
            return;
        }
        this._artifact = artifact;
        this._fields = [];

        filedContexts.forEach((it: FieldContext) => {
            if (it.fieldPropertyName && it.modelPropertyName) {
                let field = this.createPropertyField(it);
                let value: any;

                //Get property valie 
                if (it.lookup === LookupEnum.ByName) {
                    value = angular.isDefined(this._artifact[it.modelPropertyName]) ? this._artifact[it.modelPropertyName] : undefined;
                } else {
                    let propertyValue = (this._artifact.propertyValues || []).filter((value) => {
                        return it.lookup === LookupEnum.ById ?
                            value.propertyTypeId === <number>it.modelPropertyName :
                            value.propertyTypePredefined === <number>it.modelPropertyName;
                    })[0];
                    value = propertyValue ? propertyValue.value : undefined;
                }
                
                //create model property
                if (angular.isDefined(value)) {
                    if (it.primitiveType === Models.PrimitiveType.Date) {
                        value = new Date(value);
                    } else if (it.primitiveType === Models.PrimitiveType.Choice) {
                        if (value.validValueIds) {
                            value = value.validValueIds[0];  // Temporary user only one value for single select
                        }
                        value = value.toString();

                    } else if (it.primitiveType === Models.PrimitiveType.User) {
                        if (value.userGroups) {
                            value = value.map((val: any) => {
                                return val.id;
                            })[0];
                        }
                        value = value.toString();
                    }
                    this._model[it.fieldPropertyName] = value;
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

        return this._fields || [];
    }

    public getModel(): any {
        return this._model || {};
    }

    private createPropertyField(context: FieldContext): AngularFormly.IFieldConfigurationObject {

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
                }                break;
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
