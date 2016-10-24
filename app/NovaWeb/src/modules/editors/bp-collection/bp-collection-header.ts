import {IWindowManager} from "../../main/services";
import {BpArtifactInfoController} from "../../main/components/bp-artifact-info/bp-artifact-info";
import {IMessageService, ILocalizationService} from "../../core";
import {IDialogService} from "../../shared";
import {IArtifactManager, IProjectManager} from "../../managers";
import {IStatefulArtifact, IMetaDataService} from "../../managers/artifact-manager";
import {ILoadingOverlayService} from "../../core/loading-overlay";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {IBreadcrumbLink} from "../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {RapidReviewAction, AddCollectionArtifactAction} from "./actions";

export class BpCollectionHeader implements ng.IComponentOptions {
    public template: string = require("../../main/components/bp-artifact-info/bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpCollectionHeaderController;
    public transclude: boolean = true;
    public bindings: any = {
        context: "<"
    };
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

        const processArtifact = artifact as IStatefulArtifact;

        if (!processArtifact) {
            return;
        }

        this.toolbarActions.push(
            new RapidReviewAction(processArtifact, this.localization),
            new AddCollectionArtifactAction(processArtifact, this.localization)         
        );
    }
}
