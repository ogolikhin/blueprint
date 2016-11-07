import {IMessageService, IApplicationError, HttpStatusCode} from "../core";
import {IArtifactManager, IProjectManager} from "../managers";
import {IStatefulArtifact} from "../managers/artifact-manager";
import {Models, Enums} from "../main/models";

export {IArtifactManager, IProjectManager, IStatefulArtifact, IMessageService, Models, Enums}

export class BpBaseEditor {
    protected subscribers: Rx.IDisposable[];
    protected isDestroyed: boolean;
    public artifact: IStatefulArtifact;
    public isLoading: boolean;

    constructor(public messageService: IMessageService,
                public artifactManager: IArtifactManager
                ) {
        this.subscribers = [];
    }

    public $onInit() {
        this.isDestroyed = false;
        this.isLoading = true;

        this.artifact = this.artifactManager.selection.getArtifact();
        if (this.artifact) {
            const selectedArtifactSub = this.artifact.getObservable()
                .subscribeOnNext(this.onArtifactChanged);

            this.subscribers.push(selectedArtifactSub);
        }
    }

    public $onDestroy() {
        delete this.artifact;
        this.subscribers.forEach(subscriber => {
            subscriber.dispose();
        });
        delete this.subscribers;
        this.isDestroyed = true;
    }

    protected onArtifactChanged = () => {
        this.messageService.clearMessages();
        this.onArtifactReady();
    }

    public onArtifactReady() {
        this.isLoading = false;
    }
}
