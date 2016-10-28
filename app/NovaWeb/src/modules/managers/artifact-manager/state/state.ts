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
    historical?: boolean;
    misplaced?: boolean;
    invalid?: boolean;
}

export interface IArtifactState extends IState, IDispose {
    initialize(artifact: Models.IArtifact): IArtifactState;
    lock(value: Models.ILockResult): void;
    unlock();
    onStateChange: Rx.Observable<IState>;
    get(): IState;
    setState(newState: IState, notifyChange: boolean);
}

export class ArtifactState implements IArtifactState {

    constructor(private artifact: IIStatefulArtifact) {
        this.subject = new Rx.BehaviorSubject<IState>(this.currentState);
        this.initialize(artifact);
    }

    private currentState: IState = this.newState();
    private prevState: IState = this.clone(this.currentState);

    private subject: Rx.BehaviorSubject<IState>;

    private newState(): IState {
        // create a new state object with defaults
        return {
            lockedBy: Enums.LockedByEnum.None,
            lockDateTime: null,
            lockOwner: null,
            readonly: false,
            dirty: false,
            published: false,
            deleted: false,
            misplaced: false,
            invalid: false
        };
    }

    private reset() {
        this.currentState = this.newState();
        this.prevState = this.clone(this.currentState);
    }

    private clone(source: IState): IState {
        let duplicate = JSON.parse(JSON.stringify(source));
        return duplicate;
    }

    public get onStateChange(): Rx.Observable<IState> {
        // returns the subject as an observable that can be subscribed to
        // subscribers will get notified when the state changes
        return this.subject.asObservable();
    }

    public get(): IState {
        return this.currentState;
    }

    public setState(newState: IState, notifyChange: boolean = true) {
        // this function can set 1 or more state properties at once
        // if notifyChange flag is false observers will not be notified
        if (newState) {
            Object.keys(newState).forEach((item) => {
                this.currentState[item] = newState[item];
            });
            if (notifyChange) {
                this.notifyStateChange();
            } else {
                this.prevState = this.clone(this.currentState);
            }
        }
    }

    private notifyStateChange() {
        if (!this.compareEqual(this.prevState, this.currentState)) {
            this.prevState = this.clone(this.currentState);
            this.subject.onNext(this.currentState);
        }
    }

    private compareEqual(prev: IState, curr: IState): boolean {
        return JSON.stringify(prev) === JSON.stringify(curr);
    }

    public get deleted(): boolean {
        return this.currentState.deleted;
    }

    public set deleted(value: boolean) {
        this.currentState.deleted = value;
        this.notifyStateChange();
    }

    public get dirty(): boolean {
        return this.currentState.dirty;
    }

    public set dirty(value: boolean) {
        this.currentState.dirty = value;
        this.notifyStateChange();
    }

    public get invalid(): boolean {
        return this.currentState.invalid;
    }

    public set invalid(value: boolean) {
        this.currentState.invalid = value;
        this.notifyStateChange();
    }

    public get lockedBy(): Enums.LockedByEnum {
        return this.currentState.lockedBy;
    }

    public get lockDateTime(): Date {
        return this.currentState.lockDateTime;
    }

    public get lockOwner(): string {
        return this.currentState.lockOwner;
    }

    public get misplaced(): boolean {
        return this.currentState.misplaced;
    }

    public set misplaced(value: boolean) {
        this.currentState.misplaced = value;
        this.notifyStateChange();
    }

    public get published(): boolean {
        return this.currentState.published;
    }

    public set published(value: boolean) {
        this.currentState.published = value;
        this.notifyStateChange();
    }

    public get readonly(): boolean {
        return this.currentState.readonly || this.deleted ||
            this.historical ||
            this.lockedBy === Enums.LockedByEnum.OtherUser ||
            (this.artifact.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;
    }

    public set readonly(value: boolean) {
        this.currentState.readonly = value;
        this.notifyStateChange();
    }

    public get historical(): boolean {
        return this.currentState.historical;
    }

    public set historical(value: boolean) {
        this.currentState.historical = value;
        this.notifyStateChange();
    }

    public initialize(artifact: Models.IArtifact): IArtifactState {
        if (artifact) {
            // deleted state never can be changed from true to false
            const deleted = this.currentState.deleted;
            const historical = this.currentState.historical;
            this.reset();
            if (artifact.lockedByUser) {
                const newState: IState = {};
                newState.lockedBy = artifact.lockedByUser.id === this.artifact.getServices().session.currentUser.id ?
                    Enums.LockedByEnum.CurrentUser :
                    Enums.LockedByEnum.OtherUser;
                newState.lockOwner = artifact.lockedByUser.displayName;
                newState.lockDateTime = artifact.lockedDateTime;
                newState.deleted = deleted;
                newState.historical = historical;
                this.setState(newState, false);
            } else {
                this.currentState.deleted = deleted;
                this.currentState.historical = historical;
            }
        }
        return this;
    }

    public lock(value: Models.ILockResult) {
        if (value) {
            let lockInfo: IState = {};
            if (value.result === Enums.LockResultEnum.Success) {
                lockInfo.lockedBy = Enums.LockedByEnum.CurrentUser;
            } else if (value.result === Enums.LockResultEnum.AlreadyLocked) {
                lockInfo.lockedBy = Enums.LockedByEnum.OtherUser;
            }
            if (value.info) {
                lockInfo.lockDateTime = value.info.utcLockedDateTime;
                lockInfo.lockOwner = value.info.lockOwnerDisplayName;
            }
            this.setState(lockInfo);
        }
    }

    public unlock() {
        let lockInfo: IState = {
            lockedBy: Enums.LockedByEnum.None,
            lockDateTime: undefined,
            lockOwner: undefined
        };
        this.setState(lockInfo);
    }

    public dispose() {
        if (this.subject) {
            this.subject.dispose();
            delete (this.subject);
        }
    }

}
