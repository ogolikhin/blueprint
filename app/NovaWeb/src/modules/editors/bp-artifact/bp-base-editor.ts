import * as moment from "moment";
import { IMessageService, IStateManager, IPropertyChangeSet, IWindowResize, ILocalizationService } from "../../core";
import { Helper } from "../../shared";
import { Enums, Models, ISidebarToggle } from "../../main"
import { IProjectManager} from "../../main";

import { tinymceMentionsData} from "../../util/tinymce-mentions.mock"; //TODO: added just for testing

export { ILocalizationService, IProjectManager, IMessageService, IStateManager, IWindowResize, ISidebarToggle, Models, Enums }

export enum LookupEnum {
    None = 0,
    System = 1,
    Custom = 2,
    Special = 3,
}
export class BpBaseEditor {
    public static $inject: [string] = ["localization", "messageService", "stateManager", "windowResize", "sidebarToggle", "$timeout", "projectManager"];

    private _subscribers: Rx.IDisposable[];
    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: IPropertyEditor;
    public context: Models.IEditorContext;

    public isLoading: boolean = true;

    constructor(
        public localization: ILocalizationService,
        public messageService: IMessageService,
        public stateManager: IStateManager,
        public windowResize: IWindowResize,
        public sidebarToggle: ISidebarToggle,
        private $timeout: ng.ITimeoutService,
        private projectManager: IProjectManager
    ) {
        this.editor = new PropertyEditor(); 
    }

    public $onInit() {
        this._subscribers = [
            this.windowResize.width.subscribeOnNext(this.onWidthResized, this),
            this.sidebarToggle.isConfigurationChanged.subscribeOnNext(this.onWidthResized, this)
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
        delete this.fields;
        delete this.model;
    }

    private onWidthResized() {
        this.setArtifactEditorLabelsWidth();
    }
     
    
     
    public onValueChange($value: any, $model: AngularFormly.IFieldConfigurationObject) {
        //here we need to update original model
        let context = $model.data as PropertyContext;
        if (!context) {
            return;
        }
        let value = context.convertToModelValue($value);
        if ( !this.form.$invalid ) {
            let changeSet: IPropertyChangeSet = {
                lookup: LookupEnum[context.lookup],
                id: context.modelPropertyName,
                value: value
            };
            this.stateManager.addChangeSet(this.context.artifact, changeSet);
        }


    };

    public onLoading(obj: any): boolean  {
        this.fields = [];
        return !!(this.context && angular.isDefined(this.context.artifact));
    }


    public onLoad(context: Models.IEditorContext) {
        this.onUpdate(context);
        var t = this.localization.current;
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
            let state = this.stateManager.getState(context.artifact.id);

            if (state) {
                artifact = state.changedItem;
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

                if ((artifact.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit) {
                    field.type = "bpFieldReadOnly";
                }               

                this.onFieldUpdate(field);


            });
        } catch (ex) {
            this.messageService.addError(ex);
        }

        this.$timeout(() => {
            this.setArtifactEditorLabelsWidth();
        }, 0);
    }

    public setArtifactEditorLabelsWidth() {
        let artifactOverview: Element = document.querySelector(".artifact-overview");
        if (artifactOverview) {
            const propertyWidth: number = 392; // MUST match $property-width in styles/partials/_properties.scss
            let actualWidth: number = artifactOverview.querySelector(".formly") ? artifactOverview.querySelector(".formly").clientWidth : propertyWidth;
            if (actualWidth < propertyWidth) {
                artifactOverview.classList.add("single-column");
            } else {
                artifactOverview.classList.remove("single-column");
            }
        }
    };
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
    public lookup: LookupEnum;

