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
    IArtifactAttachmentsResultSet
} from "../../models";


export class StatefulArtifact implements IStatefulArtifact, IIStatefulArtifact {
    public artifactState: IArtifactState;
    public attachments: IArtifactAttachments;
    public docRefs: IDocumentRefs;
    public relationships: IArtifactRelationships;
    public customProperties: IArtifactProperties;
    public specialProperties: IArtifactProperties;
    public subArtifactCollection: ISubArtifactCollection;
    public metadata: IMetaData;
    public deleted: boolean;

    private subject: Rx.BehaviorSubject<IStatefulArtifact> ;
    private subscribers: Rx.IDisposable[];
    private changesets: IChangeCollector;
    private lockPromise: ng.IPromise<IStatefulArtifact>;
    private loadPromise: ng.IPromise<IStatefulArtifact>;
    private isLoaded = false;

    constructor(private artifact: Models.IArtifact, private services: IStatefulArtifactServices) {
        this.artifactState = new ArtifactState(this).initialize(artifact);
        this.changesets = new ChangeSetCollector(this);
        this.metadata = new MetaData(this);
        this.customProperties = new ArtifactProperties(this).initialize(artifact.customPropertyValues);
        this.specialProperties = new SpecialProperties(this).initialize(artifact.specificPropertyValues);
        this.attachments = new ArtifactAttachments(this);
        this.docRefs = new DocumentRefs(this);
        this.relationships = new ArtifactRelationships(this);
        this.subArtifactCollection = new StatefulSubArtifactCollection(this, this.services);
        this.subject = new Rx.BehaviorSubject<IStatefulArtifact>(this);
        this.deleted = false;

        this.subscribers = [
            this.artifactState.observable()
                .subscribeOnNext(this.onChanged, this),
        ];
        // this.artifactState.observable
        //     .filter((it: IArtifactState) => !!it.get().lock)
        //     .distinctUntilChanged()
        //     .subscribeOnNext(this.onChanged, this);
    }

    public dispose() {
        //TODO: implement logic to release resources
        this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this.subject.dispose();
        delete this.subscribers;
        delete this.subject;

    }

    public observable(): Rx.Observable<IStatefulArtifact> {
        return this.subject.filter(it => it !== null).asObservable();
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
           const oldValue = this[name];
           const changeset = {
               type: ChangeTypeEnum.Update,
               key: name,
               value: this.artifact[name] = value              
           } as IChangeSet;
           this.changesets.add(changeset, oldValue);
           
           this.lock(); 
        }
    }

    public discard(all: boolean = false) {

        this.changesets.reset().forEach((it: IChangeSet) => {
            if (!all) {
                this[it.key as string].value = it.value;
            }
        });

        this.customProperties.discard(all);
        this.specialProperties.discard(all);

        //TODO: need impementation
         this.attachments.discard();
         this.docRefs.discard();
         this.subArtifactCollection.list().forEach(subArtifact => {
             subArtifact.discard();
         });
    }
    
    public setValidationErrorsFlag(value: boolean) {
        this.artifactState.invalid = value;
    }

    public load(force: boolean = true):  ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        if (!this.isProject() && (force || !this.isLoaded)) {
            if (this.loadPromise) {
                return this.loadPromise;
            } else {
                this.loadPromise = deferred.promise;
                this.services.artifactService.getArtifact(this.id).then((artifact: Models.IArtifact) => {
                    this.artifact = artifact;
                    this.artifactState.initialize(artifact);
                    this.customProperties.initialize(artifact.customPropertyValues);
                    this.specialProperties.initialize(artifact.specificPropertyValues);
                    
                    const parentId = this.artifact.parentId;
                    if (parentId && parentId !== artifact.parentId) {
                        this.artifactState.set({
                            readonly: true,
                            lockedby: 0
                        });
                        this.services.messageService.addError("The artifact has been moved!");
                    }
                    this.isLoaded = true;
                    this.artifactState.outdated = false;
                    deferred.resolve(this);
                }).catch((err) => {
                    deferred.reject(err);
                }).finally(() => {
                    this.loadPromise = null;
                    this.lockPromise = null;
                });
            }
        } else {
            deferred.resolve(this);
        }
        
        return deferred.promise;
    }

    private isProject(): boolean {
        return this.itemTypeId === Enums.ItemTypePredefined.Project;
    }

    private validateLock(lock: Models.ILockResult): boolean {
        let success: boolean;
        if (lock.result === Enums.LockResultEnum.Success) {
            success = true;
            if (lock.info && lock.info.versionId !== this.version) {
                this.discard(true);
                success = false;
            }
        } else if (lock.result === Enums.LockResultEnum.AlreadyLocked) {
            this.discard(true);
            success = false;
        } else if (lock.result === Enums.LockResultEnum.DoesNotExist) {
            this.services.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[lock.result]);
            this.discard(true);
            this.artifactState.readonly = true;
        } else {
            this.services.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[lock.result]);
            this.discard(true);
            this.artifactState.readonly = true;
        }
        return success;
    }

    public lock(): ng.IPromise<IStatefulArtifact> {
        if (!this.lockPromise) {

            let deferred = this.services.getDeferred<IStatefulArtifact>();
            this.lockPromise = deferred.promise;
            
            this.services.artifactService.lock(this.id).then((result: Models.ILockResult[]) => {
                let lock = result[0];
                let success = this.validateLock(lock); 
                if (success) {
                    this.artifactState.lock(lock);
                    deferred.resolve(this);
                } else if (success === false) {
                    this.artifactState.set({outdated: true});
                    deferred.resolve(this);
                } else { // undefined | null
                    deferred.reject(lock);    
                }
            }).catch((err) => {
                deferred.reject(err);
            });

        }
        return this.lockPromise;
    }

    private onChanged(artifactState: IArtifactState) {
        this.subject.onNext(this);
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
                this.discard(true);
                this.load(true).then((it: IStatefulArtifact) => {
                    this.services.messageService.addInfo("App_Save_Artifact_Error_200");
                    deffered.resolve(it);
                }).finally(() => {

                });
            }).catch((error) => {
                deffered.reject(error);
                let message: string;
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
                        } else {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_409");
                        }
                    } else {
                        message = this.services.localizationService.get("App_Save_Artifact_Error_Other") + error.statusCode;
                    }
                }
                this.services.messageService.addError(message);
                throw new Error(message);
            }
        );
       
        return deffered.promise;
    }

    public publish(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();
        return deffered.promise;
    }

    public refresh(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();

        this.load(true)
            .then((it: IStatefulArtifact) => {
                deffered.resolve(it);
            }).catch((error) => {
                deffered.reject(error);
            });

        return deffered.promise;
    }
}
