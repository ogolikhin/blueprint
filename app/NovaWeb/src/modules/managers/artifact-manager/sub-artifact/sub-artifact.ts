import { Models, Relationships } from "../../../main/models";
// import { ArtifactState} from "../state";
import { ArtifactAttachments, IArtifactAttachments } from "../attachments";
import { IDocumentRefs, DocumentRefs } from "../docrefs";
import { ArtifactProperties, SpecialProperties } from "../properties";
import { IStatefulArtifactServices } from "../services";
import { IMetaData, MetaData } from "../metadata";

import { 
    ChangeTypeEnum, 
    IChangeCollector, 
    ChangeSetCollector, 
    IChangeSet, 
    IArtifactRelationships, 
    ArtifactRelationships
} from "../";
import {
    IStatefulArtifact,
    IArtifactProperties,
    IIStatefulSubArtifact,
    IStatefulSubArtifact,
    IArtifactAttachmentsResultSet
} from "../../models";

export class StatefulSubArtifact implements IStatefulSubArtifact, IIStatefulSubArtifact {
    private changesets: IChangeCollector;

    public deleted: boolean;
    public attachments: IArtifactAttachments;
    public docRefs: IDocumentRefs;
    public customProperties: IArtifactProperties;
    public specialProperties: IArtifactProperties;
    public metadata: IMetaData;
    public relationships: IArtifactRelationships;

    constructor(private artifact: IStatefulArtifact, private subArtifact: Models.ISubArtifact, private services: IStatefulArtifactServices) {
        this.metadata = new MetaData(this);
        this.customProperties = new ArtifactProperties(this).initialize(subArtifact.customPropertyValues);
        this.specialProperties = new SpecialProperties(this).initialize(subArtifact.specificPropertyValues);
        this.attachments = new ArtifactAttachments(this);
        this.relationships = new ArtifactRelationships(this);
        this.docRefs = new DocumentRefs(this);
        // this.changesets = new ChangeSetCollector(this.artifact);
    }

    //TODO.
    //Needs implementation of other object like
    //attachments, traces and etc.

    public get artifactState() {
        return this.artifact.artifactState;
    }

    //TODO: implement system property getters and setters
    public get id(): number {
        return this.subArtifact.id;
    }

    public get name(): string {
        return this.subArtifact.name;
    }

    public set name(value: string) {
        this.set("name", value);
    }

    public get description(): string {
        return this.subArtifact.description;
    }

    public set description(value: string) {
        this.set("description", value);
    }

    public get itemTypeId(): number {
        return this.subArtifact.itemTypeId;
    }

    public set itemTypeId(value: number) {
        this.set("itemTypeId", value);
    }

    public get itemTypeVersionId(): number {
        return this.subArtifact.itemTypeVersionId;
    }
    public get predefinedType(): Models.ItemTypePredefined {
        return this.subArtifact.predefinedType;
    }

    public get version() {
        return this.subArtifact.version;
    }

    public get prefix(): string {
        return this.subArtifact.prefix;
    }

    public get projectId(): number {
        return this.artifact.projectId;
    }

    private set(name: string, value: any) {
        if (name in this) {
           const oldValue = this[name];
           const changeset = {
               type: ChangeTypeEnum.Update,
               key: name,
               value: value
           } as IChangeSet;
           this.changesets.add(changeset, oldValue);
           this.lock();
        }
    }

    private isLoaded = false;
    public load(force: boolean = true, timeout?: ng.IPromise<any>):  ng.IPromise<IStatefulSubArtifact> {
        const deferred = this.services.getDeferred<IStatefulSubArtifact>();
        if (force || !this.isLoaded) {
            this.services.artifactService.getSubArtifact(this.artifact.id, this.id, timeout)
                .then((subArtifact: Models.ISubArtifact) => {
                    this.subArtifact = subArtifact;
                    this.customProperties.initialize(subArtifact.customPropertyValues);
                    this.specialProperties.initialize(subArtifact.specificPropertyValues);
                    //this.artifactState.initialize(subArtifact); TODO autkin why we need it???
                    this.isLoaded = true;
                    deferred.resolve(this);
            }).catch((err) => {
                deferred.reject(err);
            });
        } else {
            deferred.resolve(this);
        }
        return deferred.promise;
    }
    public changes(): Models.ISubArtifact {
        if (this.artifactState.invalid) {
            throw new Error("App_Save_Artifact_Error_400_114");
        }
        let delta: Models.ISubArtifact = {} as Models.ISubArtifact;
        delta.id = this.id;
        /*delta.customPropertyValues = [];
        this.changesets.get().forEach((it: IChangeSet) => {
            delta[it.key as string] = it.value;
        });*/
        //delta.customPropertyValues = this.customProperties.changes();
        //delta.specificPropertyValues = this.specialProperties.changes();
        delta.attachmentValues = this.attachments.changes();
        delta.docRefValues = this.docRefs.changes();
        return delta;
    }
    public discard() {

        // this.changesets.reset().forEach((it: IChangeSet) => {
        //     this[it.key as string].value = it.value;
        // });

        this.attachments.discard();
        this.docRefs.discard();

        // deferred.resolve(this);
    }

    public lock(): ng.IPromise<IStatefulArtifact> {
        return this.artifact.lock();
    }

    public getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet> {
        const deferred = this.services.getDeferred();
        this.services.attachmentService.getArtifactAttachments(this.artifact.id, this.id, true)
            .then( (result: IArtifactAttachmentsResultSet) => {
                this.attachments.initialize(result.attachments);
                this.docRefs.initialize(result.documentReferences);
                
                deferred.resolve(result);
            }, (error) => {
                if (error && error.statusCode === 404) {
                    this.deleted = true;
                }
                deferred.reject(error);
            });
        return deferred.promise;
    }

    public getRelationships(): ng.IPromise<Relationships.IRelationship[]> {
        const deferred = this.services.getDeferred();
        this.services.relationshipsService.getRelationships(this.artifact.id, this.id)
            .then( (result: Relationships.IRelationship[]) => {
                deferred.resolve(result);
            }, (error) => {
                if (error && error.statusCode === 404) {
                    this.deleted = true;
                }
                deferred.reject(error);
            });
        return deferred.promise;
    }

    public getServices(): IStatefulArtifactServices {
        return this.services;
    }
}
