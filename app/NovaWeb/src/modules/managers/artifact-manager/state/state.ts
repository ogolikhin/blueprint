import { Models, Enums } from "../../../main/models";
import { IIStatefulArtifact, IArtifactState, IState } from "../../models";

export class ArtifactState implements IArtifactState {
    private statefullArtifact: IIStatefulArtifact;
    private state: IState;
    private subject: Rx.BehaviorSubject<IArtifactState>;


    constructor(artifact: IIStatefulArtifact, state?: IState) {
        this.statefullArtifact = artifact; 
        this.state = angular.extend({
            readonly: false,
            dirty: false,
            published: false,
            lock: null }, state);

        this.subject = new Rx.BehaviorSubject<IArtifactState>(null);

    }

    public initialize(artifact: Models.IArtifact): IArtifactState {
        if (artifact) {
            if (artifact.lockedByUser) {
                this.state.lock = {
                    result: artifact.lockedByUser.id === this.statefullArtifact.getServices().session.currentUser.id ? 
                            Enums.LockResultEnum.Success : 
                            Enums.LockResultEnum.AlreadyLocked, 
                    info: {
                        lockOwnerDisplayName: artifact.lockedByUser.displayName,
                        utcLockedDateTime: artifact.lockedDateTime
                    }
                }; 
                
            }
        this.subject.onNext(this);
        }
        return this;
    }

    public get observable(): Rx.Observable<IArtifactState> {
        return this.subject.filter(it => it !== null).asObservable();
    }    


    public get lockedBy(): Enums.LockedByEnum {
        if (this.state.lock) {
                switch (this.state.lock.result) {
                    case Enums.LockResultEnum.Success:
                        return Enums.LockedByEnum.CurrentUser;
                    case Enums.LockResultEnum.AlreadyLocked:
                        return Enums.LockedByEnum.OtherUser;
                    default:
                        return Enums.LockedByEnum.None;
                }
        }
        return Enums.LockedByEnum.None;
    }
    public get lockDateTime(): Date {
        if (this.state.lock && this.state.lock.info) {
            return this.state.lock.info.utcLockedDateTime;
        }
        return undefined;
    }
    public get lockOwner(): string {
        if (this.state.lock && this.state.lock.info) {
            return this.state.lock.info.lockOwnerDisplayName;
        }
        return undefined;
    }

    public get(): IState {
        return this.state;
    }
    
    public set(value: any) {
        angular.extend(this.state, value);
        this.subject.onNext(this);
    }

    public get readonly(): boolean {
        return this.state.readonly ||
               this.lockedBy === Enums.LockedByEnum.OtherUser ||
               (this.statefullArtifact.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;
    }
    
    public set readonly(value: boolean) {
        this.set({readonly: value});
    }

    public get dirty(): boolean {
        return this.state.dirty;
    }

    public set dirty(value: boolean) {
        this.set({dirty: value});
    }

    public get published(): boolean {
        return this.state.published;
    }

    public set published(value: boolean) {
        this.set({published: value});
    }
}