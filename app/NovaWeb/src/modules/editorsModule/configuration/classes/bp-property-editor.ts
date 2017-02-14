import {BPLocale, ILocalizationService} from "../../../commonModule/localization/localization.service";
import {Enums, Models} from "../../../main";
import {ItemTypePredefined} from "../../../main/models/item-type-predefined";
import {IStatefulItem, StatefulSubArtifact} from "../../../managers/artifact-manager";
import {Helper} from "../../../shared/utils/helper";
import {IUserGroup} from "../../configuration/types/user-picker/user-picker";
import {IPropertyDescriptor} from "../../services";

export class PropertyEditor {

    private _model: any;
    private _fields: AngularFormly.IFieldConfigurationObject[];
    public propertyContexts: IPropertyDescriptor[];
    private locale: BPLocale;
    private itemid: number;

    constructor(private localization: ILocalizationService) {
        this.locale = localization.current;
    }

    public convertToModelValue(field: AngularFormly.IFieldConfigurationObject): any {
        if (!field) {
            return null;
        }

        const context = field.data as IPropertyDescriptor;
        if (!context) {
            return null;
        }

        const $modelValue: any = this.getModelValue(context.fieldPropertyName);
        if (_.isUndefined($modelValue)) {
            return null;
        }

        switch (context.primitiveType) {
            case Models.PrimitiveType.Number:
                return this.locale.toNumber($modelValue);

            case Models.PrimitiveType.Date:
                return this.locale.toDate($modelValue, true, this.locale.shortDateFormat);

            case Models.PrimitiveType.Choice:
                if (_.isArray($modelValue)) {
                    return {
                        validValues: $modelValue.map((it: string) => {
                            return {id: this.locale.toNumber(it)} as Models.IOption;
                        })
                    } as Models.IChoicePropertyValue;
                } else if (_.isObject(($modelValue))) {
                    return {customValue: $modelValue.customValue} as Models.IChoicePropertyValue;
                } else if (context.propertyTypePredefined < 0) {
                    return this.locale.toNumber($modelValue);
                }
                return {
                    validValues: [{id: this.locale.toNumber($modelValue)} as Models.IOption]
                } as Models.IChoicePropertyValue;

            case Models.PrimitiveType.User:
                if (_.isArray($modelValue)) {
                    return {
                        usersGroups: $modelValue.filter((elem: IUserGroup) => {
                            // isImported is added in the Formly User Picker controller to users
                            // from imported project who don't exist in the database
                            return !elem.isImported;
                        })
                    };
                }
                return null; // we probably should not return in this case

            default:
                if (context.isRichText) {
                    // tinyMCE returns empty tags (e.g. <p></p> when there is no content)
                    return Helper.tagsContainText($modelValue) || Helper.hasNonTextTags($modelValue) ? $modelValue : "";
                }
                return $modelValue;
        }
    }

    public convertToFieldValue(field: AngularFormly.IFieldConfigurationObject, $value: any): string | number | Date {
        const context = field.data as IPropertyDescriptor;
        if (!context || angular.isUndefined($value) || $value === null || angular.equals({}, $value)) {
            return null;
        }

        //create internal property value
        if (context.primitiveType === Models.PrimitiveType.Number) {
            return this.locale.toNumber($value);

        } else if (context.primitiveType === Models.PrimitiveType.Date) {
            return this.locale.toDate($value);

        } else if (context.primitiveType === Models.PrimitiveType.Choice) {
            if (angular.isArray($value.validValues)) {
                const values = $value.validValues.map((v: Models.IOption) => {
                    return v.id;
                });
                return context.isMultipleAllowed ? values : values[0];
                //} else if (angular.isString($value.customValue)) {
                //    return $value.customValue;
            } else if (angular.isNumber($value)) {
                return $value;
            }
        } else if (context.primitiveType === Models.PrimitiveType.User) {
            if ($value.usersGroups) {
                return $value.usersGroups;
            } else if ($value.displayName) {
                return $value.displayName;
            } else if ($value.label) {
                return $value.label;
            } else {
                return $value.toString();
            }
        } else if (context.primitiveType === Models.PrimitiveType.Text && context.isRichText) {
            return Helper.getHtmlBodyContent($value);
        }
        return $value;
    }


