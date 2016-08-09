// References to StorytellerDiagramDirective
import {ProcessModels, IProcessService} from "./";
import {ISelectionManager } from "../../main/services";
import * as Models from "../../main/models/models";
import {IMessageService} from "../../core";
import {StorytellerDiagram} from "./components/diagram/storyteller-diagram";
import {SubArtifactEditorModalOpener} from "./dialogs/sub-artifact-editor-modal-opener";
import {IDialogManager, DialogManager} from "./dialogs/dialog-manager";

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
    public dialogManager: IDialogManager;

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

        this.dialogManager = new DialogManager();
        this.subArtifactEditorModalOpener = new SubArtifactEditorModalOpener($scope, $uibModal, $rootScope, this.dialogManager);

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
            this.dialogManager
        );

        this.storytellerDiagram.createDiagram(artifact.toString());
    }
}