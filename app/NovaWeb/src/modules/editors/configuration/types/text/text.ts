import "angular";
import "angular-formly";
import { BPFieldBaseController } from "../base-controller";

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
    public controller: Function = BpFieldTextController;
}

export class BpFieldTextController extends BPFieldBaseController {
    static $inject: [string] = ["$scope"];

    constructor(private $scope: AngularFormly.ITemplateScope) {
        super();

        $scope["bpFieldText"] = {
            keyup: this.blurOnKey
        };
    }
}