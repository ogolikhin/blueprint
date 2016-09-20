import "angular"
import "angular-formly";

export class BPFieldTextMulti implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextMulti";
    public extends: string = "input";
    public template: string = require("./text-multi.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync((scope) => {
            scope["fc"].$setTouched();
        });
    };
}