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
                }
            },
            wrongFormat: {
                expression: function ($viewValue, $modelValue, scope) {
                    return validationService.numberValidation.wrongFormat($viewValue, $modelValue, scope.to.decimalPlaces,
                                                                                                     localization, scope.options.data.isValidated);
                }
            },
            max: {
                expression: function ($viewValue, $modelValue, scope) {
                    return validationService.numberValidation.isMax($viewValue, $modelValue, scope.to.max,
                                                                                        localization, scope.options.data.isValidated);
                }
            },
            min: {
                expression: function ($viewValue, $modelValue, scope) {
                    return validationService.numberValidation.isMin($viewValue, $modelValue, scope.to.min,
                                                                                       localization, scope.options.data.isValidated);
                }
            }
        };
        $scope.options["validators"] = validators;

        $scope["bpFieldNumber"] = {
            keyup: this.blurOnKey
        };
    }
}

