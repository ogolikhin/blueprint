import {UserStoryProperties} from "../../diagram/presentation/graph/shapes/user-task";
import {IDiagramNode} from "../../diagram/presentation/graph/models";
import {IArtifactManager} from "../../../../../managers";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../../managers/artifact-manager";
import {IMessageService} from "../../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../../core/localization/localizationService";

export class PreviewCenterController {
    private userStoryTitle: string = "ST-Title";
    private userStoryAcceptanceCriteria: string = "ST-Acceptance Criteria";
    private userStoryBusinessRules: string = "ST-Business Rules";
    private userStoryNFR: string = "ST-Non-Functional Requirements";

    public userStoryProperties: UserStoryProperties;
    public isUserSystemProcess: boolean;
    public subArtifactId: number;
    private isTabsVisible: boolean;
    private showMoreActiveTabIndex: number = 0;

    public title: string;
    public acceptanceCriteria: string;
    public isReadonly: boolean = false;
    public isSMB: boolean = false;
    public isProjectOnlySearch: boolean = true;
    public when: string;

    private statefulUserStoryArtifact: IStatefulArtifact;
    private subscribers: Rx.IDisposable[];

    private userStoryId: number;

    public static $inject = [
        "$window",
        "$scope",
        "$rootScope",
        "$sce",
        "artifactManager",
        "$state",
        "statefulArtifactFactory",
        "messageService",
        "localization"
    ];

    public resizeContentAreas = function (isTabSetVisible) {
        const availHeight = window.innerHeight ? window.innerHeight :
            (document.documentElement && document.documentElement.clientHeight ? document.documentElement.clientHeight :
                (document.body ? document.body.clientHeight : screen.availHeight));
        //unfortunately this is very dependant on the way the GWT window is made :-(
        //the following is based on 930px (availHeight) - (25px (hidden tabs) or 192px (visible tabs) - 500px of other elements)
        const tabSetHeight = isTabSetVisible ? 201 : 25;
        const compensationValue = 500;
        const acceptanceCriteriaMaxHeight = availHeight - compensationValue - tabSetHeight;
        const acceptanceCriteria = <any>document.body.querySelector(".modal-content-autoscroll");
        if (acceptanceCriteria) {
            acceptanceCriteria.setAttribute("style", "max-height:" + acceptanceCriteriaMaxHeight + "px");
        }
    };

    public showMore(type: string, event: any) {
        // select tab
        if (type === "label") {
            this.isTabsVisible = !this.isTabsVisible;

        } else if (type === "nfr") {
            this.isTabsVisible = true;
            this.showMoreActiveTabIndex = 0;

        } else if (type === "br") {
            this.isTabsVisible = true;
            this.showMoreActiveTabIndex = 1;
        }
        this.resizeContentAreas(this.isTabsVisible);
        this.refreshView();
        event.stopPropagation();
    }

    private refreshView() {
        let element: HTMLElement = document.getElementsByClassName("modal-dialog")[0].parentElement;

        // temporary solution from: http://stackoverflow.com/questions/8840580/force-dom-redraw-refresh-on-chrome-mac
        if (!element) {
            return;
        }

        const n = document.createTextNode(" ");
        element.appendChild(n);

        setTimeout(function () {
            n.parentNode.removeChild(n);
        }, 20);
    }

