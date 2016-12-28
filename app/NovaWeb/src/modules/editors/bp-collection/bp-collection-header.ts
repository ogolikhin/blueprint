import {BPButtonGroupAction} from "./../../shared/widgets/bp-toolbar/actions/bp-button-group-action";
import {DeleteAction} from "./../../main/components/bp-artifact-info/actions/delete-action";
import {IWindowManager} from "../../main/services";
import {BpArtifactInfoController} from "../../main/components/bp-artifact-info/bp-artifact-info";
import {IDialogService, BPMenuAction, BPButtonOrDropdownSeparator} from "../../shared";
import {IArtifactManager, IProjectManager} from "../../managers";
import {IMetaDataService} from "../../managers/artifact-manager";
import {IStatefulCollectionArtifact} from "../../editors/bp-collection/collection-artifact";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {RapidReviewAction, AddCollectionArtifactAction} from "./actions";
import {ILoadingOverlayService} from "../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IMainBreadcrumbService} from "../../main/components/bp-page-content/mainbreadcrumb.svc";
import {IAnalyticsProvider} from "../../main/components/analytics/analyticsProvider";
import {ICollectionService} from "../../editors/bp-collection/collection.svc";
import {ISession} from "../../shell/login/session.svc";

export class BpCollectionHeader implements ng.IComponentOptions {
    public template: string = require("../../main/components/bp-artifact-info/bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpCollectionHeaderController;
}

export class BpCollectionHeaderController extends BpArtifactInfoController {

    static $inject: [string] = [
        "$q",
        "$scope",
        "$element",
        "artifactManager",
        "localization",
        "session",
        "messageService",
        "dialogService",
        "windowManager",
        "loadingOverlayService",
        "navigationService",
        "projectManager",
        "metadataService",
        "mainbreadcrumbService",
        "analytics",
        "collectionService"
    ];

    constructor($q: ng.IQService,
                $scope: ng.IScope,
                $element: ng.IAugmentedJQuery,
                artifactManager: IArtifactManager,
                localization: ILocalizationService,
                session: ISession,
                messageService: IMessageService,
                dialogService: IDialogService,
                windowManager: IWindowManager,
                loadingOverlayService: ILoadingOverlayService,
                navigationService: INavigationService,
                projectManager: IProjectManager,
                metadataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService,
                analytics: IAnalyticsProvider,
                collectionService: ICollectionService) {
        super(
            $q,
            $scope,
            $element,
            artifactManager,
            localization,
            session,
            messageService,
            dialogService,
            windowManager,
            loadingOverlayService,
            navigationService,
            projectManager,
            metadataService,
            mainBreadcrumbService,
            analytics,
            collectionService
        );
    }

    protected createCustomToolbarActions(buttonGroup: BPButtonGroupAction): void {
        const collectionArtifact = this.artifact as IStatefulCollectionArtifact;

        if (!collectionArtifact) {
            return;
        }

        const deleteAction = new DeleteAction(this.artifact, this.localization, this.messageService, this.artifactManager,
            this.projectManager, this.loadingOverlayService, this.dialogService, this.navigationService);
        const rapidReviewAction = new RapidReviewAction(collectionArtifact, this.localization, this.dialogService);
        const addCollectionArtifactAction = new AddCollectionArtifactAction(collectionArtifact, this.localization, this.dialogService);

        if (buttonGroup) {
            buttonGroup.actions.push(deleteAction);
        }

        // expanded toolbar
        this.toolbarActions.push(
            rapidReviewAction,
            addCollectionArtifactAction
        );

        // collapsed toolbar
        this.additionalMenuActions.push(
            new BPButtonOrDropdownSeparator(),
            rapidReviewAction,
            addCollectionArtifactAction
        );
    }
}