    public create(statefulItem: IStatefulItem,
                  propertyDescriptors: IPropertyDescriptor[],
                  force: boolean,
                  onBeforeFieldCreatedCallback?: (context: IPropertyDescriptor) => void): boolean {

        let fieldsupdated: boolean = false;
        this._model = {};
        this.propertyContexts = propertyDescriptors;

        //Check if fields changed (from metadata)
        let fieldNamesChanged = true;
        let namesChanged = true;
        if (this._fields) {
            const newFieldNames = this.propertyContexts.map((prop) => prop.fieldPropertyName);
            const previousFieldNames = this._fields.map((field) => (field.data as IPropertyDescriptor).fieldPropertyName);
            fieldNamesChanged = _.xor(newFieldNames, previousFieldNames).length > 0;

            const newNames = this.propertyContexts.map((prop) => prop.name);
            const previousNames = this._fields.map((field) => (field.data as IPropertyDescriptor).name);
            namesChanged = _.xor(newNames, previousNames).length > 0;
        }

        if (this.itemid !== statefulItem.id || fieldNamesChanged || namesChanged || force) {
            fieldsupdated = true;
            this._fields = [];
        }

        let allPropertiesReadOnlyDueToReuse = false;
        if (statefulItem instanceof StatefulSubArtifact) {
            allPropertiesReadOnlyDueToReuse = statefulItem.isReuseSettingSRO(Enums.ReuseSettings.Subartifacts);
        }

        this.propertyContexts.forEach((propertyContext: IPropertyDescriptor) => {
            if (propertyContext.fieldPropertyName && propertyContext.modelPropertyName) {
                let modelValue: any = null;
                let isModelSet: boolean = false;

                if (propertyContext.lookup === Enums.PropertyLookupEnum.System) {
                    //System property
                    if (angular.isDefined(statefulItem[propertyContext.modelPropertyName])) {
                        modelValue = statefulItem[propertyContext.modelPropertyName];
                    } else {
                        modelValue = null;
                    }
                    isModelSet = true;
                    if (Models.PropertyTypePredefined.Name === propertyContext.propertyTypePredefined &&
                        statefulItem.readOnlyReuseSettings &&
                        ((statefulItem.readOnlyReuseSettings & Enums.ReuseSettings.Name) === Enums.ReuseSettings.Name ||
                        allPropertiesReadOnlyDueToReuse)) {
                        propertyContext.disabled = true;

                    } else if (Models.PropertyTypePredefined.Description === propertyContext.propertyTypePredefined &&
                        statefulItem.readOnlyReuseSettings &&
                        ((statefulItem.readOnlyReuseSettings & Enums.ReuseSettings.Description) === Enums.ReuseSettings.Description ||
                        allPropertiesReadOnlyDueToReuse)) {
                        propertyContext.disabled = true;
                    }
                } else if (propertyContext.lookup === Enums.PropertyLookupEnum.Custom) {
                    //Custom property
                    let custompropertyvalue = statefulItem.customProperties.get(propertyContext.modelPropertyName as number);
                    if (custompropertyvalue) {
                        modelValue = custompropertyvalue.value;
                        isModelSet = true;
                        propertyContext.disabled = custompropertyvalue.isReuseReadOnly || allPropertiesReadOnlyDueToReuse ? true : propertyContext.disabled;
                    }
                } else if (propertyContext.lookup === Enums.PropertyLookupEnum.Special) {
                    //Specific property
                    let specificPropertyValue = statefulItem.specialProperties.get(propertyContext.modelPropertyName as number);
                    isModelSet = true;
                    if (specificPropertyValue) {
                        if (statefulItem.predefinedType === ItemTypePredefined.Step &&
                            specificPropertyValue.propertyTypePredefined === Enums.PropertyTypePredefined.StepOf) {
                            modelValue = this.getActorStepOfValue(specificPropertyValue.value);
                        } else {
                            modelValue = specificPropertyValue.value;
                        }
                        propertyContext.disabled = specificPropertyValue.isReuseReadOnly || allPropertiesReadOnlyDueToReuse ? true : propertyContext.disabled;
                    }
                }
                if (isModelSet) {
                    propertyContext.isFresh = true;
                    if (_.isFunction(onBeforeFieldCreatedCallback)) {
                        onBeforeFieldCreatedCallback(propertyContext);
                    }
                    let field = this.createPropertyField(propertyContext, statefulItem);
                    this._model[propertyContext.fieldPropertyName] = this.convertToFieldValue(field, modelValue);
                    if (fieldsupdated) {
                        this._fields.push(field);
                    }
                }
            }
        });
        this.itemid = statefulItem.id;

        return fieldsupdated;
    }

