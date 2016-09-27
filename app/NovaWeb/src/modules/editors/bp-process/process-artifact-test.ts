import { ArtifactState, IArtifactState} from "../../managers/artifact-manager/state";
import { Models, Enums, Relationships } from "../../main/models";
import { ArtifactAttachments, IArtifactAttachments } from "../../managers/artifact-manager/attachments";
import { ArtifactProperties, SpecialProperties } from "../../managers/artifact-manager/properties";
import { ChangeSetCollector } from "../../managers/artifact-manager/changeset";
import { StatefulSubArtifactCollection, ISubArtifactCollection } from "../../managers/artifact-manager/sub-artifact";
import { IMetaData, MetaData } from "../../managers/artifact-manager/metadata";
import { IProcess, IProcessShape, IProcessLink} from "./models/process-models";
import { IItemStatus, IVersionInfo, IHashMapOfPropertyValues} from "./models/process-models";
import { IProcessService } from "./services/process.svc";
import {ItemTypePredefined, PropertyTypePredefined} from "../../main/models/enums";
import { IStatefulArtifactServices } from "../../managers/artifact-manager/services";
import { IArtifactRelationships, ArtifactRelationships } from "../../managers/artifact-manager/relationships";
import { ChangeTypeEnum, IChangeCollector, IChangeSet } from "../../managers/artifact-manager/changeset/";
import { IDocumentRefs, DocumentRefs }  from "../../managers/artifact-manager/docrefs/";
import { IStatefulArtifact, IArtifactProperties} from "../../managers/models";
import { IIStatefulArtifact, IArtifactAttachmentsResultSet} from "../../managers/models";


export class StatefulProcessArtifactTest implements IProcess, IStatefulArtifact, IIStatefulArtifact {
     // IStatefulArtifact fields 

    public artifactState: IArtifactState;
    public attachments: IArtifactAttachments;
    public docRefs: IDocumentRefs;
    public relationships: IArtifactRelationships;
    public customProperties: IArtifactProperties;
    public specialProperties: IArtifactProperties;
    public subArtifactCollection: ISubArtifactCollection;
    public metadata: IMetaData;
    public deleted: boolean;

   
    private process: IProcess = null;
    private subject: Rx.Subject<IStatefulArtifact>;
    private subscribers: Rx.IDisposable[];
    private changesets: IChangeCollector;
    private lockPromise: ng.IPromise<IStatefulArtifact>;
    private loadPromise: ng.IPromise<IStatefulArtifact>;

    
    
    constructor(private artifact: Models.IArtifact, private services: IStatefulArtifactServices) {

        // Note: Process model is not loaded until load() method is called 

        this.artifactState = new ArtifactState(this);
        this.changesets = new ChangeSetCollector(this);
        this.metadata = new MetaData(this);
        this.customProperties = new ArtifactProperties(this, artifact.customPropertyValues);
        this.specialProperties = new SpecialProperties(this, artifact.specificPropertyValues);
        this.attachments = new ArtifactAttachments(this);
        this.docRefs = new DocumentRefs(this);
        this.relationships = new ArtifactRelationships(this);
        this.subArtifactCollection = new StatefulSubArtifactCollection(this, this.services);
        this.subject = new Rx.Subject<IStatefulArtifact>();
        this.deleted = false;

        this.subscribers = [
            this.artifactState.observable()
                .subscribeOnNext(this.onChanged, this)
        ];
    }

    // IProcess interface 
    

    public get id(): number {
        return this.artifact.id;
    }

    public get name(): string {
        return this.artifact.name;
    }

    public get itemTypeId(): number {
        return this.artifact.itemTypeId;
    }

    public get typePrefix(): string {
        if (this.process) {
            return this.process.typePrefix;
        } else {
            throw new Error("process not defined in typePrefix getter");
        }
        
    }
      
    public get baseItemTypePredefined(): ItemTypePredefined {
        if (this.process) {
            return this.process.baseItemTypePredefined;
        } else {
            throw new Error("process not defined in baseItemTypePredefined getter");
        } 
    }

