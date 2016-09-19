import "angular"
import "angular-formly";
import { BPFieldBaseController } from "./base-controller"

export class BPFieldText implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldText";
    public extends: string = "input";
    /* tslint:disable */
    public template: string = `<div class="input-group has-messages">
                <input type="text"
                    id="{{::id}}"
                    name="{{::id}}"
                    ng-model="model[options.key]"
                    ng-keyup="keyup($event)"
                    ng-trim="false"
                    class="form-control" />
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`;
    /* tslint:enable */
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public controller: Function = BpFieldTextController;
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync((scope) => {
            scope["fc"].$setTouched();
        });
    };
}

export class BpFieldTextController extends BPFieldBaseController {
    static $inject: [string] = ["$scope"];

    constructor(private $scope: AngularFormly.ITemplateScope) {
        super();

        $scope["keyup"] = this.blurOnKey;
    }
}