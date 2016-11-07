import {ArtifactState, IArtifactState, IState} from "../state";
import {Models, Enums, Relationships} from "../../../main/models";
import {IStatefulArtifactServices} from "../services";
import {StatefulItem, IStatefulItem, IIStatefulItem} from "../item";
import {IArtifactAttachmentsResultSet} from "../attachments";
import {IChangeSet} from "../changeset";
import {ISubArtifactCollection} from "../sub-artifact";
import {MetaData} from "../metadata";
import {IDispose} from "../../models";
import {HttpStatusCode} from "../../../core/http";
import {ConfirmPublishController, IConfirmPublishDialogData} from "../../../main/components/dialogs/bp-confirm-publish";
import {IDialogSettings} from "../../../shared";
import {DialogTypeEnum} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {IApplicationError, ApplicationError} from "../../../core";

export interface IStatefulArtifact extends IStatefulItem, IDispose {
    /**
     * Unload full weight artifact
     */
    unload();
    subArtifactCollection: ISubArtifactCollection;
    //load(force?: boolean): ng.IPromise<IStatefulArtifact>;
    save(): ng.IPromise<IStatefulArtifact>;
    autosave(): ng.IPromise<IStatefulArtifact>;
    publish(): ng.IPromise<void>;
    discardArtifact(): ng.IPromise<void>;
    refresh(allowCustomRefresh?: boolean): ng.IPromise<IStatefulArtifact>;

    getObservable(): Rx.Observable<IStatefulArtifact>;
    canBeSaved(): boolean;
    canBePublished(): boolean;
}

// TODO: explore the possibility of using an internal interface for services
export interface IIStatefulArtifact extends IIStatefulItem {
}

export class StatefulArtifact extends StatefulItem implements IStatefulArtifact, IIStatefulArtifact {
    private state: IArtifactState;

    protected _subject: Rx.BehaviorSubject<IStatefulArtifact>;

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

    protected initialize(artifact: Models.IArtifact): IState {
        if (this.parentId && this.orderIndex &&
            (this.parentId !== artifact.parentId || this.orderIndex !== artifact.orderIndex)) {
            this.artifactState.misplaced = true;
        } else {
            this.artifactState.initialize(artifact);
            super.initialize(artifact);
        }
        return this.artifactState.get();
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
        this.artifactState.dirty = false;
    }
    
    public discardArtifact(): ng.IPromise<void> {
        let deffered = this.services.getDeferred<void>();
        this.services.messageService.clearMessages();
        this.services.dialogService.open(<IDialogSettings>{
            okButton: this.services.localizationService.get("App_Button_Discard"),
            cancelButton: this.services.localizationService.get("App_Button_Cancel"),
            message: this.services.localizationService.get("Discard_Single_Dialog_Message"),
            type: DialogTypeEnum.Alert,
            header: this.services.localizationService.get("App_DialogTitle_Alert"),
            css: "modal-alert nova-messaging"
        })
        .then(() => {
            let overlayId: number = this.services.loadingOverlayService.beginLoading();
            this.services.publishService.discardArtifacts([this.id])
            .then(() => {
                this.services.messageService.addInfo("Discard_Success_Message");
                this.refresh();
                deffered.resolve();
            })
            .catch((err) => {
                if (err && err.statusCode === HttpStatusCode.Conflict) {
                    this.discardDependents(err.errorContent);
                } else {
                    if (err && err.errorCode === 114) {
                        this.services.messageService.addInfo("Artifact_Lock_Refresh");
                        this.refresh();
                    } else {
                        this.services.messageService.addError(err);
                    }
                }
                deffered.reject();
            }).finally(() => {
                this.services.loadingOverlayService.endLoading(overlayId);
            });
        }).catch(() => {
            deffered.reject();
        });

        return deffered.promise;
    }

