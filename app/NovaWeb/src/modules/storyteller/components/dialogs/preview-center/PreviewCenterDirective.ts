module Storyteller {
    export class PreviewCenterControllerHelper {
        constructor() { };

        public getPropertyValueByName(properties: IArtifactProperty[], name: string) {
            for (let property of properties) {
                if (property.name.toLowerCase().indexOf(name.toLowerCase()) === 0) {
                    return property.value;
                }
            }
        }

        public getPropertyByName(properties: IArtifactProperty[], name: string) {
            for (let property of properties) {
                if (property.name.toLowerCase().indexOf(name.toLowerCase()) === 0) {
                    return property;
                }
            }
        }

        public getPropertyTypeIdByName(properties: IArtifactProperty[], name: string) {
            for (let property of properties) {
                if (property.name.toLowerCase().indexOf(name.toLowerCase()) === 0) {
                    return property.propertyTypeId;
                }
            }
        }

        public getUserStoryIdForSubArtifact(process: IProcess, subArtifactId: number) {
            if (process.shapes) {
                for (let shape of process.shapes) {
                    if (shape.id === subArtifactId) {
                        return this.getUserStoryId(shape);
                    }
                }
            }
            return null;
        }

        public getUserStoryId(processShape: IProcessShape) {
            var propertyValues = processShape.propertyValues;
            if (propertyValues) {
                var storyLinks = propertyValues["storyLinks"];
                if (storyLinks && storyLinks["value"]) {
                    return storyLinks["value"]["associatedReferenceArtifactId"];
                }
            }
            return null;
        }
    }

    export class PreviewCenterController {
        public centerTask: UserTask;
        public previousSystemTask: SystemTask;
        public nextSystemTask: SystemTask;
        public isUserSystemProcess: boolean;
        private subArtifactId: number;
        private isTabsVisible: boolean;
        private showMoreActiveTab: boolean[] = [true, false];

        public title: string;
        public acceptanceCriteria: string;
        public businessRules: string;
        public nonfunctionalRequirements: string;
        public isReadonly: boolean = false;
        public isSMB: boolean = false;
        public isProjectOnlySearch: boolean = true;
        public when: string;

        public static $inject = [
            "$window",
            "$scope",
            "$rootScope",
            "artifactUtilityService",
            "processModelService"
        ];

        public resizeContentAreas = function (isTabSetVisible) {
            var availHeight = window.innerHeight ? window.innerHeight :
                (document.documentElement && document.documentElement.clientHeight ? document.documentElement.clientHeight :
                    (document.body ? document.body.clientHeight : screen.availHeight));
            //unfortunately this is very dependant on the way the GWT window is made :-(
            //the following is based on 930px (availHeight) - (25px (hidden tabs) or 192px (visible tabs) - 500px of other elements)
            var tabSetHeight = isTabSetVisible ? 201 : 25;
            var compensationValue = 500;
            var acceptanceCriteriaMaxHeight = availHeight - compensationValue - tabSetHeight;
            var acceptanceCriteria = <any>document.body.querySelector(".modal-content-autoscroll");
            if (acceptanceCriteria) {
                acceptanceCriteria.setAttribute("style", "max-height:" + acceptanceCriteriaMaxHeight + "px");
            }
        }

        public showMore(type: string, event: any) {
            // select tab
            if (type === "label") {
                this.isTabsVisible = !this.isTabsVisible;

            } else if (type === "nfr") {
                this.isTabsVisible = true;
                this.showMoreActiveTab[0] = true;
                this.showMoreActiveTab[1] = false;

            } else if (type === "br") {
                this.isTabsVisible = true;
                this.showMoreActiveTab[0] = false;
                this.showMoreActiveTab[1] = true;
            }
            this.resizeContentAreas(this.isTabsVisible);
            event.stopPropagation();
        }

        public strIsNotEmpty = (value: string): boolean => {
            //todo: Settings of tinymce should be changed
            if (value && $(value).text().trim().replace("\u200B", "").length > 0) {
                return true;
            }
            return false;
        }

        public static getTaskLabelNameValue(name: string, label: string): string {
            if (label && 0 !== label.length) {
                return label;
            }
            if (name) {
                return name;
            }
            return "";
        }

        public static getTaskLabel(task: IDiagramNode): string {
            var label: string = "";
            if (task.model && task.model.propertyValues["label"] && task.model.propertyValues["label"]) {
                label = task.model.propertyValues["label"].value;
            }
            return label;
        }

        constructor(
            private $window: ng.IWindowService,
            private $scope: ng.IScope,
            private $rootScope: ng.IRootScopeService,
            private artifactUtilityService: Shell.IArtifactUtilityService,
            private processModelService: IProcessModelService) {

            this.isReadonly = $scope.$parent["vm"].isReadonly;

            let isSMBVal = $rootScope["config"].settings.StorytellerIsSMB;

            if (isSMBVal.toLowerCase() === "true") {
                this.isSMB = true;
            }

            this.centerTask = $scope["centerCtrl"].userTaskModel;
            
            this.when = PreviewCenterController.getTaskLabelNameValue(this.centerTask.label, PreviewCenterController.getTaskLabel(this.centerTask));
            
            this.previousSystemTask = $scope["centerCtrl"].previousSystemTask;
            this.nextSystemTask = $scope["centerCtrl"].nextSystemTask;
            const previewCenterControllerHelper = new PreviewCenterControllerHelper();
            const userStoryId = this.centerTask.userStoryId;
            if (userStoryId) {
                let revisionId: number = null;
                //Only request for revision Id when is +ve number
                if (this.processModelService &&
                    this.processModelService.processModel &&
                    this.processModelService.processModel.status &&
                    this.processModelService.processModel.status.revisionId &&
                    this.processModelService.processModel.status.revisionId > 0) {
                    revisionId = this.processModelService.processModel.status.revisionId;
                }

                this.artifactUtilityService.getProperties(userStoryId, revisionId, true).then(info => {
                    info.properties.sort(function (obj1, obj2) {
                        return obj1.propertyTypeId - obj2.propertyTypeId;
                    });
                    this.title = previewCenterControllerHelper.getPropertyValueByName(info.properties, "ST-Title");
                    this.acceptanceCriteria = previewCenterControllerHelper.getPropertyValueByName(info.properties, "ST-Acceptance Criteria");
                    this.centerTask.userStoryProperties.businessRules = previewCenterControllerHelper.getPropertyByName(info.properties, "ST-Business Rules");
                    this.centerTask.userStoryProperties.nfr = previewCenterControllerHelper.getPropertyByName(info.properties, "ST-Non-Functional Requirements");
                });
            }

            this.$window.addEventListener("resize", this.resizeContentAreas);
            this.resizeContentAreas(false);

            $scope.$on('$destroy', () => {
                this.centerTask = null;
                this.previousSystemTask = null;
                this.nextSystemTask = null;
                this.$window.removeEventListener("resize", this.resizeContentAreas);
            });
        }
    }

    export class PreviewCenterDirective implements ng.IDirective {

        public restrict = "E";

        constructor(private $rootScope: ng.IRootScopeService) {
        }

        public static directive: any[] = [
            "$rootScope",
            ($rootScope: ng.IRootScopeService) => {
                return new PreviewCenterDirective($rootScope);
            }];

        public scope = {
            userTaskModel: "=",
            previousSystemTask: "=",
            nextSystemTask: "=",
            isUserSystemProcess: "="
        };

        public controller = PreviewCenterController;
        public controllerAs = "centerCtrl";
        public bindToController = true;
        public templateUrl = "/Areas/Web/App/Components/Storyteller/Directives/PreviewCenterTemplate.html";
    }

    angular.module("Storyteller").directive("previewCenter", PreviewCenterDirective.directive);
}