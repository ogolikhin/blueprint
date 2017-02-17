import "angular";
import "angular-formly";
import {IFormlyScope} from "../../formly-config";
import {BPFieldBaseController} from "../base-controller";

export class BPFieldTextMulti implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextMulti";
    public extends: string = "input";
    public template: string = require("./text-multi.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope: IFormlyScope, $element: ng.IAugmentedJQuery) {
        $scope.$applyAsync(() => {
            $scope.fc.$setTouched();
        });

        $element.bind("dragover dragenter drop", $scope.options["data"].dragDrop);
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextMultiController;
}

export class BpFieldTextMultiController extends BPFieldBaseController {
    static $inject: [string] = ["$document", "$scope"];

    constructor(protected $document: ng.IDocumentService, private $scope: IFormlyScope) {
        super($document);

        $scope.options["data"].dragDrop = this.dragDrop;
    }
}