    public strIsNotEmpty = (value: string): boolean => {
        //todo: Settings of tinymce should be changed
        return !!(value && value.trim().replace("\u200B", "").length > 0);
    };

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
        let label: string = "";
        if (task.model && task.model.propertyValues["label"] && task.model.propertyValues["label"]) {
            label = task.model.propertyValues["label"].value;
        }
        return label;
    }

    public getTitle() {
        return this.$sce.trustAsHtml(this.title);
    }

    public getAcceptanceCriteria() {
        return this.$sce.trustAsHtml(this.acceptanceCriteria);
    }

    public navigateToUserStory() {
        let artifactId = this.userStoryId;
        const url = this.$state.href("main.item", {id: artifactId});
        this.$window.open(url, "_blank");
    }

    constructor(private $window: ng.IWindowService,
                private $scope: ng.IScope,
                private $rootScope: ng.IRootScopeService,
                private $sce: ng.ISCEService,
                private artifactManager: IArtifactManager,
                private $state: angular.ui.IStateService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private messageService: IMessageService,
                private localization: ILocalizationService) {

        this.subscribers = [];
        this.isReadonly = $scope.$parent["vm"].isReadonly;

        let isSMBVal = $rootScope["config"].settings.StorytellerIsSMB;

        if (isSMBVal.toLowerCase() === "true") {
            this.isSMB = true;
        }

        this.userStoryId = $scope["centerCtrl"].userStoryId;
        const userTaskLabel = $scope["centerCtrl"].userTaskLabel;
        const userTaskAction = $scope["centerCtrl"].userTaskAction;

        $scope["centerCtrl"].isReadonly = "disabled";

        this.when = PreviewCenterController.getTaskLabelNameValue(userTaskLabel, userTaskAction);

        this.loadUserStory(this.userStoryId);

        this.$window.addEventListener("resize", this.resizeContentAreas);
        this.resizeContentAreas(false);

        $scope.$on("$destroy", () => {
            this.$window.removeEventListener("resize", this.resizeContentAreas);

            if (this.subscribers) {
                this.subscribers.forEach(subscriber => {
                    subscriber.dispose();
                });
                delete this.subscribers;
            }

            if (this.statefulUserStoryArtifact) {
                this.statefulUserStoryArtifact.unload();
                this.statefulUserStoryArtifact = null;
            }
        });
    }

    private loadUserStory(userStoryId: number) {
        if (userStoryId) {
            const artifact = this.artifactManager.get(userStoryId);
            if (artifact) {
                this.statefulUserStoryArtifact = artifact;
            } else {
                this.statefulUserStoryArtifact = this.statefulArtifactFactory.createStatefulArtifact({id: userStoryId});
            }
            const stateObserver = this.statefulUserStoryArtifact.artifactState.onStateChange.debounce(100).subscribe(
                (state) => {
                    if (state.deleted) {
                        this.messageService.addError(this.localization.get("ST_Userstory_Has_Been_Deleted"));
                    }
                },
                (err) => {
                    throw new Error(err);
                });

            const observer = this.statefulUserStoryArtifact.getObservable().subscribe((obs: IStatefulArtifact) => {
                this.loadMetaData(obs);
            });
            this.subscribers = [observer];

            this.subscribers.push(stateObserver);
        }
    }

    private loadMetaData(statefulArtifact: IStatefulArtifact) {
        statefulArtifact.metadata.getArtifactPropertyTypes().then(propertyTypes => {
            propertyTypes.forEach((propertyType) => {
                const propertyValue = statefulArtifact.customProperties.get(propertyType.id);
                if (this.doesPropertyNameContain(propertyType.name, this.userStoryTitle)) {
                    this.title = propertyValue.value;
                } else if (this.doesPropertyNameContain(propertyType.name, this.userStoryAcceptanceCriteria)) {
                    this.acceptanceCriteria = propertyValue.value;
                }
            });
        });
    }

    private doesPropertyNameContain(propertyType: string, value: string): boolean {
        return propertyType.toLowerCase().indexOf(value.toLowerCase()) === 0;
    }
}

export class PreviewCenterComponent implements ng.IComponentOptions {
    public template: string = require("./preview-center.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PreviewCenterController;
    public controllerAs = "centerCtrl";
    public bindings: any = {
        userStoryId: "=",
        userTaskLabel: "=",
        userTaskAction: "=",
        userStoryProperties: "=",
        isUserSystemProcess: "="
    };
    public transclude: boolean = true;
}
