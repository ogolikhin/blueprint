import "angular";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import * as moment from "moment";
import {ILocalizationService} from "../core";
import {Helper} from "../core/utils/helper";

// from http://stackoverflow.com/questions/31942788/angular-ui-datepicker-format-day-header-format-with-with-2-letters
formlyDecorate.$inject = ["$provide"];
export function formlyDecorate($provide): void {
    moment.locale(Helper.getFirstBrowserLanguage());
    let weekedays = moment.weekdays();
    weekedays.forEach(function(item, index, arr) {
        arr[index] = item.substr(0, 1).toUpperCase();
    });

    delegated.$inject = ["$delegate"];
    function delegated($delegate) {
        let value = $delegate.DATETIME_FORMATS;

        value.SHORTDAY = weekedays;

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

    function uiDatePickerFormatAdaptor(format: string): string  {
        return format.replace(/D/g, "d").replace(/Y/g, "y");
    }

    angular.forEach(attributes, function(attr) {
        ngModelAttrs[Helper.camelCase(attr)] = {attribute: attr};
    });

    angular.forEach(bindings, function(binding) {
        ngModelAttrs[Helper.camelCase(binding)] = {bound: binding};
    });

    formlyConfig.setType({
        name: "tinymce",
        template: `<textarea ui-tinymce="options.data.tinymceOption" ng-model="model[options.key]" class="form-control form-tinymce"></textarea>`,
        wrapper: ["bootstrapLabel"],
        defaultOptions: {
            templateOptions: {
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standard tinymce option
                    plugins: "advlist autolink link image paste lists charmap print noneditable"
                }
            }
        }
    });

    formlyConfig.setType({
        name: "tinymceInline",
        /* tslint:disable */
        template: `<div class="form-tinymce-toolbar" ng-class="options.key"></div><div ui-tinymce="to.tinymceOption" ng-model="model[options.key]" class="form-control form-tinymce" perfect-scrollbar></div>`,
        /* tslint:enable */
        wrapper: ["bootstrapLabel"],
        defaultOptions: {
            templateOptions: {        
                tinymceOption: { // this will goes to ui-tinymce directive
                    inline: true,
                    //fixed_toolbar_container: ".form-tinymce-toolbar",
                    plugins: "advlist autolink link image paste lists charmap print noneditable" //mentions",
                    //mentions: {
                    //    source: tinymceMentionsData,
                    //    delay: 100,
                    //    items: 5,
                    //    queryBy: "fullname",
                    //    insert: function (item) {
                    //        return `<a class="mceNonEditable" href="mailto:` + item.emailaddress + `" title="ID# ` + item.id + `">` + item.fullname + `</a>`;
                    //    }
                    //},
                }
            }
        }
    });
    /*formlyConfig.setWrapper({
     name: 'hasError',
     template: `<div class="form-group" ng-class="{\'has-error\': showError}">
     <label class="control-label" for="{{id}}">{{to.label}}</label>
     <formly-transclude></formly-transclude>
     <div ng-messages="fc.$error" ng-if="showError" class="text-danger">
     <div ng-message="{{ ::name }}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
     </div>
     </div>`
     });*/
    //formlyValidationMessages.addStringMessage("dateIsBetweenMinMax", "This field is required");

    formlyConfig.setType({
        name: "datepicker",
        /* tslint:disable */
        template: `<div class="form-datepicker input-group">
                <input type="text"
                    id="{{::id}}"
                    name="{{::id}}"
                    ng-model="model[options.key]"
                    class="form-control "
                    ng-click="datepicker.open($event)"
                    uib-datepicker-popup="{{to.datepickerOptions.format}}"
                    is-open="datepicker.opened"
                    datepicker-append-to-body="to.datepickerAppendToBody" 
                    datepicker-options="to.datepickerOptions" />
                <span class="input-group-btn">
                    <button type="button" class="btn btn-default" ng-click="datepicker.open($event)" ng-disabled="to.disabled"><i class="glyphicon glyphicon-calendar"></i></button>
                </span>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bootstrapLabel", "bootstrapHasError"],
        defaultOptions: {
            ngModelAttrs: ngModelAttrs,
            templateOptions: {
                datepickerOptions: {
                    format: uiDatePickerFormatAdaptor(moment.localeData().longDateFormat("L")),
                    formatDay: "d",
                    formatDayHeader: "EEE",
                    initDate: new Date(),
                    showWeeks: false,
                    startingDay: (<any> moment.localeData()).firstDayOfWeek()
                },
                datepickerAppendToBody: true,
                clearText: localization.get("Datepicker_Clear"),
                closeText: localization.get("Datepicker_Done"),
                currentText: localization.get("Datepicker_Today"),
                placeholder: moment.localeData().longDateFormat("L"),
                onKeyup: function($viewValue, $modelValue, scope) {
                    //This is just a stub!
                    let initValue = $modelValue.initialValue || $modelValue.defaultValue;
                    let momentInit = moment(initValue, moment.localeData().longDateFormat("L"));
                    if (momentInit.isValid()) {
                        initValue = momentInit.startOf("day").format("L");
                    }
                    let inputValue = $viewValue || (<any> document.getElementById(scope.id)).value;
                    let momentInput = moment(inputValue, moment.localeData().longDateFormat("L"));
                    if (momentInput.isValid()) {
                        inputValue = momentInput.startOf("day").format("L");
                    }
                    let artifactNameDiv = document.body.querySelector(".page-content .page-heading .artifact-heading .name");
                    if (artifactNameDiv) {
                        let dirtyIcon = artifactNameDiv.querySelector("i.dirty-indicator");
                        if (dirtyIcon) {
                            artifactNameDiv.removeChild(dirtyIcon);
                        }
                        if (initValue !== inputValue) {
                            let div = document.createElement("DIV");
                            div.innerHTML = `<i class="dirty-indicator"></i>`;
                            artifactNameDiv.appendChild(div.firstChild);
                        }
                    }
                }
            },
            validation: {
                messages: {
                    required: `"` + localization.get("Property_Cannot_Be_Empty", "Value cannot be empty") + `"`,
                    date: `"` + localization.get("Property_Wrong_Format") + ` (` + moment.localeData().longDateFormat("L") + `)"`
                }
            },
            validators: {
                dateIsGreaterThanMin: {
                    expression: function($viewValue, $modelValue, scope) {
                        let value = $modelValue || $viewValue;
                        let minDate = scope.to["datepickerOptions"].minDate;

                        if (value && minDate) {
                            value = moment(value).startOf("day");
                            minDate = moment(minDate).startOf("day");

                            let isGreaterThanMin = value.isSameOrAfter(minDate, "day");
                            let messageText = localization.get("Property_Must_Be_Greater") + " " + minDate.format("L");
                            let messageId = scope.id + "-dateIsGreaterThanMin";

                            if (!isGreaterThanMin) {
                                setTimeout(function() {
                                    document.getElementById(messageId).innerHTML = messageText;
                                }, 100);
                                return false;
                            }
                        }
                        return true;
                    },
                    message: `""`
                },
                dateIsLessThanMax: {
                    expression: function($viewValue, $modelValue, scope) {
                        let value = $modelValue || $viewValue;
                        let maxDate = scope.to["datepickerOptions"].maxDate;

                        if (value && maxDate) {
                            value = moment(value).startOf("day");
                            maxDate = moment(maxDate).startOf("day");

                            let isLessThanMax = value.isSameOrBefore(maxDate, "day");
                            let messageText = localization.get("Property_Must_Be_Less") + " " + maxDate.format("L");
                            let messageId = scope.id + "-dateIsLessThanMax";

                            if (!isLessThanMax) {
                                setTimeout(function() {
                                    document.getElementById(messageId).innerHTML = messageText;
                                }, 100);
                                return false;
                            }
                        }
                        return true;
                    },
                    message: `""`
                }
            }
        },
        controller: ["$scope", function ($scope) {
            $scope.datepicker = {};

            // make sure the initial value is of type DATE!
            let currentModelVal = $scope.model[$scope.options.key];
            if (typeof (currentModelVal) === "string") {
                $scope.model[$scope.options.key] = new Date(currentModelVal);
            }

            $scope.datepicker.opened = false;

            $scope.datepicker.open = function ($event) {
                $scope.datepicker.opened = !$scope.datepicker.opened;
            };
        }]
    });
}
