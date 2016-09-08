import { IMessageService } from "../core/";
import { Models, Enums } from "../main/models";
import { 
    IArtifactAttachmentsResultSet, 
    IArtifactAttachmentsService,
    IArtifactAttachment 
} from "../shell/bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";

import {
    IArtifactService,
} from "../main/services/artifact.svc";


export { ISession } from "../shell/login/session.svc";
export { 
    IArtifactService,
    IArtifactAttachmentsResultSet, 
    IArtifactAttachmentsService 
};

export interface IBlock<T> {
    value: ng.IPromise<T[]>;
    observable: Rx.IObservable<T[]>;
    add(T): ng.IPromise<T[]>;
    remove(T): ng.IPromise<T[]>;
    update(T): ng.IPromise<T[]>;
}

export interface IState {
    readonly?: boolean;
    dirty?: boolean;
    published?: boolean;
    lock?: Models.ILockResult;
}

export enum ChangeTypeEnum {
    Initial,
    Update,
    Add,
    Delete
}

export interface IChangeSet {
    type: ChangeTypeEnum;
    key: string | number;
    value: any;
}
export interface IChangeCollector {
    add(changeset: IChangeSet, old?: any);
    collection: IChangeSet[];
    reset(): IChangeSet[];

}

export interface IArtifactAttachments extends IBlock<IArtifactAttachment> {
    // list(): IArtifactAttachment[];
    initialize(attachments: IArtifactAttachment[]);
    value: ng.IPromise<IArtifactAttachment[]>;
    observable: Rx.IObservable<IArtifactAttachment[]>;
    add(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]>;
    remove(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]>;
    update(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]>;
}

export interface IArtifactProperties {
    initialize(artifact: Models.IArtifact): IArtifactProperties; 
    observable: Rx.Observable<Models.IPropertyValue>;
    get(id: number): Models.IPropertyValue;
    set(id: number, value: any): Models.IPropertyValue;
    discard();
}

export interface IArtifactStates {
    initialize(artifact: Models.IArtifact): IArtifactStates; 
    get(): IState;
    set(value: any): void;
    locked: Enums.LockedByEnum;
    readonly: boolean;
    dirty: boolean;
    published: boolean;
    observable: Rx.Observable<IState>;
} 

export interface IStatefulArtifact extends Models.IArtifact  {
    manager: IArtifactManager;
    artifactState: IArtifactStates;
    customProperties: IArtifactProperties;
    attachments: IArtifactAttachments;
    discard(): ng.IPromise<IStatefulArtifact>;
    load(): ng.IPromise<IStatefulArtifact>;
    lock(): ng.IPromise<IState>;
}

// TODO: explore the possibility of using an internal interface for services
export interface IIStatefulArtifact extends IStatefulArtifact {
    getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet>;
}

export interface IArtifactManager {
    currentUser: Models.IUserGroup;
    list(): IStatefulArtifact[];
    add(artifact: Models.IArtifact): IStatefulArtifact;
    get(id: number): IStatefulArtifact;
    remove(id: number): IStatefulArtifact;
}

export interface IStatefulArtifactServices {
    //request<T>(config: ng.IRequestConfig): ng.IPromise<T>;
    getDeferred<T>(): ng.IDeferred<T>;
    messageService: IMessageService;
    artifactService: IArtifactService,
    attachmentService: IArtifactAttachmentsService;
}


export interface IProjectArtifact {
    artifact: IStatefulArtifact;
    parent: IProjectArtifact;
    children: IProjectArtifact[];
}

