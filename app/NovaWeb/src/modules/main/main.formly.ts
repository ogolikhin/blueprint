import "angular";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import * as moment from "moment";
import {PrimitiveType} from "./models/enums";
import {ILocalizationService} from "../core";
import {Helper} from "../core/utils/helper";

// from http://stackoverflow.com/questions/31942788/angular-ui-datepicker-format-day-header-format-with-with-2-letters
formlyDecorate.$inject = ["$provide"];
export function formlyDecorate($provide): void {
    moment.locale(Helper.getFirstBrowserLanguage());

    let weekdaysMin = moment.weekdaysMin();
    weekdaysMin.forEach(function(item, index, arr) {
        arr[index] = item.substr(0, 1).toUpperCase();
    });

    delegated.$inject = ["$delegate"];
    function delegated($delegate) {
        let value = $delegate.DATETIME_FORMATS;

        value.DAY = moment.weekdays();
        value.SHORTDAY = weekdaysMin;
        value.MONTH = moment.months();
        value.SHORTMONTH = moment.monthsShort();

        return $delegate;
    }

    $provide.decorator("$locale", delegated);
}

formlyConfigExtendedFields.$inject = ["formlyConfig", "formlyValidationMessages", "localization"];
/* tslint:disable */
export function formlyConfigExtendedFields(formlyConfig: AngularFormly.IFormlyConfig, formlyValidationMessages: AngularFormly.IValidationMessages, localization: ILocalizationService
): void {
/* tslint:enable */
    let attributes = [
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

    let bindings = [
        "datepicker-mode",
        "min-date",
        "max-date"
    ];

    let ngModelAttrs = {};

    const customProperty = 2;

    let dateFormat = moment.localeData().longDateFormat("L");
    let datePickerFormat = Helper.uiDatePickerFormatAdaptor(dateFormat);

    let datePickerDayTitle = moment.localeData().longDateFormat("LL").toUpperCase();
    datePickerDayTitle = datePickerDayTitle.indexOf("Y") < datePickerDayTitle.indexOf("M") ? "yyyy MMMM" : "MMMM yyyy";

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
        extends: "input",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <div class="read-only-input" bp-tooltip="{{tooltip}}" bp-tooltip-truncated="true">{{model[options.key]}}</div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel"],
        controller: ["$scope", function($scope) {
            let currentModelVal = $scope.model[$scope.options.key];

            $scope.tooltip = "";

            if (currentModelVal) {
                switch ($scope.options.data.primitiveType) {
                    case PrimitiveType.Text:
                        $scope.tooltip = currentModelVal;
                        break;
                    case PrimitiveType.Date:
                        if (moment(currentModelVal).isValid()) {
                            if ($scope.options.data.lookup === customProperty) {
                                $scope.model[$scope.options.key] = moment(currentModelVal).startOf("day").format(dateFormat);
                            } else {
                                $scope.model[$scope.options.key] = moment(currentModelVal).format("L") + " " + moment(currentModelVal).format("LT");
                            }
                        }
                        break;
                    case PrimitiveType.Number:
                        $scope.model[$scope.options.key] = Helper.toLocaleNumber(currentModelVal.toString());
                        break;
                    default:
                }
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
                    ng-change="bpFieldText.change($event)"
                    class="form-control" />
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            templateOptions: {
            },
            validation: {
                messages: {
                    required: `"` + localization.get("Property_Cannot_Be_Empty") + `"`
                }
            }
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldText = {};

            $scope.bpFieldText.change = function ($event) {
                //TODO: This is just a stub, it will need to be refactored when "dirty" is implemented
                let artifactNameDiv = document.body.querySelector(".page-content .page-heading .artifact-heading .name");
                if (artifactNameDiv) {
                    if ($scope.fc.$dirty) {
                        let dirtyIcon = artifactNameDiv.querySelector("i.dirty-indicator");
                        if (!dirtyIcon) {
                            let div = document.createElement("DIV");
                            div.innerHTML = `<i class="dirty-indicator"></i>`;
                            artifactNameDiv.appendChild(div.firstChild);
                        }
                    }
                }
            };

            $scope.bpFieldText.keyup = blurOnEnterKey;
        }]
    });

    formlyConfig.setType({
        name: "bpFieldTextMulti",
        extends: "input",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <textarea
                    id="{{::id}}"
                    name="{{::id}}"
                    ng-model="model[options.key]"
                    ng-keyup="bpFieldText.keyup($event)"
                    ng-change="bpFieldText.change($event)"
                    class="form-control"></textarea>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            templateOptions: {
            },
            validation: {
                messages: {
                    required: `"` + localization.get("Property_Cannot_Be_Empty") + `"`
                }
            }
        },
        controller: ["$scope", function ($scope) {
            $scope.bpFieldTextMulti = {};

            $scope.bpFieldTextMulti.change = function ($event) {
                //TODO: This is just a stub, it will need to be refactored when "dirty" is implemented
                let artifactNameDiv = document.body.querySelector(".page-content .page-heading .artifact-heading .name");
                if (artifactNameDiv) {
                    if ($scope.fc.$dirty) {
                        let dirtyIcon = artifactNameDiv.querySelector("i.dirty-indicator");
                        if (!dirtyIcon) {
                            let div = document.createElement("DIV");
                            div.innerHTML = `<i class="dirty-indicator"></i>`;
                            artifactNameDiv.appendChild(div.firstChild);
                        }
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
                    ng-change="bpFieldNumber.change($event)"
                    class="form-control" />
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            templateOptions: {
            },
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

                        let value = $modelValue || $viewValue;
                        let decimalPlaces = scope.to["decimalPlaces"];

                        if (value && angular.isNumber(decimalPlaces)) {
                            let intValue = parseInt(value, 10);

                            return $viewValue.length <= (intValue.toString().length + 1 + decimalPlaces);
                        }
                        return true;
                    }
                },
                wrongFormat: {
                    expression: function($viewValue, $modelValue, scope) {
                        let value = $modelValue || $viewValue;
                        if (value) {
                            let separator = Helper.getDecimalSeparator();
                            let regExp = new RegExp("^-?[0-9]\\d*(\\" + separator + "\\d+)?$", "g");

                            return regExp.test(value);
                        }
                        return true;
                    }
                },
                max: {
                    expression: function($viewValue, $modelValue, scope) {
                        if (!(<any> scope.options).data.isValidated) {
                            return true;
                        }

                        let value = $modelValue || $viewValue;
                        if (value) {
                            value = Helper.parseLocaleNumber($modelValue || $viewValue);
                            let max = scope.to.max;

                            if (angular.isNumber(value) && !angular.isUndefined(max)) {
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

                        let value = $modelValue || $viewValue;
                        if (value) {
                            value = Helper.parseLocaleNumber($modelValue || $viewValue);
                            let min = scope.to.min;

                            if (angular.isNumber(value) && !angular.isUndefined(min)) {
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
                $scope.model[$scope.options.key] = Helper.toLocaleNumber(currentModelVal.toString());
            }

            $scope.bpFieldNumber.change = function ($event) {
                //TODO: This is just a stub, it will need to be refactored when "dirty" is implemented
                let artifactNameDiv = document.body.querySelector(".page-content .page-heading .artifact-heading .name");
                if (artifactNameDiv) {
                    if ($scope.fc.$dirty) {
                        let dirtyIcon = artifactNameDiv.querySelector("i.dirty-indicator");
                        if (!dirtyIcon) {
                            let div = document.createElement("DIV");
                            div.innerHTML = `<i class="dirty-indicator"></i>`;
                            artifactNameDiv.appendChild(div.firstChild);
                        }
                    }
                }
            };

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
                    class="form-control has-icon"
                    ng-click="bpFieldDatepicker.select($event)"
                    ng-blur="bpFieldDatepicker.blur($event)"
                    ng-change="bpFieldDatepicker.change($event)"
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
                    format: datePickerFormat,
                    formatDay: "d",
                    formatDayHeader: "EEE",
                    formatDayTitle: datePickerDayTitle,
                    initDate: new Date(),
                    showWeeks: false,
                    startingDay: (<any> moment.localeData()).firstDayOfWeek()
                },
                datepickerAppendToBody: true,
                clearText: localization.get("Datepicker_Clear"),
                closeText: localization.get("Datepicker_Done"),
                currentText: localization.get("Datepicker_Today"),
                placeholder: datePickerFormat.toUpperCase()
            },
            validation: {
                messages: {
                    required: `"` + localization.get("Property_Cannot_Be_Empty") + `"`,
                    date: `"` + localization.get("Property_Wrong_Format") + ` (` + datePickerFormat.toUpperCase() + `)"`
                }
            },
            validators: {
                minDate: {
                    expression: function($viewValue, $modelValue, scope) {
                        if (!(<any> scope.options).data.isValidated) {
                            return true;
                        }

                        let value = $modelValue || $viewValue;
                        let minDate = scope.to["datepickerOptions"].minDate;

                        if (value && minDate) {
                            value = moment(value).startOf("day");
                            minDate = moment(minDate).startOf("day");

                            scope.to["minDate"] = minDate.format("L");

                            return value.isSameOrAfter(minDate, "day");
                        }
                        return true;
                    }
                },
                maxDate: {
                    expression: function($viewValue, $modelValue, scope) {
                        if (!(<any> scope.options).data.isValidated) {
                            return true;
                        }

                        let value = $modelValue || $viewValue;
                        let maxDate = scope.to["datepickerOptions"].maxDate;

                        if (value && maxDate) {
                            value = moment(value).startOf("day");
                            maxDate = moment(maxDate).startOf("day");

                            scope.to["maxDate"] = maxDate.format("L");

                            return value.isSameOrBefore(maxDate, "day");
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
            if (angular.isString(currentModelVal)) {
                $scope.model[$scope.options.key] = moment(currentModelVal).startOf("day").toDate();
            } else if (angular.isDate(currentModelVal)) {
                $scope.model[$scope.options.key] = Helper.toStartOfTZDay(currentModelVal);
            }

            if ($scope.defaultValue) {
                $scope.defaultValue = moment($scope.defaultValue).startOf("day").toDate();
            }
            if ($scope.to["datepickerOptions"]) {
                if (angular.isString($scope.to["datepickerOptions"].maxDate)) {
                    $scope.to["datepickerOptions"].maxDate = moment($scope.to["datepickerOptions"].maxDate).startOf("day").toDate();
                }
                if (angular.isString($scope.to["datepickerOptions"].minDate)) {
                    $scope.to["datepickerOptions"].minDate = moment($scope.to["datepickerOptions"].minDate).startOf("day").toDate();
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

            $scope.bpFieldDatepicker.change = function ($event) {
                //TODO: This is just a stub, it will need to be refactored when "dirty" is implemented
                let artifactNameDiv = document.body.querySelector(".page-content .page-heading .artifact-heading .name");
                if (artifactNameDiv) {
                    if ($scope.fc.$dirty) {
                        let dirtyIcon = artifactNameDiv.querySelector("i.dirty-indicator");
                        if (!dirtyIcon) {
                            let div = document.createElement("DIV");
                            div.innerHTML = `<i class="dirty-indicator"></i>`;
                            artifactNameDiv.appendChild(div.firstChild);
                        }
                    }
                }
            };

            $scope.bpFieldDatepicker.keyup = blurOnEnterKey;
        }]
    });

    formlyConfig.setWrapper({
        name: "bpFieldLabel",
        template: `<div>
              <label for="{{id}}" class="control-label {{to.labelSrOnly ? 'sr-only' : ''}}" ng-if="to.label">
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
