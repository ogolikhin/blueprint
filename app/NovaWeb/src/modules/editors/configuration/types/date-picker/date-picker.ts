import * as angular from "angular";
import "angular-formly";
import { ILocalizationService } from "../../../../core";
import { Helper } from "../../../../shared";
import { BPFieldBaseController } from "../base-controller";

export class BPFieldDatePicker implements AngularFormly.ITypeOptions {
    static $inject: [string] = ["$scope"];

    public name: string = "bpFieldDatepicker";
    public template: string = require("./date-picker.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public defaultOptions: AngularFormly.IFieldConfigurationObject;
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldDatePickerController;

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

export class BpFieldDatePickerController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization"];

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService) {
        super();

        let to: AngularFormly.ITemplateOptions = {
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
        };
        angular.merge($scope.to, to);

        let validation = {
            messages: {
                date: `"` + this.localization.get("Property_Wrong_Format") + ` (` + to.placeholder + `)"`
            }
        };
        angular.merge($scope.options.validation, validation);

        let validators = {
            minDateSQL: {
                expression: function($viewValue, $modelValue, scope) {
                    let date = localization.current.toDate($modelValue || $viewValue, true);
                    let minDate = scope.minDateSQL;

                    if (date && minDate) {
                        return date.getTime() >= minDate.getTime();
                    }
                    return true;
                }
            },
            minDate: {
                expression: function($viewValue, $modelValue, scope) {
                    if (!scope.options.data.isValidated) {
                        return true;
                    }

                    let date = localization.current.toDate($modelValue || $viewValue, true);
                    let minDate = localization.current.toDate(scope.to.datepickerOptions.minDate, true);

                    if (date && minDate) {
                        return date.getTime() >= minDate.getTime();
                    }
                    return true;
                }
            },
            maxDate: {
                expression: function($viewValue, $modelValue, scope) {
                    if (!scope.options.data.isValidated) {
                        return true;
                    }

                    let date = localization.current.toDate($modelValue || $viewValue, true);
                    let maxDate = localization.current.toDate(scope.to.datepickerOptions.maxDate, true);

                    if (date && maxDate) {
                        return date.getTime() <= maxDate.getTime();
                    }

                    return true;
                }
            }
        };
        $scope.options["validators"] = validators;

        // make sure the values are of type Date!
        let currentModelVal = $scope.model[$scope.options["key"]];
        if (currentModelVal) {
            $scope.model[$scope.options["key"]] = localization.current.toDate(currentModelVal, true);
        }
        if ($scope["defaultValue"]) {
            $scope["defaultValue"] = localization.current.toDate($scope["defaultValue"], true);
        }
        if (angular.isString($scope.to["datepickerOptions"].maxDate)) {
            $scope.to["datepickerOptions"].maxDate = localization.current.toDate($scope.to["datepickerOptions"].maxDate, true);
        }
        if (angular.isString($scope.to["datepickerOptions"].minDate)) {
            $scope.to["datepickerOptions"].minDate = localization.current.toDate($scope.to["datepickerOptions"].minDate, true);
        }
        // see http://stackoverflow.com/questions/3310569/what-is-the-significance-of-1-1-1753-in-sql-server
        $scope["minDateSQL"] = localization.current.toDate("1753-01-01", true);
        $scope.to["minDateSQL"] = localization.current.formatDate($scope["minDateSQL"], localization.current.shortDateFormat);

        $scope["bpFieldDatepicker"] = {
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
            keyup: this.blurOnKey
        };
    }
}
