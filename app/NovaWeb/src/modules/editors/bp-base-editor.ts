import { IMessageService } from "../core";
import { IArtifactManager, IProjectManager } from "../managers";
import { IStatefulArtifact } from "../managers/models";
import { Models, Enums } from "../main";

export { IArtifactManager, IProjectManager, IStatefulArtifact, IMessageService, Models, Enums }

export class BpBaseEditor {
    public static $inject: [string] = ["messageService", "selectionManager2"];

    protected _subscribers: Rx.IDisposable[];
    public artifact: IStatefulArtifact;
    public isLoading: boolean = true;

    constructor(
        public messageService: IMessageService,
        public artifactManager: IArtifactManager
        
    ) {
    }

    public $onInit() {
        this._subscribers = [];
        // this._subscribers.push(
        //     this.stateManager.stateChange
        //         .filter(it => this.context && this.context.artifact.id === it.originItem.id && !!it.lock)
        //         .distinctUntilChanged().subscribeOnNext(this.onLockChanged, this)
        // );
        
    }

    public $onChanges(obj: any) {
        try {
            
            this.artifact = this.artifactManager.selection.getArtifact();

            if (this.onLoading()) {
                this.onLoad();
            }
        } catch (ex) {
            this.messageService.addError(ex.message);
        }
    }

    public $onDestroy() {
        delete this.artifact;
        //delete this.artifactState;

        this._subscribers = (this._subscribers || []).filter((it: Rx.IDisposable) => { it.dispose(); return false; });

    }

    public onLoading(): boolean {
        this.isLoading = true;
        return !!this.artifact;
    }

    public onLoad() {
        this.onUpdate();
    }

    public onUpdate() {
        this.isLoading = false;
    }

    // private onLockChanged(state: ItemState) {
    //     let lock = state.lock;
    //     if (lock.result === Enums.LockResultEnum.Success) {
    //         if (lock.info.versionId !== state.originItem.version) {
    //             this.onLoad(this.context);
    //         }
    //     } else if (lock.result === Enums.LockResultEnum.AlreadyLocked) {
    //         this.onUpdate(this.context);
    //     } else if (lock.result === Enums.LockResultEnum.DoesNotExist) {
    //         this.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[lock.result]);
    //     } else {
    //         this.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[lock.result]);
    //     }

    



}


