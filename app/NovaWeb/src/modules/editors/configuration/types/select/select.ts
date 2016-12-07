import "angular-formly";
import {Enums, Models} from "../../../../main/models";
import {BPFieldBaseController} from "../base-controller";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";

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

    private isValidated: boolean;
    private allowsCustomValues: boolean;

    constructor(private $scope: AngularFormly.ITemplateScope,
                     private localization: ILocalizationService,
                     private validationService: IValidationService) {
        super();

        this.isValidated = $scope.options["data"].isValidated;
        this.allowsCustomValues = !this.isValidated && $scope.options["data"].lookup === Enums.PropertyLookupEnum.Custom;

        const to: AngularFormly.ITemplateOptions = {
            placeholder: localization.get("Property_Placeholder_Select_Option"),
            valueProp: "value",
            labelProp: "name"
        };
        angular.merge($scope.to, to);

        $scope.options["validators"] = {
            // despite what the Formly doc says, "required" is not supported in ui-select, therefore we need our own implementation.
            // See: https://github.com/angular-ui/ui-select/issues/1226#event-604773506
            requiredCustom: {
                expression: ($viewValue, $modelValue, scope) => {
                    const isValid = validationService.selectValidation.hasValueIfRequired(
                        ((<AngularFormly.ITemplateScope>scope.$parent).to.required),
                        $viewValue,
                        $modelValue,
                        this.isValidated,
                        this.allowsCustomValues
                    );

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

    private refreshResults = ($select) => {
        if (this.allowsCustomValues) {
            const search = $select.search;
            if (search) {
                let isDuplicate = false;
                $select.items.forEach((item) => {
                    if (item[this.$scope.to.labelProp] === search) {
                        isDuplicate = true;
                        return;
                    }
                });

                if (!isDuplicate) {
                    this.selectCustomValue($select, search);
                }
            } else {
                $select.items = this.filterOutCustomValues($select.items as ISelectItem[]);
                $select.activeIndex = -1;
            }
        }
    };

    private onOpenClose = ($select, isOpen: boolean) => {
        if (_.isUndefined($select.selected) || _.isNull($select.selected)) {
            $select.activeIndex = -1;
        } else {
            if (_.isObject($select.selected.value)) {
                this.selectCustomValue($select, $select.selected.value.customValue);

                // un-comment the following to make the custom value the only value in the dropdown
                // this has no effect when a standard value is selected
                // if (isOpen) {
                //     $select.search = $select.selected.value.customValue;
                // }
            }
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

        const currentModelVal = this.$scope.model[this.$scope.options["key"]] as ICustomItem;
        if (_.isObject(currentModelVal) && currentModelVal.customValue) {
            let newVal: ISelectItem = {
                value: currentModelVal,
                name: currentModelVal.customValue
            };
            options.unshift(newVal);
        }

        return options;
    };

    private filterOutCustomValues(items: ISelectItem[]): ISelectItem[] {
        const optionList: ISelectItem[] = _.clone(items);
        //remove custom values
        return optionList.filter(item => !_.isObject(item.value));
    }

    private selectCustomValue = ($select, label: string) => {
        const optionList = this.filterOutCustomValues($select.items as ISelectItem[]);
        const userInputItem: ISelectItem = {
            value: {
                customValue: label
            },
            name: label
        };
        $select.items = [userInputItem].concat(optionList);
        $select.activeIndex = 0;
    };
}
