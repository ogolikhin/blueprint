// References to StorytellerDiagramDirective
import {IProcessService} from "./";
import {ISelectionManager } from "../../main/services";
import {IMessageService} from "../../core";
import {StorytellerDiagram} from "./components/diagram/storyteller-diagram";
import {SubArtifactEditorModalOpener} from "./components/dialogs/sub-artifact-editor-modal-opener";
import {IDialogManager, DialogManager} from "./components/dialogs/dialog-manager";

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

    public storytellerDiagram: StorytellerDiagram;
    public subArtifactEditorModalOpener: SubArtifactEditorModalOpener;
    public dialogManager: IDialogManager;

    public static $inject = [
        "$rootScope",
        "$scope",
        "$element", 
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
        private $element: ng.IAugmentedJQuery,
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
        }
    }

    public $postLink() {
        if (this._context) {
            this.load(this._context);
        }
    }

    public $onDestroy() {
    }
    
    private load(artifactId: number) {
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
       
        let htmlElement = this.getHtmlElement();
         
        this.storytellerDiagram.createDiagram(artifactId, htmlElement);
        
    }

    private getHtmlElement(): HTMLElement {

        // this.$element is jqLite and does not support selectors
        // so we must traverse its children to find the designated
        // containing element  for the diagram
        let htmlElement = null;
         
        let childElements = this.$element.find("div");
        for (let i = 0; i < childElements.length; i++) {
            if (childElements[i].className.match(/storyteller-graph-container/)) {
                htmlElement = childElements[i];
                break;
            }
        } 
         
        return htmlElement;

    }
}