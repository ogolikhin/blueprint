import "angular-formly";
import {BPFieldBaseController} from "../base-controller";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";

export class BPFieldDatePicker implements AngularFormly.ITypeOptions {
    static $inject: [string] = ["$scope"];

    public name: string = "bpFieldDatepicker";
    public template: string = require("./date-picker.html");
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

        const datepickerAttributes: string[] = [
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

        const datepickerBindings: string[] = [
            "datepicker-mode",
            "min-date",
            "max-date"
        ];

        let datepickerNgModelAttrs = {};

        datepickerAttributes.forEach(function (attr) {
            datepickerNgModelAttrs[_.camelCase(attr)] = {attribute: attr};
        });

        datepickerBindings.forEach(function (binding) {
            datepickerNgModelAttrs[_.camelCase(binding)] = {bound: binding};
        });

        this.defaultOptions.ngModelAttrs = datepickerNgModelAttrs;
    }
}

export class BpFieldDatePickerController extends BPFieldBaseController {
    static $inject: [string] = ["$document", "$scope", "localization", "validationService"];

    constructor(protected $document: ng.IDocumentService,
                private $scope: AngularFormly.ITemplateScope,
                private localization: ILocalizationService,
                private validationService: IValidationService) {
        super($document);

        const to: AngularFormly.ITemplateOptions = {
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
        _.merge($scope.to, to);

        const validation = {
            messages: {
                date: `"` + localization.get("Property_Wrong_Format") + ` (` + to.placeholder + `)"`
            }
        };
        _.merge($scope.options.validation, validation);

        $scope.options["validators"] = {
            minDateSQL: {
                expression: function ($viewValue, $modelValue, scope) {
                    const isValid = validationService.dateValidation.minDateSQL($viewValue);
                    scope.to.minDateSQL = localization.current.formatDate(validationService.dateValidation.baseSQLDate, localization.current.shortDateFormat);
                    BPFieldBaseController.handleValidationMessage("minDateSQL", isValid, scope);
                    return true;
                }
            },
            minDate: {
                expression: function ($viewValue, $modelValue, scope) {
                    const isValid = validationService.dateValidation.minDate($viewValue,
                        scope.to.datepickerOptions.minDate, scope.options.data.isValidated);
                    scope.to.minDate = localization.current.formatDate(scope.to.datepickerOptions.minDate, localization.current.shortDateFormat);
                    BPFieldBaseController.handleValidationMessage("minDate", isValid, scope);
                    return true;
                }
            },
            maxDate: {
                expression: function ($viewValue, $modelValue, scope) {
                    const isValid =  validationService.dateValidation.maxDate($viewValue,
                        scope.to.datepickerOptions.maxDate, scope.options.data.isValidated);
                    scope.to.maxDate = localization.current.formatDate(scope.to.datepickerOptions.maxDate, localization.current.shortDateFormat);
                    BPFieldBaseController.handleValidationMessage("maxDate", isValid, scope);
                    return true;
                }
            }
        };

        $scope["bpFieldDatepicker"] = {
            opened: false,
            selected: false,
            open: function ($event) {
                this.opened = !this.opened;
            },
            select: function ($event) {
                let inputField = $event.target;
                inputField.focus();
                if (!this.selected && inputField.selectionStart === inputField.selectionEnd) {
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
