import "angular-formly";
import {BPFieldBaseController} from "../base-controller";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";

//fixme: only one class per file
export class BPFieldNumber implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldNumber";
    public extends: string = "input";
    public template: string = require("./number.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldNumberController;

    constructor() {
        //fixme: empty constructors can be removed

    }
}

export class BpFieldNumberController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization", "validationService"];

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService, private validationService: IValidationService) {
        super();

        let validators = {
            decimalPlaces: {
                expression: function ($viewValue, $modelValue, scope) {
                    return validationService.numberValidation.decimalPlaces($viewValue, $modelValue, scope.to.decimalPlaces, 
                                                                                                    localization, scope.options.data.isValidated);
                    // if (!scope.options.data.isValidated) {
                    //     return true;
                    // }
                    // let value = $modelValue || $viewValue;
                    // if (value) {
                    //     let decimal = value.toString().split(localization.current.decimalSeparator);
                    //     if (decimal.length === 2) {
                    //         return decimal[1].length <= scope.to.decimalPlaces;
                    //     }
                    // }
                    // return true;
                }
            },
            wrongFormat: {
                expression: function ($viewValue, $modelValue, scope) {
                    return validationService.numberValidation.wrongFormat($viewValue, $modelValue, scope.to.decimalPlaces, 
                                                                                                     localization, scope.options.data.isValidated);
                    // let value = $modelValue || $viewValue;
                    // return !value || angular.isNumber(
                    //         localization.current.toNumber(value, scope.options.data.isValidated ? scope.to.decimalPlaces : null)
                    //     );
                }
            },
            max: {
                expression: function ($viewValue, $modelValue, scope) {
                    return validationService.numberValidation.isMax($viewValue, $modelValue, scope.to.max, 
                                                                                        localization, scope.options.data.isValidated);
                    // if (!scope.options.data.isValidated) {
                    //     return true;
                    // }
                    // let max = localization.current.toNumber(scope.to.max);
                    // if (angular.isNumber(max)) {
                    //     let value = localization.current.toNumber($modelValue || $viewValue);
                    //     if (angular.isNumber(value)) {
                    //         return value <= max;
                    //     }
                    // }
                    // return true;
                }
            },
            min: {
                expression: function ($viewValue, $modelValue, scope) {
                    return validationService.numberValidation.isMin($viewValue, $modelValue, scope.to.min, 
                                                                                       localization, scope.options.data.isValidated);
                    // if (!scope.options.data.isValidated) {
                    //     return true;
                    // }
                    // let min = localization.current.toNumber(scope.to.min);
                    // if (angular.isNumber(min)) {
                    //     let value = localization.current.toNumber($modelValue || $viewValue);
                    //     if (angular.isNumber(value)) {
                    //         return value >= min;
                    //     }
                    // }
                    // return true;
                }
            }
        };
        $scope.options["validators"] = validators;

        $scope["bpFieldNumber"] = {
            keyup: this.blurOnKey
        };
    }
}

