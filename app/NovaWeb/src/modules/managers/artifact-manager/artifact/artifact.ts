import {ArtifactState, IArtifactState} from "../state";
import {Models, Enums} from "../../../main/models";
import {IStatefulArtifactServices} from "../services";
import {StatefulItem, IStatefulItem, IIStatefulItem} from "../item";
import {IArtifactAttachmentsResultSet} from "../attachments";
import {IChangeSet} from "../changeset";
import {ISubArtifactCollection, StatefulSubArtifactCollection} from "../sub-artifact";
import {MetaData} from "../metadata";
import {IDispose} from "../../models";
import {ConfirmPublishController, IConfirmPublishDialogData} from "../../../main/components/dialogs/bp-confirm-publish";
import {IDialogSettings} from "../../../shared";
import {DialogTypeEnum} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {IApplicationError, ApplicationError} from "../../../core/error/applicationError";
import {HttpStatusCode} from "../../../core/http/http-status-code";

export interface IStatefulArtifact extends IStatefulItem, IDispose {
    subArtifactCollection: ISubArtifactCollection;

    // Unload full weight artifact
    unload();
    save(ignoreInvalidValues?: boolean ): ng.IPromise<IStatefulArtifact>;
    delete(): ng.IPromise<Models.IArtifact[]>;
    autosave(): ng.IPromise<void>;
    publish(): ng.IPromise<void>;
    discardArtifact(): ng.IPromise<void>;
    refresh(allowCustomRefresh?: boolean): ng.IPromise<IStatefulArtifact>;
    getObservable(): Rx.Observable<IStatefulArtifact>;
    move(newParentId: number, orderIndex?: number): ng.IPromise<void>;
    canBeSaved(): boolean;
    canBePublished(): boolean;
    validate(): ng.IPromise<void>;
}

// TODO: explore the possibility of using an internal interface for services
export interface IIStatefulArtifact extends IIStatefulItem {
}
export class StatefulArtifact extends StatefulItem implements IStatefulArtifact, IIStatefulArtifact {
    private state: IArtifactState;

    protected _subject: Rx.BehaviorSubject<IStatefulArtifact>;
    protected _subArtifactCollection: ISubArtifactCollection;
    protected hasCustomSave: boolean = false;

    constructor(artifact: Models.IArtifact, protected services: IStatefulArtifactServices) {
        super(artifact, services);
        this.metadata = new MetaData(this);
        this.state = new ArtifactState(this);
    }

    public dispose() {
        super.dispose();
        if (this.state) {
            this.state.dispose();
        }
    }

    public unsubscribe() {
        super.unsubscribe();
        this.subject.onCompleted();
        delete this._subject;
    }

    protected get subject(): Rx.BehaviorSubject<IStatefulArtifact> {
        if (!this._subject) {
            this._subject = new Rx.BehaviorSubject<IStatefulArtifact>(null);
        }
        return this._subject;
    }

    protected initialize(artifact: Models.IArtifact): void {
        let isMisplaced: boolean;
        if (this.parentId && this.orderIndex &&
            (this.parentId !== artifact.parentId || this.orderIndex !== artifact.orderIndex)) {
            isMisplaced = true;
        }

        this.artifactState.initialize(artifact);
        super.initialize(artifact);

        if (isMisplaced && !this.artifactState.historical) {
            this.artifactState.misplaced = true;
        }
    }

    public get artifactState(): IArtifactState {
        return this.state;
    }

    public getObservable(): Rx.Observable<IStatefulArtifact> {
        if (!this.isFullArtifactLoadedOrLoading() && !this.isHeadVersionDeleted()) {
            this.loadPromise = this.load();
            const customPromises = this.getCustomArtifactPromisesForGetObservable();

            const promisesToExecute = [this.loadPromise].concat(customPromises);

            this.getServices().$q.all(promisesToExecute).then(() => {
                this.subject.onNext(this);
                this.propertyChange.onNext({item: this});
            }).catch((error) => {
                this.artifactState.readonly = true;
                this.error.onNext(error);
            }).finally(() => {
                this.loadPromise = null;
                this.runPostGetObservable();
            });
        }

        return this.subject.filter(it => !!it).asObservable();
    }

