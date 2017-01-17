import "angular";
import "angular-formly";
import {BPFieldBaseController} from "../base-controller";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {Models} from "../../../../main/models";

export class BPFieldText implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldText";
    public extends: string = "input";
    public template: string = require("./text.html");
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
                    if (scope.options["data"].propertyTypePredefined === Models.PropertyTypePredefined.Name) {
                        const isValid = validationService.systemValidation.validateName($modelValue);
                        BPFieldBaseController.handleValidationMessage("requiredCustom", isValid, scope);
                    }
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
