import "angular";
import { Enums, Models } from "../../main/models";
import { ISession } from "../../shell/login/session.svc";

export interface IStateManager {
    dispose(): void;
    reset(): void;
    stateChange: Rx.Observable<ItemState>;
    addItem(item: Models.IItem, itemtype?: Models.IItemType): ItemState;
    addChange(origin: Models.IItem, changeSet?: IPropertyChangeSet): ItemState;
    getState(item: number | Models.IItem): ItemState;
}

export interface IPropertyChangeSet {
    itemId?: number;
    lookup: Enums.PropertyLookupEnum;
    id: string | number;
    value: any;
}

export class ItemState {
    private manager: StateManager;
    private _originItem: Models.IArtifact;
    private _readonly: boolean;
    private _changesets: IPropertyChangeSet[];
    private _changedItem: Models.IArtifact;
    private _lock: Models.ILockResult;
    
    public itemType: Models.IItemType;

    constructor(manager: StateManager, item: Models.IItem, itemtype?: Models.IItemType) {
        this.manager = manager;
        this._originItem = item;
        this.itemType = itemtype;
    }

    public clear() {
        delete this.originItem;
        delete this.itemType;
        delete this._readonly;
        delete this._changedItem;
        delete this._changesets;
        delete this._lock;
    }

    public get originItem(): Models.IArtifact {
        return this._originItem;
    }
    public set originItem(value: Models.IArtifact) {
        if (!value) {
            return;
        }
        this._originItem = value;
    }

    public get isReadonly(): boolean {
        return this._readonly ||
               this.lockedBy === Enums.LockedByEnum.OtherUser ||
               (this.originItem.permissions & Enums.RolePermissions.Edit) !== Enums.RolePermissions.Edit;
    }
    public set isReadonly(value: boolean) {
        this._readonly = value;
    }

    public get lockedBy(): Enums.LockedByEnum {
        if (this.originItem.lockedByUser) {
            if (this.originItem.lockedByUser.id === this.manager.currentUser.id) {
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
                id: this.manager.currentUser.id
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
        this.manager.changeState(this);
        return this._lock;
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

    private applyChange(item: Models.IItem, changeSet: IPropertyChangeSet) {
        let propertyTypeId: number;
        let propertyValue: Models.IPropertyValue;
        switch (changeSet.lookup) {
            case Enums.PropertyLookupEnum.System:
                if (changeSet.id in this.originItem) {
                    item[changeSet.id] = changeSet.value;
                } else {
                    return false;
                }
                break;
            case Enums.PropertyLookupEnum.Custom:
                propertyTypeId = changeSet.id as number;
                propertyValue = (item.customPropertyValues || []).filter((it: Models.IPropertyValue) => {
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
                propertyValue = (item.specificPropertyValues || []).filter((it: Models.IPropertyValue) => {
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
        return true;
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
            if (angular.isArray(this._changedItem.subArtifacts)) {
                this._changedItem.subArtifacts = [];
            }
            updateItem = this._changedItem.subArtifacts.filter((it: Models.ISubArtifact) => {
                return it.id === item.id;
            })[0];
            if (!updateItem) {
                this._changedItem.subArtifacts.push(updateItem = item);
            }
            
        }

        let propertyTypeId: number;
        let propertyValue: Models.IPropertyValue;

        if (!updateItem || !changeSet) {
            return false;
        }
        changeSet.itemId = updateItem.id;

        if (this.applyChange(updateItem, changeSet)) {

            this.add(changeSet);
            this.manager.changeState(this);
            return true;
        } else if (!this.isChanged && this._changedItem) {
            //removes changed item if not changed
            delete this._changedItem;
        }
        return false;
    }

    public getArtifact(): Models.IArtifact {
        return this.changedItem || this.originItem;
    }

    public getSubArtifact(id: number): Models.ISubArtifact {
        let artifact = this.getArtifact();
        return (artifact.subArtifacts || []).filter((it: Models.ISubArtifact) => {
            return it.id === id;
        })[0];
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

    constructor(private session: ISession) {
        
    }

    private get itemChanged(): Rx.BehaviorSubject<ItemState> {
        return this._itemChanged || (this._itemChanged = new Rx.BehaviorSubject<ItemState>(null));
    }
    private get itemStateCollection(): ItemState[] {
        return this._itemStateCollection || (this._itemStateCollection = []);
    }

    public get currentUser(): Models.IUserGroup {
        return this.session.currentUser;
    }

    public changeState(value: ItemState): void {
        this.itemChanged.onNext(value);
    }

    public get stateChange(): Rx.Observable<ItemState> {
        return this.itemChanged
            .filter(it => it != null)
            .asObservable();
    }

    public reset() {
        //clear all subjects
        if (this._itemStateCollection) {
            this._itemStateCollection.forEach((it: ItemState) => {
                it.clear();
            });
            delete this._itemStateCollection;
        }
    }

    public dispose() {

        this.reset();
        if (this._itemChanged) {
            this._itemChanged.dispose();
            delete this._itemChanged;
        }
    }

    public addItem(item: Models.IItem, type?: Models.IItemType): ItemState {
        if (!item) {
            return null;
        }
        let artifact: Models.IArtifact;
        let subartifact: Models.ISubArtifact;
        let changed: boolean = false;
        if ("projectId" in item) {
            artifact = item as Models.IArtifact;
        } else {
            subartifact = item as Models.ISubArtifact;
        }

        let state = this.getState(item);

        if (!state) {
            if (artifact) {
                this.itemStateCollection.push(state = new ItemState(this, artifact, type));
                changed = true;
            } else {
                throw new Error("Artifact_Not_Found");
            }
        } else {
            if (artifact) {
                if (state.originItem !== artifact) {
                    state.originItem = artifact;
                    changed = true;
                }
            } else {
                if (angular.isArray(state.originItem.subArtifacts)) {
                    state.originItem.subArtifacts = [];
                }

                let _subartifact = state.originItem.subArtifacts.filter((it: Models.ISubArtifact) => {
                    return it.id === subartifact.id;
                })[0];
                if (_subartifact) {
                    if (_subartifact !== artifact) {
                        angular.extend(_subartifact, subartifact);
                        changed = true;
                    }
                } else {
                    state.originItem.subArtifacts.push(subartifact);
                    changed = true;
                }
            }
        }
        // removes all unchanged item states excluding current
        this._itemStateCollection = this.itemStateCollection.filter((it: ItemState) => {
            return it.isChanged || (it === state);
        });


        if (changed) {
            this.changeState(state);
        }

        return state;
    }

    public addChange(item: Models.IItem, changeSet?: IPropertyChangeSet): ItemState {
        let state = this.addItem(item);

        if (state.saveChange(item, changeSet)) {
            this.itemChanged.onNext(state);
        }

        return state;
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

}
