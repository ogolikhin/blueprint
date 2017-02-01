import {PreviewCenterController} from "./preview-center";
import {SystemTask} from "../../diagram/presentation/graph/shapes/";
//fixme: only one class per file
export class PreviewWingController {

    public wingTask: SystemTask;
    public previewDescription: string;
    public isReadonly: boolean = false;
    public givenOrThen: string;

    public static $inject = [
        "$scope",
        "$rootScope"
    ];

    constructor(private $scope: ng.IScope, private $rootScope: ng.IRootScopeService) {

        this.isReadonly = $scope.$parent["vm"].isReadonly;

        this.wingTask = $scope["wingCtrl"].systemTaskModel;

        this.givenOrThen = PreviewCenterController.getTaskLabelNameValue(this.wingTask.label, PreviewCenterController.getTaskLabel(this.wingTask));

        if (this.wingTask.description) {
            const tempParent = window.document.createElement("div");
            tempParent.innerHTML = <string>this.wingTask.description;
            if (tempParent.textContent.trim() !== "") {
                this.previewDescription = tempParent.innerHTML;
            }
        }
    }
}
export class PreviewWingDirective implements ng.IDirective {

    constructor(private $rootScope: ng.IRootScopeService) {
    }

    public static directive: any[] = [
        "$rootScope",
        ($rootScope: ng.IRootScopeService) => {
            return new PreviewWingDirective($rootScope);
        }];

    public restrict = "E";

    public scope = {
        isLeftWing: "@",
        systemTaskModel: "="
    };

    public controller = PreviewWingController;
    public controllerAs = "wingCtrl";
    public bindToController = true;

    public template: string = require("./preview-wing.html");

    public link(scope, elem, attr, ctrl) {
        function activateLinks() {

            function openUrl(event) {
                event.stopPropagation();
                event.preventDefault();
                const url = this.href;
                window.open(url, "_blank");
            }

            const divWing = <Element>elem[0];
            const aArtifacts = divWing.querySelectorAll(".storyteller-gt_text-container a");
            if (aArtifacts && aArtifacts.length) {
                const mentions = [];
                const links = [];
                for (let i = 0; i < aArtifacts.length; i++) {
                    const node = aArtifacts.item(i);
                    const nodeClass = (<Element>node).getAttribute("artifactid");
                    if (nodeClass) {
                        mentions.push(node);
                    } else {
                        links.push(node);
                    }
                }

                links.forEach(element => {
                    element.setAttribute("target", "_blank");
                    element.addEventListener("click", openUrl);
                });
            }
        }

        setTimeout(activateLinks, 0);
    }
}
