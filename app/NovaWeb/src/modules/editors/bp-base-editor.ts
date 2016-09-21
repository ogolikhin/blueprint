import { IMessageService } from "../core";
import { IArtifactManager, IProjectManager } from "../managers";
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

    public $onInit() {
    }

    private shouldbeUpdated(state: any) {

        return { isReadOnly: state.readonly,
            };
    }

    public $onChanges(obj: any) {
        // this.artifact = this.context;
        try {
//            this.artifactManager.selection.clearAll();

            this.artifactManager.get(obj.context.currentValue).then((artifact) => { // lightweight
                if (this.onLoading(artifact)) {
                    this.artifactManager.selection.setArtifact(artifact);
                    this.artifact.load(true).then(() => {
                        this.onUpdate();
                    });
                }
             });
        } catch (ex) {
            this.messageService.addError(ex.message);
            throw ex;
        }
    }

    public $onDestroy() {
        try {
            delete this.artifact;
            this.subscribers = (this.subscribers || []).filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        } catch (ex) {
            this.messageService.addError(ex.message);
            throw ex;
        }
    }

    public onLoading(artifact: IStatefulArtifact): boolean {
        this.isLoading = true;
        if (artifact) {
            this.artifact = artifact;
            this.subscribers.push(
                this.artifact.artifactState.observable().map((this.shouldbeUpdated)).distinctUntilChanged().subscribeOnNext(this.onUpdate, this)
            );
        }
        
        return !!this.artifact;
    }

    public onLoad() {
        this.onUpdate();
    }

    public onUpdate() {
        this.isLoading = false;
    }


}


