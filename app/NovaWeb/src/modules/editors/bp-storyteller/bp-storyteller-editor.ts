// References to StorytellerDiagramDirective
import {ProcessModels, IProcessService} from "./";
import {ISelectionManager } from "../../main/services";
import * as Models from "../../main/models/models";
import {IMessageService} from "../../core";
import {StorytellerDiagram} from "./components/diagram/storyteller-diagram";
import {SubArtifactEditorModalOpener} from "./dialogs/sub-artifact-editor-modal-opener-controller";
import {ICommunicationService, CommunicationService} from "./dialogs/communication-service";

export class BpStorytellerEditor implements ng.IComponentOptions {
    public template: string = require("./bp-storyteller-editor.html");
    public controller: Function = BpStorytellerEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<"
    };
    public transclude: boolean = true;
}

export class BpStorytellerEditorController {

    private _context: number;

    public storytellerDiagram : StorytellerDiagram;
    public subArtifactEditorModalOpener: SubArtifactEditorModalOpener;
    public communicationService: ICommunicationService;

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
        "messageService",
        "$uibModal"
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
        private messageService: IMessageService,
        private $uibModal: ng.ui.bootstrap.IModalService
    ) {

        this.communicationService = new CommunicationService();
        this.subArtifactEditorModalOpener = new SubArtifactEditorModalOpener($scope, $uibModal, $rootScope, this.communicationService);

    }

    public $onInit() {
        
    }

    public $onChanges(changesObj) {
        if (changesObj.context) {
            this._context = changesObj.context.currentValue;

            if (this._context) {
                this.load(this._context);
            }
        }
    }

    public $onDestroy() {
    }
    
    private load(artifact: number) {
        this.storytellerDiagram = new StorytellerDiagram(
            this.$rootScope,
            this.$scope,
            this.$state,
            this.$timeout,
            this.$q,
            this.$log,
            this.processService,
            this.messageService,
            this.communicationService
        );

        this.storytellerDiagram.createDiagram(artifact.toString());
    }
}