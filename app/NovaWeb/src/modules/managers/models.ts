// import { IMessageService } from "../core/";
import { Models, Enums } from "../main/models";
import { IStatefulArtifactServices } from "./artifact-manager/services";
import { ISession } from "../shell/login/session.svc";
import { Relationships } from "../main";
import { 
    IArtifactAttachmentsResultSet, 
    IArtifactAttachmentsService,
    IDocumentRefs,
    // IMetaDataService,
    IArtifactAttachments,
    IMetaData,
    // IArtifactService,
    ISubArtifactCollection,
    IArtifactRelationships
} from "./artifact-manager";


export { 
    ISession,
    IArtifactAttachmentsResultSet, 
    IArtifactAttachmentsService
};

export interface IDispose {
    dispose(): void;
}
export interface IBlock<T> {
    observable: Rx.IObservable<T>;
    get(refresh?: boolean): ng.IPromise<T>;
    add(T): T;
    remove(T): T;
    update(T): T;
    discard();
}

export interface IState {
    readonly?: boolean;
    dirty?: boolean;
    published?: boolean;
    lock?: Models.ILockResult;
}

export interface IArtifactProperties {
    initialize(properties: Models.IPropertyValue[]): IArtifactProperties; 
    observable: Rx.Observable<Models.IPropertyValue>;
    get(id: number): Models.IPropertyValue;
    set(id: number, value: any): Models.IPropertyValue;
    changes(): Models.IPropertyValue[];
    discard(all?: boolean);
}

export interface IArtifactState extends IDispose {
    initialize(artifact: Models.IArtifact): IArtifactState; 
    get(): IState;
    //set(value: any): void;
    lock: Models.ILockResult;
    lockedBy: Enums.LockedByEnum;
    lockDateTime?: Date;
    lockOwner?: string;
    readonly: boolean;
    dirty: boolean;
    published: boolean;
    observable: Rx.Observable<IArtifactState>;
} 

// TODO: make as a base class for IStatefulArtifact / IStatefulSubArtifact
export interface IStatefulItem extends Models.IArtifact  {
    artifactState: IArtifactState;
    customProperties: IArtifactProperties;
    specialProperties: IArtifactProperties;
    attachments: IArtifactAttachments;
    relationships: IArtifactRelationships;
    docRefs: IDocumentRefs;
    // relationships: any;
    discard(all?: boolean);
    lock(): ng.IPromise<IState>;
}

export interface IIStatefulItem extends IStatefulItem  {
    getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet>;
    getRelationships(): ng.IPromise<Relationships.IRelationship[]>;
    getServices(): IStatefulArtifactServices;
}

export interface IStatefulArtifact extends IStatefulItem  {
    dispose(): void;
//    observable: Rx.Observable<IStatefulArtifact>;
    subArtifactCollection: ISubArtifactCollection;
    metadata: IMetaData;
    load(force?: boolean): ng.IPromise<IStatefulArtifact>;
    save(): ng.IPromise<IStatefulArtifact>;
    publish(): ng.IPromise<IStatefulArtifact>;
    refresh(): ng.IPromise<IStatefulArtifact>;
}

// TODO: explore the possibility of using an internal interface for services
export interface IIStatefulArtifact extends IIStatefulItem {
}

export interface IIStatefulSubArtifact extends IIStatefulItem {
}

export interface IStatefulSubArtifact extends IStatefulItem {
    metadata: IMetaData;
    load(force?: boolean, timeout?: ng.IPromise<any>): ng.IPromise<IStatefulSubArtifact>;
}

export interface IArtifactNode {
    artifact: IStatefulArtifact;
    children?: IArtifactNode[];
    id: number;
    name: string;
    projectId: number;
    parentId: number;
    permissions: Enums.RolePermissions;
    predefinedType: Models.ItemTypePredefined;
    hasChildren?: boolean;
    loaded?: boolean;
    open: boolean;
}

