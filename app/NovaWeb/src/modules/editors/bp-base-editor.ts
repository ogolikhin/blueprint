import {IMessageService} from "../core";
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
                public artifactManager: IArtifactManager) {
        this.subscribers = [];
    }

    public $onInit() {
    }

    public $onChanges(obj: any) {
        this.isDestroyed = false;
        this.artifactManager.get(obj.context.currentValue).then((artifact) => {
            if (artifact) {
                this.isLoading = true;
                this.artifact = artifact;
                //TODO come up with better way to fix bug in use case diagram when user selects actor/ use case
                this.artifactManager.selection.setExplorerArtifact(this.artifact);
                this.artifactManager.selection.setArtifact(this.artifact);

                const artifactObserver = artifact.getObservable()
                    .subscribe(this.onArtifactChanged, this.onArtifactError);

                this.subscribers = [artifactObserver];
            }
        });
    }

    public $onDestroy() {
        delete this.artifact;
        this.subscribers.forEach(subscriber => {
            subscriber.dispose();
        });
        this.artifactManager.selection.clearAll();
        delete this.subscribers;
        this.isDestroyed = true;
    }

    protected onArtifactChanged = () => {
        this.onArtifactReady();
    }

    protected onArtifactError = (error: any) => {
        this.onArtifactReady();
    }

    public onArtifactReady() {
        this.isLoading = false;
    }

}
