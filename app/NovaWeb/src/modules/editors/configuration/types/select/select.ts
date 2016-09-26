import * as angular from "angular";
import "angular-formly";
import { ILocalizationService } from "../../../../core";
import { Enums } from "../../../../main/models";
import { BPFieldBaseController } from "../base-controller";

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

    constructor() {
    }
}

export class BpFieldSelectController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization"];

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService) {
        super();

        let to: AngularFormly.ITemplateOptions = {
            placeholder: localization.get("Property_Placeholder_Select_Option"),
            valueProp: "value",
            labelProp: "name"
        };
        angular.merge($scope.to, to);

        let newCustomValueId = function (): string {
            return (-1 * (Math.random() * 100 + 100)).toString(); // not to conflict with special IDs like project (-1) or collections (-2)
        };

        $scope["$on"]("$destroy", function () {
            if ($scope["uiSelectContainer"]) {
                $scope["uiSelectContainer"].removeEventListener("keydown", $scope["bpFieldSelect"].closeDropdownOnTab, true);
            }
        });

        let customValueId = "-1";
        // we need to generate a custom id everytime, otherwise the select won't be able to recognize two different custom values

        let currentModelVal = $scope.model[$scope.options["key"]];
        if (angular.isObject(currentModelVal) && currentModelVal.customValue) {
            let newVal = {
                value: customValueId,
                name: currentModelVal.customValue,
                isCustom: true
            };
            $scope.to.options.push(newVal);
            $scope.model[$scope.options["key"]] = customValueId;
        }

        $scope["bpFieldSelect"] = {
            closeDropdownOnTab: this.closeDropdownOnTab,
            labels: {
                noMatch: localization.get("Property_No_Matching_Options")
            },
            refreshResults: function ($select) {
                if (!$scope.options["data"].isValidated && $scope.options["data"].lookup === Enums.PropertyLookupEnum.Custom) {
                    let search = $select.search;

                    if (search) {
                        let optionList = angular.copy($select.items);

                        //remove last user input
                        optionList = optionList.filter(function (item) {
                            return !item.isCustom;
                        });

                        let isDuplicate = false;
                        $select.items.forEach(function (item) {
                            if (item[$scope.to.labelProp] === search) {
                                isDuplicate = true;
                                return;
                            }
                        });

                        if (!isDuplicate) {
                            //manually add user input and set selection
                            customValueId = newCustomValueId();
                            let userInputItem = {
                                value: { customValue: search },
                                name: search,
                                isCustom: true
                            };
                            $select.items = [userInputItem].concat(optionList);
                            $select.selected = userInputItem;
                        }
                    }
                }
            },
            onOpenClose: function (isOpen) {
                if (isOpen && $scope["uiSelectContainer"]) {
                    let currentVal = $scope.model[$scope.options["key"]];
                    if (angular.isObject(currentVal)) {
                        $scope["uiSelectContainer"].querySelector(".ui-select-choices-row").classList.add("active");
                    } else if (angular.isNumber(currentVal)) {
                        let options = $scope["uiSelectContainer"].querySelectorAll(".ui-select-choices-row");
                        [].forEach.call(options, function (option) {
                            option.classList.remove("active");
                        });
                        let elem = $scope["uiSelectContainer"].querySelector(".ui-select-choice-item-selected");
                        if (elem) {
                            while (elem && !elem.classList.contains("ui-select-choices-row")) {
                                elem = elem.parentElement;
                            }
                            if (elem) {
                                elem.classList.add("active");
                            }
                        }
                    }
                }
            }
        };
    }
}
