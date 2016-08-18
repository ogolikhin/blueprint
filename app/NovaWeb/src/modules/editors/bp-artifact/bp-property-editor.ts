import { BPLocale, } from "../../core";
import { Enums, Models} from "../../main";
import { PropertyContext} from "./bp-property-context";

import { tinymceMentionsData} from "../../util/tinymce-mentions.mock"; //TODO: added just for testing

export class PropertyEditor {

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
                        validValueIds: $value.map((it) => { return this.locale.toNumber(it); })
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
            } else if (angular.isString($value.customValue)) {
                return $value.customValue;
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

    public load(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact, properties: PropertyContext[]) {

        this._model = {};
        this._fields = [];

        if (artifact && angular.isArray(properties)) {
            var artifactOrSubArtifact = artifact;
            if (subArtifact) {
                artifactOrSubArtifact = subArtifact;
            }
            properties.forEach((propertyContext: PropertyContext) => {
                if (propertyContext.fieldPropertyName && propertyContext.modelPropertyName) {
                    let modelValue: any = null;

                    if (propertyContext.lookup === Enums.PropertyLookupEnum.System) {
                        //System property
                        if (angular.isDefined(artifactOrSubArtifact[propertyContext.modelPropertyName])) {
                            modelValue = artifactOrSubArtifact[propertyContext.modelPropertyName] || null;
                            if (Models.PropertyTypePredefined.Name === propertyContext.propertyTypePredefined &&
                                artifact.readOnlyReuseSettings &&
                                (artifact.readOnlyReuseSettings & Enums.ReuseSettings.Name) === Enums.ReuseSettings.Name) {
                                propertyContext.disabled = true;

                            } else if (Models.PropertyTypePredefined.Description === propertyContext.propertyTypePredefined &&
                                artifact.readOnlyReuseSettings &&
                                (artifact.readOnlyReuseSettings & Enums.ReuseSettings.Description) === Enums.ReuseSettings.Description) {
                                propertyContext.disabled = true;
                            }
                        } 
                    } else if (propertyContext.lookup === Enums.PropertyLookupEnum.Custom && angular.isArray(artifactOrSubArtifact.customPropertyValues)) {
                        //Custom property
                        let custompropertyvalue = artifactOrSubArtifact.customPropertyValues.filter((value: Models.IPropertyValue) => {
                            return value.propertyTypeId === propertyContext.modelPropertyName as number;
                        })[0];
                        if (custompropertyvalue) {
                            modelValue = custompropertyvalue.value || null;
                            propertyContext.disabled = custompropertyvalue.isReuseReadOnly ? true : propertyContext.disabled;
                        } 
                    } else if (propertyContext.lookup === Enums.PropertyLookupEnum.Special && angular.isArray(artifactOrSubArtifact.specificPropertyValues)) {
                        //Specific property
                        let specificpropertyvalue = artifactOrSubArtifact.specificPropertyValues.filter((value) => {                            
                            return value.propertyTypePredefined === propertyContext.modelPropertyName as number;
                        })[0];
                        if (specificpropertyvalue) {
                            modelValue = specificpropertyvalue.value || null;
                            propertyContext.disabled = specificpropertyvalue.isReuseReadOnly ? true : propertyContext.disabled;
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

    private createPropertyField(context: PropertyContext, reuseSettings?: Enums.ReuseSettings): AngularFormly.IFieldConfigurationObject {

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

        if (Models.PropertyTypePredefined.Name === context.propertyTypePredefined &&
            reuseSettings && (reuseSettings & Enums.ReuseSettings.Name) === Enums.ReuseSettings.Name) {
            field.templateOptions.disabled = true;

        } else if (Models.PropertyTypePredefined.Description === context.propertyTypePredefined &&
            reuseSettings && (reuseSettings & Enums.ReuseSettings.Description) === Enums.ReuseSettings.Description) {
            field.templateOptions.disabled = true;

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
                    field.templateOptions["maxDate"] = this.locale.formatDate(this.locale.toDate(context.maxDate), this.locale.shortDateFormat);
                    field.templateOptions["minDate"] = this.locale.formatDate(this.locale.toDate(context.minDate), this.locale.shortDateFormat);

                    field.defaultValue = context.dateDefaultValue;
                    break;
                case Models.PrimitiveType.Number:
                    field.type = "bpFieldNumber";
                    field.defaultValue = this.locale.toNumber(context.decimalDefaultValue);
                    field.templateOptions.min = this.locale.toNumber(context.minNumber);
                    field.templateOptions.max = this.locale.toNumber(context.maxNumber);
                    field.templateOptions["decimalPlaces"] = this.locale.toNumber(context.decimalPlaces);
                    break;
                case Models.PrimitiveType.Choice:
                    field.type = context.isMultipleAllowed ? "bpFieldSelectMulti" : "bpFieldSelect";
                    field.templateOptions["optionsAttr"] = "bs-options";
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
        if (field.templateOptions.disabled) {
            field.type = "bpFieldReadOnly";
        }
        return field;
    }

}
