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
    public controller: Function = BpProcessEditorController;
    public controllerAs = "$ctrl";
    public transclude: boolean = true;
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

    public onLoad() {
        super.onLoad();
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
         
        this.processDiagram.createDiagram(this.artifact.id, htmlElement);
        
    }


    public $onDestroy() {
        super.$onDestroy();
        this.subArtifactEditorModalOpener.onDestroy();
        this.processDiagram.destroy();
    }
    
    // private load(artifactId: number) {
    //     this.processDiagram = new ProcessDiagram(
    //         this.$rootScope,
    //         this.$scope,
    //         this.$timeout,
    //         this.$q,
    //         this.$log,
    //         this.processService,
    //         this.messageService,
    //         this.communicationManager,
    //         this.dialogService,
    //         this.localization
    //     );
       
    //     let htmlElement = this.getHtmlElement();
         
    //     this.processDiagram.createDiagram(artifactId, htmlElement);
        
    // }

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