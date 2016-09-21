﻿import { IMessageService } from "../core";
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
    }

    public $onInit() {
        this.subscribers = [
            this.artifact.artifactState.observable().map((this.shouldbeUpdated)).distinctUntilChanged().subscribeOnNext(this.onChange, this)
        ];                
    }

    private shouldbeUpdated(state: any) {

        return { isReadOnly: state.readonly,
            };
    }

    public $onChanges(obj: any) {
         try {
            this.artifact = this.artifactManager.selection.getArtifact();
            if (this.onLoading()) {
                this.onLoad();
            }
        } catch (ex) {
            this.messageService.addError(ex.message);
            throw ex;
        }
    }

    public $onDestroy() {
        try {
            delete this.artifact;
            //delete this.artifactState;

            this.subscribers = (this.subscribers || []).filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        } catch (ex) {
            this.messageService.addError(ex.message);
            throw ex;
        }
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

    private onChange() {
        this.onUpdate();        
        // if (state.old || 
        //     state.deleted || 
        //     state.status === Enums.LockResultEnum.AlreadyLocked || 
        //     state.status === Enums.LockResultEnum.DoesNotExist) {
        //     this.onUpdate();
        // }
    }
    

}


