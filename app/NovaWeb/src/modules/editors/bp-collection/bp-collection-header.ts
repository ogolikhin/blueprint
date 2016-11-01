import {IWindowManager} from "../../main/services";
import {BpArtifactInfoController} from "../../main/components/bp-artifact-info/bp-artifact-info";
import {IMessageService, ILocalizationService} from "../../core";
import {IDialogService} from "../../shared";
import {IArtifactManager, IProjectManager} from "../../managers";
import {IStatefulArtifact, IStatefulCollectionArtifact, IMetaDataService} from "../../managers/artifact-manager";
import {ILoadingOverlayService} from "../../core/loading-overlay";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {RapidReviewAction, AddCollectionArtifactAction} from "./actions";

export class BpCollectionHeader implements ng.IComponentOptions {
    public template: string = require("../../main/components/bp-artifact-info/bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpCollectionHeaderController;
}

export class BpCollectionHeaderController extends BpArtifactInfoController {

    static $inject: [string] = [
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
        "metadataService"
    ];

    constructor($scope: ng.IScope,
                $element: ng.IAugmentedJQuery,
                artifactManager: IArtifactManager,
                localization: ILocalizationService,
                messageService: IMessageService,
                dialogService: IDialogService,
                windowManager: IWindowManager,
                loadingOverlayService: ILoadingOverlayService,
                navigationService: INavigationService,
                projectManager: IProjectManager,
                metadataService: IMetaDataService) {
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
    }

    protected updateToolbarOptions(artifact: any): void {
        super.updateToolbarOptions(artifact);

        const processArtifact = artifact as IStatefulCollectionArtifact;

        if (!processArtifact) {
            return;
        }

        this.toolbarActions.push(new RapidReviewAction(processArtifact, this.localization));

        this.toolbarActions.push(new AddCollectionArtifactAction(processArtifact, this.localization));
    }
}
