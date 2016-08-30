import { PropertyTypePredefined, PropertyLookupEnum } from "../../main/models/enums";

export interface IStatefulArtifact {
    manager: any;
    artifactId: number;
    state: IState;
    properties: IArtifactProperties;
    //lock: void;
}



export interface IBlock<T> {
    value: ng.IPromise<T[]>;
    observable: Rx.IObservable<T[]>;
    add(T): ng.IPromise<T[]>
    remove(T): ng.IPromise<T[]>
    update(T): ng.IPromise<T[]>
}



export interface IProperty {
    propertyLookup: PropertyLookupEnum;
    propertyTypeId: number | string;
    propertyTypeVersionId?: number;
    propertyTypePredefined?: PropertyTypePredefined;
    isReuseReadOnly?: boolean;
    value: any;

}


export interface IArtifactProperties extends IBlock<IProperty>{
}


export interface IState {
    locked : boolean;
    readonly: boolean;
    dirty: boolean;
    published: boolean;

} 

export interface IArtifactState {

} 

