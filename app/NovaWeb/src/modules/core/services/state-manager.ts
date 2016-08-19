﻿import "angular";
import { Enums, Models } from "../../main/models";
import { ISession } from "../../shell/login/session.svc";

export interface IStateManager {
    dispose(): void;
    onChanged: Rx.Observable<ItemState>;
    addChange(origin: Models.IItem, changeSet?: IPropertyChangeSet): ItemState;
    getState(item: number | Models.IItem): ItemState;
    deleteState(artifact: number | Models.IItem);
}

export interface IPropertyChangeSet {
    itemId?: number;
    lookup: Enums.PropertyLookupEnum;
    id: string | number;
    value: any;
}

export class ItemState {
    constructor(userId: number, item: Models.IItem) {
        this._userId = userId;
        this._originItem = item;
    }

    private _userId: number;
    private _originItem: Models.IArtifact;
    private _readonly: boolean;
    private _changesets: IPropertyChangeSet[];
    private _changedItem: Models.IArtifact;
    private _lock: Models.ILockResult;

    public get originItem(): Models.IArtifact {
        return this._originItem;
    }
    public set originItem(value: Models.IArtifact) {
        if (!value)
            return;
        this._originItem = value;
    }

    public get isReadonly(): boolean {
        return this._readonly || (this.originItem.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;
    }
    public set isReadonly(value: boolean) {
        this._readonly = value;
    }

    public get lockedBy(): Enums.LockedByEnum {
        if (this.originItem.lockedByUser) {
            if (this.originItem.lockedByUser.id === this._userId) {
                return Enums.LockedByEnum.CurrentUser;
            }
            return Enums.LockedByEnum.OtherUser;
        } else if (this._lock) {
            switch (this._lock.result) {
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

    public get isChanged(): boolean {
        return Boolean(angular.isArray(this._changesets) && this._changesets.length);
    }

    public get changedItem(): Models.IArtifact {
        return this._changedItem;
    }

    public get lock(): Models.ILockResult {
        return this._lock;
    }

    public setLock(value: Models.ILockResult): Models.ILockResult {
        if (!value) {
            return null;
        }
        this._lock = value;
        if (value.result === Enums.LockResultEnum.Success) {
            this.originItem.lockedByUser = {
                id: this._userId
            };
        } else {
            if (value.result === Enums.LockResultEnum.AlreadyLocked) {
                this.originItem.lockedByUser = {
                    id: -1,
                    displayName: value.info.lockOwnerLogin
                };
            }
            this.revertChanges();
            this._readonly = true;
        }
        return this._lock;
    }

    public clear() {
        this._readonly = false;
        delete this._changedItem;
        delete this._changesets;
        this.originItem = null;
    }

    private add(changeSet: IPropertyChangeSet) {
        if (!this._changesets) {
            this._changesets = [];
        }
        let _changeset = this._changesets.filter((it: IPropertyChangeSet) => {
            return it.lookup === changeSet.lookup && it.id === changeSet.id;
        })[0];
        if (_changeset) {
            _changeset.value = changeSet.value;
        } else {
            this._changesets.push(changeSet);
        }
        
    }

    public saveChange(item: Models.IItem, changeSet: IPropertyChangeSet): boolean {
        if (!item || !changeSet ) {
            return false;
        }

        if (!this._changedItem) {
            this._changedItem = angular.copy(this.originItem);
        }
        let updateItem: Models.IItem;
        if ("projectId" in item) {
            updateItem = this._changedItem;
        } else {
            updateItem = this._changedItem.subArtifacts.filter((it: Models.ISubArtifact) => {
                return it.id === item.id;
            })[0];
            
        }

        let propertyTypeId: number;
        let propertyValue: Models.IPropertyValue;

        if (!updateItem || !changeSet) {
            return false;
        }
        changeSet.itemId = updateItem.id;

        switch (changeSet.lookup) {
            case Enums.PropertyLookupEnum.System:
                if (changeSet.id in this.originItem) {
                    updateItem[changeSet.id] = changeSet.value;
                } else {
                    return false;
                }
                break;
            case Enums.PropertyLookupEnum.Custom:
                propertyTypeId = changeSet.id as number;
                propertyValue = (updateItem.customPropertyValues || []).filter((it: Models.IPropertyValue) => {
                    return it.propertyTypeId === propertyTypeId;
                })[0];
                if (propertyValue) {
                    propertyValue.value = changeSet.value;
                } else {
                    return false;
                }
                break;
            case Enums.PropertyLookupEnum.Special:
                propertyTypeId = changeSet.id as number;
                propertyValue = (updateItem.specificPropertyValues || []).filter((it: Models.IPropertyValue) => {
                    return it.propertyTypeId === propertyTypeId;
                })[0];
                if (propertyValue) {
                    propertyValue.value = changeSet.value;
                } else {
                    return false;
                }
                break;
            default:
                return false;
        }
        this.add(changeSet);
        return true;

    }

    public revertChanges() {
        delete this._changesets;
        delete this._changedItem;
    }
}

export class StateManager implements IStateManager {
    static $inject: [string] = ["session"];
    private _itemStateCollection: ItemState[];

    private _itemChanged: Rx.BehaviorSubject<ItemState>;

    constructor(private session: ISession) { }

    private get itemChanged(): Rx.BehaviorSubject<ItemState> {
        return this._itemChanged || (this._itemChanged = new Rx.BehaviorSubject<ItemState>(null));
    }
    private get itemStateCollection(): ItemState[] {
        return this._itemStateCollection || (this._itemStateCollection = []);
    }

    public dispose() {
        
        //clear all subjects
        if (this._itemStateCollection) {
            this._itemStateCollection.forEach((it: ItemState) => {
                it.clear();
            });
            this._itemStateCollection = null;
        }

        if (this._itemChanged) {
            this._itemChanged.dispose();
            this._itemChanged = null;
        }
    }

    public get onChanged(): Rx.Observable<ItemState> {
        return this.itemChanged
            .filter(it => it != null)
            .asObservable();
    }

    public addChange(originItem: Models.IItem, changeSet?: IPropertyChangeSet): ItemState {
        if (!originItem) {
            return null;
        }
        let artifact: Models.IArtifact;
        let subartifact: Models.ISubArtifact;

        if ("projectId" in originItem) {
            artifact = originItem as Models.IArtifact;
        } else {
            subartifact = originItem as Models.ISubArtifact;
        }

        let state = this.getState(originItem);
        if (!state) {
            if (artifact) {
                this.itemStateCollection.push(state = new ItemState(this.session.currentUser.id, artifact));
            } else {
                throw new Error("Artifact_Not_Found");
            }
        } else {
            if (artifact) {
                state.originItem = artifact;
            } else {
                let _subartifact = state.originItem.subArtifacts.filter((it: Models.ISubArtifact) => {
                    return it.id === subartifact.id;
                })[0];
                if (_subartifact) {
                    angular.extend(_subartifact, subartifact);
                } else {
                    state.originItem.subArtifacts.push(subartifact);
                }
            }
        }
        this.clearStates(state);

        if (changeSet) {
            
            if (state.saveChange(artifact || subartifact, changeSet)) {
                this.itemChanged.onNext(state);
                return state;
            }
        }
        return null;
    }

    public clearStates(exceptState?: ItemState) {
        this._itemStateCollection = this.itemStateCollection.filter((it: ItemState) => {
            return it.isChanged || (it === exceptState);
        });
    }

    public getState(item: number | Models.IItem): ItemState {
        let id = angular.isNumber(item) ? item as number : (item ? item.id : -1);
        let state: ItemState = this.itemStateCollection.filter((it: ItemState) => {
            let result: boolean;
            if (it.originItem.id === id) {
                result = true;
            } else {
                result = !!(it.originItem.subArtifacts && it.originItem.subArtifacts.filter((sa: Models.ISubArtifact) => {
                    return sa.id === id;
                })[0]);
            }
            return result;
        })[0];

        return state;
    }

    public deleteState(item: number | Models.IItem) {
        let itemId = angular.isNumber(item) ? item as number : (item ? item.id : -1);
        this._itemStateCollection = this.itemStateCollection.filter((it: ItemState) => {
            if (it.originItem.id === itemId) {
                it.clear();
                return false;
            }
            return true;
        });
    }
}
