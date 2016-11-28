import {IArtifactManager, IProjectManager} from "../managers";
import {IStatefulArtifact} from "../managers/artifact-manager";
import {Models, Enums} from "../main/models";
import {IMessageService} from "../core/messages/message.svc";

export {IArtifactManager, IProjectManager, IStatefulArtifact, Models, Enums}

export class BpBaseEditor {
    protected subscribers: Rx.IDisposable[];
    protected isDestroyed: boolean;
    public artifact: IStatefulArtifact;
    public isLoading: boolean;

    constructor(public messageService: IMessageService,
                public artifactManager: IArtifactManager) {
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
        this.isDestroyed = true;
        this.destroy();
    }

    protected destroy(): void {
        this.artifact = undefined;

        this.subscribers.forEach(
            (subscriber: Rx.IDisposable) => {
                subscriber.dispose();
            }
        );
        this.subscribers = undefined;
    }

    protected onArtifactChanged = () => {
        this.onArtifactReady();
    }

    public onArtifactReady() {
        this.isLoading = false;
    }
}
