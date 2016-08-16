import { IMessageService, IStateManager, IPropertyChangeSet, ILocalizationService, BPLocale, ItemState } from "../../core";
import { Helper } from "../../shared";
import { Enums, Models, IWindowManager, IMainWindow } from "../../main";
import { IProjectManager} from "../../main";

import { tinymceMentionsData} from "../../util/tinymce-mentions.mock"; //TODO: added just for testing

export { ILocalizationService, IProjectManager, IMessageService, IStateManager, IWindowManager, Models, Enums }

export class BpBaseEditor {
    public static $inject: [string] = ["localization", "messageService", "stateManager", "windowManager", "projectManager"];

    private _subscribers: Rx.IDisposable[];
    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: IPropertyEditor;
    public context: Models.IEditorContext;
    public artifactState: ItemState;

    public isLoading: boolean = true;

    constructor(
        public localization: ILocalizationService,
        public messageService: IMessageService,
        public stateManager: IStateManager,
        public windowManager: IWindowManager,
        private projectManager: IProjectManager
    ) {
        this.editor = new PropertyEditor(this.localization.current);
    }

    public $onInit() {
        this._subscribers = [
            this.windowManager.mainWindow.subscribeOnNext(this.setArtifactEditorLabelsWidth, this)
        ];
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
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });

        if (this.editor) {
            this.editor.destroy();
        }
        delete this.editor;
        delete this.context;
        delete this.artifactState;
        delete this.fields;
        delete this.model;
    }

    public onLoading(obj: any): boolean  {
        this.fields = [];
        let result = !!(this.context && angular.isDefined(this.context.artifact));
        return result;
    }


    public onLoad(context: Models.IEditorContext) {
        this.onUpdate(context);
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        this.fields.push(field);
    }

    public onUpdate(context: Models.IEditorContext) {
        try {
            this.isLoading = false;
            if (!context || !this.editor) {
                return;
            }
            let artifact: Models.IArtifact;
            this.artifactState = this.stateManager.getState(context.artifact.id);

            if (this.artifactState) {
                artifact = this.artifactState.changedItem || this.artifactState.originItem;
            } else {
                artifact = this.context.artifact;
            }
            

            let fieldContexts = this.projectManager.getArtifactPropertyTypes(artifact).map((it: Models.IPropertyType) => {
                return new PropertyContext(it);
            });

            this.editor.load(artifact, fieldContexts);
            this.model = this.editor.getModel();
            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                //add property change handler to each field
                angular.extend(field.templateOptions, {
                    onChange: this.onValueChange.bind(this)
                });

                this.onFieldUpdate(field);

                if (this.artifactState && this.artifactState.isReadOnly) {
                    field.type = "bpFieldReadOnly";
                }
                if (this.artifactState && this.artifactState.isLocked) {
                    field.type = "bpFieldReadOnly";
                }



            });
        } catch (ex) {
            this.messageService.addError(ex);
        }

        this.setArtifactEditorLabelsWidth();
    }

    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: AngularFormly.ITemplateScope) {
        //here we need to update original model
        let context = $field.data as PropertyContext;
        if (!context) {
            return;
        }
        let value = this.editor.convertToModelValue($field, $value);
        let changeSet: IPropertyChangeSet = {
            lookup: context.lookup,
            id: context.modelPropertyName,
            value: value
        };
        this.stateManager.addChange(this.context.artifact, changeSet);
    };

    public setArtifactEditorLabelsWidth(mainWindow?: IMainWindow) {
        // MUST match $property-width in styles/partials/_properties.scss plus various padding/margin
        const minimumWidth: number = 392 + ((20 + 1 + 15 + 1 + 10) * 2);

        let pageBodyWrapper = document.querySelector(".page-body-wrapper") as HTMLElement;
        if (pageBodyWrapper) {
            let avaliableWidth: number = mainWindow ? mainWindow.contentWidth : pageBodyWrapper.offsetWidth;

            if (avaliableWidth < minimumWidth) {
                pageBodyWrapper.classList.add("single-column-property");
            } else {
                pageBodyWrapper.classList.remove("single-column-property");
            }
        }
    };
}

export interface IPropertyEditor {
    load(artifact: Models.IArtifact, properties: PropertyContext[]);
    destroy(): void;

    getFields(): AngularFormly.IFieldConfigurationObject[];
    getModel(): any;

