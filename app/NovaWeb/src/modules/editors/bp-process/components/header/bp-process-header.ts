import { IWindowManager,  } from "../../../../main/services";
import { BpArtifactInfoController } from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import { IMessageService, ILocalizationService} from "../../../../core";
import { IDialogService } from "../../../../shared";
import { IArtifactManager } from "../../../../managers";
import { IToolbarCommunication } from "./toolbar-communication";
import { ICommunicationManager } from "../../"; 
import { ILoadingOverlayService } from "../../../../core/loading-overlay";
import { INavigationService } from "../../../../core/navigation/navigation.svc";

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
        "$scope", 
        "$element", 
        "artifactManager", 
        "localization", 
        "messageService", 
        "dialogService", 
        "windowManager", 
        "communicationManager", 
        "loadingOverlayService",
        "navigationService"];
    
    constructor(
        $scope: ng.IScope,
        $element: ng.IAugmentedJQuery,
        artifactManager: IArtifactManager,
        localization: ILocalizationService,
        messageService: IMessageService,
        dialogService: IDialogService,
        windowManager: IWindowManager,
        communicationManager: ICommunicationManager,
        loadingOverlayService: ILoadingOverlayService,
        navigationService: INavigationService
    ) {
        super(
            $scope,
            $element,
            artifactManager,
            localization,
            messageService,
            dialogService,
            windowManager,
            loadingOverlayService,
            navigationService
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

    public clickDelete() {
        this.toolbarCommunicationManager.clickDelete();
    }

    public $onDestroy() {
        super.$onDestroy();

        //dispose subscribers
        this.toolbarCommunicationManager.removeEnableDeleteObserver(this.enableDeleteButtonHandler);
    }
    
}