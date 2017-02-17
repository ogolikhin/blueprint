import "angular";
import "angular-formly";
import {IFormlyScope} from "../../formly-config";
import {BPFieldBaseController} from "../base-controller";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {Models} from "../../../../main/models";

export class BPFieldText implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldText";
    public extends: string = "input";
    public template: string = require("./text.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = ($scope: IFormlyScope, $element: ng.IAugmentedJQuery) => {
        $scope.$applyAsync(() => {
            $scope.fc.$setTouched();
        });

        $element.bind("dragover dragenter drop", $scope.options["data"].dragDrop);
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextController;
}

export class BpFieldTextController extends BPFieldBaseController {
    static $inject: [string] = ["$document", "$scope", "validationService"];

    constructor(protected $document: ng.IDocumentService,
                private $scope: IFormlyScope,
                private validationService: IValidationService) {
        super($document);

        $scope.options["data"].dragDrop = this.dragDrop;
        $scope.options["validators"] = {
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

        $scope["bpFieldText"] = {
            keyup: this.blurOnKey
        };
    }
}
