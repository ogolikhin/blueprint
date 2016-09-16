import { IWindowManager,  } from "../../../../main/services";
import { BpArtifactInfoController , IArtifactManager} from "../../../../main/components/bp-artifact-info/bp-artifact-info";
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
    
    static $inject: [string] = [
        "$scope", "$element", "artifactManager", "localization", "messageService", 
        "dialogService", "windowManager", "communicationManager", "loadingOverlayService"];
    
    constructor(
        $scope: ng.IScope,
        $element: ng.IAugmentedJQuery,
        artifactManager: IArtifactManager,
        localization: ILocalizationService,
        messageService: IMessageService,
        dialogService: IDialogService,
        windowManager: IWindowManager,
        communicationManager: ICommunicationManager,
        loadingOverlayService: ILoadingOverlayService
    ) {
        super(
            $scope,
            $element,
            artifactManager,
            localization,
            messageService,
            dialogService,
            windowManager,
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