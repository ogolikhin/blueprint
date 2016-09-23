import { Models, Enums } from "../../../main/models";
import { IIStatefulArtifact, IDispose } from "../../models";

interface IState {
    lockedby?: Enums.LockedByEnum;
    lockdatetime?: Date;
    lockowner?: string;
    lockedBy?: Enums.LockedByEnum;
    lockDateTime?: Date;
    lockOwner?: string;
    readonly?: boolean;
    dirty?: boolean;
    published?: boolean;
    deleted?: boolean;
    outdated?: boolean;
    invalid?: boolean;
}

export interface IArtifactState extends IState, IDispose {
    initialize(artifact: Models.IArtifact): IArtifactState;
    observable(): Rx.Observable<IArtifactState>; 
    lock(value:  Models.ILockResult): void;
    get(): IState;
    set(value?: IState): void;
    error?: string;
} 


export class ArtifactState implements IArtifactState {
    private state: IState = {
        lockedby: Enums.LockedByEnum.None,
    }; 
    
    private subject: Rx.BehaviorSubject<IArtifactState>;

    constructor(private artifact: IIStatefulArtifact) {
        this.subject = new Rx.BehaviorSubject<IArtifactState>(this);
    }
    public dispose() {
        this.subject.dispose();
    }
    
    private reset(): IState {
        this.error = null;
        return this.state = {
            lockedby: Enums.LockedByEnum.None,
        }; 
    }

    public get(): IState {
        return this.state;
    }
    public set(value?: IState) {
        if (value) {
            angular.extend(this.state, value);
        }
        this.subject.onNext(this);
    }

    public initialize(artifact: Models.IArtifact): IArtifactState {
        if (artifact) {
            let state = this.reset();
            if (artifact.lockedByUser) {
                state = {
                    lockedby : artifact.lockedByUser.id === this.artifact.getServices().session.currentUser.id ?
                                                    Enums.LockedByEnum.CurrentUser :
                                                    Enums.LockedByEnum.OtherUser,
                    lockowner: artifact.lockedByUser.displayName,                                                    
                    lockdatetime: artifact.lockedDateTime                
                };
            };                
            this.set(state);
        }
        return this;
    }

    public observable(): Rx.Observable<IArtifactState> {
        return this.subject.filter(it => it !== null).asObservable();
    }    


    public get lockedBy(): Enums.LockedByEnum {
        return this.state.lockedby;
    }

    public get lockDateTime(): Date {
        return this.state.lockdatetime;
    }

    public get lockOwner(): string {
        return this.state.lockowner;
    }
    

    public lock(value: Models.ILockResult) {
        if (value) {
            let lockinfo: IState = {};
            if (value.result === Enums.LockResultEnum.Success) {
                lockinfo.lockedby = Enums.LockedByEnum.CurrentUser;
            } else if (value.result === Enums.LockResultEnum.AlreadyLocked) {
                lockinfo.lockedby = Enums.LockedByEnum.OtherUser;
            } else if (value.result === Enums.LockResultEnum.DoesNotExist) {
                lockinfo.lockedby = Enums.LockedByEnum.None;
            } else {
                lockinfo.lockedby = Enums.LockedByEnum.None;
            }
            if (value.info) {
                lockinfo.lockdatetime = value.info.utcLockedDateTime;
                lockinfo.lockowner = value.info.lockOwnerDisplayName;
            }
            this.set(lockinfo);            
        }
    
    }
    public get readonly(): boolean {
        return this.state.readonly ||
               this.lockedBy === Enums.LockedByEnum.OtherUser ||
               (this.artifact.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;
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
    public get outdated(): boolean {
        return this.state.outdated;
    }
    public set outdated(value: boolean) {
        this.set({outdated: value});
    }
    
    public get invalid(): boolean {
        return this.state.invalid;
    }

    public set invalid(value: boolean) {
        this.set({invalid: value});
    }

    public error: string;
}