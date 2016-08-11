import "angular";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import {PrimitiveType} from "./models/enums";
import {ILocalizationService} from "../core";
import {Helper} from "../shared";


formlyConfigExtendedFields.$inject = ["formlyConfig", "formlyValidationMessages", "localization"];
/* tslint:disable */
export function formlyConfigExtendedFields(formlyConfig: AngularFormly.IFormlyConfig, formlyValidationMessages: AngularFormly.IValidationMessages, localization: ILocalizationService
): void {
/* tslint:enable */
    let attributes: string[] = [
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

    let bindings: string[] = [
        "datepicker-mode",
        "min-date",
        "max-date"
    ];

    let ngModelAttrs = {};

    const customProperty: number = 2;

    angular.forEach(attributes, function(attr) {
        ngModelAttrs[Helper.toCamelCase(attr)] = {attribute: attr};
    });

    angular.forEach(bindings, function(binding) {
        ngModelAttrs[Helper.toCamelCase(binding)] = {bound: binding};
    });

    let blurOnEnterKey = function(event) {
        let inputField = event.target;
        let key = event.keyCode || event.which;
        if (inputField && key === 13) {
            let inputFieldButton = <HTMLInputElement> inputField.parentElement.querySelector("span button");
            if (inputFieldButton) {
                inputFieldButton.focus();
            } else {
                inputField.blur();
            }
        }
    };

    formlyConfig.setType({
        name: "bpFieldReadOnly",
        /* tslint:disable */
        template: `
            <div class="input-group has-messages" ng-if="options.data.primitiveType == primitiveType.Text">
                <div id="{{::id}}" ng-if="options.data.isRichText" class="read-only-input richtext always-visible" perfect-scrollbar opts="scrollOptions" ng-bind-html="model[options.key]"></div>
                <div id="{{::id}}" ng-if="options.data.isMultipleAllowed" class="read-only-input multiple always-visible" perfect-scrollbar opts="scrollOptions">{{model[options.key]}}</div>
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
            <div class="input-group has-messages" ng-if="options.data.primitiveType == primitiveType.Choice">
                <div id="{{::id}}" class="read-only-input multiple always-visible" perfect-scrollbar opts="scrollOptions">
                    <div class="choice" ng-repeat="option in to.options | filter: filterMultiChoice" bp-tooltip="{{option.name}}" bp-tooltip-truncated="true">{{option.name}}</div>
                </div>
                <div class="overflow-fade"></div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel"],
        controller: ["$scope", function($scope) {
            let currentModelVal = $scope.model[$scope.options.key];

            $scope.primitiveType = PrimitiveType;
            $scope.tooltip = "";
            $scope.scrollOptions = {
                minScrollbarLength: 20
            };

            $scope.filterMultiChoice = function(item): boolean {
                if (angular.isArray(currentModelVal)) {
                    return currentModelVal.indexOf(item.value) >= 0;
                }
                return false;
            };

                switch ($scope.options.data.primitiveType) {
                    case PrimitiveType.Text:
                        if (currentModelVal) {
                            $scope.tooltip = currentModelVal;
                        } else if ($scope.options.data && $scope.options.data.stringDefaultValue) {
                            $scope.model[$scope.options.key] = $scope.options.data.stringDefaultValue;
                        }
                        break;
                    case PrimitiveType.Date:
                        let date = localization.current.toDate(currentModelVal);
                        if (date) {
                            $scope.model[$scope.options.key] = localization.current.formatDate(date,
                                $scope.options.data.lookup === customProperty ?
                                    localization.current.shortDateFormat :
                                    localization.current.longDateFormat);
                        } else if ($scope.options.data && $scope.options.data.dateDefaultValue) {
                            $scope.model[$scope.options.key] = $scope.options.data.dateDefaultValue;
                        }
                        break;
                    case PrimitiveType.Number:
                        if (currentModelVal) {
                            $scope.model[$scope.options.key] = localization.current.formatNumber(currentModelVal);
                        } else if ($scope.options.data && $scope.options.data.decimalDefaultValue) {
                            $scope.model[$scope.options.key] = localization.current.formatNumber($scope.options.data.decimalDefaultValue);
                        }
                        break;
                    case PrimitiveType.Choice:
                        if (angular.isUndefined(currentModelVal) && $scope.options.data && $scope.options.data.defaultValidValueId) {
                            $scope.model[$scope.options.key] = [$scope.options.data.defaultValidValueId];
                        }
                        break;
                    case PrimitiveType.User:
                        if (angular.isUndefined(currentModelVal) && $scope.options.data && $scope.options.data.decimalDefaultValue) {
                            $scope.model[$scope.options.key] = $scope.options.data.userGroupDefaultValue;
                        }
                        break;
                    default:
                        break;

                }
            
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
                    class="form-control" />
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            validation: {
                messages: {
                    required: `"` + localization.get("Property_Cannot_Be_Empty") + `"`
                }
            }
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldText = {};

            $scope.bpFieldText.keyup = blurOnEnterKey;
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
                    class="form-control"></textarea>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            validation: {
                messages: {
                    required: `"` + localization.get("Property_Cannot_Be_Empty") + `"`
                }
            }
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
                <select
                    id="{{::id}}"
                    name="{{::id}}"
                    ng-model="model[options.key]"
                    class="form-control"></select>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            validation: {
                messages: {
                    required: `"` + localization.get("Property_Cannot_Be_Empty") + `"`
                }
            }
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldSelect = {};
        }]
    });

    formlyConfig.setType({
        name: "bpFieldSelectMulti",
        extends: "select",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <ui-select multiple data-ng-model="model[options.key]" data-required="{{to.required}}" data-disabled="{{to.disabled}}" remove-selected="false" ng-click="bpFieldSelectMulti.scrollIntoView($event)">
                    <ui-select-match placeholder="{{to.placeholder}}">
                        <div class="ui-select-match-item-chosen" bp-tooltip="{{$item[to.labelProp]}}" bp-tooltip-truncated="true">{{$item[to.labelProp]}}</div>
                    </ui-select-match>
                    <ui-select-choices data-repeat="option[to.valueProp] as option in to.options | filter: $select.search">
                        <div class="ui-select-choice-item" ng-bind-html="option[to.labelProp] | highlight: $select.search" bp-tooltip="{{option[to.labelProp]}}" bp-tooltip-truncated="true"></div>
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
            validation: {
                messages: {
                    required: `"` + localization.get("Property_Cannot_Be_Empty") + `"`
                }
            }
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldSelectMulti = {};

            $scope.bpFieldSelectMulti.scrollIntoView = function ($event) {
                let target = $event.target.tagName.toUpperCase() !== "INPUT" ? $event.target.querySelector("INPUT") : $event.target;

                if (target) {
                    target.scrollTop = 0;
                    target.focus();
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
                    ng-model-options="{allowInvalid: true}"
                    ng-keyup="bpFieldNumber.keyup($event)"
                    class="form-control" />
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            validation: {
                messages: {
                    required: `"` + localization.get("Property_Cannot_Be_Empty") + `"`
                }
            },
            validators: {
                decimalPlaces: {
                    expression: function($viewValue, $modelValue, scope) {
                        if (!(<any> scope.options).data.isValidated) {
                            return true;
                        }
                        let value = String($modelValue || $viewValue);
                        let decimalPlaces = localization.current.toNumber(scope.to["decimalPlaces"]);
                        if (value && decimalPlaces) {
                            let digits = String($modelValue || $viewValue).split(localization.current.decimalSeparator)[1] || "";
                            return digits.length <= decimalPlaces;
                        }
                        return true;
                    }
                },
                wrongFormat: {
                    expression: function($viewValue, $modelValue, scope) {
                        let value = $modelValue || $viewValue;
                        return value && localization.current.toNumber(value);
                    }
                },
                max: {
                    expression: function($viewValue, $modelValue, scope) {
                        if (!(<any> scope.options).data.isValidated) {
                            return true;
                        }

                        let value = localization.current.toNumber($modelValue || $viewValue);
                        if (angular.isNumber(value)) {
                            let max = localization.current.toNumber(scope.to.max);

                            if (angular.isNumber(max)) {
                                return value <= max;
                            }
                        }
                        return true;
                    }
                },
                min: {
                    expression: function($viewValue, $modelValue, scope) {
                        if (!(<any> scope.options).data.isValidated) {
                            return true;
                        }
                        let value = localization.current.toNumber($modelValue || $viewValue);
                        if (angular.isNumber(value)) {
                            let min = localization.current.toNumber(scope.to.min);

                            if (angular.isNumber(min)) {
                                return value >= min;
                            }
                        }
                        return true;
                    }
                }
            }
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldNumber = {};

            let currentModelVal = $scope.model[$scope.options.key];
            if (currentModelVal) {
                $scope.model[$scope.options.key] = localization.current.formatNumber(currentModelVal);
            }

            $scope.bpFieldNumber.keyup = blurOnEnterKey;
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
                    init_instance_callback: function(editor) {
                        Helper.autoLinkURLText(editor.getBody());
                        editor.dom.setAttrib(editor.dom.select("a"), "data-mce-contenteditable", "false");
                        editor.dom.bind(editor.dom.select("a"), "click", function(e) {
                            let element: HTMLElement = e.target;
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
        }
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
            ngModelAttrs: ngModelAttrs,
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
                    required: `"` + localization.get("Property_Cannot_Be_Empty") + `"`,
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
                            return date.getTime() > minDate.getTime();
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
                            return date.getTime() < maxDate.getTime();
                        }

                        return true;
                    }
                }
            }
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldDatepicker = {};

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

            $scope.bpFieldDatepicker.opened = false;
            $scope.bpFieldDatepicker.open = function ($event) {
                $scope.bpFieldDatepicker.opened = !$scope.bpFieldDatepicker.opened;
            };

            $scope.bpFieldDatepicker.selected = false;
            $scope.bpFieldDatepicker.select = function ($event) {
                let inputField = $event.target;
                inputField.focus();
                if (!$scope.bpFieldDatepicker.selected  && inputField.selectionStart === inputField.selectionEnd) {
                    inputField.setSelectionRange(0, inputField.value.length);
                }
                $scope.bpFieldDatepicker.selected = !$scope.bpFieldDatepicker.selected;
            };

            $scope.bpFieldDatepicker.blur = function ($event) {
                $scope.bpFieldDatepicker.selected = false;
            };

            $scope.bpFieldDatepicker.keyup = blurOnEnterKey;
        }]
    });

    formlyConfig.setWrapper({
        name: "bpFieldLabel",
        template: `<div>
              <label for="{{id}}" ng-if="to.label && !to.tinymceOption"
                class="control-label {{to.labelSrOnly ? 'sr-only' : ''}}"
                bp-tooltip="{{to.label}}" bp-tooltip-truncated="true">
                {{to.label}}
              </label>
              <formly-transclude></formly-transclude>
            </div>`
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
    formlyValidationMessages.addTemplateOptionValueMessage("wrongFormat", "", localization.get("Property_Wrong_Format"), "", localization.get("Property_Wrong_Format"));
    formlyValidationMessages.addTemplateOptionValueMessage("decimalPlaces", "decimalPlaces", localization.get("Property_Decimal_Places"), "", "Wrong decimal places");
    formlyValidationMessages.addTemplateOptionValueMessage("max", "max", localization.get("Property_Value_Must_Be"), localization.get("Property_Suffix_Or_Less"), "Number too big");
    formlyValidationMessages.addTemplateOptionValueMessage("min", "min", localization.get("Property_Value_Must_Be"), localization.get("Property_Suffix_Or_Greater"), "Number too small");
    formlyValidationMessages.addTemplateOptionValueMessage("maxDate", "maxDate", localization.get("Property_Date_Must_Be"), localization.get("Property_Suffix_Or_Earlier"), "Date too big");
    formlyValidationMessages.addTemplateOptionValueMessage("minDate", "minDate", localization.get("Property_Date_Must_Be"), localization.get("Property_Suffix_Or_Later"), "Date too small");
    /* tslint:enable */
}
