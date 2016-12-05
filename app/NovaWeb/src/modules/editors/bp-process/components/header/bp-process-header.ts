import {BPButtonGroupAction} from "./../../../../shared/widgets/bp-toolbar/actions/bp-button-group-action";
import {ProcessDeleteAction} from "./actions/process-delete-action";
import {IBPAction} from "./../../../../shared/widgets/bp-toolbar/actions/bp-action";
import {OpenProcessImpactAnalysisAction} from "./actions/open-process-impact-analysis-action";
import {IWindowManager} from "../../../../main/services";
import {BpArtifactInfoController} from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import {IDialogService, BPMenuAction, BPButtonOrDropdownAction, BPButtonOrDropdownSeparator} from "../../../../shared";
import {IArtifactManager, IProjectManager} from "../../../../managers";
import {IStatefulArtifact, IMetaDataService} from "../../../../managers/artifact-manager";
import {ICommunicationManager} from "../../";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {IUserStoryService} from "../../services/user-story.svc";
import {IPathItem, IBreadcrumbService} from "../../services/breadcrumb.svc";
import {IBreadcrumbLink} from "../../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {GenerateUserStoriesAction, ToggleProcessTypeAction, CopyAction} from "./actions";
import {StatefulProcessArtifact} from "../../process-artifact";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IMainBreadcrumbService} from "../../../../main/components/bp-page-content/mainbreadcrumb.svc";
import {ISelectionManager} from "../../../../managers/selection-manager";
import {IAnalyticsProvider} from "../../../../main/components/analytics/analyticsProvider";

export class BpProcessHeader implements ng.IComponentOptions {
    public template: string = require("./bp-process-header.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpProcessHeaderController;
}

export class BpProcessHeaderController extends BpArtifactInfoController {
    public breadcrumbLinks: IBreadcrumbLink[];
    public isToolbarCollapsed: boolean = true;

    static $inject: [string] = [
        "$q",
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
        "userStoryService",
        "mainbreadcrumbService",
        "analytics"
    ];

    constructor($q: ng.IQService,
                $scope: ng.IScope,
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
                private userStoryService: IUserStoryService,
                mainBreadcrumbService: IMainBreadcrumbService,
                analytics: IAnalyticsProvider) {
        super(
            $q,
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
            metadataService,
            mainBreadcrumbService,
            analytics
        );

        this.breadcrumbLinks = [];
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

    public navigateTo = (link: IBreadcrumbLink): void => {
        if (!!link && link.isEnabled) {
            const index = this.breadcrumbLinks.indexOf(link);

            if (index >= 0) {
                this.navigationService.navigateBack(index);
            }
        }
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
            this.communicationManager.processDiagramCommunication);
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