    protected notifySubscribers() {
        this.subject.onNext(this);
    }

    public discard() {
        super.discard();

        if (this._subArtifactCollection) {
            this._subArtifactCollection.discard();
        }

        this.artifactState.dirty = false;
    }

    public discardArtifact(): ng.IPromise<void> {
        let deffered = this.services.getDeferred<void>();
        this.services.messageService.clearMessages();
        this.services.dialogService.open(<IDialogSettings>{
            okButton: this.services.localizationService.get("App_Button_Discard"),
            cancelButton: this.services.localizationService.get("App_Button_Cancel"),
            message: this.services.localizationService.get("Discard_Single_Dialog_Message"),
            header: this.services.localizationService.get("App_DialogTitle_Alert"),
            css: "modal-alert nova-messaging"
        })
        .then(() => {
            let overlayId: number = this.services.loadingOverlayService.beginLoading();
            this.services.publishService.discardArtifacts([this.id])
            .then(() => {
                this.discard();
                this.services.messageService.addInfo("Discard_Success_Message");
                deffered.resolve();
            })
            .catch((err) => {
                if (err && err.statusCode === HttpStatusCode.Conflict) {
                    deffered.promise = this.discardDependents(err.errorContent);
                } else {
                    if (err && err.errorCode === 114) {
                        this.services.messageService.addInfo("Artifact_Lock_Refresh");
                        deffered.resolve();
                    } else {
                        this.services.messageService.addError(err);
                        deffered.reject();
                    }

                }
            }).finally(() => {
                this.services.loadingOverlayService.endLoading(overlayId);
            });
        }).catch(() => {
            deffered.reject();
        });

        return deffered.promise;
    }

    private discardDependents(dependents: Models.IPublishResultSet) {
        let deffered = this.services.getDeferred<void>();
        this.services.dialogService.open(<IDialogSettings>{
            okButton: this.services.localizationService.get("App_Button_Discard"),
            cancelButton: this.services.localizationService.get("App_Button_Cancel"),
            message: this.services.localizationService.get("Discard_Dependents_Dialog_Message"),
            template: require("../../../main/components/dialogs/bp-confirm-publish/bp-confirm-publish.html"),
            controller: ConfirmPublishController,
            css: "nova-publish modal-alert",
            header: this.services.localizationService.get("App_DialogTitle_Alert")
        },
        <IConfirmPublishDialogData>{
            artifactList: dependents.artifacts,
            projectList: dependents.projects,
            selectedProject: this.projectId
        })
        .then(() => {
            let discardOverlayId = this.services.loadingOverlayService.beginLoading();
            this.services.publishService.discardArtifacts(dependents.artifacts.map((d: Models.IArtifact) => d.id))
            .then(() => {
                this.services.messageService.addInfo("Discard_All_Success_Message", dependents.artifacts.length);
                deffered.resolve();
            })
            .catch((err) => {
                if (err && err.errorCode === 114) {
                    this.services.messageService.addInfo("Artifact_Lock_Refresh");
                    deffered.resolve();
                } else {
                    this.services.messageService.addError(err);
                    deffered.reject();
                }

            }).finally(() => {
                this.services.loadingOverlayService.endLoading(discardOverlayId);
            });
        }).catch(() => {
            deffered.reject();
        });
        return deffered.promise;
    }

    public canBeSaved(): boolean {
        if (this.artifactState.dirty && this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
            return true;
        } else {
            return false;
        }
    }

    public canBePublished(): boolean {
        if (this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser || this.version < 1) {
            return true;
        } else {
            return false;
        }
    }

    private canBeLoaded() {
        if (this.artifactState.dirty && this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
            return false;
        }
        return true;
    }

    protected load(): ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        // When we use head version of artifact and we know that artifact has been deleted
        // simulate NotFound error
        if (this.isHeadVersionDeleted()) {
            const error = this.artifactNotFoundError();
            this.error.onNext(error);
            deferred.reject(error);
            return deferred.promise;
        }
        if (this.canBeLoaded()) {
            this.getArtifactModel(this.id, this.getEffectiveVersion()).then((artifact: Models.IArtifact) => {
                this.initialize(artifact);
                deferred.resolve(this);
            }).catch((error: IApplicationError) => {
                if (error && error.statusCode === HttpStatusCode.NotFound) {
                    this.artifactState.deleted = true;
                }
                this.error.onNext(error);
                deferred.reject(error);
            });
        } else {
            deferred.resolve(this);
        }

