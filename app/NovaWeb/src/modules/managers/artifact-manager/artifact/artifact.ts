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

export interface IStatefulArtifact extends IStatefulItem, IDispose {
    /**
     * Unload full weight artifact
     */
    unload();
    subArtifactCollection: ISubArtifactCollection;
    //load(force?: boolean): ng.IPromise<IStatefulArtifact>;
    save(): ng.IPromise<IStatefulArtifact>;
    autosave(): ng.IPromise<IStatefulArtifact>;
    publish(): ng.IPromise<IStatefulArtifact>;
    refresh(): ng.IPromise<IStatefulArtifact>;

    getObservable(): Rx.Observable<IStatefulArtifact>;
    canBeSaved(): boolean;
}

// TODO: explore the possibility of using an internal interface for services
export interface IIStatefulArtifact extends IIStatefulItem {
}

export class StatefulArtifact extends StatefulItem implements IStatefulArtifact, IIStatefulArtifact {
    private state: IArtifactState;

    protected subject: Rx.BehaviorSubject<IStatefulArtifact>;

    constructor(artifact: Models.IArtifact, protected services: IStatefulArtifactServices) {
        super(artifact, services);
        this.metadata = new MetaData(this);
        this.subject = new Rx.BehaviorSubject<IStatefulArtifact>(null);

        this.state = new ArtifactState(this);
    }

    public dispose() {
        super.dispose();
        this.subject.dispose();
        delete this.subject;
        if (this.state) {
            this.state.dispose();
        }
    }

    public initialize(artifact: Models.IArtifact): IState {
        if (this.parentId && this.orderIndex &&
            (this.parentId !== artifact.parentId || this.orderIndex !== artifact.orderIndex)) {
            this.artifactState.misplaced = true;
        } else {
            this.artifactState.initialize(artifact);
            super.initialize(artifact);
        }
        if (this.historical) {
            this.artifactState.readonly = true;
        }
        return this.artifactState.get();
    }

    public get artifactState(): IArtifactState {
        return this.state;
    }

    public getObservable(): Rx.Observable<IStatefulArtifact> {
        if (!this.isFullArtifactLoadedOrLoading()) {
            this.loadPromise = this.load();
            const customPromises = this.getCustomArtifactPromisesForGetObservable();

            const promisesToExecute = [this.loadPromise].concat(customPromises);

            this.getServices().$q.all(promisesToExecute).then(() => {
                this.subject.onNext(this);
            }).catch((error) => {
                this.artifactState.readonly = true;
                this.subject.onError(error);
            }).finally(() => {
                this.loadPromise = null;
                this.runPostGetObservable();
            });
        }

        return this.subject.filter(it => !!it).asObservable();
    }

    //Hook for subclasses to provide additional promises which should be run for obtaining data
    protected getCustomArtifactPromisesForGetObservable(): angular.IPromise<IStatefulArtifact>[] {
        return [];
    }
    protected getCustomArtifactPromisesForRefresh(): ng.IPromise<any>[] {
         return [];
    }

    //Hook for subclasses to do some post processing
    protected runPostGetObservable() {
//fixme: if empty function should be removed or return undefined
    }

