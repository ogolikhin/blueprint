import { IWindowManager,  } from "../../../../main/services";
import { BpArtifactInfoController } from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import { IMessageService, ILocalizationService} from "../../../../core";
import { IDialogService } from "../../../../shared";
import { IArtifactManager } from "../../../../managers";
import { IToolbarCommunication } from "./toolbar-communication";
import { ICommunicationManager } from "../../"; 
import { ILoadingOverlayService } from "../../../../core/loading-overlay";
import { INavigationService } from "../../../../core/navigation/navigation.svc";
import { IArtifactReference, IBreadcrumbService } from "../../services/breadcrumb.svc";
import { IBreadcrumbLink } from "../../../../shared/widgets/bp-breadcrumb/breadcrumb-link";

export class BpProcessHeader implements ng.IComponentOptions {
    public template: string = require("./bp-process-header.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpProcessHeaderController;
    public transclude: boolean = true;
}

export class BpProcessHeaderController extends BpArtifactInfoController {
    private toolbarCommunicationManager: IToolbarCommunication;
    private enableDeleteButtonHandler: string;
    private breadcrumbLinks: IBreadcrumbLink[];
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
        "navigationService",
        "breadcrumbService"
    ];
    
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
        navigationService: INavigationService,
        private breadcrumbService: IBreadcrumbService
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

        this.breadcrumbLinks = [];
        this.isDeleteButtonEnabled = false;
        this.toolbarCommunicationManager = communicationManager.toolbarCommunicationManager;
        this.enableDeleteButtonHandler = this.toolbarCommunicationManager.registerEnableDeleteObserver(this.enableDeleteButton);
    }

    public enableDeleteButton = (value: boolean) => {
        this.$scope.$applyAsync((s) => {
            this.isDeleteButtonEnabled = value;
        });
    }

    public navigateTo = (link: IBreadcrumbLink): void => {
        if (!!link && link.isEnabled) {
            const index = this.breadcrumbLinks.indexOf(link);

            if (index >= 0) {
                this.navigationService.navigateBack(index);
            }
        }
    }

    public clickDelete() {
        this.toolbarCommunicationManager.clickDelete();
    }

    public $onInit() {
        const navigationState = this.navigationService.getNavigationState();
        this.breadcrumbService.getReferences(navigationState)
            .then((result: IArtifactReference[]) => {
                for (let i: number = 0; i < result.length; i++) {
                    let artifactReference = result[i];
                    this.breadcrumbLinks.push(
                        <IBreadcrumbLink>{
                            id: artifactReference.id,
                            name: artifactReference.name,
                            isEnabled: i !== result.length - 1 && !!artifactReference.link
                        }
                    );
                }
            })
            .catch((error) => {
                if (error) {
                    this.messageService.addError(error);
                }
            });

        super.$onInit();
    }

    public $onDestroy() {
        super.$onDestroy();

        //dispose subscribers
        this.toolbarCommunicationManager.removeEnableDeleteObserver(this.enableDeleteButtonHandler);
    }
}
