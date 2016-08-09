// References to StorytellerDiagramDirective
import {IProcessService} from "./";
import {ISelectionManager } from "../../main/services";
import * as Models from "../../main/models/models";
import {IMessageService} from "../../core";
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
    public storytellerDiagram: StorytellerDiagram;

    public static $inject = [
        "$rootScope",
        "$scope",
        "$state",
        "$timeout",
        "$q",
        "$log",
        "processService",
        "projectManager",
        "selectionManager",
        "messageService"
    ];

    constructor(
        private $rootScope: ng.IRootScopeService,
        private $scope: ng.IScope,
        private $state: ng.ui.IState,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $log: ng.ILogService,
        private processService: IProcessService,
        private selectionManager: ISelectionManager,
        private messageService: IMessageService
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
            this.selectionManager.selectedArtifactObservable.subscribeOnNext(this.artifactChange, this)
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