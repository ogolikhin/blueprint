import { ArtifactState, IArtifactState} from "../state";
import { Models, Enums, Relationships } from "../../../main/models";
import { ArtifactAttachments, IArtifactAttachments } from "../attachments";
import { ArtifactProperties, SpecialProperties } from "../properties";
import { ChangeSetCollector } from "../changeset";
import { StatefulSubArtifactCollection, ISubArtifactCollection } from "../sub-artifact";
import { IMetaData, MetaData } from "../metadata";
import { IStatefulArtifactServices } from "../services";
import { IArtifactRelationships, ArtifactRelationships } from "../relationships";
import { IDocumentRefs, DocumentRefs, ChangeTypeEnum, IChangeCollector, IChangeSet } from "../";
import {
    IStatefulArtifact,
    IArtifactProperties,
    IIStatefulArtifact,
    IArtifactAttachmentsResultSet,
    IState
} from "../../models";


export class StatefulArtifact implements IStatefulArtifact, IIStatefulArtifact {
    public artifactState: IArtifactState;
    public metadata: IMetaData;
    public deleted: boolean;

    private _attachments: IArtifactAttachments;
    private _docRefs: IDocumentRefs;
    private _relationships: IArtifactRelationships;
    private _customProperties: IArtifactProperties;
    private _specialProperties: IArtifactProperties;
    private _subArtifactCollection: ISubArtifactCollection;
    private _changesets: IChangeCollector;

    private subject: Rx.BehaviorSubject<IStatefulArtifact> ;
    private lockPromise: ng.IPromise<IStatefulArtifact>;
    private loadPromise: ng.IPromise<IStatefulArtifact>;

    constructor(private artifact: Models.IArtifact, protected services: IStatefulArtifactServices) {
        this.subject = new Rx.BehaviorSubject<IStatefulArtifact>(null);
        this.artifactState = new ArtifactState(this);
        this.metadata = new MetaData(this);
        this.deleted = false;
    }

