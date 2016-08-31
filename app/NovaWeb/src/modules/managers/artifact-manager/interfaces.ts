import { Models, Enums } from "../../main/models";

export interface IArtifactManagerService {
    $q: ng.IQService;
    list(): IStatefulArtifact[];
    add(artifact: Models.IArtifact);
    get(id: number): IStatefulArtifact;
    // getObeservable(id: number): Rx.Observable<IStatefulArtifact>;
    remove(id: number);
    // removeAll(); // when closing all projects
    // refresh(id); // refresh lightweight artifact
    // refreshAll(id);
}



export interface IStatefulArtifact {
    manager: IArtifactManagerService;
    state: IState;
    properties: IArtifactProperties;
    //lock: void;
}



export interface IBlock<T> {
    value: ng.IPromise<T[]>;
    observable: Rx.IObservable<T[]>;
    add(T): ng.IPromise<T[]>;
    remove(T): ng.IPromise<T[]>;
    update(T): ng.IPromise<T[]>;
}

export interface  ISystemProperty {
    id: number;
    name: string;
    description: string;
    prefix?: string;
    parentId?: number;
    itemTypeId?: number;
    itemTypeVersionId?: number;
    version?: number;
    createdOn?: Date; 
    lastEditedOn?: Date;
    createdBy?: Models.IUserGroup;
    lastEditedBy?: Models.IUserGroup;

}


export interface IProperty extends Models.IPropertyValue {
    propertyLookup: Enums.PropertyLookupEnum;
    propertyName?: string;
}


export interface IArtifactProperties extends IBlock<IProperty> {
    system(): ISystemProperty;
    custom(id: number): IProperty;
    special(id: number): IProperty;
}


export interface IState {
    locked: boolean;
    readonly: boolean;
    dirty: boolean;
    published: boolean;

} 

export interface IArtifactState {

} 

