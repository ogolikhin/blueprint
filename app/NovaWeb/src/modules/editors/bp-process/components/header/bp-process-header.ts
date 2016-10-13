import { IWindowManager,  } from "../../../../main/services";
import { BpArtifactInfoController } from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import { IMessageService, ILocalizationService} from "../../../../core";
import { IDialogService } from "../../../../shared";
import { IArtifactManager, IProjectManager } from "../../../../managers";
import { IStatefulArtifact, IMetaDataService } from "../../../../managers/artifact-manager";
import { IToolbarCommunication } from "./toolbar-communication";
import { ICommunicationManager } from "../../";
import { ILoadingOverlayService } from "../../../../core/loading-overlay";
import { INavigationService } from "../../../../core/navigation/navigation.svc";
import { IArtifactReference, IBreadcrumbService } from "../../services/breadcrumb.svc";
import { IBreadcrumbLink } from "../../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import { GenerateUserStoriesAction, ToggleProcessTypeAction } from "./actions";
import { StatefulProcessArtifact } from "../../process-artifact";

export class BpProcessHeader implements ng.IComponentOptions {
    public template: string = require("./bp-process-header.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpProcessHeaderController;
    public transclude: boolean = true;
    public bindings: any = {
        context: "<"
    };
}

export class BpProcessHeaderController extends BpArtifactInfoController {
    private toolbarCommunicationManager: IToolbarCommunication;
    public breadcrumbLinks: IBreadcrumbLink[];
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
        "breadcrumbService",
        "projectManager",
        "metadataService"
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
        private breadcrumbService: IBreadcrumbService,
        protected projectManager: IProjectManager,
        protected metadataService: IMetaDataService
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
            navigationService,
            projectManager,
            metadataService
        );

        this.breadcrumbLinks = [];
        this.isDeleteButtonEnabled = false;
        this.toolbarCommunicationManager = communicationManager.toolbarCommunicationManager;        
    }

    public $onInit() {
        this.breadcrumbService.getReferences()
            .then((result: IArtifactReference[]) => {
                for (let i: number = 0; i < result.length; i++) {
                    const artifactReference = result[i];
                    const breadcrumbLink: IBreadcrumbLink = {
                        id: artifactReference.id,
                        name: artifactReference.name,
                        isEnabled: i !== result.length - 1 && !!artifactReference.link
                    };
                    this.breadcrumbLinks.push(breadcrumbLink);
                }
            });

        super.$onInit();
    }

    public $onDestroy() {
        super.$onDestroy();        
    }
    
    public navigateTo = (link: IBreadcrumbLink): void => {
        if (!!link && link.isEnabled) {
            const index = this.breadcrumbLinks.indexOf(link);

            if (index >= 0) {
                this.navigationService.navigateBack(index);
            }
        }
    }    

    protected updateToolbarOptions(artifact: IStatefulArtifact): void {
        super.updateToolbarOptions(artifact);

        const processArtifact = artifact as StatefulProcessArtifact;
        
        if (!processArtifact) {
            return;
        }

        this.toolbarActions.push(
            new GenerateUserStoriesAction(processArtifact, this.artifactManager.selection, this.localization),
            new ToggleProcessTypeAction(processArtifact, this.toolbarCommunicationManager, this.localization)
        );
    }
}
