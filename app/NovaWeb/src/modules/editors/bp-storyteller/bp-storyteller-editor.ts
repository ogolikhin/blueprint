// References to StorytellerDiagramDirective
import {ProcessModels, IProcessService} from "./";
import {IProjectManager} from "../../main/services";
import * as Models from "../../main/models/models";
import {IMessageService} from "../../shell/messages/message.svc";
import {StorytellerDiagram} from "./components/diagram/storyteller-diagram";

export class BpStorytellerEditor implements ng.IComponentOptions {
    public template: string = require("./bp-storyteller-editor.html");
    public controller: Function = BpStorytellerEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {};
    public transclude: boolean = true;
}

export class BpStorytellerEditorController {
    private _subscribers: Rx.IDisposable[];
    public storytellerDiagram : StorytellerDiagram;

    public static $inject = [
        "$rootScope",
        "$scope",
        "$state",
        "$timeout",
        "$q",
        "$log",
        "processService",
        "projectManager",
        "messageService"
    ];

    constructor(
        public $rootScope: ng.IRootScopeService,
        public $scope: ng.IScope,
        public $state: ng.ui.IState,
        public $timeout: ng.ITimeoutService,
        public $q: ng.IQService,
        public $log: ng.ILogService,
        public processService: IProcessService,
        public projectManager: IProjectManager,
        public messageService: IMessageService
    ) {

    }

    public $onInit() {

        this.storytellerDiagram = new StorytellerDiagram(
            this.$rootScope,
            this.$scope,
            this.$state,
            this.$timeout,
            this.$q,
            this.$log,
            this.processService,
            this.messageService
        );

        this._subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.projectManager.currentArtifact.subscribeOnNext(this.artifactChange, this)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    //TODO: Temporary code to detect loading of the process.
    private artifactChange(artifact: Models.IArtifact) {
        if (artifact && artifact.predefinedType === Models.ItemTypePredefined.Process) {
            this.storytellerDiagram.createDiagram(artifact.id.toString());
        } else {
            this.storytellerDiagram.processModel = null;
            this.storytellerDiagram.debugInformation = null;
        }

    }
}