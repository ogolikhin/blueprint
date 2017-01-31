import {BPButtonGroupAction} from "../../shared/widgets/bp-toolbar/actions/bp-button-group-action";
import {DeleteAction} from "../../main/components/bp-artifact-info/actions/delete-action";
import {IWindowManager} from "../../main/services";
import {BpArtifactInfoController} from "../../main/components/bp-artifact-info/bp-artifact-info";
import {IDialogService, BPButtonOrDropdownSeparator} from "../../shared";
import {IProjectManager} from "../../managers";
import {IMetaDataService} from "../../managers/artifact-manager";
import {IStatefulCollectionArtifact} from "./collection-artifact";
import {INavigationService} from "../../commonModule/navigation/navigation.service";
import {RapidReviewAction, AddCollectionArtifactAction} from "./actions";
import {ILoadingOverlayService} from "../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../commonModule/localization/localization.service";
import {IMainBreadcrumbService} from "../../main/components/bp-page-content/mainbreadcrumb.svc";
import {ICollectionService} from "./collection.svc";
import {IItemInfoService} from "../../commonModule/itemInfo/itemInfo.service";
import {IMessageService} from "../../main/components/messages/message.svc";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {IProjectExplorerService} from "../../main/components/bp-explorer/project-explorer.service";

export class BpCollectionHeader implements ng.IComponentOptions {
    public template: string = require("../../main/components/bp-artifact-info/bp-artifact-info.html");
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
        "projectManager",
        "projectExplorerService",
        "metadataService",
        "mainbreadcrumbService",
        "collectionService",
        "itemInfoService"
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
                projectManager: IProjectManager,
                projectExplorerService: IProjectExplorerService,
                metadataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService,
                collectionService: ICollectionService,
                itemInfoService: IItemInfoService) {
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
            projectManager,
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

        const deleteAction = new DeleteAction(this.artifact, this.localization, this.messageService, this.selectionManager,
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
