
export interface IBlock<T> {
    value: ng.IPromise<T[]>;
    observable: Rx.IObservable<T[]>;
    add(T): ng.IPromise<T[]>
    remove(T): ng.IPromise<T[]>
    update(T): ng.IPromise<T[]>
}


