import "angular-formly";
import {IUsersAndGroupsService, IUserOrGroupInfo} from "../../../../core";
import {Models} from "../../../../main/models";
import {IPropertyDescriptor} from "./../../property-descriptor-builder";
import {BPFieldBaseController} from "../base-controller";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";

export interface IUserGroup extends Models.IUserGroup {
    isImported?: boolean;
}

interface IUserPickerItem {
    value?: IUserGroup;
    name?: string;
    email?: string;
    isGroup?: boolean;
    isLoginEnabled?: boolean;
    selected?: boolean;
}

export class BPFieldUserPicker implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldUserPicker";
    public extends: string = "select";
    public template: string = require("./user-picker.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();

            let uiSelectContainer = $element[0].querySelector(".ui-select-container");
            if (uiSelectContainer) {
                $scope["uiSelectContainer"] = uiSelectContainer;

                const uiSelectInput = uiSelectContainer.querySelector("div") as HTMLElement;
                if (uiSelectInput) {
                    uiSelectInput.classList.add("ui-select-input");
                    uiSelectInput.addEventListener("keydown", $scope["bpFieldUserPicker"].onKeyDown, true);
                    uiSelectInput.addEventListener("click", $scope["bpFieldUserPicker"].scrollIntoView, true);
                }

                const uiSelectChoices = uiSelectContainer.querySelector("ul.ui-select-choices") as HTMLElement;
                if (uiSelectChoices) {
                    $scope["bpFieldUserPicker"].setupResultsElement(uiSelectContainer);
                }

                $scope["bpFieldUserPicker"].toggleScrollbar();
                $scope["uiSelectContainer"].firstElementChild.scrollTop = 0;
            }
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldUserPickerController;
}

export class BpFieldUserPickerController extends BPFieldBaseController {
    static $inject: [string] = ["$document", "$scope", "localization", "usersAndGroupsService", "$compile", "validationService"];

