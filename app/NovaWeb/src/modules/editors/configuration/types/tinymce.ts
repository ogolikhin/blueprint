import "angular";
import "angular-formly";

export class BPFieldTinymce implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTinymce";
    public template: string = require("./tinymce.template.html");
    public wrapper: string = "bpFieldLabel";
    public defaultOptions: AngularFormly.IFieldConfigurationObject;
    public controller: Function = BpFieldTinymceController;

    constructor() {
        this.defaultOptions = {};
    }
}

export class BpFieldTinymceController {
    static $inject: [string] = ["$scope"];

    constructor(private $scope: AngularFormly.ITemplateScope) {
        let to: AngularFormly.ITemplateOptions = {
            tinymceOption: { // this will go to ui-tinymce directive
                plugins: "advlist autolink link image paste lists charmap print noneditable mention",
                mentions: {} // an empty mentions is needed when including the mention plugin and not using it
            }
        };
        angular.merge($scope.to, to);
    }
}