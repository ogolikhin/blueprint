import { Models, Enums } from "../../../main/models";
import { ArtifactState} from "../state";
import { ArtifactAttachments, IArtifactAttachments } from "../attachments";
import { IDocumentRefs, DocumentRefs } from "../";
import { CustomProperties } from "../properties";
import { ChangeSetCollector } from "../changeset";
import { StatefulSubArtifactCollection } from "../sub-artifact";
import { IMetaData, MetaData } from "../metadata";
import {
    ChangeTypeEnum,
    IChangeCollector,
    IChangeSet,
    IStatefulArtifact,
    ISubArtifactCollection,
    IArtifactState,
    IArtifactProperties,
    IState,
    IStatefulArtifactServices,
    IIStatefulArtifact,
    IArtifactAttachmentsResultSet
} from "../../models";


export class StatefulArtifact implements IStatefulArtifact, IIStatefulArtifact {
    public artifactState: IArtifactState;
    public attachments: IArtifactAttachments;
    public docRefs: IDocumentRefs;
    public customProperties: IArtifactProperties;
    public specialProperties: IArtifactProperties;
    public subArtifactCollection: ISubArtifactCollection;
    public metadata: IMetaData;
    
    private changesets: IChangeCollector;

    constructor(private artifact: Models.IArtifact, private services: IStatefulArtifactServices) {
        this.artifactState = new ArtifactState(this).initialize(artifact);
        this.changesets = new ChangeSetCollector();
        this.metadata = new MetaData(this);
        this.customProperties = new CustomProperties(this).initialize(artifact);
        this.attachments = new ArtifactAttachments(this);
        this.docRefs = new DocumentRefs(this);
        this.subArtifactCollection = new StatefulSubArtifactCollection(this, this.services);

        this.artifactState.observable
            .filter((it: IState) => !!it.lock)
            .distinctUntilChanged()
            .subscribeOnNext(this.onLockChanged, this);
    }

    //TODO. 
    //Needs implementation of other object like 
    //attachments, traces and etc.

    //TODO: implement system property getters and setters    
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

    public discard(): ng.IPromise<IStatefulArtifact>   {
        let deferred = this.services.getDeferred<IStatefulArtifact>();

        this.changesets.reset().forEach((it: IChangeSet) => {
            this[it.key as string].value = it.value;
        });

        this.customProperties.discard();
        this.attachments.discard();

        deferred.resolve(this);
        return deferred.promise;
    }

    public load(timeout?: ng.IPromise<any>):  ng.IPromise<IStatefulArtifact>   {
        let deferred = this.services.getDeferred<IStatefulArtifact>();

        this.services.artifactService.getArtifact(this.id).then((artifact: Models.IArtifact) => {
            this.artifact = artifact;
            this.customProperties.initialize(artifact);
            this.artifactState.initialize(artifact);
            deferred.resolve(this);
        }).catch((err) => {
            deferred.reject(err);
        });
        return deferred.promise;
        
    }

    public lock(): ng.IPromise<IState> {
        let deferred = this.services.getDeferred<IState>();

        this.services.artifactService.lock(this.id).then((result: Models.ILockResult[]) => {
            this.artifactState.set({lock: result[0]} as IState);
            deferred.resolve(this.artifactState.get());
        }).catch((err) => {
            deferred.reject(err);
        });
        return deferred.promise;
    }

    private onLockChanged(artifactState: IArtifactState) {
        let state = artifactState.get();
        if (!state.lock) {
            return;
        }
        if (state.lock.result === Enums.LockResultEnum.Success) {
            if (state.lock.info.versionId !== this.version) {
                this.load();
            }
        } else if (state.lock.result === Enums.LockResultEnum.AlreadyLocked) {
            this.services.messageService.addWarning("Artifact_Lock_" + Enums.LockResultEnum[state.lock.result]);
            this.load();
        } else if (state.lock.result === Enums.LockResultEnum.DoesNotExist) {
            this.services.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[state.lock.result]);
        } else {
            this.services.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[state.lock.result]);
        }
    }

    public getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet> {
        return this.services.attachmentService.getArtifactAttachments(this.id, null, true)
            .then( (result: IArtifactAttachmentsResultSet) => {
                // load attachments
                this.attachments.initialize(result.attachments);

                // load docRefs
                this.docRefs.initialize(result.documentReferences);

                return result;
            });
    }

    public getServices(): IStatefulArtifactServices {
        return this.services;
    }
}