    private discardDependents(dependents: Models.IPublishResultSet) {
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
            this.services.publishService.discardArtifacts(dependents.artifacts.map((d: Models.IArtifact) => d.id ))
            .then(() => {
                this.services.messageService.addInfoWithPar("Discard__All_Success_Message", [dependents.artifacts.length]);
                this.refresh();
            })
            .catch((err) => {
                if (err && err.errorCode === 114) {
                    this.services.messageService.addInfo("Artifact_Lock_Refresh");
                    this.refresh();
                } else {
                    this.services.messageService.addError(err);
                }
            }).finally(() => {
                this.services.loadingOverlayService.endLoading(discardOverlayId);
            });
        });
    }
    
    public canBeSaved(): boolean {
        if (this.isProject()) {
            return false;
        } else if (this.artifactState.dirty && this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
            return true;
        } else {
            return false;
        }
    }

    public canBePublished(): boolean {
        if (this.isProject()) {
            return false;
        } else if (this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser || this.version < 1) {
            return true;
        } else {
            return false;
        }
    }

    private canBeLoaded() {
        if (this.isProject()) {
            return false;
        } else if (this.artifactState.dirty && this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
            return false;
        } else if (this.artifactState.misplaced) {
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

    private isProject(): boolean {
        return this.itemTypeId === Enums.ItemTypePredefined.Project;
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
                this.subject.onNext(this);
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

    protected getAttachmentsDocRefsInternal(): ng.IPromise<IArtifactAttachmentsResultSet> {
        return this.services.attachmentService.getArtifactAttachments(this.id, undefined, this.getEffectiveVersion());
    }

    protected getRelationshipsInternal() {
        return this.services.relationshipsService.getRelationships(this.id, undefined, this.getEffectiveVersion());
    }

    public changes(): Models.IArtifact {
        if (this.artifactState.invalid) {
            return null;
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
        delta.traces = this.relationships.changes();
        this.addSubArtifactChanges(delta);

        return delta;
    }

    private addSubArtifactChanges(delta: Models.IArtifact) {
        const subArtifacts = this.subArtifactCollection.list();
        delta.subArtifacts = new Array<Models.ISubArtifact>();
        subArtifacts.forEach(subArtifact => {
            const changes = subArtifact.changes();
            if (changes) {
                delta.subArtifacts.push(changes);
            }
        });
    }

    public save(): ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        this.services.messageService.clearMessages();
        const saveCustomArtifact = this.getCustomArtifactPromisesForSave();
        if (saveCustomArtifact) {
            saveCustomArtifact.then(() => {
                this.saveArtifact().then(() => {
                    deferred.resolve(this);
                })
                .catch((error) => {
                    this.customHandleSaveFailed();
                    deferred.reject(error);
                });
            })
            .catch((error) => {
                // if error is undefined it means that it handled on upper level (http-error-interceptor.ts)
                if (error) {
                    deferred.reject(this.handleSaveError(error));
                } else {
                    deferred.reject(error);
                }
            });
        } else {
            this.saveArtifact()
                .then(() => {
                    deferred.resolve(this);
                })
                .catch((error) => {
                    deferred.reject(error);
                });
        }

        return deferred.promise;
    }

    private saveArtifact(): ng.IPromise<IStatefulArtifact> {
        let deferred = this.services.getDeferred<IStatefulArtifact>();

        let changes = this.changes();
        if (!changes) {
            const compoundId: string = this.prefix + this.id.toString();
            let message: string = this.services.localizationService.get("App_Save_Artifact_Error_400_114");
            deferred.reject(new Error(message.replace("{0}", compoundId)));
        } else {
            this.services.artifactService.updateArtifact(changes)
                .then((artifact: Models.IArtifact) => {
                    this.discard();
                    this.refresh().then((a) => {
                        deferred.resolve(a);
                    }).catch((error) => {
                        deferred.reject(error);
                    });
                }).catch((error) => {
                    // if error is undefined it means that it handled on upper level (http-error-interceptor.ts)
                    if (error) {
                        deferred.reject(this.handleSaveError(error));
                    } else {
                        deferred.reject(error);
                    }
                }
            );
        }
        return deferred.promise;
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

    //TODO: stub - replace with implementation
    public autosave(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();
        deffered.resolve();
        return deffered.promise;
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
            css: "nova-publish"
        },
        <IConfirmPublishDialogData>{
            artifactList: dependents.artifacts,
            projectList: dependents.projects,
            selectedProject: this.projectId
        })
        .then(() => {
            let publishOverlayId = this.services.loadingOverlayService.beginLoading();
            this.services.publishService.publishArtifacts(dependents.artifacts.map((d: Models.IArtifact) => d.id ))
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

        promisesToExecute.push(this.services.metaDataService.remove(this.projectId));

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

    //Hook for subclasses to provide additional promises which should be run for obtaining data
    protected getCustomArtifactPromisesForGetObservable(): ng.IPromise<IStatefulArtifact>[] {
        return [];
    }
    protected getCustomArtifactPromisesForRefresh(): ng.IPromise<any>[] {
        return [];
    }
    protected getCustomArtifactPromisesForSave(): ng.IPromise <IStatefulArtifact> {
        return null;
    }
    protected customHandleSaveFailed(): void {
        ;
    }

    //Hook for subclasses to do some post processing
    protected runPostGetObservable() {
        ;
    }
}
