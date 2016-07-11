module Storyteller {
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

            this.previewDescription;
            if (this.wingTask.description) {
                var tempParent = window.document.createElement('div');
                tempParent.innerHTML = <string>this.wingTask.description;
                if (tempParent.textContent.trim() != "") this.previewDescription = tempParent.innerHTML;
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

        public templateUrl = "/Areas/Web/App/Components/Storyteller/Directives/PreviewWingTemplate.html";

        public link(scope, elem, attr, ctrl) {
            function activateLinks() {
                function openPropertyMW(event) {
                    event.stopPropagation();
                    event.preventDefault();
                    var artifactId = this.getAttribute("artifactid");
                    var parentHelper = document.getElementById("artifact-property-modal-helper");
                    if (parentHelper) {
                        parentHelper.innerHTML = artifactId;
                        parentHelper.click();
                    }
                }

                function openUrl(event) {
                    event.stopPropagation();
                    event.preventDefault();
                    var url = this.href;
                    window.open(url, "_blank");
                }

                var divWing = <Element>elem.context.childNodes[0];
                var aArtifacts = divWing.querySelectorAll(".storyteller-gt_text-container a");
                if (aArtifacts && aArtifacts.length) {
                    var mentions = [];
                    var links = [];
                    for (var i = 0; i < aArtifacts.length; i++) {
                        var node = aArtifacts.item(i);
                        var nodeClass = (<Element>node).getAttribute("artifactid");
                        if (nodeClass) {
                            mentions.push(node);
                        } else {
                            links.push(node);
                        }
                    }
                    mentions.forEach(elem => {
                        elem.setAttribute("target", "");
                        elem.addEventListener("click", openPropertyMW);
                    });
                    links.forEach(elem => {
                        elem.setAttribute("target", "_blank");
                        elem.addEventListener("click", openUrl);
                    });
                }
            }
            
            setTimeout(activateLinks, 0);
        }
    }

    angular.module("Storyteller").directive("previewWing", PreviewWingDirective.directive);
}
