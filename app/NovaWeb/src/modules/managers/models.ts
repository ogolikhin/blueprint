import { Models, Enums } from "../main/models";
import { IArtifactState } from "./artifact-manager";
import { IStatefulArtifactServices } from "./artifact-manager/services";
import { ISession } from "../shell/login/session.svc";
import { Relationships } from "../main";
import { 
    IArtifactAttachmentsResultSet, 
    IArtifactAttachmentsService,
    IDocumentRefs,
    IArtifactAttachments,
    IMetaData,
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

export interface IArtifactProperties {
    initialize(properties: Models.IPropertyValue[]): IArtifactProperties; 
    observable: Rx.Observable<Models.IPropertyValue>;
    get(id: number): Models.IPropertyValue;
    set(id: number, value: any): Models.IPropertyValue;
    changes(): Models.IPropertyValue[];
    discard(all?: boolean);
}

// TODO: make as a base class for IStatefulArtifact / IStatefulSubArtifact
export interface IStatefulItem extends Models.IArtifact {
    deleted: boolean;
    metadata: IMetaData;
    artifactState: IArtifactState;
    customProperties: IArtifactProperties;
    specialProperties: IArtifactProperties;
    attachments: IArtifactAttachments;
    relationships: IArtifactRelationships;
    docRefs: IDocumentRefs;
    discard(all?: boolean);
    lock(): ng.IPromise<IStatefulArtifact>;
}

export interface IIStatefulItem extends IStatefulItem  {
    getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet>;
    getRelationships(): ng.IPromise<Relationships.IRelationship[]>;
    getServices(): IStatefulArtifactServices;
}

export interface IStatefulArtifact extends IStatefulItem, IDispose  {
    observable(): Rx.Observable<IStatefulArtifact>;
    subArtifactCollection: ISubArtifactCollection;
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

export interface IStatefulSubArtifact extends IStatefulItem, Models.ISubArtifact {
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
