import {ICommunicationManager} from "../../";
import {ILoadingOverlayService} from "../../../../core/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../core/localization/localization.service";
import {IMessageService} from "../../../../core/messages/message.svc";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {BpArtifactInfoController} from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import {IMainBreadcrumbService} from "../../../../main/components/bp-page-content/mainbreadcrumb.svc";
import {IWindowManager} from "../../../../main/services";
import {IArtifactManager, IProjectManager} from "../../../../managers";
import {IMetaDataService} from "../../../../managers/artifact-manager";
import {BPButtonOrDropdownSeparator, IDialogService} from "../../../../shared";
import {IBreadcrumbLink} from "../../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {BPButtonGroupAction} from "../../../../shared/widgets/bp-toolbar/actions/bp-button-group-action";
import {StatefulProcessArtifact} from "../../process-artifact";
import {IBreadcrumbService, IPathItem} from "../../services/breadcrumb.svc";
import {IUserStoryService} from "../../services/user-story.svc";
import {CopyAction, GenerateUserStoriesAction, ToggleProcessTypeAction} from "./actions";
import {OpenProcessImpactAnalysisAction} from "./actions/open-process-impact-analysis-action";
import {ProcessDeleteAction} from "./actions/process-delete-action";
import {ICollectionService} from "../../../bp-collection/collection.svc";
import {IItemInfoService} from "../../../../core/navigation/item-info.svc";

export class BpProcessHeader implements ng.IComponentOptions {
    public template: string = require("../../../../main/components/bp-artifact-info/bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpProcessHeaderController;
}

export class BpProcessHeaderController extends BpArtifactInfoController {
    static $inject: [string] = [
        "$q",
        "$scope",
        "$element",
        "$timeout",
        "artifactManager",
        "localization",
        "messageService",
        "dialogService",
        "windowManager",
        "loadingOverlayService",
        "navigationService",
        "projectManager",
        "metadataService",
        "mainbreadcrumbService",
        "communicationManager",
        "breadcrumbService",
        "userStoryService",
        "collectionService",
        "itemInfoService"
    ];

    constructor($q: ng.IQService,
                $scope: ng.IScope,
                $element: ng.IAugmentedJQuery,
                $timeout: ng.ITimeoutService,
                artifactManager: IArtifactManager,
                localization: ILocalizationService,
                messageService: IMessageService,
                dialogService: IDialogService,
                windowManager: IWindowManager,
                loadingOverlayService: ILoadingOverlayService,
                navigationService: INavigationService,
                projectManager: IProjectManager,
                metadataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService,
                private communicationManager: ICommunicationManager,
                private breadcrumbService: IBreadcrumbService,
                private userStoryService: IUserStoryService,
                collectionService: ICollectionService,
                public itemInfoService: IItemInfoService) {
        super(
            $q,
            $scope,
            $element,
            $timeout,
            artifactManager,
            localization,
            messageService,
            dialogService,
            windowManager,
            loadingOverlayService,
            navigationService,
            projectManager,
            metadataService,
            mainBreadcrumbService,
            collectionService,
            itemInfoService
        );
    }

    public $onInit() {
        this.breadcrumbService.getReferences()
            .then((result: IPathItem[]) => {
                for (let i: number = 0; i < result.length - 1; i++) {
                    const pathItem = result[i];
                    const breadcrumbLink: IBreadcrumbLink = {
                        id: pathItem.id,
                        name: pathItem.accessible ? pathItem.name : this.localization.get("ST_Breadcrumb_InaccessibleArtifact"),
                        version: pathItem.version,
                        isEnabled: pathItem.accessible ? i !== result.length - 1 : false
                    };
                    this.breadcrumbLinks.push(breadcrumbLink);
                }
            });

        super.$onInit();
    }

    public $onDestroy() {
        if (this.toolbarActions) {
            const toggleAction =
                <ToggleProcessTypeAction>_.find(this.toolbarActions, action => action instanceof ToggleProcessTypeAction);
            if (toggleAction) {
                toggleAction.dispose();
            }

            const copyAction =
                <CopyAction>_.find(this.toolbarActions, action => action instanceof CopyAction);
            if (copyAction) {
                copyAction.dispose();
            }

            const generateUserStoriesAction =
                <GenerateUserStoriesAction>_.find(this.toolbarActions, action => action instanceof GenerateUserStoriesAction);
            if (generateUserStoriesAction) {
                generateUserStoriesAction.dispose();
            }

            const openProcessImpactAnalysisAction =
                <OpenProcessImpactAnalysisAction>_.find(this.toolbarActions, action => action instanceof OpenProcessImpactAnalysisAction);
            if (openProcessImpactAnalysisAction) {
                openProcessImpactAnalysisAction.dispose();
            }

            const processDeleteAction =
                <ProcessDeleteAction>_.find(this.toolbarActions, action => action instanceof ProcessDeleteAction);
            if (processDeleteAction) {
                processDeleteAction.dispose();
            }
        }

        super.$onDestroy();
    }

    protected createCustomToolbarActions(buttonGroup: BPButtonGroupAction): void {
        const processArtifact = this.artifact as StatefulProcessArtifact;

        if (!processArtifact) {
            return;
        }

        const processDeleteAction = new ProcessDeleteAction(
            processArtifact, this.localization, this.messageService, this.artifactManager, this.projectManager,
            this.loadingOverlayService, this.dialogService, this.navigationService, this.communicationManager.processDiagramCommunication);
        const openProcessImpactAnalysisAction = new OpenProcessImpactAnalysisAction(
            processArtifact,
            this.localization,
            this.communicationManager.processDiagramCommunication);
        const generateUserStoriesAction = new GenerateUserStoriesAction(
            processArtifact,
            this.userStoryService,
            this.messageService,
            this.localization,
            this.dialogService,
            this.loadingOverlayService,
            this.communicationManager.processDiagramCommunication,
            this.projectManager);
        const copyAction = new CopyAction(
            processArtifact,
            this.communicationManager,
            this.localization);
        const toggleProcessTypeAction = new ToggleProcessTypeAction(
            processArtifact,
            this.communicationManager.toolbarCommunicationManager,
            this.localization);

        if (buttonGroup) {
            buttonGroup.actions.push(processDeleteAction);
        }

        // expanded toolbar
        this.toolbarActions.push(
            openProcessImpactAnalysisAction,
            generateUserStoriesAction,
            copyAction,
            toggleProcessTypeAction
        );

        // collapsed toolbar
        const dropdownSeparator = new BPButtonOrDropdownSeparator();
        this.additionalMenuActions.push(
            dropdownSeparator,
            openProcessImpactAnalysisAction,
            dropdownSeparator,
            ...this.getNestedDropdownActions(generateUserStoriesAction),
            dropdownSeparator,
            copyAction
        );
        this.collapsedToolbarActions.unshift(toggleProcessTypeAction);
    }
}
