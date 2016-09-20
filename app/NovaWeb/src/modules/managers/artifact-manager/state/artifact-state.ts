import { Models, Enums } from "../../../main/models";
import { IIStatefulArtifact, IDispose } from "../../models";

export interface IState {
    readonly?: boolean;
    dirty?: boolean;
    published?: boolean;
    lock?: Models.ILockResult;
    deleted: boolean;
    moved: boolean;
}

export interface IArtifactState extends IDispose {
    initialize(artifact: Models.IArtifact): IArtifactState; 
    get(): IState;
    //set(value: any): void;
    lock: Models.ILockResult;
    lockedBy: Enums.LockedByEnum;
    lockDateTime?: Date;
    lockOwner?: string;
    readonly: boolean;
    dirty: boolean;
    published: boolean;
    observable(): Rx.Observable<IArtifactState>;
} 


export class ArtifactState implements IArtifactState {
    private statefullArtifact: IIStatefulArtifact;
    private state: IState;
    private lockedby: Enums.LockedByEnum = Enums.LockedByEnum.None;
    private lockdatetime: Date;
    private lockowner: string;
    
    private subject: Rx.BehaviorSubject<IArtifactState>;

    constructor(artifact: IIStatefulArtifact, state?: IState) {
        this.statefullArtifact = artifact; 
        this.subject = new Rx.BehaviorSubject<IArtifactState>(null);
        this.reset();

    }
    public dispose() {
        this.subject.dispose();
    }
    
    private reset() {
        delete this.lockedby;
        delete this.lockowner;
        delete this.lockdatetime;
        this.state = {
            readonly: false,
            dirty: false,
            published: false,
            deleted: false,
            moved: false,
            lock: this.state ? this.state.lock : null 
        };        
    }

    public initialize(artifact: Models.IArtifact): IArtifactState {
        if (artifact) {
            this.reset();
            if (artifact.lockedByUser) {
                this.lockedby = artifact.lockedByUser.id === this.statefullArtifact.getServices().session.currentUser.id ?
                                Enums.LockedByEnum.CurrentUser :
                                Enums.LockedByEnum.OtherUser;
                this.lockowner =  artifact.lockedByUser.displayName;
                this.lockdatetime =  artifact.lockedDateTime;
            }
            this.subject.onNext(this);
        }
        return this;
    }

    public observable(): Rx.Observable<IArtifactState> {
        return this.subject.filter(it => it !== null).asObservable();
    }    

    public get lockedBy(): Enums.LockedByEnum {
        return this.lockedby;
    }

    public get lockDateTime(): Date {
        return this.lockdatetime;
    }

    public get lockOwner(): string {
        return this.lockowner;
    }

    public get(): IState {
        return this.state;
    }
    
    public set(value: any) {
        angular.extend(this.state, value);
        this.subject.onNext(this);
    }

    public get lock(): Models.ILockResult {
        return this.state.lock;
    }

    public set lock(value: Models.ILockResult ) {
        if (value) {
            if (value.result === Enums.LockResultEnum.Success) {
                this.lockedby = Enums.LockedByEnum.CurrentUser;
            } else if (value.result === Enums.LockResultEnum.AlreadyLocked) {
                this.lockedby = Enums.LockedByEnum.OtherUser;
                this.state.readonly = true;
            } else {
                this.lockedby = Enums.LockedByEnum.None;
            }
            if (value.info) {
                this.lockdatetime = value.info.utcLockedDateTime;
            }            
            if (value.info) {
                this.lockowner = value.info.lockOwnerDisplayName;
            }            
        }
        this.set({lock: value});
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
