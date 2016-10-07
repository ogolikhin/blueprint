import { IWindowManager,  } from "../../../../main/services";
import { BpArtifactInfoController } from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import { IMessageService, ILocalizationService} from "../../../../core";
import { IDialogService } from "../../../../shared";
import { IArtifactManager, IProjectManager } from "../../../../managers";
import { IToolbarCommunication } from "./toolbar-communication";
import { ICommunicationManager } from "../../"; 
import { ILoadingOverlayService } from "../../../../core/loading-overlay";
import { INavigationService } from "../../../../core/navigation/navigation.svc";
import { IArtifactReference, IBreadcrumbService } from "../../services/breadcrumb.svc";
import { IBreadcrumbLink } from "../../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import { 
    BPButtonToolbarOption,
    BPDropdownToolbarOption, 
    BPDropdownMenuItemToolbarOption,
    BPToggleToolbarOption, 
    BPButtonGroupToolbarOption 
} from "../../../../shared";

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
    private enableDeleteButtonHandler: string;
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
        "projectManager"
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
        protected projectManager: IProjectManager
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
            projectManager
        );

        this.breadcrumbLinks = [];
        this.isDeleteButtonEnabled = false;
        this.toolbarCommunicationManager = communicationManager.toolbarCommunicationManager;
        this.enableDeleteButtonHandler = this.toolbarCommunicationManager.registerEnableDeleteObserver(this.enableDeleteButton);
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

        //dispose subscribers
        this.toolbarCommunicationManager.removeEnableDeleteObserver(this.enableDeleteButtonHandler);
    }

    public enableDeleteButton = (value: boolean) => {
        this.$scope.$applyAsync((s) => {
            this.isDeleteButtonEnabled = value;
        });
    }

    public navigateTo = (link: IBreadcrumbLink): void => {
        if (!!link && link.isEnabled) {
            const index = this.breadcrumbLinks.indexOf(link);

            if (index >= 0) {
                this.navigationService.navigateBack(index);
            }
        }
    }

    public clickDelete() {
        this.toolbarCommunicationManager.clickDelete();
    }

    protected updateToolbarOptions(): void {
        super.updateToolbarOptions();

        this.toolbarOptions.push(
            new BPDropdownToolbarOption(
                () => true,
                "fonticon fonticon2-news",
                undefined,
                "Generate User Stories",
                new BPDropdownMenuItemToolbarOption(
                    "Generate from Task",
                    () => "Generate from Task clicked",
                    () => true
                ),
                new BPDropdownMenuItemToolbarOption(
                    "Generate All", 
                    () => "Generate All clicked", 
                    () => true
                )
            ),
            new BPToggleToolbarOption(
                () => true,
                new BPButtonToolbarOption(
                    () => "Business Process toggled",
                    () => true,
                    "fonticon fonticon2-user-user",
                    "Business Process mode"
                ),
                new BPButtonToolbarOption(
                    () => "User-To-System Process toggled",
                    () => true,
                    "fonticon fonticon2-user-system",
                    "User-System Process mode"
                )
            )
        );
    }
}