    constructor(type: Models.IPropertyType, specialType?: string) {
        angular.extend(this, type);
        let propertyTypeName: string = Helper.toCamelCase(String(Models.PropertyTypePredefined[this.propertyTypePredefined]));
        if (this.isSystem(this.propertyTypePredefined)) {
            this.lookup = LookupEnum.System;
            this.fieldPropertyName = propertyTypeName;
            this.modelPropertyName = propertyTypeName;
        } else if (angular.isUndefined(this.propertyTypePredefined) && angular.isNumber(this.id)) {
            this.lookup = LookupEnum.Custom;
            this.fieldPropertyName = `${LookupEnum[this.lookup]}_${this.id.toString()}`;
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
    public convertToModelValue($value: any): any {
        if (angular.isDefined($value)) {
            switch (this.primitiveType) {
                case Models.PrimitiveType.Number:
                    if (!angular.isNumber($value)) {
                        if (!$value) {
                            return undefined;
                        }
                        return this.decimalPlaces ? parseFloat($value.toString()) : parseInt($value.toString(), 10);
                    }
                    break;
                case Models.PrimitiveType.Date:
                    if (!angular.isDate($value)) {
                        if (!$value) {
                            return undefined;
                        }
                        return new Date($value);
                    }
                    break;
                case Models.PrimitiveType.Choice:
                    let values = $value.toString().split(",").map((it: string) => {
                        return parseInt(it, 10);
                    });
                    if (values.length > 1) {
                        return {
                            validValueIds: values
                        };
                    } else if (values.length === 1) {
                        return values[0];
                    }
                    return null;
                default:
                    break;
            }
        } 
        return $value;
    }

    public convertToFieldValue($value: any): any {
        let fieldValue: any;
        switch (this.primitiveType) {
            case Enums.PrimitiveType.Date:
                //if (moment($value).isValid()) {
                //    if (this.lookup === LookupEnum.Custom) {
                //        fieldValue = moment($value).startOf("day").format(dateFormat);
                //    } else {
                //        fieldValue = moment($value).format("L") + " " + moment($value).format("LT");
                //    }
                //}
                //} else if ($scope.options.data && $scope.options.data.dateDefaultValue) {
                //    $scope.model[$scope.options.key] = $scope.options.data.dateDefaultValue;
                //}
                break;
            case Enums.PrimitiveType.Number:
                //if (angular.isNumber($value)) {
                //    fieldValue = Helper.toLocaleNumber(currentModelVal.toString());
                //} else if ($scope.options.data && $scope.options.data.decimalDefaultValue) {
                //    $scope.model[$scope.options.key] = Helper.toLocaleNumber($scope.options.data.decimalDefaultValue);
                //}
                break;
            case Enums.PrimitiveType.User:

                //if (currentModelVal) {
                //    $scope.model[$scope.options.key] = currentModelVal;
                //} else if ($scope.options.data && $scope.options.data.decimalDefaultValue) {
                //    $scope.model[$scope.options.key] = $scope.options.data.userGroupDefaultValue;
                //}
                break;
            default:
        }
        return fieldValue;
    }
}

export class PropertyEditor implements IPropertyEditor {
    private _artifact: Models.IArtifact;

    private _fields: AngularFormly.IFieldConfigurationObject[];
    private _model: any = {};

    constructor() {}

    public load(artifact: Models.IArtifact, properties: PropertyContext[]) {

        this._fields = [];
        var $this = this;
        if (artifact && angular.isArray(properties)) {
            this._artifact = artifact;

            properties.forEach((propertyContext: PropertyContext) => {
                if (propertyContext.fieldPropertyName && propertyContext.modelPropertyName) {
                    let field = this.createPropertyField(propertyContext);
                    let value: any;

                    //Get property value 
                    if (propertyContext.lookup === LookupEnum.System) {
                        if (angular.isDefined(this._artifact[propertyContext.modelPropertyName])) {
                            value = this._artifact[propertyContext.modelPropertyName];
                        } else {
                            field.hide = true;
                        }

                    } else if (propertyContext.lookup === LookupEnum.Custom && angular.isArray(this._artifact.customPropertyValues)) {
                        let propertyValue = this._artifact.customPropertyValues.filter((value) => {
                            return value.propertyTypeId === <number>propertyContext.modelPropertyName;
                        })[0];
                        if (propertyValue) {
                            value = propertyValue.value;
                        } else {
                            field.hide = true;
                        }
                    } else if (propertyContext.lookup === LookupEnum.Special && angular.isArray(this._artifact.specificPropertyValues)) {
                        let propertyValue = this._artifact.specificPropertyValues.filter((value) => {
                            return value.propertyTypeId === <number>propertyContext.modelPropertyName;
                        })[0];
                        if (propertyValue) {
                            value = propertyValue.value;
                        } else {
                            field.hide = true;
                        }
                    }
                    if (!field.hide) {
                        if (value && !angular.equals({}, value)) {
                            //create internal model property value
                            if (propertyContext.primitiveType === Models.PrimitiveType.Date) {
                                value = new Date(value);

                            } else if (propertyContext.primitiveType === Models.PrimitiveType.Choice) {
                                if (angular.isArray(value.validValueIds)) {
                                    let values = [];
                                    value.validValueIds.forEach((v: number) => {
                                        values.push(v.toString());
                                    });
                                    value = propertyContext.isMultipleAllowed ? values : values[0];
                                } else {
                                    value = value.toString();
                                }
                            } else if (propertyContext.primitiveType === Models.PrimitiveType.User) {
                                //TODO: must be changed when  a field editor for this type of property is created
                                if (value.usersGroups) {
                                    value = value.usersGroups.map((val: Models.IUserGroup) => {
                                        return val.displayName;
                                    }).join(", ");
                                } else if (value.displayName) {
                                    value = value.displayName;
                                } else if (value.label) {
                                    value = value.label;
                                } else {
                                    value = value.toString();
                                }
                            }
                        }
                        this._model[propertyContext.fieldPropertyName] = value;
                        this._fields.push(field);
                    }
                }
            });
        }
    }

    public destroy() {
        delete this._artifact;
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
                    if (context.isMultipleAllowed) {
                        field.type = "bpFieldSelectMulti";
                        field.templateOptions["optionsAttr"] = "bs-options";
                    } else {
                        field.type = "bpFieldSelect";
                    }
                    field.templateOptions.options = [];
                    if (context.validValues && context.validValues.length) {
                        field.templateOptions.options = context.validValues.map(function (it) {
                            return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.value };
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
