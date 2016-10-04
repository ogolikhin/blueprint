import * as angular from "angular";
import "angular-formly";

export class BPFieldTextRTF implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTF";
    public template: string = require("./text-rtf.template.html");
    public wrapper: string = "bpFieldLabel";
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextRTFController;

    constructor() {
    }
}

export class BpFieldTextRTFController {
    static $inject: [string] = ["$scope"];

    constructor(private $scope: AngularFormly.ITemplateScope) {
        let to: AngularFormly.ITemplateOptions = {
            tinymceOptions: { // this will go to ui-tinymce directive
                plugins: "advlist autolink link image paste lists charmap print noneditable mention",
                mentions: {} // an empty mentions is needed when including the mention plugin and not using it
            }
        };
        angular.merge($scope.to, to);
    }
}