    public get shapes(): IProcessShape[] {
        if (this.process) {
            return this.process.shapes;
        } else {
            throw new Error("process not defined in shapes getter");
        } 
    }

    public get links(): IProcessLink[] {
        if (this.process) {
            return this.process.links;
        } else {
            throw new Error("process not defined in links getter");
        } 
    }

    public get decisionBranchDestinationLinks(): IProcessLink[] {
        if (this.process) {
            return this.process.decisionBranchDestinationLinks;
        } else {
            throw new Error("process not defined in typePrefix getter");
        } 
    }

    public get propertyValues(): IHashMapOfPropertyValues {
        if (this.process) {
            return this.process.propertyValues;
        } else {
            throw new Error("process not defined in propertyValues getter");
        } 
    }

    public get status(): IItemStatus {
        if (this.process) {
            return this.process.status;
        } else {
            throw new Error("process not defined in status getter");
        } 
    }

    public get requestedVersionInfo(): IVersionInfo {
        if (this.process) {
            return this.process.requestedVersionInfo;
        } else {
            throw new Error("process not defined in requestedVersionInfo getter");
        } ;
    }


    public dispose() {
        //TODO: implement logic to release resources
        this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this.subject.dispose();
        delete this.subscribers;
        delete this.subject;
        
    }

    public observable(): Rx.Observable<IStatefulArtifact> {
        return this.subject.asObservable();
    }    

    // IStatefulArtifact interface 
    public get projectId() {
       return this.artifact.projectId;
    }

    public set projectId(value: number) {
        this.artifact.projectId = value;
    }
     
    public get description(): string {
        return this.artifact.description;
    }

    public set description(value: string) {
       this.artifact.description = value;
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

    public get version(): number {
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
           this.changesets.add(changeset);
           
           this.lock(); 
        }
    }

    public discard(all: boolean = false) {

        this.changesets.reset().forEach((it: IChangeSet) => {
            if (!all) {
                this[it.key as string].value = it.value;
            }
        });

        this.customProperties.discard();
        this.specialProperties.discard();

        //TODO: need impementation
         this.attachments.discard();
         this.docRefs.discard();
         this.subArtifactCollection.list().forEach(subArtifact => {
             subArtifact.discard();
         });
         this.artifactState.dirty = false;
    }
    
    public setValidationErrorsFlag(value: boolean) {
        this.artifactState.invalid = value;
    }

    public load(force: boolean = true):  ng.IPromise<IStatefulArtifact> {

        // loads process 
        let defer: ng.IDeferred<any> = this.services.getDeferred();
        
        setTimeout(() => {
               
                defer.resolve(this);
                 
        }, 1000);
        

        return defer.promise;

    }

    private isProject(): boolean {
        return this.itemTypeId === Enums.ItemTypePredefined.Project;
    }

    private validateLock(lock: Models.ILockResult) {
        if (lock.result === Enums.LockResultEnum.Success) {
            this.artifactState.lock(lock);
            if (lock.info && lock.info.versionId !== this.version) {
                this.artifactState.outdated = true;
                this.discard(true);
            }
        } else {
            this.artifactState.readonly = true;
            this.artifactState.outdated = true;
            this.discard(true);
            if (lock.result === Enums.LockResultEnum.AlreadyLocked) {
                this.artifactState.lock(lock);
            } else if (lock.result === Enums.LockResultEnum.DoesNotExist) {
                this.artifactState.error = "Artifact_Lock_" + Enums.LockResultEnum[lock.result];
            } else {
                this.services.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[lock.result]);
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
                this.validateLock(lock); 
                //modifies all other state at once 
                this.artifactState.set(this.artifactState.get());
                deferred.resolve(this);
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

    public publish(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();
        return deffered.promise;
    }

    public refresh(): ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        const disposable = this.observable()
            .filter(artifact => artifact && !artifact.artifactState.outdated)
            .subscribe((artifact) => {

            disposable.dispose();
            deferred.resolve(artifact);
        });
        this.artifactState.set({ outdated: true });

        return deferred.promise;
    }
}
