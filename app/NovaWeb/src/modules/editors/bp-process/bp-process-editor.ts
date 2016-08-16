import {IProcessService} from "./";
import {ISelectionManager } from "../../main/services";
import {ILocalizationService, IMessageService, IStateManager} from "../../core";
import {ProcessDiagram} from "./components/diagram/process-diagram";
import {SubArtifactEditorModalOpener} from "./components/dialogs/sub-artifact-editor-modal-opener";
import {IDialogManager, DialogManager} from "./components/dialogs/dialog-manager";
import {IWindowManager, IMainWindow, ResizeCause, IProjectManager} from "../../main";

export class BpProcessEditor implements ng.IComponentOptions {
    public template: string = require("./bp-process-editor.html");
    public controller: Function = BpProcessEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<"
    };
    public transclude: boolean = true;
}

export class BpProcessEditorController {

    private _context: number;

    public processDiagram: ProcessDiagram;
    public subArtifactEditorModalOpener: SubArtifactEditorModalOpener;
    public dialogManager: IDialogManager;
    private _subscribers: Rx.IDisposable[];
    private contentAreaWidth: number;

    public static $inject: [string] = [
        "$rootScope",
        "$scope",
        "$element",
        "$state",
        "$q",
        "$log",
        "processService",
        "selectionManager",
        "$uibModal",
        "localization",
        "messageService",
        "stateManager",
        "windowManager",
        "$timeout",
        "projectManager"
    ];

    constructor(
        private $rootScope: ng.IRootScopeService,
        private $scope: ng.IScope,
        private $element: ng.IAugmentedJQuery,
        private $state: ng.ui.IState,
        private $q: ng.IQService,
        private $log: ng.ILogService,
        private processService: IProcessService,
        private selectionManager: ISelectionManager,
        private $uibModal: ng.ui.bootstrap.IModalService,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private stateManager: IStateManager,
        private windowManager: IWindowManager,
        private $timeout: ng.ITimeoutService,
        private projectManager: IProjectManager
    ) {
       // super(localization, messageService, stateManager, windowManager, $timeout, projectManager);

        this.dialogManager = new DialogManager();
        this.subArtifactEditorModalOpener = new SubArtifactEditorModalOpener($scope, $uibModal, $rootScope, this.dialogManager);
        this.contentAreaWidth = null;
    }

    public $onInit() {
        //super.$onInit();
        this._subscribers = [
            this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this)
        ];
        
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

        this.contentAreaWidth = this.$element[0].parentElement.clientWidth + 40;
    }

    public $onDestroy() {
        //super.$onDestroy();
    }
    
    private load(artifactId: number) {
        this.processDiagram = new ProcessDiagram(
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
         
        this.processDiagram.createDiagram(artifactId, htmlElement);
        
    }

    private getHtmlElement(): HTMLElement {

        // this.$element is jqLite and does not support selectors
        // so we must traverse its children to find the designated
        // containing element  for the diagram
        let htmlElement = null;
         
        let childElements = this.$element.find("div");
        for (let i = 0; i < childElements.length; i++) {
            if (childElements[i].className.match(/process-graph-container/)) {
                htmlElement = childElements[i];
                break;
            }
        } 
         
        return htmlElement;

    }

    public onWidthResized(mainWindow: IMainWindow) {
        if (
            (mainWindow.causeOfChange === ResizeCause.browserResize || mainWindow.causeOfChange === ResizeCause.sidebarToggle)
            && !!this.processDiagram
        ) {
            //let deltaX = ((toggleAction % 2) * 2 - 1) * 270;
            let deltaX: number = mainWindow.contentWidth - this.contentAreaWidth;
            this.contentAreaWidth = mainWindow.contentWidth;
            this.processDiagram.resize(deltaX);
        }
    }

}