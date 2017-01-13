import "angular";
import "angular-formly";

export class BPFieldTextMulti implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextMulti";
    public extends: string = "input";
    public template: string = require("./text-multi.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = ($scope, $element, $attrs) => {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
        });
    };
}
