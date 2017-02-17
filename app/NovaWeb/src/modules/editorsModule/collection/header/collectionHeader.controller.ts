import {BPButtonGroupAction} from "../../../shared/widgets/bp-toolbar/actions/bp-button-group-action";
import {DeleteAction} from "../../../main/components/bp-artifact-info/actions/delete-action";
import {IWindowManager} from "../../../main/services";
import {BpArtifactInfoController} from "../../../main/components/bp-artifact-info/bp-artifact-info";
import {IDialogService, BPButtonOrDropdownSeparator} from "../../../shared";
import {IMetaDataService} from "../../../managers/artifact-manager";
import {IStatefulCollectionArtifact} from "../../configuration/classes/collection-artifact";
import {INavigationService} from "../../../commonModule/navigation/navigation.service";
import {RapidReviewAction, AddCollectionArtifactAction} from "../actions";
import {ILoadingOverlayService} from "../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IMainBreadcrumbService} from "../../../main/components/bp-page-content/mainbreadcrumb.svc";
import {ICollectionService} from "../collection.service";
import {IItemInfoService} from "../../../commonModule/itemInfo/itemInfo.service";
import {IMessageService} from "../../../main/components/messages/message.svc";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {IProjectExplorerService} from "../../../main/components/bp-explorer/project-explorer.service";
import {IExtendedAnalyticsService} from "../../../main/components/analytics/analytics";

export class BpCollectionHeader implements ng.IComponentOptions {
    public template: string = require("../../../main/components/bp-artifact-info/bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpCollectionHeaderController;
}

export class BpCollectionHeaderController extends BpArtifactInfoController {

    static $inject: [string] = [
        "$q",
        "$scope",
        "$element",
        "$timeout",
        "selectionManager",
        "localization",
        "messageService",
        "dialogService",
        "windowManager",
        "loadingOverlayService",
        "navigationService",
        "projectExplorerService",
        "metadataService",
        "mainbreadcrumbService",
        "collectionService",
        "itemInfoService",
        "Analytics"
    ];

    constructor($q: ng.IQService,
                $scope: ng.IScope,
                $element: ng.IAugmentedJQuery,
                $timeout: ng.ITimeoutService,
                selectionManager: ISelectionManager,
                localization: ILocalizationService,
                messageService: IMessageService,
                dialogService: IDialogService,
                windowManager: IWindowManager,
                loadingOverlayService: ILoadingOverlayService,
                navigationService: INavigationService,
                projectExplorerService: IProjectExplorerService,
                metadataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService,
                collectionService: ICollectionService,
                itemInfoService: IItemInfoService,
                protected analytics: IExtendedAnalyticsService) {
        super(
            $q,
            $scope,
            $element,
            $timeout,
            selectionManager,
            localization,
            messageService,
            dialogService,
            windowManager,
            loadingOverlayService,
            navigationService,
            projectExplorerService,
            metadataService,
            mainBreadcrumbService,
            collectionService,
            itemInfoService
        );
    }

    protected createCustomToolbarActions(buttonGroup: BPButtonGroupAction): void {
        const collectionArtifact = this.artifact as IStatefulCollectionArtifact;

        if (!collectionArtifact) {
            return;
        }

        const deleteAction = new DeleteAction(this.artifact, this.localization, this.messageService,
            this.projectExplorerService, this.loadingOverlayService, this.dialogService, this.navigationService);
        const rapidReviewAction = new RapidReviewAction(collectionArtifact, this.localization, this.dialogService, this.analytics);
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
