import "angular";
import "angular-formly";
import { ILocalizationService, IUsersAndGroupsService, IUserOrGroupInfo } from "../../../core";
import { Models } from "../../../main/models";
import { BPFieldBaseController } from "./base-controller";

export class BPFieldUserPicker implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldUserPicker";
    public extends: string = "select";
    public template: string = require("./user-picker.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
            ($scope["options"] as AngularFormly.IFieldConfigurationObject).validation.show = ($scope["fc"] as ng.IFormController).$invalid;

            let uiSelectContainer = $element[0].querySelector(".ui-select-container");
            if (uiSelectContainer) {
                $scope["uiSelectContainer"] = uiSelectContainer;
                uiSelectContainer.addEventListener("keydown", $scope["bpFieldUserPicker"].closeDropdownOnTab, true);
                uiSelectContainer.addEventListener("click", $scope["bpFieldUserPicker"].scrollIntoView, true);

                // perfect-scrollbar steals the mousewheel events unless inner elements have a "ps-child" class
                // Not needed for textareas
                let uiSelectInput = uiSelectContainer.querySelector("div:not(.ps-child)") as HTMLElement;
                if (uiSelectInput && !uiSelectInput.classList.contains("ps-child")) {
                    uiSelectInput.classList.add("ps-child");
                }

                let uiSelectChoices = uiSelectContainer.querySelector("ul.ui-select-choices") as HTMLElement;
                if (uiSelectChoices) {
                    $scope["bpFieldUserPicker"].setupResultsElement(uiSelectChoices);
                }

                $scope["bpFieldUserPicker"].toggleScrollbar();
                $scope["uiSelectContainer"].firstElementChild.scrollTop = 0;
            }
        });
    };
    public controller: Function = BpFieldUserPickerController;

    constructor() {
    }
}

