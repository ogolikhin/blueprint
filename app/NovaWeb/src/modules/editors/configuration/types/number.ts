import "angular";
import "angular-formly";
import { ILocalizationService } from "../../../core";
import { BPFieldBaseController } from "./base-controller";

export class BPFieldNumber implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldNumber";
    public extends: string = "input";
    public template: string = require("./number.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public defaultOptions: AngularFormly.IFieldConfigurationObject;
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
        });
    };
    public controller: Function = BpFieldNumberController;

    constructor() {
        this.defaultOptions = {};
    }
}

export class BpFieldNumberController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization"];

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService) {
        super();

        let validators = {
            decimalPlaces: {
                expression: function ($viewValue, $modelValue, scope) {
                    if (!scope.options.data.isValidated) {
                        return true;
                    }
                    let value = $modelValue || $viewValue;
                    if (value) {
                        let decimal = value.toString().split(localization.current.decimalSeparator);
                        if (decimal.length === 2) {
                            return decimal[1].length <= scope.to.decimalPlaces;
                        }
                    }
                    return true;
                }
            },
            wrongFormat: {
                expression: function ($viewValue, $modelValue, scope) {
                    let value = $modelValue || $viewValue;
                    return !value || angular.isNumber(
                        localization.current.toNumber(value, scope.options.data.isValidated ? scope.to.decimalPlaces : null)
                        );
                }
            },
            max: {
                expression: function ($viewValue, $modelValue, scope) {
                    if (!scope.options.data.isValidated) {
                        return true;
                    }
                    let max = localization.current.toNumber(scope.to.max);
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
                expression: function ($viewValue, $modelValue, scope) {
                    if (!scope.options.data.isValidated) {
                        return true;
                    }
                    let min = localization.current.toNumber(scope.to.min);
                    if (angular.isNumber(min)) {
                        let value = localization.current.toNumber($modelValue || $viewValue);
                        if (angular.isNumber(value)) {
                            return value >= min;
                        }
                    }
                    return true;
                }
            }
        };
        $scope.options["validators"] = validators;

        $scope["bpFieldNumber"] = {
            keyup: this.blurOnKey
        };
    }
}