        return deferred.promise;
    }

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<Models.IArtifact> {
        return this.services.artifactService.getArtifact(id, versionId);
    }

    private artifactNotFoundError() {
        const error = new ApplicationError();
        error.statusCode = HttpStatusCode.NotFound;
        return error;
    }
    public unload() {
        super.unload();
        // sets initial value on subject so it doesn't send up update with old info
        // null values get filtered out before it gets to the observer
        this.subject.onNext(null);
    }

    public lock(): ng.IPromise<IStatefulArtifact> {
        if (this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
            return;
        }
        if (!this.lockPromise) {
            const deferred = this.services.getDeferred<IStatefulArtifact>();
            this.lockPromise = deferred.promise;

            const loadingId = this.services.loadingOverlayService.beginLoading();
            this.services.artifactService.lock(this.id).then((result: Models.ILockResult[]) => {
                const lock = result[0];
                this.processLock(lock);
                deferred.resolve(this);
            }).catch((err) => {
                deferred.reject(err);
            }).finally(() => {
                this.lockPromise = null;
                this.services.loadingOverlayService.endLoading(loadingId);
            });
        }

        return this.lockPromise;
    }

    private processLock(lock: Models.ILockResult) {
        if (lock.result === Enums.LockResultEnum.Success) {
            this.artifactState.lock(lock);
            if (lock.info.versionId !== this.version) {
                this.refresh();
                this.services.messageService.addInfo("Artifact_Lock_Refresh", 6000);
            } else {
                if (lock.info.parentId !== this.parentId || lock.info.orderIndex !== this.orderIndex) {
                    this.artifactState.misplaced = true;
                }
            }
        } else {
            if (lock.result === Enums.LockResultEnum.AlreadyLocked) {
                this.refresh();
                if (lock.info.versionId !== this.version) {
                    //Show the refresh message only if the version has changed.
                    this.services.messageService.addInfo("Artifact_Lock_Refresh", 6000);
                }
            } else {
                this.discard();
                if (lock.result === Enums.LockResultEnum.DoesNotExist) {
                    this.artifactState.deleted = true;
                    const error = this.artifactNotFoundError();
                    this.error.onNext(error);
                } else {
                    this.artifactState.readonly = true;
                }
                this.subject.onNext(this);
            }
        }
    }

    public get subArtifactCollection() {
        if (!this._subArtifactCollection) {
            this._subArtifactCollection = new StatefulSubArtifactCollection(this, this.services);
        }
        return this._subArtifactCollection;
    }

    protected getAttachmentsDocRefsInternal(): ng.IPromise<IArtifactAttachmentsResultSet> {
        return this.services.attachmentService.getArtifactAttachments(this.id, undefined, this.getEffectiveVersion());
    }

    protected getRelationshipsInternal() {
        return this.services.relationshipsService.getRelationships(this.id, undefined, this.getEffectiveVersion());
    }

    public changes(): Models.IArtifact {

        const delta = {} as Models.IArtifact;

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
        delta.traces = this.relationships.changes();
        const subArtifactChanges = this.getSubArtifactChanges();
        if (!!subArtifactChanges) {
            delta.subArtifacts = subArtifactChanges;
        } else {
            return null;
        }
        return delta;
    }

    //if any subartifact is invalid, do not return any changes. In future, we might send information about which artifacts are invalid to improve messaging
    private getSubArtifactChanges(): Models.ISubArtifact[] {
        const subArtifacts = this.subArtifactCollection.list();
        const subArtifactChanges = new Array<Models.ISubArtifact>();
        subArtifacts.forEach(subArtifact => {
            const changes = subArtifact.changes();
            if (changes) {
                subArtifactChanges.push(changes);
            } else {
                return null;
            }
        });
        return subArtifactChanges;
    }

    public save(ignoreInvalidValues: boolean = false): ng.IPromise<IStatefulArtifact> {
        this.services.messageService.clearMessages();
        const changes = this.changes();

        let validatePromise = this.services.$q.defer<any>();
        if (ignoreInvalidValues) {
            validatePromise.resolve();
        } else {
            validatePromise.promise = this.validate();
        }

        return validatePromise.promise.then(() => {
            return this.getCustomArtifactPromiseForSave();
        }).then(() => {
            return this.saveArtifact(changes).catch((error) => {
                if (this.hasCustomSave) {
                    this.customHandleSaveFailed();
                }
                return this.services.$q.reject(error);
            });
        }).catch((error) => {
            return this.services.$q.reject(error);
        });
    }

    private saveArtifact(changes: Models.IArtifact): ng.IPromise<IStatefulArtifact> {
        return this.services.artifactService.updateArtifact(changes).catch((error) => {
            // if error is undefined it means that it handled on upper level (http-error-interceptor.ts)
            if (error) {
                return this.services.$q.reject(this.handleSaveError(error));
            }
            return this.services.$q.reject(error);
        }).then((artifact: Models.IArtifact) => {
            this.discard();
            return this.refresh().catch((error) => {
                return this.services.$q.reject(error);
            });
        });
    }

    protected handleSaveError(error: any): Error {
        let message: string;

        if (error.statusCode === 400) {
            if (error.errorCode === 114) {
                message = this.services.localizationService.get("App_Save_Artifact_Error_400_114");
            } else {
                message = this.services.localizationService.get("App_Save_Artifact_Error_400") + error.message;
            }
        } else if (error.statusCode === HttpStatusCode.NotFound) {
            message = this.services.localizationService.get("App_Save_Artifact_Error_404");
        } else if (error.statusCode === HttpStatusCode.Conflict) {
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

        const compoundId: string = this.prefix + this.id.toString();
        message = message.replace("{0}", compoundId);
        return new Error(message);
    }

    public autosave(): ng.IPromise<void> {
        if (this.canBeSaved() ) {
            return this.save(true).catch(() => {
                return this.services.dialogService.confirm("Autosave has failed. Continue without saving?").then(() => {
                    this.discard();
                });
            });
        }
        return this.services.$q.resolve();
    }

    public publish(): ng.IPromise<void> {
        let deffered = this.services.getDeferred<void>();

        let savePromise = this.services.$q.defer<IStatefulArtifact>();
        if (this.canBeSaved()) {
            savePromise.promise = this.save();
        } else {
            savePromise.resolve();
        }

        savePromise.promise.then(() => {
            this.doPublish().then(() => {
                deffered.resolve();
            }).catch(() => {
                deffered.reject();
            });
        })
            .catch((err) => {
                deffered.reject(err);
            });

        return deffered.promise;
    }

    private doPublish(): ng.IPromise<void> {
        let deffered = this.services.getDeferred<void>();

        this.services.publishService.publishArtifacts([this.id])
            .then(() => {
                this.services.messageService.addInfo("Publish_Success_Message");
                this.artifactState.unlock();
                this.refresh();
                deffered.resolve();
            })
            .catch((err) => {
                if (err && err.statusCode === HttpStatusCode.Conflict && err.errorContent) {
                    this.publishDependents(err.errorContent);                
                } else if (err && err.statusCode === HttpStatusCode.Unavailable && !err.message) {
                    this.services.messageService.addError("Publish_Artifact_Failure_Message");
                } else {
                    this.services.messageService.addError(err);
                }
                deffered.reject();
            });

        return deffered.promise;
    }

    private publishDependents(dependents: Models.IPublishResultSet) {
        this.services.dialogService.open(<IDialogSettings>{
                okButton: this.services.localizationService.get("App_Button_Publish"),
                cancelButton: this.services.localizationService.get("App_Button_Cancel"),
                message: this.services.localizationService.get("Publish_Dependents_Dialog_Message"),
                template: require("../../../main/components/dialogs/bp-confirm-publish/bp-confirm-publish.html"),
                controller: ConfirmPublishController,
            css: "nova-publish",
            header: this.services.localizationService.get("App_DialogTitle_Confirmation")
            },
            <IConfirmPublishDialogData>{
                artifactList: dependents.artifacts,
                projectList: dependents.projects,
                selectedProject: this.projectId
            })
            .then(() => {
                let publishOverlayId = this.services.loadingOverlayService.beginLoading();
                this.services.publishService.publishArtifacts(dependents.artifacts.map((d: Models.IArtifact) => d.id))
                    .then(() => {
                        this.services.messageService.addInfo("Publish_Success_Message");
                        this.artifactState.unlock();
                        this.refresh();
                    })
                    .catch((err) => {
                        this.services.messageService.addError(err);
                    }).finally(() => {
                    this.services.loadingOverlayService.endLoading(publishOverlayId);
                });
            });
    }

    public refresh(allowCustomRefresh: boolean = true): ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        this.discard();

        const promisesToExecute: ng.IPromise<any>[] = [];

        promisesToExecute.push(this.load());

        if (this._attachments) {
            //this will also reload docRefs, so no need to call docRefs.refresh()
            promisesToExecute.push(this._attachments.refresh());
        }

        if (this._relationships) {
            promisesToExecute.push(this._relationships.refresh());
        }

        //History and Discussions are excluded from here.
        //They refresh independently, triggered by artifact's observable.

        //Disabled due to major performance concerns for large projects.
        //This is normally done to refresh custom property type changes (property types added/removed to artifacts)
        //Also see: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=3338&fullScreen=false
        //promisesToExecute.push(this.services.metaDataService.remove(this.projectId));

        if (allowCustomRefresh) {
            // get promises for custom artifact refresh operations
            // this operation merges two arrays
            Array.prototype.push.apply(promisesToExecute, this.getCustomArtifactPromisesForRefresh());
        }

        this.getServices().$q.all(promisesToExecute)
            .then(() => {
                this.subject.onNext(this);
                deferred.resolve(this);
            })
            .catch((error) => {
                deferred.reject(error);

                //Project manager is listening to this, and will refresh the project.
                this.subject.onNext(this);

                this.error.onNext(error);
            });

        return deferred.promise;
    }

    public delete(): ng.IPromise<Models.IArtifact[]> {
        let deferred = this.services.getDeferred<Models.IArtifact[]>();

        this.services.artifactService.deleteArtifact(this.id).then((it: Models.IArtifact[]) => {
            this.artifactState.deleted = true;
            deferred.resolve(it);

        }).catch((error: IApplicationError) => {
            if (error.statusCode === HttpStatusCode.Conflict && error.errorContent) {
                error.message =
                `The artifact ${error.errorContent.prefix || ""}${error.errorContent.id} is already locked by another user.`;
            }
            this.error.onNext(error);
            deferred.reject(error);
        });

        return deferred.promise;

    }

    public move(newParentId: number, orderIndex?: number): ng.IPromise<void> {
        let moveOverlayId = this.services.loadingOverlayService.beginLoading();

        return this.services.artifactService.moveArtifact(this.id, newParentId, orderIndex)
        .catch((error: IApplicationError) => {
            this.error.onNext(error);
            return this.services.$q.reject(error);
        }).finally(() => {
            this.services.loadingOverlayService.endLoading(moveOverlayId);
        });
    }

    //Hook for subclasses to provide additional promises which should be run for obtaining data
    protected getCustomArtifactPromisesForGetObservable(): ng.IPromise<IStatefulArtifact>[] {
        return [];
    }

    protected getCustomArtifactPromisesForRefresh(): ng.IPromise<any>[] {
        return [];
    }

    protected getCustomArtifactPromiseForSave(): ng.IPromise <IStatefulArtifact> {
        return this.services.$q.when(this);
    }

    protected customHandleSaveFailed(): void {
        ;
    }

    //Hook for subclasses to do some post processing
    protected runPostGetObservable() {
        ;
    }
                
    public validate(): ng.IPromise<void> {
        
        let message: string = `The artifact ${this.prefix + this.id.toString()} cannot be saved. Please ensure all values are correct.`;

        return this.services.propertyDescriptor.createArtifactPropertyDescriptors(this).then((propertyTypes) => {
            const isItemValid = this.validateItem(propertyTypes);
            if (isItemValid) {
                return this.subArtifactCollection.validate().catch(() => {
                    return this.services.$q.reject(new Error(message));
                });
            } else {                
                return this.services.$q.reject(new Error(message));
            }
        });
    }
    

}

