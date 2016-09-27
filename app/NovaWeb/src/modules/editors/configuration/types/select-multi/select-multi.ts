import * as angular from "angular";
import "angular-formly";
import { ILocalizationService } from "../../../../core";
import { BPFieldBaseController } from "../base-controller";

export class BPFieldSelectMulti implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldSelectMulti";
    public extends: string = "select";
    public template: string = require("./select-multi.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
            ($scope["options"] as AngularFormly.IFieldConfigurationObject).validation.show = ($scope["fc"] as ng.IFormController).$invalid;

            let uiSelectContainer = $element[0].querySelector(".ui-select-container");
            if (uiSelectContainer) {
                $scope["uiSelectContainer"] = uiSelectContainer;

                // perfect-scrollbar steals the mousewheel events unless inner elements have a "ps-child" class
                // Not needed for textareas
                let uiSelectInput = uiSelectContainer.querySelector("div:not(.ps-child)") as HTMLElement;
                if (uiSelectInput && !uiSelectInput.classList.contains("ps-child")) {
                    uiSelectInput.classList.add("ps-child");
                    uiSelectInput.classList.add("ui-select-input");
                    uiSelectInput.addEventListener("keydown", $scope["bpFieldSelectMulti"].closeDropdownOnTab, true);
                    uiSelectInput.addEventListener("click", $scope["bpFieldSelectMulti"].scrollIntoView, true);
                }

                $scope["bpFieldSelectMulti"].toggleScrollbar();
                $scope["uiSelectContainer"].firstElementChild.scrollTop = 0;
            }
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldSelectMultiController;

    constructor() {
    }
}

