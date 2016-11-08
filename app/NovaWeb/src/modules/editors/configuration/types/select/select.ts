import "angular-formly";
import {Enums, Models} from "../../../../main/models";
import {BPFieldBaseController} from "../base-controller";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export class BPFieldSelect implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldSelect";
    public extends: string = "select";
    public template: string = require("./select.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();

            const $scopeOptions = $scope["options"] as AngularFormly.IFieldConfigurationObject;
            $scopeOptions.validation.show = ($scope["fc"] as ng.IFormController).$invalid;
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
    static $inject: [string] = ["$scope", "localization"];

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService) {
        super();

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
                expression: function ($viewValue, $modelValue, scope) {
                    if ((<AngularFormly.ITemplateScope>scope.$parent).to.required) { // TODO: find a better way to get the "required" flag
                        const isInvalid = _.isNull($modelValue) || _.isUndefined($modelValue);
                        scope.options.validation.show = isInvalid;
                        return !isInvalid;
                    }
                    return true;
                }
            }
        };

        const isCustomSelect = !$scope.options["data"].isValidated && $scope.options["data"].lookup === Enums.PropertyLookupEnum.Custom;

        $scope.options["expressionProperties"] = {
            "templateOptions.options": () => {
                let options: ISelectItem[] = [];
                let context: Models.IPropertyType = $scope.options["data"];
                if (context.validValues && context.validValues.length) {
                    options = context.validValues.map(function (it) {
                        return {value: it.id, name: it.value} as any;
                    });
                }

                const currentModelVal = $scope.model[$scope.options["key"]];
                if (_.isObject(currentModelVal) && currentModelVal.customValue) {
                    let newVal: ISelectItem = {
                        value: currentModelVal,
                        name: currentModelVal.customValue
                    };
                    options.push(newVal);
                }

                return options;
            }
        };

        function filterCustomItems(items: ISelectItem[]): ISelectItem[] {
            let optionList: ISelectItem[] = _.clone(items);
            //remove custom values
            return optionList.filter(function (item) {
                return !_.isObject(item.value);
            });
        }

        function selectCustomItem($select, label: string) {
            let optionList = filterCustomItems($select.items as ISelectItem[]);
            const userInputItem: ISelectItem = {
                value: {
                    customValue: label
                },
                name: label
            };
            $select.items = [userInputItem].concat(optionList);
            // $select.selected = userInputItem;
            $select.activeIndex = 0;
        }

        $scope["bpFieldSelect"] = {
            closeDropdownOnTab: this.closeDropdownOnTab,
            labels: {
                noMatch: localization.get("Property_No_Matching_Options")
            },
            refreshResults: function ($select) {
                if (isCustomSelect) {
                    const search = $select.search;
                    if (search) {
                        let isDuplicate = false;
                        $select.items.forEach(function (item) {
                            if (item[$scope.to.labelProp] === search) {
                                isDuplicate = true;
                                return;
                            }
                        });

                        if (!isDuplicate) {
                            selectCustomItem($select, search);
                        }
                    } else {
                        $select.items = filterCustomItems($select.items as ISelectItem[]);
                        $select.activeIndex = -1;
                    }
                }
            },
            onOpenClose: function ($select, isOpen) {
                if (_.isUndefined($select.selected) || _.isNull($select.selected)) {
                    $select.activeIndex = -1;
                } else {
                    if (_.isObject($select.selected.value)) {
                        selectCustomItem($select, $select.selected.value.customValue);

                        // un-comment the following to make the custom value the only value in the dropdown
                        // this has no effect when a standard value is selected
                        // if (isOpen) {
                        //     $select.search = $select.selected.value.customValue;
                        // }
                    }
                }
            }
        };
    }
}
