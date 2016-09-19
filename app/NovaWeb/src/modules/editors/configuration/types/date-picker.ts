import "angular"
import "angular-formly";
import { ILocalizationService } from "../../../core";
import { Models, Enums } from "../../../main/models";
import { Helper } from "../../../shared";

export class BPFieldDatePicker implements AngularFormly.ITypeOptions {
    static $inject: [string] = ["$scope"];

    public name: string = "bpFieldDatepicker";
    /* tslint:disable */
    public template: string = `<div class="input-group has-messages">
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
            </div>`;
    /* tslint:enable */
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public defaultOptions: AngularFormly.IFieldConfigurationObject;/* = {
        ngModelAttrs: this.datepickerNgModelAttrs,
        templateOptions: {
            datepickerOptions: {
                //format: this.localization.current.datePickerFormat,
                formatDay: "d",
                formatDayHeader: "EEE",
                //formatDayTitle: this.localization.current.datePickerDayTitle,
                initDate: new Date(),
                showWeeks: false,
                //startingDay: this.localization.current.firstDayOfWeek
            },
            datepickerAppendToBody: true,
            //clearText: this.localization.get("Datepicker_Clear"),
            //closeText: this.localization.get("Datepicker_Done"),
            //currentText: this.localization.get("Datepicker_Today"),
            //placeholder: this.localization.current.datePickerFormat.toUpperCase()
        },
        validation: {
            messages: {
                //date: `"` + this.localization.get("Property_Wrong_Format") + ` (` + this.localization.current.datePickerFormat.toUpperCase() + `)"`
            }
        },
        validators: {
            minDateSQL: {
                expression: function($viewValue, $modelValue, scope) {
                    let date = this.localization.current.toDate($modelValue || $viewValue, true);
                    let minDate = scope["minDateSQL"];

                    if (date && minDate) {
                        return date.getTime() >= minDate.getTime();
                    }
                    return true;
                }
            },
            minDate: {
                expression: function($viewValue, $modelValue, scope) {
                    if (!(<any> scope.options).data.isValidated) {
                        return true;
                    }

                    let date = this.localization.current.toDate($modelValue || $viewValue, true);
                    let minDate = this.localization.current.toDate(scope.to["datepickerOptions"].minDate, true);

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

                    let date = this.localization.current.toDate($modelValue || $viewValue, true);
                    let maxDate = this.localization.current.toDate(scope.to["datepickerOptions"].maxDate, true);

                    if (date && maxDate) {
                        return date.getTime() <= maxDate.getTime();
                    }

                    return true;
                }
            }
        }
    };*/
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync((scope) => {
            scope["fc"].$setTouched();
        });
    };
    public controller: Function = BpFieldDatePickerController;

    constructor() {
        this.defaultOptions = {};

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

        this.defaultOptions.ngModelAttrs = datepickerNgModelAttrs;
    }
}

export class BpFieldDatePickerController {
    static $inject: [string] = ["$scope", "localization"];

    private currentModelVal;

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService) {
        this.currentModelVal = $scope.model[$scope.options["key"]];

        /*// make sure the values are of type Date!
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
        // see http://stackoverflow.com/questions/3310569/what-is-the-significance-of-1-1-1753-in-sql-server
        $scope.minDateSQL = localization.current.toDate("1753-01-01", true);
        $scope.to["minDateSQL"] = localization.current.formatDate($scope.minDateSQL, localization.current.shortDateFormat);

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
        };*/
    }
}