export class BpFieldSelectMultiController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization", "$timeout"];

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService, private $timeout: ng.ITimeoutService) {
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

        $scope["$on"]("$destroy", function () {
            if ($scope["uiSelectContainer"]) {
                if ($scope["uiSelectContainer"]) {
                    let uiSelectInput = $scope["uiSelectContainer"].querySelector(".ui-select-input") as HTMLElement;
                    if (uiSelectInput) {
                        uiSelectInput.removeEventListener("keydown", $scope["bpFieldSelectMulti"].closeDropdownOnTab, true);
                        uiSelectInput.removeEventListener("click", $scope["bpFieldSelectMulti"].scrollIntoView, true);
                    }
                }
            }
        });

        $scope["bpFieldSelectMulti"] = {
            $select: null,
            items: [],
            itemsHeight: 24,
            maxItemsToRender: 50,
            startingItem: 0,
            firstVisibleItem: 0,
            currentSelectedItem: -1,
            isScrolling: false,
            isOpen: false,
            labels: {
                noMatch: localization.get("Property_No_Matching_Options")
            },
            isChoiceSelected: function (item, $select): boolean {
                return $select.selected.map(function (e) { return e[$scope.to.valueProp]; }).indexOf(item[$scope.to.valueProp]) !== -1;
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
            closeDropdownOnTab: this.closeDropdownOnTab,
            scrollIntoView: this.scrollIntoView,
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
            limitItems: function ($select) {
                if ($select.items.length > this.maxItemsToRender) {
                    $select.items = $select.items.slice(0, this.maxItemsToRender);
                }
            },
            onScroll: function (event) {
                let dropdown = this;
                if (!$scope["bpFieldSelectMulti"].isScrolling) {
                    //using requestAnimationFrame to throttle the event (see: https://developer.mozilla.org/en-US/docs/Web/Events/scroll)
                    window.requestAnimationFrame(() => {
                        let $select = $scope["bpFieldSelectMulti"].$select;
                        let items = $scope["bpFieldSelectMulti"].items;
                        if ($select.search !== "") {
                            items = items.filter((item) => {
                                return item[$scope["to"].labelProp].toLowerCase().indexOf($select.search.toLowerCase()) !== -1;
                            });
                        }
                        let itemsHeight = $scope["bpFieldSelectMulti"].itemsHeight;
                        let itemsContainer = dropdown.firstElementChild as HTMLElement;
                        let maxItemsToRender = $scope["bpFieldSelectMulti"].maxItemsToRender;

                        let firstVisibleItem = Math.round(dropdown.scrollTop / itemsHeight);
                        let lastVisibleItem = Math.round((dropdown.scrollTop + dropdown.offsetHeight) / itemsHeight);
                        let visibleItems = lastVisibleItem - firstVisibleItem;
                        let itemsToKeepOffscreen = Math.round((maxItemsToRender - visibleItems) / 2);

                        let newStartingItem: number;
                        if (firstVisibleItem - itemsToKeepOffscreen <= 0) {
                            newStartingItem = 0;
                        } else if (lastVisibleItem + itemsToKeepOffscreen >= items.length) {
                            newStartingItem = items.length - maxItemsToRender;
                        } else {
                            newStartingItem = firstVisibleItem - itemsToKeepOffscreen;
                        }
                        if (firstVisibleItem !== $scope["bpFieldSelectMulti"].firstVisibleItem ||
                            newStartingItem !== $scope["bpFieldSelectMulti"].startingItem) {
                            $scope["$applyAsync"](() => {
                                let newIndex: number;
                                if (firstVisibleItem > $scope["bpFieldSelectMulti"].firstVisibleItem) { // scrolling down
                                    newIndex = lastVisibleItem - newStartingItem - 1;
                                } else { // scrolling up
                                    newIndex = firstVisibleItem - newStartingItem;
                                }
                                $select.activeIndex = newIndex;
                                $scope["bpFieldSelectMulti"].firstVisibleItem = firstVisibleItem;
                                $scope["bpFieldSelectMulti"].startingItem = newStartingItem;
                                $select.items = items.slice(newStartingItem, newStartingItem + maxItemsToRender);
                            });
                        }

                        let marginTop: number;
                        if (firstVisibleItem - itemsToKeepOffscreen <= 0) {
                            marginTop = 0;
                        } else if (lastVisibleItem + itemsToKeepOffscreen >= items.length) {
                            marginTop = (items.length - maxItemsToRender) * itemsHeight;
                        } else {
                            marginTop = (firstVisibleItem - itemsToKeepOffscreen) * itemsHeight;
                        }

                        let marginBottom: number;
                        if (lastVisibleItem + itemsToKeepOffscreen >= items.length) {
                            marginBottom = 0;
                        } else if (firstVisibleItem - itemsToKeepOffscreen <= 0) {
                            marginBottom = (items.length - maxItemsToRender) * itemsHeight;
                        } else {
                            marginBottom = (items.length - (lastVisibleItem + itemsToKeepOffscreen)) * itemsHeight;
                        }

                        itemsContainer.style.marginTop = marginTop.toString() + "px";
                        itemsContainer.style.marginBottom = marginBottom.toString() + "px";

                        $scope["bpFieldSelectMulti"].isScrolling = false;
                    });
                }
                $scope["bpFieldSelectMulti"].isScrolling = true;
            },
            onOpenClose: function (isOpen: boolean, $select, options) {
                this.isOpen = isOpen;
                this.items = options;
                this.$select = $select;

                let dropdown = this.findDropdown($select);
                if (dropdown && options.length > this.maxItemsToRender) {
                    let itemsContainer = dropdown.firstElementChild as HTMLElement;
                    if (isOpen) {
                        if (this.startingItem === 0) {
                            itemsContainer.style.marginTop = "0";
                            itemsContainer.style.marginBottom = ((options.length - this.maxItemsToRender) * this.itemsHeight).toString() + "px";
                            $select.activeIndex = 0;
                        }
                        angular.element(dropdown).on("scroll", this.onScroll);
                    } else {
                        angular.element(dropdown).off("scroll", this.onScroll);
                    }
                }
            },
            onHighlight: function (option, $select) {
                if (this.isChoiceSelected(option, $select)) {
                    if (this.areStillChoicesAvailable($select)) {
                        if ($select.activeIndex >= this.currentSelectedItem) {
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
            },
            onRemove: function ($item, $select, formControl: ng.IFormController, options: AngularFormly.IFieldConfigurationObject) {
                if (this.isOpen) {
                    $select.open = true; // force the dropdown to stay open on remove (if already open)

                    if (this.startingItem !== 0) { // user selected an item after scrolling
                        $select.items = this.items.slice(this.startingItem, this.startingItem + this.maxItemsToRender);
                    }

                    $select.activeIndex = $select.items.map(function (e) { return e[$scope.to.valueProp]; }).indexOf($item[$scope.to.valueProp]);
                }
                options.validation.show = formControl.$invalid;
                this.toggleScrollbar(true);
            },
            onSelect: function ($item, $select) {
                // On ENTER the ui-select reset the activeIndex to the first item of the list.
                // We need to hide the highlight until we select the proper entry
                if ($scope["uiSelectContainer"]) {
                    $scope["uiSelectContainer"].querySelector(".ui-select-choices").classList.add("disable-highlight");
                }

                if (this.startingItem !== 0) { // user selected an item after scrolling
                    $select.items = this.items.slice(this.startingItem, this.startingItem + this.maxItemsToRender);
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