    convertToModelValue(field: AngularFormly.IFieldConfigurationObject, $value: any): any;
    convertToFieldValue(field: AngularFormly.IFieldConfigurationObject, $value: any): string | number | Date;

}

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
        }
        //} else {
        //    this.lookup = LookupEnum.Special;
        //    this.fieldPropertyName = `${LookupEnum[this.lookup]}_${this.id.toString()}`;
        //    this.modelPropertyName = this.id;
        //}
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

    private _model: any;
    private _fields: AngularFormly.IFieldConfigurationObject[];

    constructor(private locale: BPLocale) { }

    public convertToModelValue(field: AngularFormly.IFieldConfigurationObject, $value: any): any {
        if (!field) {
            return null;
        }
        let context = field.data as PropertyContext;
        if (!context || angular.isUndefined($value)) {
            return null;
        }

        switch (context.primitiveType) {
            case Models.PrimitiveType.Number:
                return this.locale.toNumber($value);

            case Models.PrimitiveType.Date:
                return this.locale.toDate($value);

            case Models.PrimitiveType.Choice:
                if (angular.isArray($value)) {
                    return {
                        validValueIds : $value.map((it) => { return this.locale.toNumber(it); })
                    };
                }
                return this.locale.toNumber($value);

            case Models.PrimitiveType.User:
                //TODO: please implement on time of user editor field implementation
                return $value;

            default:
                return $value;
        }
    }

    public convertToFieldValue(field: AngularFormly.IFieldConfigurationObject, $value: any): string | number | Date { 
        let context = field.data as PropertyContext;
        if (!context || angular.isUndefined($value) || $value === null || angular.equals({}, $value)) {
            return null;
        }

        //create internal property value
        if (context.primitiveType === Models.PrimitiveType.Number) {
            return this.locale.toNumber($value);

        } else if (context.primitiveType === Models.PrimitiveType.Date) {
            return this.locale.toDate($value);

        } else if (context.primitiveType === Models.PrimitiveType.Choice) {
            if (angular.isArray($value.validValueIds)) {
                let values = $value.validValueIds.map((v: number) => {
                    return v;
                });
                return context.isMultipleAllowed ? values : values[0];
            } else if (angular.isNumber($value)) {
                return $value;
            }
        } else if (context.primitiveType === Models.PrimitiveType.User) {
            //TODO: must be changed when  a field editor for this type of property is created
            if ($value.usersGroups) {
                return $value.usersGroups.map((val: Models.IUserGroup) => {
                    return val.displayName;
                }).join(", ");
            } else if ($value.displayName) {
                return $value.displayName;
            } else if ($value.label) {
                return $value.label;
            } else {
                return $value.toString();
            }
        } 
        return $value;
    }

    public load(artifact: Models.IArtifact, properties: PropertyContext[]) {

        this._model = {};
        this._fields = [];

        if (artifact && angular.isArray(properties)) {

            properties.forEach((propertyContext: PropertyContext) => {
                if (propertyContext.fieldPropertyName && propertyContext.modelPropertyName) {
                    let modelValue: any;

                    //Get property value 
                    if (propertyContext.lookup === Enums.PropertyLookupEnum.System) {
                        if (angular.isDefined(artifact[propertyContext.modelPropertyName])) {
                            modelValue = artifact[propertyContext.modelPropertyName] || null;
                        }

                    } else if (propertyContext.lookup === Enums.PropertyLookupEnum.Custom && angular.isArray(artifact.customPropertyValues)) {
                        modelValue = artifact.customPropertyValues.filter((value) => {
                            return value.propertyTypeId === propertyContext.modelPropertyName as number;
                        })[0];
                        if (modelValue) {
                            modelValue = modelValue.value || null;
                        } 
                    } else if (propertyContext.lookup === Enums.PropertyLookupEnum.Special && angular.isArray(artifact.specificPropertyValues)) {
                        modelValue = artifact.specificPropertyValues.filter((value) => {
                            return value.propertyTypeId === propertyContext.modelPropertyName as number;
                        })[0];
                        if (modelValue) {
                            modelValue = modelValue.value || null;
                        } 
                    }
                    if (angular.isDefined(modelValue)) {
                        let field = this.createPropertyField(propertyContext);
                        this._model[propertyContext.fieldPropertyName] = this.convertToFieldValue(field, modelValue);
                        this._fields.push(field);
                    }
                }
            });
        }
    }

    public destroy() {
        delete this._fields;
        delete this._model;
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
        
        if ([Models.PropertyTypePredefined.CreatedBy,
            Models.PropertyTypePredefined.CreatedOn,
            Models.PropertyTypePredefined.LastEditedBy,
            Models.PropertyTypePredefined.LastEditedOn].indexOf(context.propertyTypePredefined) >= 0) {
            field.type = "bpFieldReadOnly";

        } else {
            switch (context.primitiveType) {
                case Models.PrimitiveType.Text:
                    field.type = context.isRichText ? "bpFieldInlineTinymce" : (context.isMultipleAllowed ? "bpFieldTextMulti" : "bpFieldText");
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
                    field.type = "bpFieldDatepicker";
                    field.templateOptions["datepickerOptions"] = {
                        maxDate: context.maxDate,
                        minDate: context.minDate
                    };

                    field.defaultValue = context.dateDefaultValue;
                    break;
                case Models.PrimitiveType.Number:
                    field.type = "bpFieldNumber";
                    field.defaultValue = this.locale.toNumber(context.decimalDefaultValue);
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
                    if (context.isMultipleAllowed) {
                        field.type = "bpFieldSelectMulti";
                        field.templateOptions["optionsAttr"] = "bs-options";
                    } else {
                        field.type = "bpFieldSelect";
                    }
                    field.templateOptions.options = [];
                    if (context.validValues && context.validValues.length) {
                        field.templateOptions.options = context.validValues.map(function (it) {
                            return { value: it.id, name: it.value } as any;
                        });
                        if (angular.isNumber(context.defaultValidValueId)) {
                            field.defaultValue = context.defaultValidValueId.toString();
                        }
                    }
                    break;
                case Models.PrimitiveType.User:
                    //TODO needs to be changed to user selection
                    field.type = "bpFieldReadOnly"; 
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

        }
        return field;
    }

}
