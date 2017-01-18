import "angular-formly";
import {BPFieldBaseController} from "../base-controller";
import {ILocalizationService} from "../../../../core/localization/localization.service";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";

//fixme: only one class per file
export class BPFieldNumber implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldNumber";
    public extends: string = "input";
    public template: string = require("./number.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldNumberController;
}

export class BpFieldNumberController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization", "validationService"];

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService, private validationService: IValidationService) {
        super();

        let validators = {
            decimalPlaces: {
                expression: function ($viewValue, $modelValue, scope) {
                    const isValid = validationService.numberValidation.decimalPlaces($viewValue, $modelValue, scope.to.decimalPlaces,
                                                                                                     scope.options.data.isValidated);
                    BPFieldBaseController.handleValidationMessage("decimalPlaces", isValid, scope);
                    return true;
                }
            },
            wrongFormat: {
                expression: function ($viewValue, $modelValue, scope) {
                    const isValid = validationService.numberValidation.wrongFormat($viewValue, $modelValue, scope.to.decimalPlaces,
                                                                                                     scope.options.data.isValidated);
                    BPFieldBaseController.handleValidationMessage("wrongFormat", isValid, scope);

                    return true;
                }
            },
            max: {
                expression: function ($viewValue, $modelValue, scope) {
                    const isValid = validationService.numberValidation.isMax($viewValue, $modelValue, scope.to.max,
                                                                                        scope.options.data.isValidated);
                    BPFieldBaseController.handleValidationMessage("max", isValid, scope);
                    return true;
                }
            },
            min: {
                expression: function ($viewValue, $modelValue, scope) {
                    const isValid = validationService.numberValidation.isMin($viewValue, $modelValue, scope.to.min,
                                                                                       scope.options.data.isValidated);
                    BPFieldBaseController.handleValidationMessage("min", isValid, scope);
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
