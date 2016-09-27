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
//                this.artifact = artifact;
                const artifactObserver = artifact
                        .getObservable()
                        .subscribe(this.onArtifactChanged, this.onError);

//                this.artifact.refresh();
                this.subscribers = [artifactObserver];
            }
        });
    }

    public $onDestroy() {
        delete this.artifact;
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this.isDestroyed = true;
    }

    protected onArtifactChanged = (artifact: IStatefulArtifact) =>  {
        this.artifact = artifact;
        this.artifactManager.selection.setArtifact(this.artifact);
        this.artifactManager.selection.setExplorerArtifact(this.artifact);
        this.onLoad();
    }

    public onLoad() {
        //NOTE: setExplorerArtifact method does not trigger notification
        this.update();
    }
    public onError = (error: any) => {
        this.messageService.addError(error);

    }

    protected update() {
        this.onUpdate();

    }
    public onUpdate() {
   }
}