import {IArtifactState, IState} from "../state";
import {Models, Enums, Relationships} from "../../../main/models";
import {ArtifactAttachments, IArtifactAttachments, IArtifactAttachmentsResultSet} from "../attachments";
import {ArtifactProperties, SpecialProperties} from "../properties";
import {ChangeSetCollector, ChangeTypeEnum, IChangeCollector, IChangeSet} from "../changeset";
import {StatefulSubArtifactCollection, ISubArtifactCollection} from "../sub-artifact";
import {IMetaData} from "../metadata";
import {IDocumentRefs, DocumentRefs} from "../docrefs";
import {IStatefulArtifactServices} from "../services";
import {IArtifactProperties} from "../properties";
import {IArtifactRelationships, ArtifactRelationships} from "../relationships";
import {HttpStatusCode} from "../../../core";

export interface IStatefulItem extends Models.IArtifact {
    artifactState: IArtifactState;

    deleted: boolean;
    metadata: IMetaData;

    customProperties: IArtifactProperties;
    specialProperties: IArtifactProperties;
    attachments: IArtifactAttachments;
    relationships: IArtifactRelationships;
    docRefs: IDocumentRefs;
    lock();
    discard();
    changes(): Models.ISubArtifact;
}

export interface IIStatefulItem extends IStatefulItem {
    getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet>;
    getRelationships(): ng.IPromise<Relationships.IArtifactRelationshipsResultSet>;
    getServices(): IStatefulArtifactServices;
}

export abstract class StatefulItem implements IIStatefulItem {
//    public artifactState: IArtifactState;
    public metadata: IMetaData;
    public deleted: boolean;

    protected _attachments: IArtifactAttachments;
    protected _docRefs: IDocumentRefs;
    protected _relationships: IArtifactRelationships;
    protected _customProperties: IArtifactProperties;
    protected _specialProperties: IArtifactProperties;
    protected _subArtifactCollection: ISubArtifactCollection;
    protected _changesets: IChangeCollector;
    protected lockPromise: ng.IPromise<IStatefulItem>;
    protected loadPromise: ng.IPromise<IStatefulItem>;


    constructor(private artifact: Models.IArtifact, protected services: IStatefulArtifactServices) {
//        this.subject = new Rx.BehaviorSubject<IStatefulArtifact>(null);

        this.deleted = false;
    }

    public dispose() {
        this.artifact.parentId = null;
    }

    public get id(): number {
        return this.artifact.id;
    }

    public get projectId() {
        return this.artifact.projectId;
    }

    public set projectId(value: number) {
        this.set("projectId", value);
    }

    public get name(): string {
        return this.artifact.name;
    }

    public set name(value: string) {
        this.set("name", value);
    }

    public get description(): string {
        return this.artifact.description;
    }

    public set description(value: string) {
        this.set("description", value);
    }

    public get itemTypeId(): number {
        return this.artifact.itemTypeId;
    }

    public set itemTypeId(value: number) {
        this.set("itemTypeId", value);
    }

    public get itemTypeVersionId(): number {
        return this.artifact.itemTypeVersionId;
    }

    public get predefinedType(): Models.ItemTypePredefined {
        return this.artifact.predefinedType;
    }

    public get permissions(): Enums.RolePermissions {
        return this.artifact.permissions;
    }

    public get version() {
        return this.artifact.version;
    }

    public get prefix(): string {
        return this.artifact.prefix;
    }

    public get parentId(): number {
        return this.artifact.parentId;
    }

    public get orderIndex(): number {
        return this.artifact.orderIndex;
    }

    public get createdOn(): Date {
        return this.artifact.createdOn;
    }

    public get lastEditedOn(): Date {
        return this.artifact.lastEditedOn;
    }

    public get createdBy(): Models.IUserGroup {
        return this.artifact.createdBy;
    }

    public get lastEditedBy(): Models.IUserGroup {
        return this.artifact.lastEditedBy;
    }

    public get hasChildren(): boolean {
        return this.artifact.hasChildren;
    }

    public get readOnlyReuseSettings(): Enums.ReuseSettings {
        return this.artifact.readOnlyReuseSettings;
    }

    public getServices(): IStatefulArtifactServices {
        return this.services;
    }

    public set(name: string, value: any) {
        if (name in this) {
            const changeset = {
                type: ChangeTypeEnum.Update,
                key: name,
                value: this.artifact[name] = value
            } as IChangeSet;
            this.changesets.add(changeset);

            this.lock();
        }
    }

