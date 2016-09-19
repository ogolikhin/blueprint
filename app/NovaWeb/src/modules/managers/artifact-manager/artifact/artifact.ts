import { Models, Enums } from "../../../main/models";
import { ArtifactState} from "../state";
import { ArtifactAttachments, IArtifactAttachments } from "../attachments";
import { IDocumentRefs, DocumentRefs, ChangeTypeEnum, IChangeCollector, IChangeSet } from "../";
import { ArtifactProperties, SpecialProperties } from "../properties";
import { ChangeSetCollector } from "../changeset";
import { StatefulSubArtifactCollection, ISubArtifactCollection } from "../sub-artifact";
import { IMetaData, MetaData } from "../metadata";
import {
    IStatefulArtifact,
    IStatefulSubArtifact,
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
//    private subject: Rx.BehaviorSubject<IStatefulArtifact>;
    
    private changesets: IChangeCollector;

    constructor(private artifact: Models.IArtifact, private services: IStatefulArtifactServices) {
        this.artifactState = new ArtifactState(this).initialize(artifact);
        this.changesets = new ChangeSetCollector(this);
        this.metadata = new MetaData(this);
        this.customProperties = new ArtifactProperties(this).initialize(artifact.customPropertyValues);
        this.specialProperties = new SpecialProperties(this).initialize(artifact.specificPropertyValues);
        this.attachments = new ArtifactAttachments(this);
        this.docRefs = new DocumentRefs(this);
        this.subArtifactCollection = new StatefulSubArtifactCollection(this, this.services);
//        this.subject = new Rx.BehaviorSubject<IStatefulArtifact>(null);
        
        this.artifactState.observable
            .filter((it: IArtifactState) => !!it.get().lock)
            .distinctUntilChanged()
            .subscribeOnNext(this.onLockChanged, this);
        // this.artifactState.observable
        //     .filter((it: IArtifactState) => !!it.get().lock)
        //     .distinctUntilChanged()
        //     .subscribeOnNext(this.onChanged, this);
            
    }
    public dispose() {
        //TODO: implement logic to release resources
    }

    // public get observable(): Rx.Observable<IStatefulArtifact> {
    //     return this.subject.filter(it => it !== null).asObservable();
    // }    


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
        // this.attachments.discard(all);
        // this.docRefs.discard(all);
        // this.subArtifactCollection.discard(all);

    }

    private isLoaded = false;
    public load(force: boolean = false):  ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        if (force || !this.isLoaded) {
            this.services.artifactService.getArtifact(this.id).then((artifact: Models.IArtifact) => {
                this.artifact = artifact;
                this.artifactState.initialize(artifact);
                this.customProperties.initialize(artifact.customPropertyValues);
                this.specialProperties.initialize(artifact.specificPropertyValues);
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

    public lock(): ng.IPromise<IState> {
        let deferred = this.services.getDeferred<IState>();

        this.services.artifactService.lock(this.id).then((result: Models.ILockResult[]) => {
            this.artifactState.lock = result[0];
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
                //this.discard();
                this.load();
            }
        } else if (state.lock.result === Enums.LockResultEnum.AlreadyLocked) {
            //this.discard();
            this.load();
        } else if (state.lock.result === Enums.LockResultEnum.DoesNotExist) {
            this.services.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[state.lock.result]);
        } else {
            this.services.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[state.lock.result]);
        }
    }
//     private onChanged(artifactState: IArtifactState) {
//         this.subject.onNext(this);
//     }

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


    private changes(): Models.IArtifact {
            // if (this._hasValidationErrors) {
            //     throw new Error("App_Save_Artifact_Error_400_114");
            // }

            let delta: Models.IArtifact = {} as Models.Artifact;

            delta.id = this.id;
            delta.projectId = this.projectId;
            delta.customPropertyValues = [];
            this.changesets.get().forEach((it: IChangeSet) => {
                delta[it.key as string] = it.value;
            });

            delta.customPropertyValues = this.customProperties.changes();
            delta.specificPropertyValues = this.specialProperties.changes();
            
            return delta;
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
                deffered.reject(it);
                    let message: string;
                    if (error) {
                        message = "App_Save_Artifact_Error_" + error.statusCode + "_" + error.errorCode;
                    }
                    throw new Error(message);
            });
        return deffered.promise;
    }

    public publish(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();
        return deffered.promise;
    }

    public refresh(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();
        return deffered.promise;
    }
}
