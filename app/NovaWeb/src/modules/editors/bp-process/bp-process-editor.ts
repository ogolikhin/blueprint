// References to StorytellerDiagramDirective
//import {BpBaseEditor} from "../bp-artifact/bp-base-editor";
import {IProcessService} from "./";
import {ICommunicationManager} from "./";
import {ILocalizationService, IMessageService, INavigationService} from "../../core";
import {ProcessDiagram} from "./components/diagram/process-diagram";
import {SubArtifactEditorModalOpener} from "./components/modal-dialogs/sub-artifact-editor-modal-opener";
import {IWindowManager, IMainWindow, ResizeCause } from "../../main";
import {BpBaseEditor, IArtifactManager} from "../bp-base-editor";
import {IDialogService} from "../../shared";

export class BpProcessEditor implements ng.IComponentOptions {
    public template: string = require("./bp-process-editor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpProcessEditorController;
    public controllerAs = "$ctrl";
    public transclude: boolean = true;
    public bindings: any = {
        context: "<"
    };
}

export class BpProcessEditorController extends BpBaseEditor {
    public processDiagram: ProcessDiagram;
    public subArtifactEditorModalOpener: SubArtifactEditorModalOpener;
    
    public static $inject: [string] = [
        "messageService", 
        "artifactManager", 
        "windowManager",
        "$rootScope",
        "$scope",
        "$element", 
        "$q",
        "$log",
        "processService",
        "$uibModal",
        "localization",
        "$timeout", 
        "communicationManager",
        "dialogService",
        "navigationService"
    ];

    constructor(
        messageService: IMessageService,
        artifactManager: IArtifactManager,
        private windowManager: IWindowManager,
        private $rootScope: ng.IRootScopeService,
        private $scope: ng.IScope,
        private $element: ng.IAugmentedJQuery,
        private $q: ng.IQService,
        private $log: ng.ILogService,
        private processService: IProcessService,
        private $uibModal: ng.ui.bootstrap.IModalService,
        private localization: ILocalizationService,
        private $timeout: ng.ITimeoutService,
        private communicationManager: ICommunicationManager,
        private dialogService: IDialogService,
        private navigationService: INavigationService
    ) {
       super(messageService, artifactManager);

       this.subArtifactEditorModalOpener = new SubArtifactEditorModalOpener(
           $scope, $uibModal, $rootScope, communicationManager.modalDialogManager, localization);
    }

    public $onInit() {
        super.$onInit();
        this.subscribers.push(this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this));
    }

    public onArtifactReady() {
        // when this method is called we should have a valid 
        // process artifact in the base class' artifact
        // property.

        // here we create a new process diagram  passing in the
        // process artifact and the html element that will contain
        // the graph

        this.processDiagram = new ProcessDiagram(
            this.$rootScope,
            this.$scope,
            this.$timeout,
            this.$q,
            this.$log,
            this.processService,
            this.messageService,
            this.communicationManager,
            this.dialogService,
            this.localization,
            this.navigationService
        );
       
        let htmlElement = this.getHtmlElement();

        this.processDiagram.createDiagram(this.artifact, htmlElement);

        super.onArtifactReady();
    }

    public $onDestroy() {
        super.$onDestroy();

        this.destroy();
    }

    private destroy() {
        if (this.subArtifactEditorModalOpener) {
            this.subArtifactEditorModalOpener.onDestroy();
        }
        
        if (this.processDiagram) {
            this.processDiagram.destroy();
        }
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
        if (this.processDiagram && this.processDiagram.resize) {
            if (mainWindow.causeOfChange === ResizeCause.sidebarToggle && !!this.processDiagram) {
                this.processDiagram.resize(mainWindow.contentWidth, mainWindow.contentHeight);
            } else {
                this.processDiagram.resize(0, 0);
            }
        }
    }
}
