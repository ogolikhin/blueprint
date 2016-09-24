import { IMessageService } from "../core";
import { IArtifactManager, IProjectManager } from "../managers";
import { IArtifactState } from "../managers/artifact-manager";
import { IStatefulArtifact, } from "../managers/models";
import { Models, Enums } from "../main/models";

export { IArtifactManager, IProjectManager, IStatefulArtifact, IMessageService, Models, Enums }

export class BpBaseEditor {
    protected subscribers: Rx.IDisposable[];
    public artifact: IStatefulArtifact;
    public isLoading: boolean;

    constructor(
        public messageService: IMessageService,
        public artifactManager: IArtifactManager) {
        this.subscribers = [];
    }

    public $onInit() { }

    public $onChanges(obj: any) {
        this.artifactManager.get(obj.context.currentValue).then((artifact) => { // lightweight
            if (this.onLoading(artifact)) {
                this.artifact.artifactState.set({outdated: true});
            }
        });
    }

    public $onDestroy() {
        delete this.artifact;
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    public onLoading(artifact: IStatefulArtifact): boolean {
        this.isLoading = true;
        if (artifact) {
            this.artifact = artifact;
            this.subscribers.push(
                this.artifact.artifactState.observable()
                    .filter(state => state.outdated)
                    .subscribeOnNext(this.onStateChange, this)
            );
        }
        
        return !!this.artifact;
    }

    public onLoad() {
        this.artifactManager.selection.setArtifact(this.artifact);
        this.artifact.load(this.artifact.artifactState.outdated).then(() => {
            this.onUpdate();
        });
    }

    public onUpdate() {
        this.isLoading = false;
    }

    private onStateChange(state: IArtifactState) {
        this.onLoad();
    }
}
