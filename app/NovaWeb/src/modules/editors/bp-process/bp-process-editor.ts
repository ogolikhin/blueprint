import {IFileUploadService} from "../../core/file-upload/fileUploadService";
import {ILoadingOverlayService} from "../../core/loading-overlay/loading-overlay.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IMessageService} from "../../core/messages/message.svc";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {IMainWindow, IWindowManager, ResizeCause} from "../../main/services/window-manager";
import {IStatefulArtifactFactory} from "../../managers/artifact-manager/artifact/artifact.factory";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {IDialogService} from "../../shared/widgets/bp-dialog/bp-dialog";
import {IUtilityPanelService} from "../../shell/bp-utility-panel/utility-panel.svc";
import {BpBaseEditor} from "../bp-base-editor";
import {ICommunicationManager} from "./";
import {ShapesFactory} from "./components/diagram/presentation/graph/shapes/shapes-factory";
import {ProcessDiagram} from "./components/diagram/process-diagram";
import {SubArtifactEditorModalOpener} from "./components/modal-dialogs/sub-artifact-editor-modal-opener";
import {ILocalizationService} from "../../core/localization/localization.service";
import {IClipboardService} from "./services/clipboard.svc";
import {IFileUploadService} from "../../core/fileUpload/fileUpload.service";
import {IMessageService} from "../../main/components/messages/message.svc";

export class BpProcessEditor implements ng.IComponentOptions {
    public template: string = require("./bp-process-editor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpProcessEditorController;
}

export class BpProcessEditorController extends BpBaseEditor {
    private processDiagram: ProcessDiagram;
    private subArtifactEditorModalOpener: SubArtifactEditorModalOpener;

    public static $inject: [string] = [
        "messageService",
        "selectionManager",
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
        "utilityPanelService",
        "clipboardService",
        "fileUploadService",
        "loadingOverlayService"
    ];

    constructor(messageService: IMessageService,
                selectionManager: ISelectionManager,
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
                private utilityPanelService: IUtilityPanelService,
                private clipboard: IClipboardService = null,
                private fileUploadService: IFileUploadService = null,
                private loadingOverlayService: ILoadingOverlayService = null) {
        super(messageService, selectionManager);

        this.subArtifactEditorModalOpener = new SubArtifactEditorModalOpener(
            $uibModal, communicationManager.modalDialogManager, localization);
    }

    public $onInit() {
        super.$onInit();

        this.subscribers.push(
            this.windowManager.mainWindow
                .subscribeOnNext(this.onWidthResized, this),
        );
    }

    protected onArtifactReady() {
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
            this.utilityPanelService,
            this.clipboard,
            this.selectionManager,
            this.fileUploadService,
            this.loadingOverlayService
        );

        let htmlElement = this.getHtmlElement();

        this.processDiagram.createDiagram(this.artifact, htmlElement);

        super.onArtifactReady();
    }

    protected destroy(): void {
        if (this.subArtifactEditorModalOpener) {
            this.subArtifactEditorModalOpener.destroy();
        }

        if (this.processDiagram) {
            this.processDiagram.destroy();
        }

        super.destroy();
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

    private onWidthResized(mainWindow: IMainWindow) {
        if (this.processDiagram && this.processDiagram.resize) {
            if (mainWindow.causeOfChange === ResizeCause.sidebarToggle) {
                this.processDiagram.resize(mainWindow.contentWidth, mainWindow.contentHeight);
            } else {
                this.processDiagram.resize(0, 0);
            }
        }
    }
}
