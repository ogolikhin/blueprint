import "angular";
import { Models} from "../../main/models";

export interface IStateManager {
    dispose(): void;
    onChanged: Rx.Observable<ItemState>;
    addChangeSet(origin: Models.IItem, changeSet: IPropertyChangeSet): void;
    getState(item: number | Models.IItem): ItemState;
    deleteState(artifact: number | Models.IItem);
}

export interface IPropertyChangeSet {
    lookup: string;
    id: string | number;
    value: any;
}

export class ItemState {
    constructor(item: Models.IItem) {
        this.originItem = item;
        this._changesets = [];
    }

    private _changed: boolean = false;
    private _readonly: boolean = false;
    private _changesets: IPropertyChangeSet[];
    private _changedItem: Models.IItem;
    
    public originItem: Models.IItem;
    public isLocked: boolean = false;

    public get isReadOnly(): boolean {
        return this._readonly;
    }
    public get isChanged(): boolean {
        return this._changed;
    }

    public get changedItem(): Models.IItem {
        return this._changedItem || (this._changedItem = angular.copy(this.originItem));
    }

    public clear() {
        this.originItem = null;
        this._changedItem = null;
        this._changesets = null;
        this._changed = false;
        this._readonly = false;
        this.isLocked = false;
    }

    private saveChange(changeSet: IPropertyChangeSet) {
        let _changeset = this._changesets.filter((it: IPropertyChangeSet) => {
            return it.lookup === changeSet.lookup && it.id === changeSet.id;
        })[0]
        if (_changeset) {
            _changeset.value = changeSet.value;
        } else {
            this._changesets.push(changeSet);
        }
        
    }

    public addChange(changeSet: IPropertyChangeSet) {
        if (!changeSet) {
            return;
        }
        switch ((changeSet.lookup || "").toLowerCase()) {
            case "system":
                if (changeSet.id in this.originItem) {
                    this.changedItem[changeSet.id] = changeSet.value;
                } else {
                    return;
                }
                break; 
            case "custom":
                let propertyTypeId = changeSet.id as number;
                let customProperty = (this.changedItem.customPropertyValues || []).filter((it: Models.IPropertyValue) => {
                    return it.propertyTypeId === propertyTypeId;
                })[0];
                if (customProperty) {
                    customProperty.value = changeSet.value;
                } else {
                    return;
                }
                break; 
            case "special":
                //TODO: needs to be implemented
                break; 
            default:
                break;
        }
        this.saveChange(changeSet);
        this._changed = true;
    }

}

export class StateManager implements IStateManager {

    private _itemStateCollection: ItemState[];

    private _itemChanged: Rx.BehaviorSubject<ItemState>;

    constructor() { }

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

    public addChangeSet(originItem: Models.IItem, changeSet: IPropertyChangeSet) {
        let state = this.itemStateCollection.filter((it: ItemState) => {
            return it.originItem.id === originItem.id;
        })[0];
        if (!state) {
            this.itemStateCollection.push(state = new ItemState(originItem));
        }

        state.addChange(changeSet);
        this.itemChanged.onNext(state);
    }


    public get onChanged(): Rx.Observable<ItemState> {
        return this.itemChanged
            .filter(it => it != null)
            .asObservable();
    }

    public getState(item: number | Models.IItem): ItemState {
        let itemId = angular.isNumber(item) ? item as number : (item ? item.id : -1);
        let state: ItemState = this.itemStateCollection.filter((it: ItemState) => {
            return it.originItem.id === itemId;
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
