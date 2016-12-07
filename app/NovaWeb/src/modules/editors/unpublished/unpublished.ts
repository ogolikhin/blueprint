import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IBPAction} from "../../shared/widgets/bp-toolbar/actions/bp-action";
import {BPButtonGroupAction} from "../../shared/widgets/bp-toolbar/actions/bp-button-group-action";
import {IArtifact, IPublishResultSet} from "../../main/models/models";
import {ILoadingOverlayService} from "../../core/loading-overlay/loading-overlay.svc";
import {DiscardArtifactsAction} from "../../main/components/bp-artifact-info/actions/discard-artifacts-action";
import {IProjectManager} from "../../managers/project-manager/project-manager";
import {PublishArtifactsAction} from "../../main/components/bp-artifact-info/actions/publish-artifacts-action";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {IUnpublishedArtifactsService} from "./unpublished.svc";
import {ItemTypePredefined} from "../../main/models/enums";

export class UnpublishedComponent implements ng.IComponentOptions {
    public template: string = require("./unpublished.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = UnpublishedController;
}

interface IArtifactWithProject extends IArtifact {
    projectName: string;
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

    public toolbarActions: IBPAction[];
    public selectedArtifacts: IArtifactWithProject[];
    public unpublishedArtifacts: IArtifactWithProject[];
    public isLoading: boolean;

    private publishArtifactsButton: PublishArtifactsAction;
    private discardArtifactsButton: DiscardArtifactsAction;
    private unpublishedArtifactsObserver: Rx.IDisposable;
    private processedArtifactsObserver: Rx.IDisposable;

    constructor(private $log: ng.ILogService,
                public localization: ILocalizationService,
                public messageService: IMessageService,
                private publishService: IUnpublishedArtifactsService,
                private loadingOverlayService: ILoadingOverlayService,
                private navigationService: INavigationService,
                private projectManager: IProjectManager) {
        this.toolbarActions = [];
        this.selectedArtifacts = [];
        this.unpublishedArtifacts = [];
    }

    public $onInit() {
        this.isLoading = true;

        this.unpublishedArtifactsObserver = this.publishService.unpublishedArtifactsObservable
            .subscribeOnNext(this.onUnpublishedArtifactsChanged, this);
        this.processedArtifactsObserver = this.publishService.processedArtifactsObservable
            .subscribeOnNext(this.onArtifactsPublishedOrDiscarded, this);

        this.publishService.getUnpublishedArtifacts().finally(() => {
            this.isLoading = false;
        });

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
        this.processedArtifactsObserver.dispose();
    }

    private onUnpublishedArtifactsChanged(unpublishedArtifacts: IPublishResultSet) {
        this.unpublishedArtifacts = _.map(unpublishedArtifacts.artifacts, artifact => {
            const project = _.find(unpublishedArtifacts.projects, project => project.id === artifact.projectId);
            return _.extend({}, artifact, unpublishedArtifacts.projects, {projectName: project.name});
        });
        this.updateSelectedArtifacts();
        this.updateToolbarButtons();
    };

    private onArtifactsPublishedOrDiscarded(processedResult: IPublishResultSet) {
        _.pullAllBy(this.unpublishedArtifacts, processedResult.artifacts, "id");
        this.updateSelectedArtifacts();
        this.updateToolbarButtons();
    }

    private updateSelectedArtifacts() {
        this.selectedArtifacts = _.intersectionBy(this.selectedArtifacts, this.unpublishedArtifacts, "id");
    }

    public toggleSelection(artifact: IArtifactWithProject) {
        const selectedId = this.selectedArtifacts.indexOf(artifact);

        if (selectedId > -1) {
            this.selectedArtifacts.splice(selectedId, 1);
        } else {
            this.selectedArtifacts.push(artifact);
        }

        this.updateToolbarButtons();
    }

    public toggleAll() {
        if (this.selectedArtifacts.length === this.unpublishedArtifacts.length) {
            this.selectedArtifacts = [];
        } else {
            this.selectedArtifacts = this.unpublishedArtifacts.slice(0);
        }

        this.updateToolbarButtons();
    }

    public isGroupToggleChecked(): boolean {
        return this.unpublishedArtifacts.length > 0
            && this.unpublishedArtifacts.length === this.selectedArtifacts.length;
    }

    private updateToolbarButtons() {
        this.publishArtifactsButton.updateList(this.selectedArtifacts);
        this.discardArtifactsButton.updateList(this.selectedArtifacts);
    }

    public isSelected(artifact: IArtifactWithProject): boolean {
        return this.selectedArtifacts.indexOf(artifact) > -1;
    }

    public isNavigatable(artifact: IArtifactWithProject): boolean {
        return artifact.predefinedType !== ItemTypePredefined.ArtifactBaseline
            && artifact.predefinedType !== ItemTypePredefined.Baseline
            && artifact.predefinedType !== ItemTypePredefined.BaselineFolder
            && artifact.predefinedType !== ItemTypePredefined.ArtifactReviewPackage;
    }
}
