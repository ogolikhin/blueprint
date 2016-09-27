import { IMessageService } from "../core";
import { IArtifactManager, IProjectManager } from "../managers";
import { IArtifactState } from "../managers/artifact-manager";
import { IStatefulArtifact, } from "../managers/models";
import { Models, Enums } from "../main/models";

export { IArtifactManager, IProjectManager, IStatefulArtifact, IMessageService, Models, Enums }

export class BpBaseEditor {
    protected subscribers: Rx.IDisposable[];
    protected isDestroyed: boolean;
    public artifact: IStatefulArtifact;
    public isLoading: boolean;

    constructor(
        public messageService: IMessageService,
        public artifactManager: IArtifactManager) {
        this.subscribers = [];
    }

    public $onInit() { }

    public $onChanges(obj: any) {
        this.isDestroyed = false;
        this.artifactManager.get(obj.context.currentValue).then((artifact) => { // lightweight
            if (artifact) {
                this.isLoading = true;
                this.artifact = artifact;
                const stateObserver = this.artifact.artifactState.observable()
                        .filter(state => state.outdated || state.deleted)
                        .subscribeOnNext(this.onLoad, this);

                this.artifact.refresh();
                this.subscribers = [stateObserver];
            }
        });
    }

    public $onDestroy() {
        delete this.artifact;
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this.isDestroyed = true;
    }

    public onLoad() {
        this.artifactManager.selection.setArtifact(this.artifact);
        //NOTE: setExplorerArtifact method does not trigger notification
        this.artifactManager.selection.setExplorerArtifact(this.artifact);

        this.artifact.load(this.artifact.artifactState.outdated).then(() => {
            this.onUpdate();
        }).catch((error) => {
            this.onUpdate();
            this.messageService.addError(error);
        }).finally(() => {
            this.isLoading = false;
        })
        ;
    }

    public onUpdate() {
    }
}
