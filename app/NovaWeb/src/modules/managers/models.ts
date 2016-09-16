import { IMessageService } from "../core/";
import { Models, Enums } from "../main/models";
// import { IProjectManager } from  "./project-manager";
import { 
    IArtifactAttachmentsResultSet, 
    IArtifactAttachmentsService,
    // IArtifactAttachment,
    IDocumentRefs,
    IMetaDataService,
    IArtifactAttachments,
    IMetaData,
    IArtifactService
} from "./artifact-manager";



import { ISession } from "../shell/login/session.svc";

export { 
    ISession,
    IArtifactService,
    IArtifactAttachmentsResultSet, 
    IArtifactAttachmentsService
};

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
    lockedBy: Enums.LockedByEnum;
    readonly: boolean;
    dirty: boolean;
    published: boolean;
    observable: Rx.Observable<IState>;
} 

// TODO: make as a base class for IStatefulArtifact / IStatefulSubArtifact
export interface IStatefulItem extends Models.IArtifact  {
    artifactState: IArtifactStates;
    customProperties: IArtifactProperties;
    attachments: IArtifactAttachments;
    docRefs: IDocumentRefs;
    // relationships: any;
    discard(): ng.IPromise<IStatefulArtifact>;
    lock(): ng.IPromise<IState>;
}

export interface IIStatefulItem extends IStatefulItem  {
    getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet>;
    getServices(): IStatefulArtifactServices;
}

export interface IStatefulArtifact extends IStatefulItem  {
    subArtifactCollection: ISubArtifactCollection;
    load(): ng.IPromise<IStatefulArtifact>;
    metadata: IMetaData;
}

// TODO: explore the possibility of using an internal interface for services
export interface IIStatefulArtifact extends IIStatefulItem {
}
export interface IIStatefulSubArtifact extends IIStatefulItem {
}

export interface IStatefulSubArtifact extends IStatefulItem {
}

export interface ISubArtifactCollection {
    list(): IStatefulSubArtifact[];
    add(subArtifact: IStatefulSubArtifact): IStatefulSubArtifact;
    get(id: number): IStatefulSubArtifact;
    remove(id: number): IStatefulSubArtifact;
}

export interface IStatefulArtifactServices {
    //request<T>(config: ng.IRequestConfig): ng.IPromise<T>;
    getDeferred<T>(): ng.IDeferred<T>;
    messageService: IMessageService;
    session: ISession;
    artifactService: IArtifactService;
    attachmentService: IArtifactAttachmentsService;
    metaDataService: IMetaDataService;
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

