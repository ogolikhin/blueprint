import { Models, Enums } from "../../../main/models";
import { ArtifactState} from "../state";
import { ArtifactAttachments, IArtifactAttachments } from "../attachments";
import { IDocumentRefs, DocumentRefs, ChangeTypeEnum, IChangeCollector, IChangeSet } from "../";
import { CustomProperties } from "../properties";
import { ChangeSetCollector } from "../changeset";
import { StatefulSubArtifactCollection, ISubArtifactCollection } from "../sub-artifact";
import { IMetaData, MetaData } from "../metadata";
import {
    IStatefulArtifact,
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

    //TODO autkin temp
    public get specificPropertyValues() {
        return this.artifact.specificPropertyValues;
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

    private isLoaded = false;
    public load(timeout?: ng.IPromise<any>):  ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        if (!this.isLoaded) {
            this.services.artifactService.getArtifact(this.id).then((artifact: Models.IArtifact) => {
                this.artifact = artifact;
                this.customProperties.initialize(artifact);
                this.artifactState.initialize(artifact);
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

    //TODO: moved from bp-artifactinfo 
    // public saveChanges() {
    public save() {
        throw new Error("Not implemented yet");
    //     let overlayId: number = this.loadingOverlayService.beginLoading();
    //     try {
    //         let state: ItemState = this.stateManager.getState(this._artifactId);
    //         let artifactDelta: Models.IArtifact = state.generateArtifactDelta();
    //         this.artifactService.updateArtifact(artifactDelta)
    //             .then((artifact: Models.IArtifact) => {
    //                     let oldArtifact = state.getArtifact();
    //                     if (artifact.version) {
    //                         state.updateArtifactVersion(artifact.version);
    //                     }
    //                     if (artifact.lastSavedOn) {
    //                         state.updateArtifactSavedTime(artifact.lastSavedOn);
    //                     }
    //                     this.messageService.addMessage(new Message(MessageType.Info, this.localization.get("App_Save_Artifact_Error_200")));
    //                     state.finishSave();
    //                     this.isChanged = false;
    //                     this.projectManager.updateArtifactName(state.getArtifact());
    //                 }, (error) => {
    //                     let message: string;
    //                     if (error) {
    //                         if (error.statusCode === 400) {
    //                             if (error.errorCode === 114) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_400_114");
    //                             } else {
    //                                 message = this.localization.get("App_Save_Artifact_Error_400") + error.message;
    //                             }
    //                         } else if (error.statusCode === 404) {
    //                             message = this.localization.get("App_Save_Artifact_Error_404");
    //                         } else if (error.statusCode === 409) {
    //                             if (error.errorCode === 116) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_116");
    //                             } else if (error.errorCode === 117) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_117");
    //                             } else if (error.errorCode === 111) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_111");
    //                             } else if (error.errorCode === 115) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_115");
    //                             } else {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409");
    //                             }

    //                         } else {
    //                             message = this.localization.get("App_Save_Artifact_Error_Other") + error.statusCode;
    //                         }
    //                     }
    //                     this.messageService.addError(message);
    //                 }
    //             ).finally(() => this.loadingOverlayService.endLoading(overlayId));
    //     } catch (Error) {
    //         this.messageService.addError(this.localization.get(Error));
    //         this.loadingOverlayService.endLoading(overlayId);
    //     }
    }

    public publish() {
        throw new Error("Not implemented yet");
    }

    public refresh() {
        throw new Error("Not implemented yet");
    }
}