    public discard() {
        super.discard();
        this.artifactState.dirty = false;
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
    private isNeedToLoad() {
        if (this.isProject()) {
            return false;
        } else if (this.artifactState.dirty && this.artifactState.lockedBy === Enums.LockedByEnum.CurrentUser) {
            return false;
        } else if (this.artifactState.misplaced) {
            return false;
        } else if (this.artifactState.deleted) {
            return false;
        }
        return true;
    }

    protected load(): ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        if (this.isNeedToLoad()) {
            this.services.artifactService.getArtifact(this.id, this.getEffectiveVersion()).then((artifact: Models.IArtifact) => {
                this.initialize(artifact);
                if (this.artifactState.misplaced) {
                    deferred.reject(this);
                } else {
                    deferred.resolve(this);
                }
            }).catch((err) => {
                if (err && err.statusCode === HttpStatusCode.NotFound) {
                    this.artifactState.deleted = true;
                }

                deferred.reject(err);
            });
        } else {
            deferred.resolve(this);
        }

        return deferred.promise;
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

    private processLock(lock: Models.ILockResult) {
        if (lock.result === Enums.LockResultEnum.Success) {
            this.artifactState.lock(lock);
            if (lock.info.versionId !== this.version) {
                this.refresh();
                this.services.messageService.addInfo("Artifact_Lock_Refresh");
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
                    this.services.messageService.addInfo("Artifact_Lock_Refresh");
                }
            } else {
                this.discard();
                if (lock.result === Enums.LockResultEnum.DoesNotExist) {
                    this.artifactState.deleted = true;
                } else {
                    this.artifactState.readonly = true;
                }
                this.subject.onNext(this);
            }
        }
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

    protected getAttachmentsDocRefsInternal(): ng.IPromise<IArtifactAttachmentsResultSet> {
        return this.services.attachmentService.getArtifactAttachments(this.id, undefined, this.getEffectiveVersion());
    }

    protected getRelationshipsInternal() {
        return this.services.relationshipsService.getRelationships(this.id, undefined, this.getEffectiveVersion());
    }

    public changes(): Models.IArtifact {
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
        delta.traces = this.relationships.changes();
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
        let deferred = this.services.getDeferred<IStatefulArtifact>();

        let changes = this.changes();
        this.services.artifactService.updateArtifact(changes)
            .then((artifact: Models.IArtifact) => {
                this.discard();
                this.refresh().then((a) => {
                    deferred.resolve(a);
                }).catch((error) => {
                    deferred.reject(error);
                });
                this.services.messageService.addInfo("App_Save_Artifact_Error_200");
            }).catch((error) => {
                deferred.reject(error);
                let message: string;
                // if error is undefined it means that it handled on upper level (http-error-interceptor.ts)
                if (error) {
                    if (error.statusCode === 400) {
                        if (error.errorCode === 114) {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_400_114");
                        } else {
                            message = this.services.localizationService.get("App_Save_Artifact_Error_400") + error.message;
                        }
                    } else if (error.statusCode === HttpStatusCode.NotFound) {
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

        return deferred.promise;
    }

    //TODO: stub - replace with implementation
    public autosave(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();
        deffered.resolve();
        return deffered.promise;
    }

    public publish(): ng.IPromise<IStatefulArtifact> {
        let deffered = this.services.getDeferred<IStatefulArtifact>();

        let savePromise = this.services.$q.defer<any>();
        if (this.canBeSaved()) {
            savePromise.promise = this.save();
        } else {
            savePromise.resolve();
        }

        savePromise.promise.then(() => {
            //dependentIds.unshift(this.id);
            this.internalPublish([])
            .then(() => {
                this.services.messageService.addInfo("Published artifact succesfully");
                //this.subject.onNext(this);
                this.artifactState.unlock();
                deffered.resolve();
            })
            .catch((err) => {
                if (err && err.statusCode === 409) {
                    let data: Models.IPublishResultSet = err.data;
                    this.services.dialogService.open(<IDialogSettings>{
                        okButton: this.services.localizationService.get("App_Button_Publish"),
                        cancelButton: this.services.localizationService.get("App_Button_Cancel"),
                        message: this.services.localizationService.get("Publish_All_Dialog_Message"),
                        template: require("../../../main/components/dialogs/bp-confirm-publish/bp-confirm-publish.html"),
                        controller: ConfirmPublishController,
                        css: "nova-messaging" // removed modal-resize-both as resizing the modal causes too many artifacts with ag-grid
                    },
                    <IConfirmPublishDialogData>{
                        artifactList: data.artifacts,
                        projectList: data.projects,
                        selectedProject: this.projectId
                    })
                    .then(() => {
                        this.internalPublish(data.artifacts.map((d: Models.IArtifact) => {return d.id; }))
                        .then(() => {
                            this.services.messageService.addInfo("Published artifact succesfully");
                            this.artifactState.unlock();
                            deffered.resolve();
                        })
                        .catch((err) => {
                            this.services.messageService.addError(err);
                            deffered.reject();
                        });
                    });
                }
                this.services.messageService.addError(err);
                deffered.reject();
            });
        })
        .catch(() => {
            deffered.reject();
        });

        return deffered.promise;
    }

    private internalPublish(artifactIds: number[]): ng.IPromise<any> {
        artifactIds.unshift(this.id);
        return this.services.publishService.publishArtifacts(artifactIds);
    }

    public refresh(): ng.IPromise<IStatefulArtifact> {
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

        // get promises for custom artifact refresh operations
        promisesToExecute.push.apply(promisesToExecute,
            this.getCustomArtifactPromisesForRefresh());

        this.getServices().$q.all(promisesToExecute).then(() => {
            this.subject.onNext(this);
            deferred.resolve(this);
        }).catch(error => {
            deferred.reject(error);

            //Project manager is listening to this, and will refresh the project.
            this.subject.onNext(this);
        });


        return deferred.promise;
    }


}
