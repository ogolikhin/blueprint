import "angular"
import "angular-formly";

export class BPFieldTextMulti implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextMulti";
    public extends: string = "input";
    /* tslint:disable */
    public template: string = `<div class="input-group has-messages">
                <textarea
                    id="{{::id}}"
                    name="{{::id}}"
                    ng-model="model[options.key]"
                    ng-trim="false"
                    class="form-control"></textarea>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`;
    /* tslint:enable */
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync((scope) => {
            scope["fc"].$setTouched();
        });
    };
}