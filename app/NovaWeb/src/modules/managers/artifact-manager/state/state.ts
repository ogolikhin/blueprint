import * as angular from "angular";
import {Models, Enums} from "../../../main/models";
import {IDispose} from "../../models";
import {IIStatefulArtifact} from "../artifact";

export interface IState {
    lockedBy?: Enums.LockedByEnum;
    lockDateTime?: Date;
    lockOwner?: string;
    readonly?: boolean;
    dirty?: boolean;
    published?: boolean;
    deleted?: boolean;
    misplaced?: boolean;
    invalid?: boolean;
}

export interface IArtifactState extends IState, IDispose {
    initialize(artifact: Models.IArtifact): IArtifactState;
    lock(value: Models.ILockResult): void;
    onStateChange: Rx.Observable<IState>;
    get(): IState;
    //set(value?: IState): void;
} 

export class ArtifactState implements IArtifactState {
    
    constructor(private artifact: IIStatefulArtifact) {
        this._subject = new Rx.BehaviorSubject<IState>(this._state);
        this.initialize(artifact);
    }
    private _prevState: IState;
    private _state: IState;

    private _subject: Rx.BehaviorSubject<IState>;
    
    public get onStateChange(): Rx.Observable<IState> {
        return this._subject.asObservable();
    }
    public get(): IState {
        return this._state;
        //fixme: empty functions should be removed as empty does nothing thus not needed

    }

    private set(value?: IState) {
        if (value) {
            angular.extend(this._state, value);
            if (!this.compareEqual(this._prevState, this._state)) {
                angular.extend(this._prevState, value);
                // notify subscribers that the state has changed
                this._subject.onNext(this._state);
            }
        }
    }

    private compareEqual(prev: IState, curr: IState): boolean {
        return JSON.stringify(prev) === JSON.stringify(curr);
    }
 
    public get deleted(): boolean {
        return this._state.deleted;
    }

    public set deleted(value: boolean) {
        this.set({ deleted: value });
    }

    public get dirty(): boolean {
        return this._state.dirty;
    }

    public set dirty(value: boolean) {
        this.set({ dirty: value });
    }

    public get invalid(): boolean {
        return this._state.invalid;
    }

    public set invalid(value: boolean) {
        this.set({ invalid: value });
    }

    public get lockedBy(): Enums.LockedByEnum {
        return this._state.lockedBy;
    }
   
    public get lockDateTime(): Date {
        return this._state.lockDateTime;
    }

    public get lockOwner(): string {
        return this._state.lockOwner;
    }

    public get misplaced(): boolean {
        return this._state.misplaced;
    }

    public set misplaced(value: boolean) {
        this.set({ misplaced: value });
    }

    public get published(): boolean {
        return this._state.published;
    }

    public set published(value: boolean) {
        this.set({ published: value });
    }

    public get readonly(): boolean {
        return this._state.readonly || this.deleted ||
            this.lockedBy === Enums.LockedByEnum.OtherUser ||
            (this.artifact.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;
    }

    public set readonly(value: boolean) {
        this.set({ readonly: value });
    }
     
    public initialize(artifact: Models.IArtifact): IArtifactState {
        if (artifact) {
            this.reset();
            if (artifact.lockedByUser) {
                let lockinfo: IState = {
                    lockedBy: Enums.LockedByEnum.None
                };
                lockinfo.lockedBy = artifact.lockedByUser.id === this.artifact.getServices().session.currentUser.id ?
                    Enums.LockedByEnum.CurrentUser :
                    Enums.LockedByEnum.OtherUser;
                lockinfo.lockOwner = artifact.lockedByUser.displayName;
                lockinfo.lockDateTime = artifact.lockedDateTime;
                angular.extend(this._state, lockinfo);
                angular.extend(this._prevState, this._state);
            };
            
        }
        return this;
    }

    public lock(value: Models.ILockResult) {
        if (value) {
            let lockinfo: IState = {
                lockedBy: Enums.LockedByEnum.None
            };
            if (value.result === Enums.LockResultEnum.Success) {
                lockinfo.lockedBy = Enums.LockedByEnum.CurrentUser;
            } else if (value.result === Enums.LockResultEnum.AlreadyLocked) {
                lockinfo.lockedBy = Enums.LockedByEnum.OtherUser;
            }
            if (value.info) {
                lockinfo.lockDateTime = value.info.utcLockedDateTime;
                lockinfo.lockOwner = value.info.lockOwnerDisplayName;
            }
            this.set(lockinfo);
        }
    }
    
    private reset() {
        this._state = {};
        this._state.dirty = false;
        this._state.lockedBy = Enums.LockedByEnum.None;
        this._prevState = {};

        angular.extend(this._prevState, this._state);
         
    }

    public dispose() {
        if (this._subject) {
            this._subject.dispose();
            delete (this._subject);
        }
    }

}
