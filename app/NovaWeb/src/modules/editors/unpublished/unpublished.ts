import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IBPAction} from "../../shared/widgets/bp-toolbar/actions/bp-action";
import {BPButtonGroupAction} from "../../shared/widgets/bp-toolbar/actions/bp-button-group-action";
import {IArtifact} from "../../main/models/models";
import {ILoadingOverlayService} from "../../core/loading-overlay/loading-overlay.svc";
import {DiscardArtifactsAction} from "../../main/components/bp-artifact-info/actions/discard-artifacts-action";
import {IProjectManager} from "../../managers/project-manager/project-manager";
import {PublishArtifactsAction} from "../../main/components/bp-artifact-info/actions/publish-artifacts-action";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {IUnpublishedArtifactsService} from "./unpublished.svc";

export class UnpublishedComponent implements ng.IComponentOptions {
    public template: string = require("./unpublished.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = UnpublishedController;
}

export class UnpublishedController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "messageService",
        "publishService",
        "loadingOverlayService",
        "navigationService",
        "projectManager"
    ];

    public toolbarActions: IBPAction[] = [];
    public selectedArtifacts: IArtifact[];
    public isLoading: boolean;

    private publishArtifactsButton: PublishArtifactsAction;
    private discardArtifactsButton: DiscardArtifactsAction;
    private unpublishedArtifactsObserver: Rx.IDisposable;

    constructor(private $log: ng.ILogService,
                public localization: ILocalizationService,
                public messageService: IMessageService,
                private publishService: IUnpublishedArtifactsService,
                private loadingOverlayService: ILoadingOverlayService,
                private navigationService: INavigationService,
                private projectManager: IProjectManager) {

        this.isLoading = true;
        this.selectedArtifacts = [];
        this.unpublishedArtifactsObserver = this.publishService.unpublishedArtifactsObservable.subscribeOnNext(this.updateSelectedArtifacts, this);
        this.publishService.getUnpublishedArtifacts().finally(() => {
            this.isLoading = false;
        });
    }

    public $onInit() {
        this.publishArtifactsButton = new PublishArtifactsAction(
            this.publishService,
            this.localization,
            this.messageService,
            this.loadingOverlayService);
        this.discardArtifactsButton = new DiscardArtifactsAction(
            this.publishService,
            this.localization,
            this.messageService,
            this.loadingOverlayService,
            this.projectManager);

        this.toolbarActions.push(
            new BPButtonGroupAction(
                this.publishArtifactsButton,
                this.discardArtifactsButton
            )
        );
    }

    public $onDestroy() {
        this.publishArtifactsButton = undefined;
        this.discardArtifactsButton = undefined;
        this.unpublishedArtifactsObserver.dispose();
    }

    private updateSelectedArtifacts(unpublishedArtifacts: IArtifact[]) {
        this.selectedArtifacts = _.intersectionBy(this.selectedArtifacts, unpublishedArtifacts, "id");
        this.updateToolbarButtons();
    };

    public toggleSelection(artifact: IArtifact) {
        const selectedId = this.selectedArtifacts.indexOf(artifact);

        if (selectedId > -1) {
            this.selectedArtifacts.splice(selectedId, 1);
        } else {
            this.selectedArtifacts.push(artifact);
        }

        this.updateToolbarButtons();
    }

    public toggleAll() {
        if (this.selectedArtifacts.length === this.publishService.unpublishedArtifacts.length) {
            this.selectedArtifacts = [];
        } else {
            this.selectedArtifacts = this.publishService.unpublishedArtifacts.slice(0);
        }

        this.updateToolbarButtons();
    }

    public isGroupToggleChecked(): boolean {
        return this.publishService.unpublishedArtifacts.length > 0 && this.publishService.unpublishedArtifacts.length === this.selectedArtifacts.length;
    }

    private updateToolbarButtons() {
        this.publishArtifactsButton.updateList(this.selectedArtifacts);
        this.discardArtifactsButton.updateList(this.selectedArtifacts);
    }

    public isSelected(artifact: IArtifact): boolean {
        return this.selectedArtifacts.indexOf(artifact) > -1;
    }
}
