import * as angular from "angular";
import { Models, Enums } from "../../../main/models";
import { IDispose } from "../../models";
import { IIStatefulArtifact } from "../artifact";

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
    lock(value:  Models.ILockResult): void;
    get(): IState;
    set(value?: IState): void;
} 


export class ArtifactState implements IArtifactState {
    private state: IState = {
        lockedBy: Enums.LockedByEnum.None,
    }; 
    
    constructor(private artifact: IIStatefulArtifact) {
        this.initialize(artifact);
        
    }
    public dispose() {
    }
    
    private reset(): IState {
        return this.state = {
            lockedBy: Enums.LockedByEnum.None,
        }; 
    }

    public get(): IState {
        return this.state;
    }
    public set(value?: IState) {
        if (value) {
            angular.extend(this.state, value);
        }
    }

    public initialize(artifact: Models.IArtifact): IArtifactState {
        if (artifact) {
            this.reset();
            if (artifact.lockedByUser) {
                this.state.lockedBy = artifact.lockedByUser.id === this.artifact.getServices().session.currentUser.id ?
                                                Enums.LockedByEnum.CurrentUser :
                                                Enums.LockedByEnum.OtherUser;
                this.state.lockOwner = artifact.lockedByUser.displayName;                                                    
                this.state.lockDateTime = artifact.lockedDateTime;              
            };                
            //this.set(state);
        }
        return this;
    }

    public get lockedBy(): Enums.LockedByEnum {
        return this.state.lockedBy;
    }

    public get lockDateTime(): Date {
        return this.state.lockDateTime;
    }

    public get lockOwner(): string {
        return this.state.lockOwner;
    }
    
    public lock(value: Models.ILockResult) {
        if (value) {
            let lockinfo: IState = {
                lockedBy : Enums.LockedByEnum.None
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
            angular.extend(this.state, lockinfo);            
        }
    
    }
    public get readonly(): boolean {
        return this.state.readonly || this.deleted ||
               this.lockedBy === Enums.LockedByEnum.OtherUser ||
               (this.artifact.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;
    }
    public set readonly(value: boolean) {
        this.state.readonly = value;
        //this.set({readonly: value});
    }

    public get dirty(): boolean {
        return this.state.dirty;
    }
    public set dirty(value: boolean) {
        this.state.dirty = value;
        //this.set({dirty: value});
    }

    public get published(): boolean {
        return this.state.published;
    }
    public set published(value: boolean) {
        this.state.published = value;
    }
    
    public get invalid(): boolean {
        return this.state.invalid;
    }

    public set invalid(value: boolean) {
        this.state.invalid = value;
    }

    public get misplaced(): boolean {
        return this.state.misplaced;
    }

    public set misplaced(value: boolean) {
        this.state.misplaced = value;
    }

    public get deleted(): boolean {
        return this.state.deleted;
    }

    public set deleted(value: boolean) {
        this.state.deleted = value;
    }

}