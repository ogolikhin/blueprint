import { Models, Enums } from "../main/models";
import { IArtifactState, IState } from "./artifact-manager";
import { IStatefulItem, IIStatefulItem } from "./artifact-manager/item";
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
    IArtifactAttachmentsService,
    IState
};

export interface IDispose {
    dispose(): void;
}

export interface IBlock<T> {
    getObservable(): Rx.IObservable<T>;
    add(T): T;
    remove(T): T;
    update(T): T;
    refresh(): ng.IPromise<T>;
    discard();
}

export interface IArtifactProperties extends IDispose {
    initialize(properties: Models.IPropertyValue[]); 
    get(id: number): Models.IPropertyValue;
    set(id: number, value: any): Models.IPropertyValue;
    changes(): Models.IPropertyValue[];
    discard();
}

export interface IStatefulArtifact extends IStatefulItem, IDispose  {
    /**
     * Unload full weight artifact
     */
    unload();
    subArtifactCollection: ISubArtifactCollection;
    //load(force?: boolean): ng.IPromise<IStatefulArtifact>;
    save(): ng.IPromise<IStatefulArtifact>;
    autosave(): ng.IPromise<IStatefulArtifact>;
    publish(): ng.IPromise<IStatefulArtifact>;
    refresh(): ng.IPromise<IStatefulArtifact>;
    
    setValidationErrorsFlag(value: boolean);

    getObservable(): Rx.Observable<IStatefulArtifact>;
}

// TODO: explore the possibility of using an internal interface for services
export interface IIStatefulArtifact extends IIStatefulItem {
}

export interface IIStatefulSubArtifact extends IIStatefulItem {
}

export interface IStatefulSubArtifact extends IStatefulItem, Models.ISubArtifact {
    load(force?: boolean, timeout?: ng.IPromise<any>): ng.IPromise<IStatefulSubArtifact>;
    changes(): Models.ISubArtifact;
}

export interface IArtifactNode extends IDispose {
    artifact: IStatefulArtifact;
    children?: IArtifactNode[];
    parentNode: IArtifactNode;
    id: number;
    name: string;
    projectId: number;
    //parentId: number;
    permissions: Enums.RolePermissions;
    predefinedType: Models.ItemTypePredefined;
    hasChildren?: boolean;
    loaded?: boolean;
    open?: boolean;
}
