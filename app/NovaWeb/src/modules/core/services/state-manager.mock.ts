// import {IStateManager, ItemState, IPropertyChangeSet} from "./state-manager";
// import { Models, Enums } from "../../main/models";

// export class StateManagerMock implements IStateManager {
//     public static $inject = ["$q"];
//     constructor(private $q: ng.IQService) { }

//     public dispose(): void { }
//     public reset(): void { }

//     public stateChange: Rx.Observable<ItemState> = Rx.Observable.empty<ItemState>();
//     public addItem(origin: Models.IItem, itemtype?: Models.IItemType): ItemState {
//         return null;
//     }
//     public addChange(origin: Models.IItem, changeSet?: IPropertyChangeSet): ItemState {
//         return null;
//     }
//     public getState(item: number | Models.IItem): ItemState {
//         return null;
//     }
    
//     public lockArtifact(state: ItemState): ng.IPromise<Models.ILockResult> {
//         var deferred = this.$q.defer<Models.ILockResult>();
//         deferred.resolve({
//             result: Enums.LockResultEnum.Success,
//             info: {
//                 lockOwnerLogin: "user"
//             } as Models.IVersionInfo
//         } as Models.ILockResult);
//         return deferred.promise;

//     }
// }