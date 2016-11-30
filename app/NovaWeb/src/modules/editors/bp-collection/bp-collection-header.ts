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
import {ISelectionManager} from "../../managers/selection-manager";
import {IAnalyticsProvider} from "../../main/components/analytics/analyticsProvider";

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
        "messageService",
        "dialogService",
        "windowManager",
        "loadingOverlayService",
        "navigationService",
        "projectManager",
        "metadataService",
        "mainbreadcrumbService",
        "selectionManager",
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
                loadingOverlayService: ILoadingOverlayService,
                navigationService: INavigationService,
                projectManager: IProjectManager,
                metadataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService,
                selectionManager: ISelectionManager,
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
            selectionManager,
            analytics
        );
    }

    protected updateToolbarOptions(artifact: any): void {
        super.updateToolbarOptions(artifact);

        const collectionArtifact = artifact as IStatefulCollectionArtifact;

        if (!collectionArtifact) {
            return;
        }

        // ORIGINAL CODE, DO NOT REMOVE
        // this.toolbarActions.push(new RapidReviewAction(collectionArtifact, this.localization, this.dialogService));
        //
        // this.toolbarActions.push(new AddCollectionArtifactAction(collectionArtifact, this.localization, this.dialogService));

        // NEW PROPOSED CODE
        const rapidReviewAction = new RapidReviewAction(collectionArtifact, this.localization, this.dialogService);
        const addCollectionArtifactAction = new AddCollectionArtifactAction(collectionArtifact, this.localization, this.dialogService);
        // AT SOME POINT, WE CAN DO THIS BASED ON THE AVAILABLE WIDTH
        // if (availableWidth > XXX) {
        //     this.toolbarActions.push(rapidReviewAction, addCollectionArtifactAction);
        // } else {
            for (let i = 0; i < this.toolbarActions.length; i++) {
                if (this.toolbarActions[i].type === "menu") {
                    const buttonDropdown = this.toolbarActions[i] as BPMenuAction;
                    const dropdownSeparator = new BPButtonOrDropdownSeparator();
                    buttonDropdown.actions.push(dropdownSeparator, rapidReviewAction, addCollectionArtifactAction);
                }
            }
        // }
    }
}
