
import {IArtifactProperty, IProcess, IProcessShape} from "../../../models/process-models";
import {UserTask, SystemTask} from "../../diagram/presentation/graph/shapes/";
import {IDiagramNode} from "../../diagram/presentation/graph/models";
import {IArtifactService} from "../../../../../main/services/";

import {Models} from "../../../../../main";

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
        "artifactService"
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
        private artifactService: IArtifactService) {

        this.isReadonly = $scope.$parent["vm"].isReadonly;

        let isSMBVal = $rootScope["config"].settings.StorytellerIsSMB;

        if (isSMBVal.toLowerCase() === "true") {
            this.isSMB = true;
        }

        this.centerTask = $scope["centerCtrl"].userTaskModel;

        this.when = PreviewCenterController.getTaskLabelNameValue(this.centerTask.label, PreviewCenterController.getTaskLabel(this.centerTask));

        this.previousSystemTask = $scope["centerCtrl"].previousSystemTask;
        this.nextSystemTask = $scope["centerCtrl"].nextSystemTask;
        const userStoryId = this.centerTask.userStoryId;
        if (userStoryId) {
            let revisionId: number = null;

            this.artifactService.getArtifact(userStoryId).then((it: Models.IArtifact) => {
                it.customPropertyValues.forEach((property) => {
                    if (property.name === "ST-Title") {
                        this.title = property.value;
                    } else if (property.name === "ST-Acceptance Criteria") {
                        this.acceptanceCriteria = property.value;
                    } else if (property.name === "ST-Business Rules") {
                        this.centerTask.userStoryProperties.businessRules = property.value;
                    } else if (property.name === "ST-Non-Functional Requirements") {
                        this.centerTask.userStoryProperties.nfr = property.value;
                    }
                });
            })
            //Only request for revision Id when is +ve number
            //if (this.processModelService &&
            //    this.processModelService.processModel &&
            //    this.processModelService.processModel.status &&
            //    this.processModelService.processModel.status.revisionId &&
            //    this.processModelService.processModel.status.revisionId > 0) {
            //    revisionId = this.processModelService.processModel.status.revisionId;
            //}

            //this.artifactUtilityService.getProperties(userStoryId, revisionId, true).then(info => {
            //    info.properties.sort(function (obj1, obj2) {
            //        return obj1.propertyTypeId - obj2.propertyTypeId;
            //    });
            //    this.title = previewCenterControllerHelper.getPropertyValueByName(info.properties, "ST-Title");
            //    this.acceptanceCriteria = previewCenterControllerHelper.getPropertyValueByName(info.properties, "ST-Acceptance Criteria");
            //    this.centerTask.userStoryProperties.businessRules = previewCenterControllerHelper.getPropertyByName(info.properties, "ST-Business Rules");
            //    this.centerTask.userStoryProperties.nfr = previewCenterControllerHelper.getPropertyByName(info.properties, "ST-Non-Functional Requirements");
            //});
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

export class PreviewCenterComponent implements ng.IComponentOptions {
    public template: string = require("./preview-center.html");
    public controller: Function = PreviewCenterController;
    public controllerAs = "centerCtrl";
    public bindings: any = {
        userTaskModel: "=",
        previousSystemTask: "=",
        nextSystemTask: "=",
        isUserSystemProcess: "="
    };
    public transclude: boolean = true;
}
