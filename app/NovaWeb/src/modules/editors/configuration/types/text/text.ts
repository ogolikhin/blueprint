import "angular";
import "angular-formly";
import {BPFieldBaseController} from "../base-controller";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";

export class BPFieldText implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldText";
    public extends: string = "input";
    public template: string = require("./text.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextController;
}

export class BpFieldTextController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "validationService"];

    constructor(private $scope: AngularFormly.ITemplateScope, private validationService: IValidationService) {
        super();

        let validators = {
            requiredCustom: {
                expression: function ($viewValue, $modelValue, scope) {
                    const isValid = validationService.systemValidation.validateName($modelValue);
                    BPFieldBaseController.handleValidationMessage("requiredCustom", isValid, scope);
                    return true;
                }
            }
        };
        $scope.options["validators"] = validators;

        $scope["bpFieldText"] = {
            keyup: this.blurOnKey
        };
    }
}
