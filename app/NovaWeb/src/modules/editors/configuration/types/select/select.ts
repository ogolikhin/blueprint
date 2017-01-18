import "angular-formly";
import {Enums, Models} from "../../../../main/models";
import {BPFieldBaseController} from "../base-controller";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {IPropertyDescriptor} from "../../property-descriptor-builder";
export class BPFieldSelect implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldSelect";
    public extends: string = "select";
    public template: string = require("./select.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldSelectController;
}

interface ICustomItem {
    customValue: string;
}

interface ISelectItem {
    name: string;
    value: number | ICustomItem;
}

export class BpFieldSelectController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization", "validationService"];
    private propertyDescriptor: IPropertyDescriptor;
    private allowsCustomValues: boolean;
    private customValue: ISelectItem;

    constructor(private $scope: AngularFormly.ITemplateScope,
                private localization: ILocalizationService,
                private validationService: IValidationService) {
        super();
        this.propertyDescriptor = $scope.options["data"];

        this.allowsCustomValues = !this.propertyDescriptor.isValidated;
        this.customValue = null;

        const to: AngularFormly.ITemplateOptions = {
            placeholder: localization.get("Property_Placeholder_Select_Option"),
            valueProp: "value",
            labelProp: "name"
        };
        _.assign($scope.to, to);

        $scope.options["validators"] = {
            // despite what the Formly doc says, "required" is not supported in ui-select, therefore we need our own implementation.
            // See: https://github.com/angular-ui/ui-select/issues/1226#event-604773506
            requiredCustom: {
                expression: ($viewValue, $modelValue, scope) => {
                    const isValid = validationService.selectValidation.hasValueIfRequired(
                        this.propertyDescriptor.isRequired,
                        $modelValue);

                    BPFieldBaseController.handleValidationMessage("requiredCustom", isValid, scope);
                    return true;
                }
            }
        };

        $scope.options["expressionProperties"] = {
            "templateOptions.options": this.refreshOptions
        };

        $scope["bpFieldSelect"] = {
            closeDropdownOnTab: this.closeDropdownOnTab,
            closeDropdownOnBlur: this.closeDropdownOnBlur,
            labels: {
                noMatch: localization.get("Property_No_Matching_Options")
            },
            showSelected: this.showSelected,
            refreshResults: this.refreshResults,
            onOpenClose: this.onOpenClose
        };
    }

    private showSelected = (selected: ISelectItem | ICustomItem): string => {
        if (_.isUndefined(selected) || _.isNull(selected)) {
            return "";
        }
        if (_.isObject((selected as ISelectItem).value) && ((selected as ISelectItem).value as ICustomItem).customValue) {
            return ((selected as ISelectItem).value as ICustomItem).customValue;
        }
        if (_.isObject(selected) && (selected as ICustomItem).customValue) {
            return (selected as ICustomItem).customValue;
        }
        return (selected as ISelectItem).name;
    };

    private removeTempCustomValues = (items: ISelectItem[]): ISelectItem[] => {
        const _items = _.clone(items);
        while (_items.length && _.isObject(_items[0].value) && !_.isEqual(_items[0], this.customValue)) {
            _items.shift();
        }
        return _items;
    };

    private refreshResults = ($select) => {
        if (this.allowsCustomValues) {
            $select.items = this.removeTempCustomValues($select.items);
            const search = $select.search || "";
            if (search !== "" && _.findIndex($select.items, item => (item as ISelectItem).name === search) === -1) {
                const newCustomValue: ISelectItem = {
                    value: {
                        customValue: search
                    },
                    name: search
                };
                $select.items.unshift(newCustomValue);
            }
        }
    };

    private onOpenClose = ($select, isOpen: boolean) => {
        this.closeDropdownOnBlur(isOpen, $select.searchInput);

        if (!isOpen) {
            $select.items = this.refreshOptions();
        }
    };

    private refreshOptions = (): ISelectItem[] => {
        let options: ISelectItem[] = [];
        const context: Models.IPropertyType = this.$scope.options["data"];
        if (context.validValues && context.validValues.length) {
            options = context.validValues.map((it: Models.IOption) => {
                return {
                    value: it.id,
                    name: it.value
                } as ISelectItem;
            });
        }

        if (this.allowsCustomValues) {
            const currentModelVal = this.$scope.model[this.$scope.options["key"]] as ICustomItem;
            if (_.isObject(currentModelVal) && currentModelVal.customValue) {
                const newVal: ISelectItem = {
                    value: currentModelVal,
                    name: currentModelVal.customValue
                };
                this.customValue = newVal;
                options.unshift(newVal);
            }
        }

        return options;
    };
}
