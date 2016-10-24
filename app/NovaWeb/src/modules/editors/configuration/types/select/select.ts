import * as angular from "angular";
import "angular-formly";
import {ILocalizationService} from "../../../../core";
import {Enums, Models} from "../../../../main/models";
import {BPFieldBaseController} from "../base-controller";

export class BPFieldSelect implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldSelect";
    public extends: string = "select";
    public template: string = require("./select.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
            ($scope["options"] as AngularFormly.IFieldConfigurationObject).validation.show = ($scope["fc"] as ng.IFormController).$invalid;

            let uiSelectContainer = $element[0].querySelector(".ui-select-container");
            if (uiSelectContainer) {
                $scope["uiSelectContainer"] = uiSelectContainer;
                uiSelectContainer.addEventListener("keydown", $scope["bpFieldSelect"].closeDropdownOnTab, true);
            }
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldSelectController;
}

interface Items {

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

        $scope["$on"]("$destroy", function () {
            if ($scope["uiSelectContainer"]) {
                $scope["uiSelectContainer"].removeEventListener("keydown", $scope["bpFieldSelect"].closeDropdownOnTab, true);
            }
        });

        $scope.options["expressionProperties"] = {
            "templateOptions.options": () => {
                let options = [];
                let context: Models.IPropertyType = $scope.options["data"];
                if (context.validValues && context.validValues.length) {
                    options = context.validValues.map(function (it) {
                        return {value: it.id, name: it.value} as any;
                    });
                }

                const currentModelVal = $scope.model[$scope.options["key"]];
                if (_.isObject(currentModelVal) && currentModelVal.customValue) {
                    let newVal = {
                        value: currentModelVal,
                        name: currentModelVal.customValue
                    };
                    options.push(newVal);
                }

                return options;
            }
        };

        $scope["bpFieldSelect"] = {
            closeDropdownOnTab: this.closeDropdownOnTab,
            labels: {
                noMatch: localization.get("Property_No_Matching_Options")
            },
            refreshResults: function ($select) {
                if (!$scope.options["data"].isValidated && $scope.options["data"].lookup === Enums.PropertyLookupEnum.Custom) {
                    let optionList = angular.copy($select.items);

                    //remove last user input
                    optionList = optionList.filter(function (item) {
                        return !_.isObject(item.value);
                    });

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
                            //manually add user input and set selection
                            const userInputItem = {
                                value: {customValue: search},
                                name: search
                            };
                            $select.items = [userInputItem].concat(optionList);
                            $select.selected = userInputItem;
                        }
                    } else {
                        $select.items = optionList;
                    }
                }
            },
            onOpenClose: function ($select, isOpen) {
                if (isOpen && $scope["uiSelectContainer"]) {
                    if (_.isObject(($select.selected.value))) {
                        let optionList = angular.copy($select.items);
                        const userInputItem = {
                            value: $select.selected.value,
                            name: $select.selected.name
                        };
                        $select.items = [userInputItem].concat(optionList);
                    }
                    // let currentVal = $scope.model[$scope.options["key"]];
                    // if (angular.isObject(currentVal)) {
                    //     $scope["uiSelectContainer"].querySelector(".ui-select-choices-row").classList.add("active");
                    // } else if (angular.isNumber(currentVal)) {
                    //     let options = $scope["uiSelectContainer"].querySelectorAll(".ui-select-choices-row");
                    //     [].forEach.call(options, function (option) {
                    //         option.classList.remove("active");
                    //     });
                    //     let elem = $scope["uiSelectContainer"].querySelector(".ui-select-choice-item-selected");
                    //     if (elem) {
                    //         while (elem && !elem.classList.contains("ui-select-choices-row")) {
                    //             elem = elem.parentElement;
                    //         }
                    //         if (elem) {
                    //             elem.classList.add("active");
                    //         }
                    //     }
                    // }
                }
            }
        };
    }

    private filterCustomItems = (items: any[]): any[] => {
return [];
    }
}