    constructor(protected $document: ng.IDocumentService,
                private $scope: AngularFormly.ITemplateScope,
                private localization: ILocalizationService,
                private usersAndGroupsService: IUsersAndGroupsService,
                private $compile: ng.ICompileService, private validationService: IValidationService) {
        super($document);

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
                    const isValid = validationService.userPickerValidation.hasValueIfRequired(
                        ((<AngularFormly.ITemplateScope>scope.$parent).to.required),
                        $modelValue);

                    BPFieldBaseController.handleValidationMessage("requiredCustom", isValid, scope);
                    return true;

                }
            }
        };

        let options: IUserPickerItem[] = [];
        $scope.options["expressionProperties"] = {
            "templateOptions.options": () => {
                const context: IPropertyDescriptor = $scope.options["data"];
                if (context.isFresh) {
                    const currentModelVal = $scope.model[$scope.options["key"]];
                    if (currentModelVal) {
                        if (_.isArray(currentModelVal) && currentModelVal.length) {
                            // create the initial options in the dropdown just to be able to display the selected options in the field
                            // the dropdown will be dynamically loaded from the webservice
                            options = currentModelVal.map((it: IUserGroup) => {
                                return {
                                    value: it,
                                    name: (it.isGroup ? localization.get("Label_Group_Identifier") + " " : "") + it.displayName
                                } as IUserPickerItem;
                            });
                            context.isFresh = false;
                        } else if (_.isString(currentModelVal)) {
                            $scope.model[$scope.options["key"]] = currentModelVal.split(",").map((it: string) => {
                                return {
                                    id: -1,
                                    displayName: it,
                                    isImported: true
                                } as IUserGroup;
                            });
                        }
                    }
                } else {
                    options = [];
                }

                return options;
            }
        };

        $scope["$on"]("$destroy", function () {
            if ($scope["uiSelectContainer"]) {
                let uiSelectInput = $scope["uiSelectContainer"].querySelector(".ui-select-input") as HTMLElement;
                if (uiSelectInput) {
                    uiSelectInput.removeEventListener("keydown", $scope["bpFieldUserPicker"].onKeyDown, true);
                    uiSelectInput.removeEventListener("click", $scope["bpFieldUserPicker"].scrollIntoView, true);
                }
            }
        });

        // The following is a workaround to make $compile "stick"!
        // If the userpicker object is created like the other Formly types (e.g. select-multi), $compile disappears!
        $scope["bpFieldUserPicker"] = this.createUserPicker($scope, localization, usersAndGroupsService, $compile);
    }

    private createUserPicker($scope: AngularFormly.ITemplateScope,
                             localization: ILocalizationService,
                             usersAndGroupsService: IUsersAndGroupsService,
                             $compile: ng.ICompileService) {
        return {
            $select: null,
            currentSelectedItem: -1,
            currentState: null,
            currentLimit: 1000,
            maxLimit: 100,
            minLimit: 5,
            minimumInputLength: 2,
            showResultsCount: false,
            showLoadMore: false,
            searchInputElement: null,
            listItemElement: null,
            itemsHeight: 40,
            maxVisibleItems: 7,
            isScrolling: false,
            labels: {
                noMatch: localization.get("Property_No_Matching_Options"),
                minimumLength: localization.get("Property_UserPicker_Placeholder"),
                searching: localization.get("Property_UserPicker_Searching"),
                showMore: localization.get("Property_UserPicker_ShowMore"),
                topResults: localization.get("Property_UserPicker_Display_Top_N_Results")
            },
            isChoiceSelected: function (item, $select): boolean {
                let userValue: Models.IUserGroup = item[$scope.to.valueProp];
                return $select.selected.some(function (elem) {
                    let elemValue: Models.IUserGroup = elem[$scope.to.valueProp];
                    return userValue.id === elemValue.id && Boolean(userValue.isGroup) === Boolean(elemValue.isGroup);
                });
            },
            areStillChoicesAvailable: function ($select): boolean {
                return $select.items.some((elem) => {
                    return !this.isChoiceSelected(elem, $select);
                });
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
            onKeyDown: (event) => {
                const key = event.keyCode || event.which;
                const userPicker = $scope["bpFieldUserPicker"];
                const $select = userPicker.$select;
                if (key === 9) { // 9 = Tab
                    if ($select.open && userPicker.showLoadMore) {
                        let button = $scope["uiSelectContainer"].querySelector(".ui-select-results-count button");
                        if (button) {
                            if (document.activeElement === button && event.shiftKey) {
                                userPicker.searchInputElement.focus();
                            } else if (document.activeElement !== button && !event.shiftKey) {
                                $select.activeIndex = -1;
                                button.focus();
                            }
                        } else {
                            this.closeDropdownOnTab(event);
                        }
                        event.stopPropagation();
                        event.stopImmediatePropagation();
                        event.preventDefault();
                    } else {
                        this.closeDropdownOnTab(event);
                    }
                } else if (key === 38 && $select.open) { // 38 = Arrow up
                    setTimeout(() => {
                        userPicker.searchInputElement.selectionStart = userPicker.searchInputElement.selectionEnd = userPicker.searchInputElement.value.length;
                    }, 150);
                }
            },
            scrollIntoView: this.scrollIntoView,
            catchClick: this.catchClick,
            catchClickId: _.toString(_.random(1000000)),
            resetSettings: function () {
                this.currentState = null;
                this.currentLimit = this.minLimit;
                this.showResultsCount = false;
                this.showLoadMore = false;
                if (this.listItemElement) {
                    this.listItemElement.parentElement.style.height = "";
                }
            },
            setupResultsElement: function (uiSelectContainer: HTMLElement) {
                let uiSelectChoices = uiSelectContainer.querySelector("ul.ui-select-choices") as HTMLElement;
                let uiSelectLoadMore = `
                    <li ng-if="bpFieldUserPicker.showResultsCount" class="ui-select-results-count">
                        <div ng-bind="bpFieldUserPicker.labels.topResults.replace('{0}', bpFieldUserPicker.currentLimit)"></div>
                        <button
                            ng-if="bpFieldUserPicker.showLoadMore"
                            ng-click="bpFieldUserPicker.loadMore()"
                            ng-keydown="bpFieldUserPicker.onKeyDown($event)"
                            ng-bind="bpFieldUserPicker.labels.showMore"></button>
                    </li>`;
                angular.element(uiSelectChoices).append($compile(uiSelectLoadMore)(<any>$scope));

                this.searchInputElement = uiSelectContainer.querySelector("input.ui-select-search") as HTMLElement;
                this.listItemElement = uiSelectChoices.querySelector("li.ui-select-choices-group") as HTMLElement;
            },
            removeImportedUsers: function ($select) {
                if ($select.selected && $select.selected.length > 0) {
                    $select.selected = $select.selected.filter((elem) => {
                        return !elem.value.isImported;
                    });
                }

                let model = $scope.model[$scope.options["key"]];
                if (model && angular.isArray(model) && model.length > 0) {
                    model = model.filter((elem) => {
                        return !elem.isImported;
                    });
                    $scope.model[$scope.options["key"]] = model;
                }
            },
            loadMore: function () {
                if (this.currentLimit < this.maxLimit && this.$select) {
                    this.refreshResults(this.$select, true);
                    if (this.searchInputElement) {
                        this.searchInputElement.focus();
                    }
                }
            },
            refreshResults: function ($select, loadMore?: boolean) {
                let query = $select.search;
                if (query.length >= this.minimumInputLength) {
                    if (loadMore) {
                        this.currentLimit = this.maxLimit;
                    }
                    this.currentState = "searching";
                    usersAndGroupsService.search(
                        $select.search,
                        true, //emailDiscussion has to be set to true so that als users without email get returned
                        //max number of users to return. We ask for 1 more so to know if we need to show the "Load more"
                        //if more than maxLimit, we ask for maxLimit
                        this.currentLimit < this.maxLimit ? this.minLimit + 1 : this.maxLimit,
                        false //do not include guest users
                    ).then(
                        (users) => {
                            // TODO: remove <any> - need to return property interface from map return method
                            $scope.to.options = <any>users.map((item: IUserOrGroupInfo) => {
                                let e: IUserPickerItem = {};
                                e[$scope.to.valueProp] = {
                                    id: parseInt(angular.isNumber(item.id) ? item.id : item.id.substr(1), 10),
                                    displayName: item.name,
                                    isGroup: item.isGroup
                                } as Models.IUserGroup;
                                e[$scope.to.labelProp] = (item.isGroup ? localization.get("Label_Group_Identifier") + " " : "") + item.name;
                                e.email = item.email;
                                e.isGroup = item.isGroup;
                                e.isLoginEnabled = item.isLoginEnabled;
                                e.selected = this.isChoiceSelected(e, $select);
                                return e;
                            });
                            $select.items = $scope.to.options;
                            this.currentState = $scope.to.options.length ? null : "no-match";

                            this.showResultsCount = (
                                    $scope.to.options.length > this.minLimit && !loadMore
                                ) || (
                                    $scope.to.options.length === this.maxLimit &&
                                    this.currentLimit === this.maxLimit
                                );
                            this.showLoadMore = this.currentLimit !== this.maxLimit;

                            if (this.listItemElement) {
                                let height = this.showResultsCount ? $scope.to.options.length - 1 : $scope.to.options.length;
                                if (height > this.maxVisibleItems) {
                                    height = this.maxVisibleItems;
                                }
                                height = (height * this.itemsHeight) + (this.showResultsCount ? 56 : 0) + 2; //borders
                                this.listItemElement.parentElement.style.height =
                                    this.listItemElement.parentElement.style.maxHeight = height.toString() + "px";
                            }

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
            onOpenClose: function (isOpen: boolean, $select, options) {
                this.catchClick(isOpen, this.catchClickId);

                $select.items = [];
                $scope.to.options = [];

                this.resetSettings();

                if (this.listItemElement) {
                    if (isOpen) {
                        angular.element(this.listItemElement).on("scroll", this.onScroll);
                    } else {
                        angular.element(this.listItemElement).off("scroll", this.onScroll);
                    }
                }
            },
            onScroll: function (event) {
                let dropdown = this;
                if (!$scope["bpFieldUserPicker"].isScrolling) {
                    //using requestAnimationFrame to throttle the event (see: https://developer.mozilla.org/en-US/docs/Web/Events/scroll)
                    window.requestAnimationFrame(() => {
                        let $select = $scope["bpFieldUserPicker"].$select;
                        let itemsHeight = $scope["bpFieldUserPicker"].itemsHeight;
                        let scrollTop = dropdown.scrollTop;
                        if (isNaN($select.activeIndex)) {
                            $select.activeIndex = 0;
                        }
                        if (scrollTop > ($select.activeIndex) * itemsHeight) {
                            $select.activeIndex = Math.round(scrollTop / itemsHeight);
                        }
                        $scope["bpFieldUserPicker"].isScrolling = false;
                    });
                }
                $scope["bpFieldUserPicker"].isScrolling = true;
            },
            onHighlight: function (option, $select) {
                if (this.isChoiceSelected(option, $select)) {
                    if (this.areStillChoicesAvailable($select)) {
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
                        $select.activeIndex = -1;
                    }
                } else {
                    this.currentSelectedItem = $select.activeIndex;
                }

                // need to manual scroll the list as the dropdown is not the default UI-select one
                if (this.listItemElement) {
                    let selectedTop = this.currentSelectedItem * this.itemsHeight;
                    let selectedBottom = selectedTop + this.itemsHeight;
                    let scrollTop = this.listItemElement.scrollTop;
                    let allowance = this.maxVisibleItems * this.itemsHeight;
                    if (selectedBottom > scrollTop + allowance) {
                        this.listItemElement.scrollTop = selectedBottom - allowance;
                    } else if (selectedTop < scrollTop) {
                        this.listItemElement.scrollTop = selectedTop;
                    }
                }
            },
            onRemove: function ($item, $select, formControl: ng.IFormController, options: AngularFormly.IFieldConfigurationObject) {
                options.validation.show = formControl.$invalid;
                this.removeImportedUsers($select);
                this.toggleScrollbar(true);
            },
            onSelect: function ($item, $select) {
                // On ENTER the ui-select reset the activeIndex to the first item of the list.
                // We need to hide the highlight until we select the proper entry
                if ($scope["uiSelectContainer"]) {
                    $scope["uiSelectContainer"].querySelector(".ui-select-choices").classList.add("disable-highlight");
                }

                let currentItem = $select.items.map(function (e) {
                    return e[$scope.to.valueProp];
                }).indexOf($item[$scope.to.valueProp]);

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
                this.removeImportedUsers($select);
                this.toggleScrollbar();
            }
        };
    }
}
