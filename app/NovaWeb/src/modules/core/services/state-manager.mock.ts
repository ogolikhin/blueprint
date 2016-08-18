import {IStateManager, ItemState, IPropertyChangeSet} from "./state-manager";
import { Models } from "../../main/models";

export class StateManagerMock implements IStateManager {

    public dispose(): void { }

    public onChanged: Rx.Observable<ItemState>;
    public addChange(origin: Models.IItem, changeSet?: IPropertyChangeSet): void { }
    public getState(item: number | Models.IItem): ItemState {
        return null;
    }
    public deleteState(artifact: number | Models.IItem) {

    }
}