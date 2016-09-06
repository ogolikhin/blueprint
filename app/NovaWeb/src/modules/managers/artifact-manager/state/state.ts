import { Models, Enums } from "../../../main/models";
import { IStatefulArtifact, IArtifactStates, IState } from "../interfaces";

export class ArtifactState implements IArtifactStates {
    private statefullArtifact: IStatefulArtifact;
    private state: IState;
    private subject: Rx.BehaviorSubject<IState>;


    constructor(artifact: IStatefulArtifact, state?: IState) {
        this.statefullArtifact = artifact; 
        this.state = angular.extend({
            readonly: false,
            dirty: false,
            published: false,
            lock: null }, state);
        this.subject = new Rx.BehaviorSubject<IState>(null);

    }

    public initialize(artifact: Models.IArtifact): IArtifactStates {
        if (artifact) {
            if (artifact.lockedByUser) {
                this.state.lock = {
                    result: artifact.lockedByUser.id === this.statefullArtifact.manager.currentUser.id ? 
                            Enums.LockResultEnum.Success : 
                            Enums.LockResultEnum.AlreadyLocked,
                    info: {
                        lockOwnerLogin: artifact.lockedByUser.displayName,
                        utcLockedDateTime: artifact.lockedDateTime
                    }
                }; 
                
            }
        }
        return this;
    }

    public get observable(): Rx.Observable<IState> {
        return this.subject.filter(it => it !== null).asObservable();
    }    


    public get locked(): Enums.LockedByEnum {
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

    public get(): IState {
        return this.state;
    }
    
    public set(value: any) {
        angular.extend(this.state, value);
        this.subject.onNext(this.state);
    }

    public get readonly(): boolean {
        return this.state.readonly ||
               this.locked === Enums.LockedByEnum.OtherUser ||
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