export class BpFieldUserPickerController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization", "usersAndGroupsService", "$compile"];

    constructor(
        private $scope: AngularFormly.ITemplateScope,
        private localization: ILocalizationService,
        private usersAndGroupsService: IUsersAndGroupsService,
        private $compile: ng.ICompileService
    ) {
        super();

        let to: AngularFormly.ITemplateOptions = {
            placeholder: localization.get("Property_Placeholder_Select_Option"),
            valueProp: "value",
            labelProp: "name"
        };
        angular.merge($scope.to, to);

        let validators = {
            // despite what the Formly doc says, "required" is not supported in ui-select, therefore we need our own implementation.
            // See: https://github.com/angular-ui/ui-select/issues/1226#event-604773506
            requiredCustom: {
                expression: function ($viewValue, $modelValue, scope) {
                    if ((<AngularFormly.ITemplateScope>scope.$parent).to.required) { // TODO: find a better way to get the "required" flag
                        if (angular.isArray($modelValue) && $modelValue.length === 0) {
                            return false;
                        }
                    }
                    return true;
                }
            }
        };
        $scope.options["validators"] = validators;

        let currentModelVal = $scope.model[$scope.options["key"]];
        if (currentModelVal && angular.isArray(currentModelVal) && currentModelVal.length) {
            // create the initial options in the dropdown just to be able to display the selected options in the field
            // the dropdown will be dynamically loaded from the webservice
            $scope.to.options = currentModelVal.map((it: Models.IUserGroup) => {
                return {
                    value: (it.isGroup ? "g" : "u") + it.id.toString(),
                    name: (it.isGroup ? localization.get("Label_Group_Identifier") + " " : "") + it.displayName
                } as any;
            });
            $scope.model[$scope.options["key"]] = currentModelVal.map((it: Models.IUserGroup) => {
                return (it.isGroup ? "g" : "u") + it.id.toString();
            });
        }

        $scope["$on"]("$destroy", function () {
            if ($scope["uiSelectContainer"]) {
                $scope["uiSelectContainer"].removeEventListener("keydown", $scope["bpFieldUserPicker"].closeDropdownOnTab, true);
                $scope["uiSelectContainer"].removeEventListener("click", $scope["bpFieldUserPicker"].scrollIntoView, true);
            }
        });

        // The following is a workaround to make $compile "stick"!
        // If the userpicker object is created like the other Formly types (e.g. select-multi), $compile disappears!
        $scope["bpFieldUserPicker"] = this.createUserPicker($scope, localization, usersAndGroupsService, $compile);
    }

    private createUserPicker(
        $scope: AngularFormly.ITemplateScope,
        localization: ILocalizationService,
        usersAndGroupsService: IUsersAndGroupsService,
        $compile: ng.ICompileService
    ) {
        return {
            $select: null,
            currentState: null,
            currentLimit: 5,
            maxLimit: 100,
            loadMoreAmount: 5,
            minimumInputLength: 2,
            showResultsCount: false,
            showLoadMore: false,
            labels: {
                noMatch: localization.get("Property_No_Matching_Options"),
                minimumLength: localization.get("Property_UserPicker_Placeholder"),
                searching: localization.get("Property_UserPicker_Searching"),
                showMore: localization.get("Property_UserPicker_ShowMore"),
                topResults: localization.get("Property_UserPicker_Display_Top_N_Results")
            },
            isChoiceSelected: function (item, $select): boolean {
                return $select.selected.map(function (e) { return e[$scope.to.valueProp]; }).indexOf(item[$scope.to.valueProp]) !== -1;
            },
            toggleScrollbar: function (removeScrollbar?: boolean) {
                if (!removeScrollbar) {
                    if ($scope["uiSelectContainer"]) {
                        let elem = $scope["uiSelectContainer"].querySelector("div") as HTMLElement;
                        if (elem && elem.scrollHeight > elem.clientHeight) {
                            $scope["uiSelectContainer"].classList.add("has-scrollbar");
                        } else {
                            removeScrollbar = true;
                        }
                    }
                }
                if (removeScrollbar) {
                    if ($scope["uiSelectContainer"] && $scope["uiSelectContainer"].classList.contains("has-scrollbar")) {
                        let elem = $scope["uiSelectContainer"].querySelector("div") as HTMLElement;
                        if (elem && elem.scrollHeight <= elem.clientHeight) {
                            $scope["uiSelectContainer"].classList.remove("has-scrollbar");
                        }
                    }
                }
            },
            closeDropdownOnTab: this.closeDropdownOnTab,
            scrollIntoView: this.scrollIntoView,
            resetSettings: function () {
                this.currentState = null;
                this.currentLimit = this.loadMoreAmount;
                this.showResultsCount = false;
                this.showLoadMore = false;
            },
            setupResultsElement: function(uiSelectChoices: HTMLElement) {
                let uiSelectLoadMore = `
                    <li ng-if="bpFieldUserPicker.showResultsCount" class="ui-select-results-count"><br><br><br><br>
                        <div ng-bind="bpFieldUserPicker.labels.topResults.replace('{0}', bpFieldUserPicker.currentLimit)"></div>
                        <button
                            ng-if="bpFieldUserPicker.showLoadMore"
                            ng-click="bpFieldUserPicker.loadMore()"
                            ng-bind="bpFieldUserPicker.labels.showMore"></button>
                    </li>`;
                angular.element(uiSelectChoices).append($compile(uiSelectLoadMore)(<any>$scope));
            },
            loadMore: function () {
                if (this.currentLimit < this.maxLimit && this.$select) {
                    this.refreshResults(this.$select, true);
                }
            },
            refreshResults: function ($select, loadMore?: boolean) {
                let query = $select.search;
                if (query.length >= this.minimumInputLength) {
                    if (loadMore) {
                        this.currentLimit += this.loadMoreAmount;
                    }
                    this.currentState = "searching";
                    usersAndGroupsService.search(
                        $select.search,
                        true, //emailDiscussion has to be set to true so that als users without email get returned
                        //max number of users to return. We ask for 1 more so to know if we need to show the "Load more"
                        //if more than maxLimit, we ask for maxLimit
                        this.currentLimit < this.maxLimit ? this.currentLimit + 1 : this.maxLimit,
                        false //do not include guest users
                    ).then(
                        (users) => {
                            $scope.to.options = users.map((item: IUserOrGroupInfo) => {
                                let e: any = {};
                                e[$scope.to.valueProp] = item.id.toString();
                                e[$scope.to.labelProp] = item.name;
                                e.email = item.email;
                                e.isGroup = item.isGroup;
                                e.selected = this.isChoiceSelected(e, $select);
                                e.disabled = item.isBlocked || e.selected;
                                return e;
                            });
                            $select.items = $scope.to.options;
                            this.currentState = $scope.to.options.length ? null : "no-match";

                            this.showResultsCount = $scope.to.options.length > this.currentLimit;
                            this.showLoadMore = $scope.to.options.length > this.currentLimit && $scope.to.options.length < this.maxLimit;

                            this.$select = $select;
                        },
                        () => {
                            $scope.to.options = [];
                            $select.items = $scope.to.options;

                            this.resetSettings();
                        }
                    );
                } else {
                    $scope.to.options = [];
                    $select.items = $scope.to.options;

                    this.resetSettings();
                }
            },
            findDropdown: function ($select): HTMLElement {
                let dropdown: HTMLElement;
                let elements = $select.$element.find("ul");
                for (let i = 0; i < elements.length; i++) {
                    if (elements[i].classList.contains("ui-select-choices")) {
                        dropdown = elements[i];
                        break;
                    }
                }
                return dropdown;
            },
            onOpenClose: function (isOpen: boolean, $select, options) {
                $select.items = [];
                $scope.to.options = [];

                this.resetSettings();
            },
            onHighlight: function (option, $select) {
                if (this.isChoiceSelected(option, $select)) {
                    if ($select.activeIndex > this.currentSelectedItem) {
                        if ($select.activeIndex < $select.items.length - 1) {
                            $select.activeIndex++;
                        } else {
                            this.currentSelectedItem = $select.activeIndex;
                            $select.activeIndex--;
                        }
                    } else {
                        if ($select.activeIndex > 0) {
                            $select.activeIndex--;
                        } else {
                            this.currentSelectedItem = $select.activeIndex;
                            $select.activeIndex++;
                        }
                    }
                } else {
                    this.currentSelectedItem = $select.activeIndex;
                }
            },
            onRemove: function ($item, $select, formControl: ng.IFormController, options: AngularFormly.IFieldConfigurationObject) {
                options.validation.show = formControl.$invalid;
                this.toggleScrollbar(true);
            },
            onSelect: function ($item, $select) {
                // On ENTER the ui-select reset the activeIndex to the first item of the list.
                // We need to hide the highlight until we select the proper entry
                if ($scope["uiSelectContainer"]) {
                    $scope["uiSelectContainer"].querySelector(".ui-select-choices").classList.add("disable-highlight");
                }

                let currentItem = $select.items.map(function (e) { return e[$scope.to.valueProp]; }).indexOf($item[$scope.to.valueProp]);

                $scope["$applyAsync"](() => {
                    if ($scope["uiSelectContainer"]) {
                        $scope["uiSelectContainer"].querySelector(".ui-select-choices").classList.remove("disable-highlight");
                        $scope["uiSelectContainer"].querySelector("input").focus();
                    }
                    if (currentItem < $select.items.length - 1) {
                        this.currentSelectedItem = currentItem++;
                        $select.activeIndex = currentItem;
                    } else {
                        this.currentSelectedItem = $select.items.length - 1;
                        $select.activeIndex = -1;
                    }
                });
                this.toggleScrollbar();
            }
        };
    }
}
