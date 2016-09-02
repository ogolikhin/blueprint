import { IProjectManager, IWindowManager, IArtifactService, ICommunicationManager } from "../../../../main/services";
import { BpArtifactInfoController } from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import { IMessageService, ILocalizationService, IStateManager } from "../../../../core";
import { IDialogService } from "../../../../shared";
import { IToolbarCommunication } from "./toolbar-communication";
export class BpProcessHeader implements ng.IComponentOptions {
    public template: string = require("./bp-process-header.html");
    public controller: Function = BpProcessHeaderController;
    public controllerAs: string = "$ctrl";
    public bindings: any = {
        context: "<"
    };
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
        communicationManager: ICommunicationManager 
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
            communicationManager
        );
        this.isDeleteButtonEnabled = false;
        this.toolbarCommunicationManager = communicationManager.toolbarCommunicationManager;
        this.enableDeleteButtonHandler = this.toolbarCommunicationManager.registerEnableDeleteObserver(this.enableDeleteButton);
    }

    public enableDeleteButton = (value: boolean) => {
        this.isDeleteButtonEnabled = value;
        this.$scope.$apply();
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