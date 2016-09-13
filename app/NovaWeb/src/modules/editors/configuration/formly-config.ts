import "angular";
import "angular-sanitize";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import {PrimitiveType, PropertyLookupEnum} from "../../main/models/enums";
import {ILocalizationService, IMessageService} from "../../core";
import {Helper} from "../../shared";
import { FiletypeParser } from "../../shared/utils/filetypeParser";
import { IArtifactAttachments, IArtifactAttachmentsResultSet } from "../../shell/bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";
import { documentController } from "./controllers/document-field-controller";
import { actorController } from "./controllers/actor-field-controller";


formlyConfig.$inject = ["formlyConfig", "formlyValidationMessages", "localization", "$sce", "artifactAttachments", "$window", "messageService"];
/* tslint:disable */
export function formlyConfig(
    formlyConfig: AngularFormly.IFormlyConfig,
    formlyValidationMessages: AngularFormly.IValidationMessages,
    localization: ILocalizationService,
    $sce: ng.ISCEService,
    artifactAttachments: IArtifactAttachments,
    $window: ng.IWindowService,
    messageService: IMessageService
): void {
    /* tslint:enable */

    let datepickerAttributes: string[] = [
        "date-disabled",
        "custom-class",
        "show-weeks",
        "starting-day",
        "init-date",
        "min-mode",
        "max-mode",
        "format-day",
        "format-month",
        "format-year",
        "format-day-header",
        "format-day-title",
        "format-month-title",
        "year-range",
        "shortcut-propagation",
        "datepicker-popup",
        "show-button-bar",
        "current-text",
        "clear-text",
        "close-text",
        "close-on-date-selection",
        "datepicker-append-to-body"
    ];

    let datepickerBindings: string[] = [
        "datepicker-mode",
        "min-date",
        "max-date"
    ];

    let datepickerNgModelAttrs = {};

    angular.forEach(datepickerAttributes, function (attr) {
        datepickerNgModelAttrs[Helper.toCamelCase(attr)] = { attribute: attr };
    });

    angular.forEach(datepickerBindings, function (binding) {
        datepickerNgModelAttrs[Helper.toCamelCase(binding)] = { bound: binding };
    });

    let scrollIntoView = function (event) {
        let target = event.target.tagName.toUpperCase() !== "INPUT" ? event.target.querySelector("INPUT") : event.target;

        if (target) {
            target.scrollTop = 0;
            target.focus();
            angular.element(target).triggerHandler("click");
        }
    };

    let blurOnKey = function (event: KeyboardEvent, keyCode?: number | number[]) {
        let _keyCode: number[];
        if (!keyCode) {
            _keyCode = [13]; // 13 = Enter
        } else if (angular.isNumber(keyCode)) {
            _keyCode = [keyCode];
        } else if (angular.isArray(keyCode)) {
            _keyCode = keyCode;
        }

        let inputField = event.target as HTMLElement;
        let key = event.keyCode || event.which;
        if (_keyCode && inputField && _keyCode.indexOf(key) !== -1) {
            let inputFieldButton = inputField.parentElement.querySelector("span button") as HTMLElement;
            if (inputFieldButton) {
                inputFieldButton.focus();
            } else {
                inputField.blur();
            }
            event.stopPropagation();
            event.stopImmediatePropagation();
        }
    };

    let closeDropdownOnTab = function (event) {
        let key = event.keyCode || event.which;
        if (key === 9) { // 9 = Tab
            let escKey = document.createEvent("Events");
            escKey.initEvent("keydown", true, true);
            escKey["which"] = 27; // 27 = Escape
            escKey["keyCode"] = 27;
            event.target.dispatchEvent(escKey);

            blurOnKey(event, 9);
        }
    };

    formlyConfig.setType({
        name: "bpFieldReadOnly",
        /* tslint:disable */
        template: `
            <div class="input-group has-messages" ng-if="options.data.primitiveType == primitiveType.Text">
                <div id="{{::id}}" ng-if="options.data.isRichText" class="read-only-input richtext always-visible" perfect-scrollbar opts="scrollOptions"><div ng-bind-html="model[options.key]"></div></div>
                <div id="{{::id}}" ng-if="options.data.isMultipleAllowed && !options.data.isRichText" class="read-only-input multiple always-visible" perfect-scrollbar opts="scrollOptions"><div ng-bind-html="model[options.key]"></div></div>
                <div id="{{::id}}" ng-if="!options.data.isMultipleAllowed && !options.data.isRichText" class="read-only-input simple" bp-tooltip="{{tooltip}}" bp-tooltip-truncated="true">{{model[options.key]}}</div>
                <div ng-if="options.data.isMultipleAllowed || options.data.isRichText" class="overflow-fade"></div>
            </div>
            <div class="input-group has-messages" ng-if="options.data.primitiveType == primitiveType.Date">
                <div id="{{::id}}" class="read-only-input simple" bp-tooltip="{{tooltip}}" bp-tooltip-truncated="true">{{model[options.key]}}</div>
            </div>
            <div class="input-group has-messages" ng-if="options.data.primitiveType == primitiveType.Number">
                <div id="{{::id}}" class="read-only-input simple" bp-tooltip="{{tooltip}}" bp-tooltip-truncated="true">{{model[options.key]}}</div>
            </div>
            <div class="input-group has-messages" ng-if="options.data.primitiveType == primitiveType.User">
                <div id="{{::id}}" class="read-only-input simple" bp-tooltip="{{tooltip}}" bp-tooltip-truncated="true">{{model[options.key]}}</div>
            </div>
            <div class="input-group has-messages" ng-if="options.data.primitiveType == primitiveType.Choice && options.data.isMultipleAllowed">
                <div id="{{::id}}" class="read-only-input multiple always-visible" perfect-scrollbar opts="scrollOptions">
                    <div class="choice" ng-repeat="option in to.options | filter: filterMultiChoice" bp-tooltip="{{option.name}}" bp-tooltip-truncated="true">{{option.name}}</div>
                </div>
                <div class="overflow-fade"></div>
            </div>
            <div class="input-group has-messages" ng-if="options.data.primitiveType == primitiveType.Choice && !options.data.isMultipleAllowed">
                <div id="{{::id}}" class="read-only-input simple" bp-tooltip="{{tooltip}}" bp-tooltip-truncated="true">{{model[options.key]}}</div>
            </div>`
        ,
        /* tslint:enable */
        wrapper: ["bpFieldLabel"],
        controller: ["$scope", function ($scope) {
            let currentModelVal = $scope.model[$scope.options.key];
            let newValue: any;

            $scope.primitiveType = PrimitiveType;
            $scope.tooltip = "";
            $scope.scrollOptions = {
                minScrollbarLength: 20,
                scrollYMarginOffset: 4
            };

            $scope.filterMultiChoice = function (item): boolean {
                if (angular.isArray(currentModelVal)) {
                    return currentModelVal.indexOf(item.value) >= 0;
                }
                return false;
            };

            switch ($scope.options.data.primitiveType) {
                case PrimitiveType.Text:
                    if (currentModelVal) {
                        newValue = currentModelVal;
                    } else if ($scope.options.data) {
                        newValue = $scope.options.data.stringDefaultValue;
                    }
                    $scope.tooltip = newValue;
                    if ($scope.options.data.isRichText) {
                        newValue = $sce.trustAsHtml(Helper.stripWingdings(newValue));
                    } else if ($scope.options.data.isMultipleAllowed) {
                        newValue = $sce.trustAsHtml(Helper.escapeHTMLText(newValue || "").replace(/(?:\r\n|\r|\n)/g, "<br />"));
                    }
                    break;
                case PrimitiveType.Date:
                    let date = localization.current.toDate(currentModelVal || ($scope.options.data ? $scope.options.data.dateDefaultValue : null));
                    if (date) {
                        newValue = localization.current.formatDate(date,
                            $scope.options.data.lookup === PropertyLookupEnum.Custom ?
                                localization.current.shortDateFormat :
                                localization.current.longDateFormat);
                    } else {
                        newValue = $scope.options.data.stringDefaultValue;
                    }
                    $scope.tooltip = newValue;
                    break;
                case PrimitiveType.Number:
                    let decimal = localization.current.toNumber($scope.options.data.decimalPlaces);
                    newValue = localization.current.formatNumber(
                        currentModelVal || ($scope.options.data ? $scope.options.data.decimalDefaultValue : null), decimal);
                    $scope.tooltip = newValue;
                    break;
                case PrimitiveType.Choice:
                    newValue = currentModelVal || ($scope.options.data ? $scope.options.data.defaultValidValueId : null);
                    if (!$scope.options.data.isMultipleAllowed && $scope.options.data.validValues) {
                        if (angular.isNumber(newValue)) {
                            let values = $scope.options.data.validValues;
                            for (let key in values) {
                                if (values[key].id === newValue) {
                                    newValue = values[key].value;
                                    $scope.tooltip = newValue;
                                    break;
                                }
                            }
                        } else if (angular.isObject(newValue) && newValue.customValue) {
                            newValue = newValue.customValue;
                            $scope.tooltip = newValue;
                        }
                    }
                    break;
                case PrimitiveType.User:
                    newValue = currentModelVal || ($scope.options.data ? $scope.options.data.userGroupDefaultValue : null);
                    $scope.tooltip = newValue;
                    break;
                default:
                    break;

            }
            $scope.model[$scope.options.key] = newValue;
        }]
    });

    formlyConfig.setType({
        name: "bpFieldText",
        extends: "input",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <input type="text"
                    id="{{::id}}"
                    name="{{::id}}"
                    ng-model="model[options.key]"
                    ng-keyup="bpFieldText.keyup($event)"
                    ng-trim="false"
                    class="form-control" />
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        /*defaultOptions: {
         },*/
        link: function ($scope, $element, $attrs) {
            $scope.$applyAsync((scope) => {
                scope["fc"].$setTouched();
            });
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldText = {
                keyup: blurOnKey
            };
        }]
    });

    formlyConfig.setType({
        name: "bpFieldTextMulti",
        extends: "textarea",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <textarea
                    id="{{::id}}"
                    name="{{::id}}"
                    ng-model="model[options.key]"
                    ng-trim="false"
                    class="form-control"></textarea>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        /*defaultOptions: {
         },*/
        link: function ($scope, $element, $attrs) {
            $scope.$applyAsync((scope) => {
                scope["fc"].$setTouched();
            });
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldTextMulti = {};
        }]
    });

    formlyConfig.setType({
        name: "bpFieldSelect",
        extends: "select",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <div class="ui-select-single" ng-class="options.data.isValidated || options.data.lookup !== 2 ? 'no-custom' : 'allow-custom'"><ui-select
                    ng-model="model[options.key]"
                    ng-disabled="{{to.disabled}}"
                    uis-open-close="bpFieldSelect.onOpenClose(isOpen)">
                    <ui-select-match placeholder="{{to.placeholder}}">
                        <div class="ui-select-match-item-chosen" bp-tooltip="{{$select.selected[to.labelProp]}}" bp-tooltip-truncated="true">{{$select.selected[to.labelProp]}}</div>
                    </ui-select-match>
                    <ui-select-choices class="ps-child"
                        data-repeat="option[to.valueProp] as option in to.options | filter: {'name': $select.search}"
                        refresh="bpFieldSelect.refreshResults($select)"
                        refresh-delay="0">
                        <div class="ui-select-choice-item"
                            ng-class="{'ui-select-choice-item-selected': $select.selected[to.valueProp] === option[to.valueProp]}"
                            ng-bind-html="option[to.labelProp] | bpEscapeAndHighlight: $select.search"
                            bp-tooltip="{{option[to.labelProp]}}" bp-tooltip-truncated="true"></div>
                    </ui-select-choices>
                    <ui-select-no-choice>${localization.get("Property_No_Matching_Options")}</ui-select-no-choice>
                </ui-select></div>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            templateOptions: {
                placeholder: localization.get("Property_Placeholder_Select_Option"),
                valueProp: "value",
                labelProp: "name"
            }
        },
        link: function ($scope, $element, $attrs) {
            $scope.$applyAsync((scope) => {
                scope["fc"].$setTouched();
                (scope["options"] as AngularFormly.IFieldConfigurationObject).validation.show = (scope["fc"] as ng.IFormController).$invalid;

                let uiSelectContainer = $element[0].querySelector(".ui-select-container");
                if (uiSelectContainer) {
                    scope["uiSelectContainer"] = uiSelectContainer;
                    uiSelectContainer.addEventListener("keydown", closeDropdownOnTab, true);
                }
            });
        },
        controller: ["$scope", function ($scope) {
            let newCustomValueId = function (): number {
                return -1 * (Math.random() * 100 + 100); // not to conflict with special IDs like project (-1) or collections (-2)
            };

            $scope.$on("$destroy", function () {
                if ($scope["uiSelectContainer"]) {
                    $scope["uiSelectContainer"].removeEventListener("keydown", closeDropdownOnTab, true);
                }
            });

            let customValueId = -1;
            // we need to generate a custom id everytime, otherwise the select won't be able to recognize two different custom values

            let currentModelVal = $scope.model[$scope.options.key];
            if (angular.isObject(currentModelVal) && currentModelVal.customValue) {
                let newVal = currentModelVal.customValue;
                $scope.to.options.push({
                    value: customValueId,
                    name: newVal,
                    isCustom: true
                });
                $scope.model[$scope.options.key] = customValueId;
            }

            $scope.bpFieldSelect = {
                refreshResults: function ($select) {
                    if (!$scope.options.data.isValidated && $scope.options.data.lookup === PropertyLookupEnum.Custom) {
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
                        let currentVal = $scope.model[$scope.options.key];
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
        }]
    });

    formlyConfig.setType({
        name: "bpFieldSelectMulti",
        extends: "select",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <ui-select class="has-scrollbar"
                    multiple
                    ng-model="model[options.key]"
                    ng-disabled="{{to.disabled}}"
                    remove-selected="false"
                    on-remove="bpFieldSelectMulti.onRemove($item, $select, fc, options)"
                    on-select="bpFieldSelectMulti.onSelect($item, $select)"
                    close-on-select="false"
                    uis-open-close="bpFieldSelectMulti.onOpenClose(isOpen, $select, to.options)"
                    ng-mouseover="bpFieldSelectMulti.setUpDropdown($event, $select)"
                    ng-keydown="bpFieldSelectMulti.setUpDropdown($event, $select)">
                    <ui-select-match placeholder="{{to.placeholder}}">
                        <div class="ui-select-match-item-chosen" bp-tooltip="{{$item[to.labelProp]}}" bp-tooltip-truncated="true">{{$item[to.labelProp]}}</div>
                    </ui-select-match>
                    <ui-select-choices class="ps-child"
                        on-highlight="bpFieldSelectMulti.onHighlight(option, $select)"
                        data-repeat="option[to.valueProp] as option in to.options | filter: {'name': $select.search}">
                        <div class="ui-select-choice-item"
                            ng-bind-html="option[to.labelProp] | bpEscapeAndHighlight: $select.search"
                            bp-tooltip="{{option[to.labelProp]}}" bp-tooltip-truncated="true"></div>
                    </ui-select-choices>
                    <ui-select-no-choice>${localization.get("Property_No_Matching_Options")}</ui-select-no-choice>
                </ui-select>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            templateOptions: {
                placeholder: localization.get("Property_Placeholder_Select_Option"),
                valueProp: "value",
                labelProp: "name"
            },
            validators: {
                // despite what the Formly doc says, "required" is not supported in ui-select, therefore we need our own implementation.
                // See: https://github.com/angular-ui/ui-select/issues/1226#event-604773506
                requiredCustom: {
                    expression: function ($viewValue, $modelValue, $scope) {
                        if ((<any>$scope).$parent.to.required) { // TODO: find a better way to get the "required" flag
                            if (angular.isArray($modelValue) && $modelValue.length === 0) {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
        },
        link: function ($scope, $element, $attrs) {
            $scope.$applyAsync((scope) => {
                scope["fc"].$setTouched();
                (scope["options"] as AngularFormly.IFieldConfigurationObject).validation.show = (scope["fc"] as ng.IFormController).$invalid;

                let uiSelectContainer = $element[0].querySelector(".ui-select-container");
                if (uiSelectContainer) {
                    scope["uiSelectContainer"] = uiSelectContainer;
                    uiSelectContainer.addEventListener("keydown", closeDropdownOnTab, true);
                    uiSelectContainer.addEventListener("click", scrollIntoView, true);

                    scope["bpFieldSelectMulti"].toggleScrollbar();
                    scope["uiSelectContainer"].firstElementChild.scrollTop = 0;
                }
            });
        },
        controller: ["$scope", function ($scope) {
            let direction = Object.freeze({
                UP: -1,
                SAME: 0,
                DOWN: 1
            });
            let lastHighlighted = -1;

            $scope.$on("$destroy", function () {
                if ($scope["uiSelectContainer"]) {
                    $scope["uiSelectContainer"].removeEventListener("keydown", closeDropdownOnTab, true);
                    $scope["uiSelectContainer"].removeEventListener("click", scrollIntoView, true);
                }
            });

            $scope.bpFieldSelectMulti = {
                $select: null,
                items: [],
                itemsHeight: 24,
                maxItemsToRender: 50,
                startingItem: 0,
                firstVisibleItem: 0,
                isScrolling: false,
                isOpen: false,
                isChoiceSelected: function (item, $select): boolean {
                    return $select.selected.map(function (e) { return e[$scope.to.valueProp]; }).indexOf(item[$scope.to.valueProp]) !== -1;
                },
                nextFocusableChoice: function ($item, $select, dir): number {
                    let itemIndex = $select.items.map(function (e) { return e[$scope.to.valueProp]; }).indexOf($item[$scope.to.valueProp]);
                    if (itemIndex !== -1 && dir === direction.SAME) {
                        return itemIndex;
                    } else if (dir === direction.DOWN) {
                        for (let i = itemIndex + 1; i < $select.items.length; i++) {
                            let isSelected = this.isChoiceSelected($select.items[i], $select);
                            if (!isSelected) {
                                return i;
                            }
                        }
                    } else if (dir === direction.UP) {
                        for (let i = itemIndex - 1; i >= 0; i--) {
                            let isSelected = this.isChoiceSelected($select.items[i], $select);
                            if (!isSelected) {
                                return i;
                            }
                        }
                    }
                    return -1;
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
                    this.isOpen = isOpen;
                    this.$select = $select;
                    this.items = options;

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
                onScroll: function (event) {
                    let dropdown = this;
                    if (!$scope.bpFieldSelectMulti.isScrolling) {
                        //using requestAnimationFrame to throttle the event (see: https://developer.mozilla.org/en-US/docs/Web/Events/scroll)
                        window.requestAnimationFrame(() => {
                            let $select = $scope.bpFieldSelectMulti.$select;
                            let items = $scope.bpFieldSelectMulti.items;
                            if ($select.search !== "") {
                                items = items.filter((item) => {
                                    return item[$scope["to"].labelProp].toLowerCase().indexOf($select.search.toLowerCase()) !== -1;
                                });
                            }
                            let itemsHeight = $scope.bpFieldSelectMulti.itemsHeight;
                            let itemsContainer = dropdown.firstElementChild as HTMLElement;
                            let maxItemsToRender = $scope.bpFieldSelectMulti.maxItemsToRender;

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
                            if (firstVisibleItem !== $scope.bpFieldSelectMulti.firstVisibleItem ||
                                newStartingItem !== $scope.bpFieldSelectMulti.startingItem) {
                                $scope.$applyAsync(() => {
                                    let newIndex: number;
                                    if (firstVisibleItem > $scope.bpFieldSelectMulti.firstVisibleItem) { // scrolling down
                                        newIndex = lastVisibleItem - newStartingItem - 1;
                                    } else { // scrolling up
                                        newIndex = firstVisibleItem - newStartingItem;
                                    }
                                    $select.activeIndex = newIndex;
                                    $scope.bpFieldSelectMulti.firstVisibleItem = firstVisibleItem;
                                    $scope.bpFieldSelectMulti.startingItem = newStartingItem;
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

                            $scope.bpFieldSelectMulti.isScrolling = false;
                        });
                    }
                    $scope.bpFieldSelectMulti.isScrolling = true;
                },
                onHighlight: function (option, $select) {
                    let nextIndex = -1;
                    let highlightIndex = $select.items.map(function (e) { return e[$scope.to.valueProp]; }).indexOf(option[$scope.to.valueProp]);
                    if (this.isChoiceSelected(option, $select)) {
                        nextIndex = this.nextFocusableChoice(option, $select, lastHighlighted < highlightIndex ? direction.DOWN : direction.UP);
                        if (nextIndex !== -1) {
                            $select.activeIndex = nextIndex;
                            lastHighlighted = nextIndex;
                        } else {
                            $select.activeIndex = lastHighlighted;
                        }
                    } else {
                        lastHighlighted = highlightIndex;
                    }
                },
                onRemove: function ($item, $select, formControl: ng.IFormController, options: AngularFormly.IFieldConfigurationObject) {
                    if (this.isOpen) {
                        $select.open = true; // force the dropdown to stay open on remove (if already open)
                        $select.activeIndex = this.nextFocusableChoice($item, $select, direction.SAME);
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

                    let items = $scope.bpFieldSelectMulti.items;
                    let startingItem = $scope.bpFieldSelectMulti.startingItem;
                    let maxItemsToRender = $scope.bpFieldSelectMulti.maxItemsToRender;
                    if (startingItem !== 0) { // user selected an item after scrolling
                        $select.items = items.slice(startingItem, startingItem + maxItemsToRender);
                    }

                    let nextItem = this.nextFocusableChoice($item, $select, direction.DOWN);
                    if (nextItem === -1) {
                        nextItem = this.nextFocusableChoice($item, $select, direction.UP);
                    }
                    $select.activeIndex = nextItem;
                    $scope.$applyAsync((scope) => {
                        if (scope["uiSelectContainer"]) {
                            scope["uiSelectContainer"].querySelector(".ui-select-choices").classList.remove("disable-highlight");
                            scope["uiSelectContainer"].querySelector("input").focus();
                        }
                    });
                    this.toggleScrollbar();
                },
                // perfect-scrollbar steals the mousewheel events unless inner elements have a "ps-child" class.
                // Not needed for textareas
                setUpDropdown: function ($event, $select) {
                    if ($scope["uiSelectContainer"]) {
                        let elem = $scope["uiSelectContainer"].querySelector("div:not(.ps-child)") as HTMLElement;
                        if (elem && !elem.classList.contains("ps-child")) {
                            elem.classList.add("ps-child");
                        }
                    }
                    if ($select.items.length > this.maxItemsToRender) {
                        $select.items = $select.items.slice(0, this.maxItemsToRender);
                    }
                }
            };
        }]
    });

    formlyConfig.setType({
        name: "bpFieldNumber",
        extends: "input",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <input type="text"
                    id="{{::id}}"
                    name="{{::id}}"
                    ng-model="model[options.key]"
                    ng-keyup="bpFieldNumber.keyup($event)"
                    class="form-control" />
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            validators: {
                decimalPlaces: {
                    expression: function ($viewValue, $modelValue, $scope) {
                        if (!(<any>$scope.options).data.isValidated) {
                            return true;
                        }
                        let value = $modelValue || $viewValue;
                        if (value) {
                            let decimal = value.toString().split(localization.current.decimalSeparator);
                            if (decimal.length === 2) {
                                return decimal[1].length <= $scope.to["decimalPlaces"];
                            }
                        }
                        return true;
                    }
                },
                wrongFormat: {
                    expression: function ($viewValue, $modelValue, $scope) {
                        let value = $modelValue || $viewValue;
                        return !value ||
                            angular.isNumber(localization.current.toNumber(value, (
                                <any>$scope.options).data.isValidated ? $scope.to["decimalPlaces"] : null
                            ));
                    }
                },
                max: {
                    expression: function ($viewValue, $modelValue, $scope) {
                        if (!(<any>$scope.options).data.isValidated) {
                            return true;
                        }
                        let max = localization.current.toNumber($scope.to.max);
                        if (angular.isNumber(max)) {
                            let value = localization.current.toNumber($modelValue || $viewValue);
                            if (angular.isNumber(value)) {
                                return value <= max;
                            }
                        }
                        return true;
                    }
                },
                min: {
                    expression: function ($viewValue, $modelValue, $scope) {
                        if (!(<any>$scope.options).data.isValidated) {
                            return true;
                        }
                        let min = localization.current.toNumber($scope.to.min);
                        if (angular.isNumber(min)) {
                            let value = localization.current.toNumber($modelValue || $viewValue);
                            if (angular.isNumber(value)) {
                                return value >= min;
                            }
                        }
                        return true;
                    }
                }
            }
        },
        link: function ($scope, $element, $attrs) {
            $scope.$applyAsync((scope) => {
                scope["fc"].$setTouched();
            });
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldNumber = {
                keyup: blurOnKey
            };
        }]
    });

    formlyConfig.setType({
        name: "bpFieldTinymce",
        template: `<textarea ui-tinymce="options.data.tinymceOption" ng-model="model[options.key]" class="form-control form-tinymce"></textarea>`,
        wrapper: ["bpFieldLabel"],
        defaultOptions: {
            templateOptions: {
                tinymceOption: { // this will goes to ui-tinymce directive
                    plugins: "advlist autolink link image paste lists charmap print noneditable mention",
                    mentions: {} // an empty mentions is needed when including the mention plugin and not using it
                }
            }
        }
    });

    formlyConfig.setType({
        name: "bpFieldInlineTinymce",
        /* tslint:disable */
        template: `<div class="form-tinymce-toolbar" ng-class="options.key"></div><div ui-tinymce="to.tinymceOption" ng-model="model[options.key]" class="form-control form-tinymce" perfect-scrollbar></div>`,
        /* tslint:enable */
        defaultOptions: {
            templateOptions: {
                tinymceOption: { // this will goes to ui-tinymce directive
                    inline: true,
                    plugins: "advlist autolink link image paste lists charmap print noneditable mention",
                    init_instance_callback: function (editor) {
                        Helper.autoLinkURLText(editor.getBody());
                        editor.dom.setAttrib(editor.dom.select("a"), "data-mce-contenteditable", "false");
                        editor.dom.bind(editor.dom.select("a"), "click", function (e) {
                            let element = e.target as HTMLElement;
                            while (element && element.tagName.toUpperCase() !== "A") {
                                element = element.parentElement;
                            }
                            if (element && element.getAttribute("href")) {
                                window.open(element.getAttribute("href"), "_blank");
                            }
                        });
                    },
                    mentions: {} // an empty mentions is needed when including the mention plugin and not using it
                }
            }
        },
        controller: ["$scope", function ($scope) {
            let currentModelVal = $scope.model[$scope.options.key];
            $scope.model[$scope.options.key] = Helper.stripWingdings(currentModelVal);
        }]
    });
    formlyConfig.setType({
        name: "bpDocumentFile",
        /* tslint:disable:max-line-length */
        template:
        `<div ng-if="hasFile"> 
            <span class="input-group has-messages">
                <span class="input-group-addon">
                    <div class="thumb {{extension}}"></div>
                </span>
                <span class="form-control-wrapper">
                    <input type="text" value="{{fileName}}" class="form-control" readonly bp-tooltip="{{fileName}}" bp-tooltip-truncated="true" />
                </span>
                <span class="input-group-addon">
                    <span class="icon fonticon2-delete"></span>
                </span>
                <span class="input-group-addon">
                    <button class="btn btn-white btn-bp-small" ng-disabled="false" bp-tooltip="Change">Change</button>
                </span>
                <span class="input-group-addon">
                    <button class="btn btn-primary btn-bp-small" bp-tooltip="Download" ng-click="downloadFile()">Download</button>
                </span>
            </span>
         </div>
         <div ng-if="!hasFile">
            <span class="input-group has-messages">
                <span class="input-group-addon">
                    <div class="thumb fonticon2-attachment"></div>
                </span>
                <span class="form-control-wrapper">
                    <input type="text" " class="form-control" readonly/>
                </span>    
                <span class="input-group-addon">
                    <button class="btn btn-primary btn-bp-small" ng-disabled="false" bp-tooltip="Upload">Upload</button>
                </span>
            </span>
          </div>`,
        /* tslint:enable:max-line-length */
        controller: ["$scope", function ($scope) {
            documentController($scope, localization, artifactAttachments, $window, messageService);
        }]
    });

    formlyConfig.setType({
        name: "bpFieldDatepicker",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <input type="text"
                    id="{{::id}}"
                    name="{{::id}}"
                    ng-model="model[options.key]"
                    ng-model-options="{allowInvalid: true}"
                    class="form-control has-icon"
                    ng-click="bpFieldDatepicker.select($event)"
                    ng-blur="bpFieldDatepicker.blur($event)"
                    ng-keyup="bpFieldDatepicker.keyup($event)"
                    uib-datepicker-popup="{{to.datepickerOptions.format}}"
                    is-open="bpFieldDatepicker.opened"
                    datepicker-append-to-body="to.datepickerAppendToBody" 
                    datepicker-options="to.datepickerOptions" />
                <span class="input-group-btn">
                    <button type="button" class="btn btn-default" ng-click="bpFieldDatepicker.open($event)" ng-disabled="to.disabled"><i class="glyphicon glyphicon-calendar"></i></button>
                </span>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            ngModelAttrs: datepickerNgModelAttrs,
            templateOptions: {
                datepickerOptions: {
                    format: localization.current.datePickerFormat,
                    formatDay: "d",
                    formatDayHeader: "EEE",
                    formatDayTitle: localization.current.datePickerDayTitle,
                    initDate: new Date(),
                    showWeeks: false,
                    startingDay: localization.current.firstDayOfWeek
                },
                datepickerAppendToBody: true,
                clearText: localization.get("Datepicker_Clear"),
                closeText: localization.get("Datepicker_Done"),
                currentText: localization.get("Datepicker_Today"),
                placeholder: localization.current.datePickerFormat.toUpperCase()
            },
            validation: {
                messages: {
                    date: `"` + localization.get("Property_Wrong_Format") + ` (` + localization.current.datePickerFormat.toUpperCase() + `)"`
                }
            },
            validators: {
                minDate: {
                    expression: function($viewValue, $modelValue, scope) {
                        if (!(<any> scope.options).data.isValidated) {
                            return true;
                        }

                        let date = localization.current.toDate($modelValue || $viewValue, true);
                        let minDate = localization.current.toDate(scope.to["datepickerOptions"].minDate, true);

                        if (date && minDate) {
                            return date.getTime() >= minDate.getTime();
                        }
                        return true;
                    }
                },
                maxDate: {
                    expression: function($viewValue, $modelValue, scope) {
                        if (!(<any> scope.options).data.isValidated) {
                            return true;
                        }

                        let date = localization.current.toDate($modelValue || $viewValue, true);
                        let maxDate = localization.current.toDate(scope.to["datepickerOptions"].maxDate, true);

                        if (date && maxDate) {
                            return date.getTime() <= maxDate.getTime();
                        }

                        return true;
                    }
                }
            }
        },
        link: function($scope, $element, $attrs) {
            $scope.$applyAsync((scope) => {
                scope["fc"].$setTouched();
            });
        },
        controller: ["$scope", function ($scope) {
            // make sure the values are of type Date!
            let currentModelVal = $scope.model[$scope.options.key];
            if (currentModelVal) {
                $scope.model[$scope.options.key] = localization.current.toDate(currentModelVal, true);
            }

            if ($scope.defaultValue) {
                $scope.defaultValue = localization.current.toDate($scope.defaultValue, true);
            }
            if ($scope.to["datepickerOptions"]) {
                if (angular.isString($scope.to["datepickerOptions"].maxDate)) {
                    $scope.to["datepickerOptions"].maxDate = localization.current.toDate($scope.to["datepickerOptions"].maxDate, true);
                }
                if (angular.isString($scope.to["datepickerOptions"].minDate)) {
                    $scope.to["datepickerOptions"].minDate = localization.current.toDate($scope.to["datepickerOptions"].minDate, true);
                }
            }

            $scope.bpFieldDatepicker = {
                opened: false,
                selected: false,
                open: function ($event) {
                    this.opened = !this.opened;
                },
                select: function ($event) {
                    let inputField = $event.target;
                    inputField.focus();
                    if (!this.selected  && inputField.selectionStart === inputField.selectionEnd) {
                        inputField.setSelectionRange(0, inputField.value.length);
                    }
                    this.selected = !this.selected;
                },
                blur: function ($event) {
                    this.selected = false;
                },
                keyup: blurOnKey
            };
        }]
    });

    formlyConfig.setWrapper({
        name: "bpFieldLabel",
        template: `<div>
              <label for="{{id}}" ng-if="to.label && !to.tinymceOption"
                class="control-label {{to.labelSrOnly ? 'sr-only' : ''}}">
                <div bp-tooltip="{{to.label}}" bp-tooltip-truncated="true">{{to.label}}</div><div>:</div>
              </label>
              <formly-transclude></formly-transclude>
            </div>`
    });

    //<span class="input-group-btn" >
    //    <button type="button" class="btn btn-default" ng- click="bpFieldInheritFrom.delete($event)" > +</button>
    //        < /span>

    formlyConfig.setType({
        name: "bpFieldImage",
        /* tslint:disable:max-line-length */
        template: `<div class="inheritance-group">
                    <img ng-src="{{model[options.key]}}" class="actor-image" />
                    <i ng-show="model[options.key].length > 0" class="icon fonticon2-delete" bp-tooltip="Delete"  
                                                        ng-click="bpFieldInheritFrom.delete($event)"></i>
                    <i ng-hide="model[options.key].length > 0" bp-tooltip="Add"
                                    class="glyphicon glyphicon-plus image-actor-group" 
                                    ng-click="bpFieldInheritFrom.delete($event)"></i>
                </div>`
        /* tslint:enable:max-line-length */
    });

    //<input type="text"
    //id = "{{::id}}"
    //name = "{{::id}}"
    //ng - model="model[options.key].pathToProject"
    //ng - keyup="bpFieldText.keyup($event)"
    //class="form-control read-only-input"
    //enable = "false" />

//    <label class="control-label" >
//        <div bp- tooltip="{{ model[options.key].pathToProject }}" bp- tooltip - truncated="true" > {{ model[options.key].pathToProject }
//} </div>
//    < /label>                    
//    < a href= "#" > {{model[options.key].actorPrefix }}{ { model[options.key].actorId } }:{ { model[options.key].actorName } } </a>

    formlyConfig.setType({
        name: "bpFieldInheritFrom",
        /* tslint:disable:max-line-length */
        template: `<div class="input-group inheritance-group">
                    <div class="inheritance-path" ng-show="model[options.key].actorName.length > 0">
                        <div ng-show="{{model[options.key].pathToProject.length > 0 && (model[options.key].pathToProject.toString().length + model[options.key].actorPrefix.toString().length + model[options.key].actorId.toString().length + model[options.key].actorName.toString().length) < 38}}">
                            <span>{{model[options.key].pathToProject[0]}}</span>
                                <span ng-repeat="item in model[options.key].pathToProject track by $index"  ng-hide="$first">
                                  {{item}}
                                </span>   
                                <span><a href="#">{{model[options.key].actorPrefix }}{{ model[options.key].actorId }}:{{ model[options.key].actorName }}</a></span>                           
                            </div>                                                
                        <div ng-hide="{{model[options.key].pathToProject.length > 0 && (model[options.key].pathToProject.toString().length + model[options.key].actorPrefix.toString().length + model[options.key].actorId.toString().length + model[options.key].actorName.toString().length) < 38}}" bp-tooltip="{{model[options.key].pathToProject.join(' > ')}}">
                            <a  href="#">{{model[options.key].actorPrefix }}{{ model[options.key].actorId }}:{{ model[options.key].actorName }}</a>
                        </div>
                    </div>    
                    <div class="inheritance-path" ng-hide="model[options.key].actorName.length > 0">  </div>

                    <div ng-show="model[options.key].actorName.length > 0">
                        <div class="din">
                            <span class="icon fonticon2-delete" ng-disabled="to.isReadOnly" ng-click="bpFieldInheritFrom.delete($event)"
                                bp-tooltip="Delete"></span>
                        </div>   
                         <div class="fr">
                            <button class="btn btn-white btn-bp-small" ng-disabled="to.isReadOnly" bp-tooltip="Change"
                                    ng-click="bpFieldInheritFrom.change($event)">Change</button>
                        </div>        
                    </div>         
                    <div ng-hide="model[options.key].actorName.length > 0">
                         <button class="btn btn-primary btn-bp-small" ng-disabled="to.isReadOnly" bp-tooltip="Select"
                                ng-click="selectBaseActor()">Select</button>                       
                    </div>             
            </div>`,
        /* tslint:enable:max-line-length */
        wrapper: ["bpFieldLabel"],
        controller: ["$scope", function ($scope) {
            actorController($scope, localization, artifactAttachments, $window, messageService);
        }]
    });
 
    /* tslint:disable */
    /* not using this template yet
    formlyConfig.setWrapper({
        name: "bpFieldHasError",
        template: `<div class="form-group" ng-class="{'has-error': showError}">
                <label class="control-label" for="{{id}}">{{to.label}}</label>
                <formly-transclude></formly-transclude>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`
    });*/
    /* tslint:enable */

    /* tslint:disable */
    // the order in which the messages are defined is important!
    formlyValidationMessages.addTemplateOptionValueMessage("decimalPlaces", "decimalPlaces", localization.get("Property_Decimal_Places"), "", "Wrong decimal places");
    formlyValidationMessages.addTemplateOptionValueMessage("wrongFormat", "", localization.get("Property_Wrong_Format"), "", localization.get("Property_Wrong_Format"));
    formlyValidationMessages.addTemplateOptionValueMessage("max", "max", localization.get("Property_Value_Must_Be"), localization.get("Property_Suffix_Or_Less"), "Number too big");
    formlyValidationMessages.addTemplateOptionValueMessage("min", "min", localization.get("Property_Value_Must_Be"), localization.get("Property_Suffix_Or_Greater"), "Number too small");
    formlyValidationMessages.addTemplateOptionValueMessage("maxDate", "maxDate", localization.get("Property_Date_Must_Be"), localization.get("Property_Suffix_Or_Earlier"), "Date too big");
    formlyValidationMessages.addTemplateOptionValueMessage("minDate", "minDate", localization.get("Property_Date_Must_Be"), localization.get("Property_Suffix_Or_Later"), "Date too small");
    formlyValidationMessages.addTemplateOptionValueMessage("requiredCustom", "", localization.get("Property_Cannot_Be_Empty"), "", localization.get("Property_Cannot_Be_Empty"));
    formlyValidationMessages.addTemplateOptionValueMessage("required", "", localization.get("Property_Cannot_Be_Empty"), "", localization.get("Property_Cannot_Be_Empty"));
    /* tslint:enable */
}
