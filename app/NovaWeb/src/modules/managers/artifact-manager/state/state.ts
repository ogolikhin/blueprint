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
    needsToBeUpdated?: boolean;
}

export interface IArtifactState extends IState, IDispose {
    initialize(artifact: Models.IArtifact): IArtifactState;
    observable(): Rx.Observable<IArtifactState>; 
    lock(value:  Models.ILockResult): void;
    get(): IState;
    set(value?: IState): void;
} 


export class ArtifactState implements IArtifactState {
    private state: IState;
    
    private subject: Rx.BehaviorSubject<IArtifactState>;

    constructor(private artifact: IIStatefulArtifact) {
        this.subject = new Rx.BehaviorSubject<IArtifactState>(null);
        this.reset();

    }
    public dispose() {
        this.subject.dispose();
    }
    
    private reset() {
        this.state = {
            lockedby: Enums.LockedByEnum.None
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
            this.reset();
            if (artifact.lockedByUser) {
                this.state.lockedby = artifact.lockedByUser.id === this.artifact.getServices().session.currentUser.id ?
                                Enums.LockedByEnum.CurrentUser :
                                Enums.LockedByEnum.OtherUser;
                this.state.lockowner =  artifact.lockedByUser.displayName;
                this.state.lockdatetime =  artifact.lockedDateTime;
            }
            this.set();
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
            if (value.result === Enums.LockResultEnum.Success) {
                this.state.lockedby = Enums.LockedByEnum.CurrentUser;
            } else if (value.result === Enums.LockResultEnum.AlreadyLocked) {
                this.state.lockedby = Enums.LockedByEnum.OtherUser;
            } else if (value.result === Enums.LockResultEnum.DoesNotExist) {
                this.state.lockedby = Enums.LockedByEnum.None;
            } else {
                this.state.lockedby = Enums.LockedByEnum.None;
            }
            if (value.info) {
                this.state.lockdatetime = value.info.utcLockedDateTime;
                this.state.lockowner = value.info.lockOwnerDisplayName;
            }
            this.set();            
        }
    }

    public get readonly(): boolean {
        return this.state.readonly ||
               this.lockedBy === Enums.LockedByEnum.OtherUser ||
               (this.artifact.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;
    }
    
    public set readonly(value: boolean) {
        this.state.readonly = value;
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