import {IStatefulArtifact, IIStatefulArtifact} from "./artifact";
import {IArtifactState, ArtifactState} from "../state";
import {IMetaData} from "../metadata";
import {IArtifactProperties} from "../properties";
import {IItemChangeSet} from "../changeset";
import {IArtifactAttachments, IArtifactAttachmentsResultSet} from "../attachments";
import {IArtifactRelationships} from "../relationships";
import {Models, Enums, Relationships} from "../../../main/models";
import {IDocumentRefs} from "../docrefs";
import {IApplicationError} from "../../../shell/error/applicationError";
import {IPropertyDescriptor} from "../../../editorsModule/services";
import {IStatefulArtifactServices} from "../services";
import {ISubArtifactCollection} from "../sub-artifact";
import {IArtifactAttachment, IArtifactDocRef} from "../../../managers/artifact-manager";
import {IRelationship} from "../../../main/models/relationshipModels";

export class StatefulArtifactMock implements IStatefulArtifact, IIStatefulArtifact {
    static $inject: string[] = [
        "$q"
    ];

    constructor(public $q: ng.IQService) {
        //this.artifactState.initialize(this);
        this.artifactState = new ArtifactState(this);
    }

    //-------------------IIStatefulItem-------------------
    propertyChange: Rx.BehaviorSubject<IItemChangeSet>;

    getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet> {
        return this.$q.resolve({} as IArtifactAttachmentsResultSet);
    }
    getRelationships(): ng.IPromise<Relationships.IArtifactRelationshipsResultSet> {
        return this.$q.resolve({} as Relationships.IArtifactRelationshipsResultSet);
    }
    getServices(): IStatefulArtifactServices {
        return null;
    }

    //-------------------IStatefulArtifact-------------------
    lastSaveInvalid: boolean;
    isDisposed: boolean;
    subArtifactCollection: ISubArtifactCollection;

    // Unload full weight artifact
    unload() {
        //
    }
    save(ignoreInvalidValues?: boolean ): ng.IPromise<IStatefulArtifact> {
        return this.$q.resolve({} as IStatefulArtifact);
    }
    delete(): ng.IPromise<Models.IArtifact[]> {
        return this.$q.resolve({} as Models.IArtifact[]);
    }
    publish(): ng.IPromise<void> {
         return this.$q.resolve();
    }
    discardArtifact(): ng.IPromise<void> {
        return this.$q.resolve();
    }
    refresh(allowCustomRefresh?: boolean): ng.IPromise<IStatefulArtifact> {
        return this.$q.resolve({} as IStatefulArtifact);
    }
    getObservable(): Rx.Observable<IStatefulArtifact> {
        return new Rx.BehaviorSubject<IStatefulArtifact>(this).asObservable();
    }
    getValidationObservable(): Rx.Observable<number[]> {
        return new Rx.Subject<number[]>().asObservable();
    }
    move(newParentId: number, orderIndex?: number): ng.IPromise<Models.IArtifact> {
        return this.$q.resolve({});
    }
    copy(newParentId: number, orderIndex?: number): ng.IPromise<Models.ICopyResultSet> {
        return this.$q.resolve({} as Models.ICopyResultSet);
    }
    canBeSaved(): boolean {
        return true;
    }
    canBePublished(): boolean {
        return true;
    }

    //-------------------IStatefulItem-------------------
    artifactState: IArtifactState;
    metadata: IMetaData;
    customProperties: IArtifactProperties;
    specialProperties: IArtifactProperties;
    attachments: IArtifactAttachments;
    relationships: IArtifactRelationships;
    docRefs: IDocumentRefs;

    supportRelationships(): boolean {
        return true;
    }
    lock() {
        //
    }
    discard() {
        //
    }
    changes(): Models.ISubArtifact {
        return null;
    }
    errorObservable(): Rx.Observable<IApplicationError> {
        return new Rx.BehaviorSubject<IApplicationError>(null).asObservable();
    }
    unsubscribe() {
        //
    }
    getEffectiveVersion(): number {
        return 0;
    }
    getPropertyObservable(): Rx.Observable<IItemChangeSet> {
        return new Rx.BehaviorSubject<IItemChangeSet>(null).asObservable();
    }
    validateItem(propertyDescriptors: IPropertyDescriptor[]): boolean {
        return true;
    }
    isReuseSettingSRO(reuseSetting: Enums.ReuseSettings): boolean {
        return true;
    }


    //-------------------IArtifact-------------------
    projectId?: number;
    orderIndex?: number;
    //version?: number;

    createdOn?: Date;
    lastEditedOn?: Date;
    createdBy?: Models.IUserGroup;
    lastEditedBy?: Models.IUserGroup;
    lastSavedOn?: Date;

    lockedByUser?: Models.IUserGroup;
    lockedDateTime?: Date;

    permissions?: Enums.RolePermissions;
    readOnlyReuseSettings?: Enums.ReuseSettings;

    hasChildren?: boolean;
    subArtifacts?: Models.ISubArtifact[];

    itemTypeIconId?: number;
    itemTypeName?: string;
    //lastSaveInvalid?: boolean;
    //for client use
    children?: Models.IArtifact[];
    loaded?: boolean;
    // for artifact picker use
    artifactPath?: string[];
    idPath?: number[];
    parentPredefinedType?: Models.ItemTypePredefined;

    //-------------------IItem-------------------
    id: number;
    name?: string;
    description?: string;
    prefix?: string;
    parentId?: number;
    itemTypeId?: number;
    itemTypeVersionId?: number;
    version?: number;
    customPropertyValues?: Models.IPropertyValue[];
    specificPropertyValues?: Models.IPropertyValue[];
    systemPropertyValues?: Models.IPropertyValue[];
    traces?: IRelationship[];

    predefinedType?: Models.ItemTypePredefined;

    attachmentValues?: IArtifactAttachment[];
    docRefValues?: IArtifactDocRef[];

    //-------------------IDispose-------------------
    dispose() {
        //
    }
}
