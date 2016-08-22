import {IStateManager, ItemState, IPropertyChangeSet} from "./state-manager";
import { Models } from "../../main/models";

export class StateManagerMock implements IStateManager {

    public dispose(): void { }

    public stateChange: Rx.Observable<ItemState>;
    public addItem(origin: Models.IItem, itemtype?: Models.IItemType): ItemState {
        return null;
    }
    public addChange(origin: Models.IItem, changeSet?: IPropertyChangeSet): ItemState {
        return null;
    }
    public getState(item: number | Models.IItem): ItemState {
        return null;
    }
    
}