import { Models, Enums } from "../../main/models";
// import { IArtifactAttachment } from "../../shell/bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";

export interface IArtifactManager {
    $q: ng.IQService;
    currentUser: Models.IUserGroup;
    list(): IStatefulArtifact[];
    add(artifact: Models.IArtifact);
    get(id: number): IStatefulArtifact;
    // getObeservable(id: number): Rx.Observable<IStatefulArtifact>;
    remove(id: number);
    // removeAll(); // when closing all projects
    // refresh(id); // refresh lightweight artifact
    // refreshAll(id);
    request<T>(config: ng.IRequestConfig): ng.IPromise<T>;
}

export interface IStatefulArtifact extends Models.IArtifact  {
    
    manager: IArtifactManager;
    state: IState;
    customProperties: IArtifactPropertyValues;

    lock();
}



export interface IBlock<T> {
    value: ng.IPromise<T[]>;
    observable: Rx.IObservable<T[]>;
    add(T): ng.IPromise<T[]>;
    remove(T): ng.IPromise<T[]>;
    update(T): ng.IPromise<T[]>;
}

// from artifact-attachments.svc
export interface IArtifactAttachment {
    userId: number;
    userName: string;
    fileName: string;
    attachmentId: number;
    uploadedDate: string;
    guid?: string;
}

export interface IArtifactAttachments extends IBlock<IArtifactAttachment> {
    // list(): IArtifactAttachment[];
    value: ng.IPromise<IArtifactAttachment[]>;
    observable: Rx.IObservable<IArtifactAttachment[]>;
    add(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]>;
    remove(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]>;
    update(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]>;
}


export interface IArtifactPropertyValues {
    initialize(artifact: Models.IArtifact): IArtifactPropertyValues; 
    get(id: number): Models.IPropertyValue;
    observable: Rx.IObservable<Models.IPropertyValue>;
    update(id: number, value: any): ng.IPromise<Models.IPropertyValue>;
}


export interface IState {
    readonly?: boolean;
    dirty?: boolean;
    published?: boolean;
    lock?: Models.ILockResult;
} 

export interface IArtifactState {
    initialize(artifact: Models.IArtifact): IArtifactState; 
    locked: Enums.LockedByEnum;
    readonly: boolean;
    dirty: boolean;
    published: boolean;
    get(): IState;
    set(value: any): void;
    observable: Rx.Observable<IState>;
} 

export { ISession } from "../../shell/login/session.svc";