﻿import {ICommunicationManager} from "./";
import {ProcessDiagram} from "./components/diagram/process-diagram";
import {SubArtifactEditorModalOpener} from "./components/modal-dialogs/sub-artifact-editor-modal-opener";
import {IWindowManager, IMainWindow, ResizeCause} from "../../main";
import {BpBaseEditor, IArtifactManager} from "../bp-base-editor";
import {IDialogService} from "../../shared";
import {IDiagramNode} from "./components/diagram/presentation/graph/models/";
import {ISelection, IStatefulArtifactFactory, IStatefulSubArtifact} from "../../managers/artifact-manager";
import {IStatefulProcessSubArtifact} from "./process-subartifact";
import {ShapesFactory} from "./components/diagram/presentation/graph/shapes/shapes-factory";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {BpAccordionPanelService} from "../../main/components/bp-accordion/bp-accordion";

export class BpProcessEditor implements ng.IComponentOptions {
    public template: string = require("./bp-process-editor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpProcessEditorController;
}

export class BpProcessEditorController extends BpBaseEditor {
    private disposing: boolean = false;

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
        "$uibModal",
        "localization",
        "$timeout",
        "communicationManager",
        "dialogService",
        "navigationService",
        "statefulArtifactFactory",
        "shapesFactory",
        "bpAccordionPanelService"
    ];

    constructor(messageService: IMessageService,
                artifactManager: IArtifactManager,
                private windowManager: IWindowManager,
                private $rootScope: ng.IRootScopeService,
                private $scope: ng.IScope,
                private $element: ng.IAugmentedJQuery,
                private $q: ng.IQService,
                private $log: ng.ILogService,
                private $uibModal: ng.ui.bootstrap.IModalService,
                private localization: ILocalizationService,
                private $timeout: ng.ITimeoutService,
                private communicationManager: ICommunicationManager,
                private dialogService: IDialogService,
                private navigationService: INavigationService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private shapesFactory: ShapesFactory = null,
                private bpAccordionPanelService: BpAccordionPanelService) {
        super(messageService, artifactManager);

        this.subArtifactEditorModalOpener = new SubArtifactEditorModalOpener(
            $uibModal, communicationManager.modalDialogManager, localization);
    }

    public $onInit() {
        super.$onInit();
        this.subscribers.push(this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this));
        this.subscribers.push(
            //subscribe for current artifact change (need to distinct artifact)
            this.artifactManager.selection.selectionObservable
                .filter(this.clearSelectionFilter)
                .subscribeOnNext(this.clearSelection, this)
        );
    }

    private clearSelectionFilter = (selection: ISelection) => {
        return this.artifact
            && selection != null
            && selection.artifact
            && selection.artifact.id === this.artifact.id
            && !selection.subArtifact;
    }

    private clearSelection(value: ISelection) {
        if (this.processDiagram) {
            this.processDiagram.clearSelection();
        }
    }

    public onArtifactReady() {
        // when this method is called the process artifact should
        // be loaded and assigned to the base class' artifact
        // property (this.artifact)

        // here we create a new process diagram  passing in the
        // process artifact and the html element that will contain
        // the graph

        if (this.processDiagram) {
            this.processDiagram.destroy();
        } else {
            //When the process is navigated to from explorer, inline trace, manual trace etc. we want to reset the shapes factory.
            //This allows the temp ids to be generated from -1, -2 again. Otherwise the temp ids will continue to decrease incrementally across processes
            this.shapesFactory.reset();
        }

        this.processDiagram = new ProcessDiagram(
            this.$rootScope,
            this.$scope,
            this.$timeout,
            this.$q,
            this.$log,
            <IMessageService>this.messageService,
            this.communicationManager,
            this.dialogService,
            this.localization,
            this.navigationService,
            this.statefulArtifactFactory,
            this.shapesFactory,
            this.bpAccordionPanelService
        );

        let htmlElement = this.getHtmlElement();

        this.processDiagram.addSelectionListener((element) => {
            this.onSelectionChanged(element);
        });

        this.processDiagram.createDiagram(this.artifact, htmlElement);

        super.onArtifactReady();
    }

    public $onDestroy() {
        this.disposing = true;
        this.destroy();

        super.$onDestroy();

        this.disposing = false;
    }

    private destroy() {
        if (this.subArtifactEditorModalOpener) {
            this.subArtifactEditorModalOpener.destroy();
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

    private onSelectionChanged = (elements: IDiagramNode[]) => {
        if (this.disposing || this.isDestroyed) {
            return;
        }

        if (elements.length > 0) {
            const subArtifact = <IStatefulProcessSubArtifact>this.artifact.subArtifactCollection.get(elements[0].model.id);
            if (subArtifact) {
                subArtifact.loadProperties()
                    .then((loadedSubArtifact: IStatefulSubArtifact) => {
                        if (this.disposing || this.isDestroyed) {
                            return;
                        }

                        this.artifactManager.selection.setSubArtifact(loadedSubArtifact);
                });
            }
        } else {
            this.artifactManager.selection.clearSubArtifact();
        }
    }
}