    private getActorStepOfValue(propertyValue: any): string {
        if (propertyValue) {
            return this.localization.get("App_Properties_Actor_StepOf_Actor");
        }
        return this.localization.get("App_Properties_Actor_StepOf_System");
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

    public getModelValue(propertyName: string): any {
        return this.getModel()[propertyName];
    }

    private createPropertyField(context: IPropertyDescriptor,
                                item: IStatefulItem,
                                reuseSettings?: Enums.ReuseSettings): AngularFormly.IFieldConfigurationObject {

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

        if (Models.PropertyTypePredefined.Name === context.propertyTypePredefined &&
            reuseSettings && (reuseSettings & Enums.ReuseSettings.Name) === Enums.ReuseSettings.Name) {
            field.templateOptions.disabled = true;

        } else if (Models.PropertyTypePredefined.Description === context.propertyTypePredefined &&
            reuseSettings && (reuseSettings & Enums.ReuseSettings.Description) === Enums.ReuseSettings.Description) {
            field.templateOptions.disabled = true;

        } else {
            switch (context.primitiveType) {
                case Models.PrimitiveType.Text:
                    field.type = context.isRichText ? (
                        context.isMultipleAllowed ||
                        Models.PropertyTypePredefined.Description === context.propertyTypePredefined ? "bpFieldTextRTF" : "bpFieldTextRTFInline"
                    ) : (
                        context.isMultipleAllowed ? "bpFieldTextMulti" : "bpFieldText"
                    );
                    field.defaultValue = context.stringDefaultValue;
                    if (context.isRichText && Enums.PropertyLookupEnum.Special !== context.lookup) {
                        field.templateOptions["hideLabel"] = field.type === "bpFieldTextRTF";
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
                    field.templateOptions.min = this.locale.toNumber(context.minNumber);
                    field.templateOptions.max = this.locale.toNumber(context.maxNumber);
                    field.templateOptions["decimalPlaces"] = this.locale.toNumber(context.decimalPlaces);
                    break;
                case Models.PrimitiveType.Choice:
                    field.type = context.isMultipleAllowed ? "bpFieldSelectMulti" : "bpFieldSelect";
                    field.templateOptions["optionsAttr"] = "bs-options";
                    field.templateOptions.options = [];
                    if (context.validValues && context.validValues.length && _.isNumber(context.defaultValidValueId)) {
                        field.defaultValue = context.defaultValidValueId.toString();
                    }
                    break;
                case Models.PrimitiveType.User:
                    field.type = "bpFieldUserPicker";
                    field.templateOptions["optionsAttr"] = "bs-options";
                    field.templateOptions.options = [];
                    if (context.userGroupDefaultValue && context.userGroupDefaultValue.length) {
                        field.defaultValue = context.userGroupDefaultValue;
                    }
                    break;
                case Models.PrimitiveType.Image:
                    field.type = "bpFieldImage";
                    break;
                case Models.PrimitiveType.ActorInheritance:
                    field.type = "bpFieldInheritFrom";
                    break;
                case Models.PrimitiveType.DocumentFile:
                    field.type = "bpDocumentFile";
                    field.templateOptions["artifactId"] = item.id;
                    if (item.artifactState.historical) {
                        field.templateOptions["versionId"] = item.getEffectiveVersion();
                    }
                    break;
                default:
                    //case Models.PrimitiveType.Image:
                    field.type = "input"; // needs to be changed to image editor
                    field.defaultValue = (context.defaultValidValueId || 0).toString();
                    field.templateOptions.options = [];
                    if (context.validValues) {
                        field.templateOptions.options = context.validValues.map(function (it) {
                            return <AngularFormly.ISelectOption>{value: it.id.toString(), name: it.value};
                        });
                    }
                    break;
            }
        }
        if (field.templateOptions.disabled) {
            if (field.type !== "bpFieldImage" &&
                field.type !== "bpFieldInheritFrom" &&
                field.type !== "bpDocumentFile") {
                field.type = "bpFieldReadOnly";
            }
        }
        return field;
    }

}