    public dispose() {
        this.subject.dispose();
        delete this.subject;
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

    private set(name: string, value: any) {
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

    public getObservable(): Rx.Observable<IStatefulArtifact> {
        if (!this.isFullArtifactLoadedOrLoading()) {
            this.loadPromise = this.load();

            this.loadPromise.then((artifact) => {
                this.subject.onNext(artifact);
            }).catch((error) => {
                this.subject.onError(error);
            }).finally(() => {
                this.loadPromise = null;
            });
        }
        return this.subject.filter(it => !!it).asObservable();
    }

    protected isFullArtifactLoadedOrLoading() {
        return this._customProperties && this._specialProperties || this.loadPromise;
    }

    public unload() {
        if ( this._customProperties) {
            this._customProperties.dispose();
            delete this._customProperties;
        }
        if ( this._specialProperties) {
            this._specialProperties.dispose();
            delete this._specialProperties;
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
        if (this._subArtifactCollection) {
            this._subArtifactCollection.discard();
        }
        this.artifactState.dirty = false;
    }
    
    public setValidationErrorsFlag(value: boolean) {
        this.artifactState.invalid = value;
    }

    private loadInternal(artifact: Models.IArtifact): IState {
        const artifactBeforeUpdate = this.artifact;
        
        this.artifact = artifact;
        this.artifactState.initialize(artifact);
        this.customProperties.initialize(artifact.customPropertyValues);
        this.specialProperties.initialize(artifact.specificPropertyValues);
        
        let state = this.artifactState.get();
        if (artifactBeforeUpdate.parentId !== artifact.parentId || artifactBeforeUpdate.orderIndex !== artifact.orderIndex) {
            state.misplaced = true;
        }

        return state;
    }

    protected load():  ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        if (! this.isProject() && !(this.artifactState.dirty && this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser)) {
            this.services.artifactService.getArtifact(this.id).then((artifact: Models.IArtifact) => {
                let state = this.loadInternal(artifact);
                //modify states all at once
                this.artifactState.set(state);
                deferred.resolve(this);
            }).catch((err) => {
                this.artifactState.readonly = true;
                deferred.reject(new Error(err.message));
            });
        } else {
            deferred.resolve(this);
        }
        
        return deferred.promise;
    }

    private isProject(): boolean {
        return this.itemTypeId === Enums.ItemTypePredefined.Project;
    }

    private processLock(lock: Models.ILockResult) {
        if (lock.result === Enums.LockResultEnum.Success) {
            this.artifactState.lock(lock);
            if (lock.info.versionId !== this.version) {
                this.refresh();             
            } else {
                if (lock.info.parentId !== this.parentId || lock.info.orderIndex !== this.orderIndex) {
                    this.artifactState.misplaced = true;
                }
                this.subject.onNext(this);
            }

        } else {
            if (lock.result === Enums.LockResultEnum.AlreadyLocked) {
                this.refresh();
            } else {
                this.discard();
                if (lock.result === Enums.LockResultEnum.DoesNotExist) {
                    this.artifactState.deleted = true;
                } else {
                    this.artifactState.readonly = true;
                }
                this.subject.onNext(this);
                this.subject.onError(new Error("Artifact_Lock_" + Enums.LockResultEnum[lock.result]));
            }
        }
    }

    public lock(): ng.IPromise<IStatefulArtifact> {
        if (this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
            return;
        }
        if (!this.lockPromise) {

            let deferred = this.services.getDeferred<IStatefulArtifact>();
            this.lockPromise = deferred.promise;
            
            this.services.artifactService.lock(this.id).then((result: Models.ILockResult[]) => {
                let lock = result[0];
                this.processLock(lock); 
                //modifies all other state at once 
                this.artifactState.set(this.artifactState.get());
                deferred.resolve(this);
            }).catch((err) => {
                deferred.reject(err);
            });

        }
        return this.lockPromise;
    }

    public getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet> {
        const deferred = this.services.getDeferred();
        this.services.attachmentService.getArtifactAttachments(this.id, null, true)
            .then( (result: IArtifactAttachmentsResultSet) => {
                // load attachments
                this.attachments.initialize(result.attachments);

                // load docRefs
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
        this.services.relationshipsService.getRelationships(this.id)
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

    private changes(): Models.IArtifact {
        if (this.artifactState.invalid) {
            throw new Error("App_Save_Artifact_Error_400_114");
        }

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
        this.addSubArtifactChanges(delta);

        return delta;
    }

    private addSubArtifactChanges(delta: Models.IArtifact) {
        let subArtifacts = this.subArtifactCollection.list();
        delta.subArtifacts = new Array<Models.ISubArtifact>();
        subArtifacts.forEach(subArtifact => {
            delta.subArtifacts.push(subArtifact.changes());
        });
    }
    //TODO: moved from bp-artifactinfo 
    
    public save(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();
       
        let changes = this.changes();
        this.services.artifactService.updateArtifact(changes)
            .then((artifact: Models.IArtifact) => {
                this.discard();
                this.refresh();
                this.services.messageService.addInfo("App_Save_Artifact_Error_200");

            }).catch((error) => {
                deffered.reject(error);
                let message: string;
                // if error is undefined it means that it handled on upper level (http-error-interceptor.ts)
                if (error) {
                    if (error.statusCode === 400) {
                        if (error.errorCode === 114) {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_400_114");
                        } else {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_400") + error.message;
                        }
                    } else if (error.statusCode === 404) {
                        message = this.services.localizationService.get("App_Save_Artifact_Error_404");
                    } else if (error.statusCode === 409) {
                        if (error.errorCode === 116) {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_409_116");
                        } else if (error.errorCode === 117) {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_409_117");
                        } else if (error.errorCode === 111 || error.errorCode === 115) {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_409_115");
                        } else if (error.errorCode === 124) {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_409_123");
                        } else {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_409");
                        }
                    } else {
                        message = this.services.localizationService.get("App_Save_Artifact_Error_Other") + error.statusCode;
                    }
                    this.services.messageService.addError(message);
                    throw new Error(message);
                }                                
            }
        );
       
        return deffered.promise;
    }

    //TODO: stub - replace with implementation
    public autosave(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();
        deffered.resolve();
        return deffered.promise;
    }

    public publish(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();
        return deffered.promise;
    }

    public refresh(): ng.IPromise<IStatefulArtifact> {
         const deferred = this.services.getDeferred<IStatefulArtifact>();
        this.discard();

        this.load().then((artifact: IStatefulArtifact) => {
            //TODO: initialize all components
            this.subject.onNext(artifact);
            deferred.resolve(artifact);
        }).catch((error) => {
            this.subject.onError(error);
            deferred.reject(error);
        }).finally(() => {

        });
        
        // TODO: also load subartifacts and the rest of the
        // if (this._attachments) {
        //     this._attachments.get(true);
        // }


        // TODO: return void, no more promises
        return deferred.promise;
    }
}