    public get customProperties() {
        if (!this._customProperties) {
            this._customProperties = new ArtifactProperties(this);
        }
        return this._customProperties;
    }

    public get changesets() {
        if (!this._changesets) {
            this._changesets = new ChangeSetCollector(this);
        }
        return this._changesets;
    }

    public get specialProperties() {
        if (!this._specialProperties) {
            this._specialProperties = new SpecialProperties(this);
        }
        return this._specialProperties;
    }

    public get attachments() {
        if (!this._attachments) {
            this._attachments = new ArtifactAttachments(this);
        }
        return this._attachments;
    }

    public get docRefs() {
        if (!this._docRefs) {
            this._docRefs = new DocumentRefs(this);
        }
        return this._docRefs;
    }

    public get relationships() {
        if (!this._relationships) {
            this._relationships = new ArtifactRelationships(this);
        }
        return this._relationships;
    }

    public get subArtifactCollection() {
        if (!this._subArtifactCollection) {
            this._subArtifactCollection = new StatefulSubArtifactCollection(this, this.services);
        }
        return this._subArtifactCollection;
    }

    public lock() {
        //fixme: if empty function should be removed or return undefined
    }

    protected isFullArtifactLoadedOrLoading() {
        return (this._customProperties && this._customProperties.isLoaded &&
            this._specialProperties && this._specialProperties.isLoaded) ||
            this.loadPromise;
    }

    public unload() {
        if (this._customProperties) {
            this._customProperties.dispose();
            delete this._customProperties;
        }
        if (this._specialProperties) {
            this._specialProperties.dispose();
            delete this._specialProperties;
        }
        if (this._attachments) {
            this._attachments.dispose();
            delete this._attachments;
        }
        if (this._docRefs) {
            this._docRefs.dispose();
            delete this._docRefs;
        }
        //TODO: REMOVE WHEN AUTO-SAVE GETS COMPLETED. AUTO-SAVE SHOULD ALREADY HAVE THIS FLAG SET TO FALSE.
        if (this.artifactState) {
            this.artifactState.dirty = false;
        }
        //TODO: implement the same for all objects
    }

    public discard() {
        this.changesets.reset();
        if (this._customProperties) {
            this._customProperties.discard();
        }
        if (this._specialProperties) {
            this._specialProperties.discard();
        }
        if (this._attachments) {
            this._attachments.discard();
        }
        if (this._docRefs) {
            this._docRefs.discard();
        }
        if (this._relationships) {
            this._relationships.discard();
        }
        if (this._subArtifactCollection) {
            this._subArtifactCollection.discard();
        }
    }

    public initialize(artifact: Models.IArtifact): IState {

        this.artifact = artifact;
        this.customProperties.initialize(artifact.customPropertyValues);
        this.specialProperties.initialize(artifact.specificPropertyValues);

        return {} as IState;
    }


    public getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet> {
        const deferred = this.services.getDeferred();
        this.services.attachmentService.getArtifactAttachments(this.id, null, true)
            .then((result: IArtifactAttachmentsResultSet) => {
                // load attachments
                this.attachments.initialize(result.attachments);

                // load docRefs
                this.docRefs.initialize(result.documentReferences);

                deferred.resolve(result);
            }, (error) => {
                if (error && error.statusCode === HttpStatusCode.NotFound) {
                    this.deleted = true;
                }
                deferred.reject(error);
            });
        return deferred.promise;
    }

    public getRelationships(): ng.IPromise<Relationships.IArtifactRelationshipsResultSet> {
        const deferred = this.services.getDeferred();
        this.services.relationshipsService.getRelationships(this.id)
            .then((result: Relationships.IArtifactRelationshipsResultSet) => {
                deferred.resolve(result);
            }, (error) => {
                if (error && error.statusCode === HttpStatusCode.NotFound) {
                    this.deleted = true;
                }
                deferred.reject(error);
            });
        return deferred.promise;
    }

    public changes(): Models.IArtifact {
        let delta: Models.IArtifact = {} as Models.Artifact;

        delta.id = this.id;
        delta.projectId = this.projectId;
        delta.customPropertyValues = [];
        this.changesets.get().forEach((it: IChangeSet) => {
            delta[it.key as string] = it.value;
        });

        delta.customPropertyValues = this.customProperties.changes();
        delta.specificPropertyValues = this.specialProperties.changes();
        delta.attachmentValues = this.attachments.changes();
        delta.docRefValues = this.docRefs.changes();

        return delta;
    }

    //TODO: moved from bp-artifactinfo

    abstract artifactState: IArtifactState;

}
