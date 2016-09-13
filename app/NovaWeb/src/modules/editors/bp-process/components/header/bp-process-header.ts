import { IProjectManager, IWindowManager, IArtifactService } from "../../../../main/services";
import { BpArtifactInfoController } from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import { IMessageService, ILocalizationService, IStateManager } from "../../../../core";
import { IDialogService } from "../../../../shared";
import { IToolbarCommunication } from "./toolbar-communication";
import { ICommunicationManager } from "../../"; 
import { ILoadingOverlayService } from "../../../../core/loading-overlay";

export class BpProcessHeader implements ng.IComponentOptions {
    public template: string = require("./bp-process-header.html");
    public controller: Function = BpProcessHeaderController;
    public transclude: boolean = true;
}

export class BpProcessHeaderController extends BpArtifactInfoController {
    private toolbarCommunicationManager: IToolbarCommunication;
    private enableDeleteButtonHandler: string;
    public isDeleteButtonEnabled: boolean;
    
    constructor(
        $scope: ng.IScope,
        projectManager: IProjectManager,
        localization: ILocalizationService,
        stateManager: IStateManager,
        messageService: IMessageService,
        dialogService: IDialogService,
        $element: ng.IAugmentedJQuery,
        windowManager: IWindowManager,
        artifactService: IArtifactService,
        communicationManager: ICommunicationManager,
        loadingOverlayService: ILoadingOverlayService
    ) {
        super(
            $scope,
            projectManager,
            localization,
            stateManager,
            messageService,
            dialogService,
            $element,
            windowManager,
            artifactService,
            communicationManager,
            loadingOverlayService
        );
        this.isDeleteButtonEnabled = false;
        this.toolbarCommunicationManager = communicationManager.toolbarCommunicationManager;
        this.enableDeleteButtonHandler = this.toolbarCommunicationManager.registerEnableDeleteObserver(this.enableDeleteButton);
    }

    public enableDeleteButton = (value: boolean) => {
        this.$scope.$applyAsync((s) => {
            this.isDeleteButtonEnabled = value;
        });
    }

    private clickDelete() {
        this.toolbarCommunicationManager.clickDelete();
    }

    public $onDestroy() {
        super.$onDestroy();

        //dispose subscribers
        this.toolbarCommunicationManager.removeEnableDeleteObserver(this.enableDeleteButtonHandler);
    }
    
}