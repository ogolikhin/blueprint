// import { IMessageService } from "../../core/";
// import { Models, Enums } from "../../main/models";
// // import { IArtifactAttachment } from "../../shell/bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";
// export { ISession } from "../../shell/login/session.svc";


// export interface IBlock<T> {
//     value: ng.IPromise<T[]>;
//     observable: Rx.IObservable<T[]>;
//     add(T): ng.IPromise<T[]>;
//     remove(T): ng.IPromise<T[]>;
//     update(T): ng.IPromise<T[]>;
// }

// export interface IState {
//     readonly?: boolean;
//     dirty?: boolean;
//     published?: boolean;
//     lock?: Models.ILockResult;
// } 
// export enum ChangeTypeEnum {
//     Initial,
//     Update,
//     Add,
//     Delete
// }
// export interface IChangeSet {
//     type: ChangeTypeEnum,
//     key: string | number;
//     value: any;
// }
// export interface IChangeCollector {
//     add(changeset: IChangeSet, old?: any);
//     collection: IChangeSet[];
//     reset(): IChangeSet[];

// }

// // from artifact-attachments.svc
// export interface IArtifactAttachment {
//     userId: number;
//     userName: string;
//     fileName: string;
//     attachmentId: number;
//     uploadedDate: string;
//     guid?: string;
// }


// export interface IArtifactAttachments extends IBlock<IArtifactAttachment> {
//     // list(): IArtifactAttachment[];
//     value: ng.IPromise<IArtifactAttachment[]>;
//     observable: Rx.IObservable<IArtifactAttachment[]>;
//     add(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]>;
//     remove(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]>;
//     update(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]>;
// }

// export interface IArtifactProperties {
//     initialize(artifact: Models.IArtifact): IArtifactProperties; 
//     observable: Rx.Observable<Models.IPropertyValue>;
//     get(id: number): Models.IPropertyValue;
//     set(id: number, value: any): Models.IPropertyValue;
//     discard();

// }

// export interface IArtifactStates {
//     initialize(artifact: Models.IArtifact): IArtifactStates; 
//     get(): IState;
//     set(value: any): void;
//     locked: Enums.LockedByEnum;
//     readonly: boolean;
//     dirty: boolean;
//     published: boolean;
//     observable: Rx.Observable<IState>;
// } 

// export interface IStatefulArtifact extends Models.IArtifact  {
//     manager: IArtifactManager;
//     artifactState: IArtifactStates;
//     customProperties: IArtifactProperties;
//     attachments: IArtifactAttachments;
//     discard(): ng.IPromise<IStatefulArtifact>;
//     load(): ng.IPromise<IStatefulArtifact>;
//     lock(): ng.IPromise<IState>;
// }

// export interface IArtifactManager {
//     $q: ng.IQService;
//     messages: IMessageService;
//     currentUser: Models.IUserGroup;
//     list(): IStatefulArtifact[];
//     add(artifact: Models.IArtifact): IStatefulArtifact;
//     get(id: number): IStatefulArtifact;
//     // getObeservable(id: number): Rx.Observable<IStatefulArtifact>;
//     remove(id: number): IStatefulArtifact;
//     // removeAll(); // when closing all projects
//     // refresh(id); // refresh lightweight artifact
//     // refreshAll(id);
//     request<T>(config: ng.IRequestConfig): ng.IPromise<T>;
// }


