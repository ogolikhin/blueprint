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
    deleted?: boolean;
    historical?: boolean;
    misplaced?: boolean;
    invalid?: boolean;
}

export interface IArtifactState extends IState, IDispose {
    published: boolean;
    everPublished: boolean;
    onStateChange: Rx.Observable<IArtifactState>;

    initialize(artifact: Models.IArtifact): void;
    setState(newState: IState, notifyChange?: boolean): void;
    lock(value: Models.ILockResult): void;
    unlock(): void;
}

export class ArtifactState implements IArtifactState {
    private subject: Rx.BehaviorSubject<IArtifactState>;
    private currentState: IState = this.createDefaultState();

    constructor(private artifact: IIStatefulArtifact) {
        this.subject = new Rx.BehaviorSubject<IArtifactState>(undefined);
        this.initialize(artifact);
    }

    // create a new state object with defaults
    private createDefaultState(): IState {
        return <IState>{
            lockedBy: Enums.LockedByEnum.None,
            lockDateTime: null,
            lockOwner: null,
            readonly: false,
            dirty: false,
            deleted: false,
            historical: false,
            misplaced: false,
            invalid: false
        };
    }

    public get onStateChange(): Rx.Observable<IArtifactState> {
        // returns the subject as an observable that can be subscribed to
        // subscribers will get notified when the state changes
        return this.subject.filter(state => !!state).asObservable();
    }

    // this function can set 1 or more state properties at once
    // if notifyChange flag is false observers will not be notified
    public setState(newState: IState, notifyChange: boolean = true): void {
        if (!newState) {
            throw new Error("newState is invalid");
        }

        let changed: boolean = false;

        Object.keys(newState).forEach(key => {
            if (!_.isEqual(this.currentState[key], newState[key])) {
                this.currentState[key] = newState[key];
                changed = true;
            }
        });
        
        if (changed && notifyChange) {
            this.notifyStateChange();
        }
    }

    private notifyStateChange(): void {
        this.subject.onNext(this);
    }

    public get deleted(): boolean {
        return this.currentState.deleted;
    }

    public set deleted(value: boolean) {
        if (this.currentState.deleted === value) {
            return;
        }

        this.currentState.deleted = value;
        this.currentState.readonly = this.currentState.readonly || value;
        this.notifyStateChange();
    }

    public get dirty(): boolean {
        return this.currentState.dirty;
    }

    public set dirty(value: boolean) {
        if (this.currentState.dirty === value) {
            return;
        }

        this.currentState.dirty = value;
        this.notifyStateChange();
    }

    public get invalid(): boolean {
        return this.currentState.invalid;
    }

    public set invalid(value: boolean) {
        if (this.currentState.invalid === value) {
            return;
        }

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
        if (this.currentState.misplaced === value) {
            return;
        }

        this.currentState.misplaced = value;
        this.notifyStateChange();
    }

    // fixme: Read the correct published state from the server-side
    // This method doesn't correctly represent the published state of the artifact in all cases.
    // In case when the manual trace is added to this artifact, we do not lock the artifact
    // but the artifact gets added to unpublished changes. We cannot at this point determine
    // this condition with given information.
    public get published(): boolean {
        return this.artifact.version > 0 && this.lockedBy !== Enums.LockedByEnum.CurrentUser;
    }

    public get everPublished(): boolean {
        return this.artifact.version > 0;
    }

    public get readonly(): boolean {
        return this.currentState.readonly;
    }

    public set readonly(value: boolean) {
        if (this.currentState.readonly === value) {
            return;
        }

        this.currentState.readonly = value;
        this.notifyStateChange();
    }

    public get historical(): boolean {
        return this.currentState.historical;
    }

    public set historical(value: boolean) {
        if (this.currentState.historical === value) {
            return;
        }

        this.currentState.historical = value;
        this.currentState.readonly = this.currentState.readonly || value;
        this.notifyStateChange();
    }

    public initialize(artifact: Models.IArtifact): void {
        if (!artifact) {
            throw new Error("artifact is invalid");
        }

        // deleted state never can be changed from true to false
        const deleted = this.currentState.deleted;
        const historical = this.currentState.historical;

        // reset to default state
        this.currentState = this.createDefaultState();

        const noEditPermission = (artifact.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;

        if (artifact.lockedByUser) {
            const lockedBy = artifact.lockedByUser.id === this.artifact.getServices().session.currentUser.id ?
                            Enums.LockedByEnum.CurrentUser :
                            Enums.LockedByEnum.OtherUser;
            const newState: IState = {
                lockedBy: lockedBy,
                lockOwner: artifact.lockedByUser.displayName,
                lockDateTime: artifact.lockedDateTime,
                deleted: deleted,
                historical: historical,
                readonly: deleted || 
                            historical || 
                            lockedBy === Enums.LockedByEnum.OtherUser || 
                            noEditPermission
            };

            this.setState(newState, false);
        } else {
            this.currentState.deleted = deleted;
            this.currentState.historical = historical;
            this.currentState.readonly = deleted || 
                                            historical || 
                                            noEditPermission;
        }
    }

    public lock(value: Models.ILockResult): void {
        if (!value) {
            return;
        }

        let newState: IState = {};

        if (value.result === Enums.LockResultEnum.Success) {
            newState.lockedBy = Enums.LockedByEnum.CurrentUser;
        } else if (value.result === Enums.LockResultEnum.AlreadyLocked) {
            newState.lockedBy = Enums.LockedByEnum.OtherUser;
            newState.readonly = true;
        }

        if (value.info) {
            newState.lockDateTime = value.info.utcLockedDateTime;
            newState.lockOwner = value.info.lockOwnerDisplayName;
        }

        this.setState(newState);
    }

    public unlock(): void {
        let newState: IState = {
            lockedBy: Enums.LockedByEnum.None,
            lockDateTime: undefined,
            lockOwner: undefined
        };

        this.setState(newState);
    }

    public dispose() {
        if (this.subject) {
            this.subject.dispose();
        }
    }
}
