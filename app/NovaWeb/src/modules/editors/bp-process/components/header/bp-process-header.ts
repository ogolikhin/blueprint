import {IWindowManager} from "../../../../main/services";
import {BpArtifactInfoController} from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import {IDialogService} from "../../../../shared";
import {IArtifactManager, IProjectManager} from "../../../../managers";
import {IStatefulArtifact, IMetaDataService} from "../../../../managers/artifact-manager";
import {ICommunicationManager} from "../../";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {IUserStoryService} from "../../services/user-story.svc";
import {IArtifactReference, IBreadcrumbService} from "../../services/breadcrumb.svc";
import {IBreadcrumbLink} from "../../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {GenerateUserStoriesAction, ToggleProcessTypeAction} from "./actions";
import {StatefulProcessArtifact} from "../../process-artifact";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export class BpProcessHeader implements ng.IComponentOptions {
    public template: string = require("./bp-process-header.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpProcessHeaderController;
}

export class BpProcessHeaderController extends BpArtifactInfoController {
    public breadcrumbLinks: IBreadcrumbLink[];

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
        "metadataService",
        "userStoryService"
    ];

    constructor($scope: ng.IScope,
                $element: ng.IAugmentedJQuery,
                artifactManager: IArtifactManager,
                localization: ILocalizationService,
                messageService: IMessageService,
                dialogService: IDialogService,
                windowManager: IWindowManager,
                private communicationManager: ICommunicationManager,
                loadingOverlayService: ILoadingOverlayService,
                navigationService: INavigationService,
                private breadcrumbService: IBreadcrumbService,
                protected projectManager: IProjectManager,
                protected metadataService: IMetaDataService,
                private userStoryService: IUserStoryService) {
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
    }

    public $onInit() {
        this.breadcrumbService.getReferences()
            .then((result: IArtifactReference[]) => {
                for (let i: number = 0; i < result.length; i++) {
                    const artifactReference = result[i];
                    const breadcrumbLink: IBreadcrumbLink = {
                        id: artifactReference.id,
                        name: artifactReference.name,
                        version: artifactReference.version,
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
            new GenerateUserStoriesAction(
                processArtifact,
                this.userStoryService,
                this.artifactManager.selection,
                this.messageService,
                this.localization,
                this.dialogService,
                this.loadingOverlayService,
                this.communicationManager.processDiagramCommunication
            ),
            new ToggleProcessTypeAction(
                processArtifact,
                this.communicationManager.toolbarCommunicationManager,
                this.localization
            )
        );
    }
}
