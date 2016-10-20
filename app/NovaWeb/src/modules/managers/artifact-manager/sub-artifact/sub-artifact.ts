import {Models, Relationships} from "../../../main/models";
import {IStatefulArtifactServices} from "../services";
import {IStatefulArtifact} from "../artifact";
import {StatefulItem, IStatefulItem, IIStatefulItem} from "../item";
import {IArtifactAttachmentsResultSet} from "../attachments";
import {MetaData} from "../metadata";
import {HttpStatusCode} from "../../../core/http";

export interface IIStatefulSubArtifact extends IIStatefulItem {
}

export interface IStatefulSubArtifact extends IStatefulItem, Models.ISubArtifact {
    getObservable(): Rx.Observable<IStatefulSubArtifact>;
}

export class StatefulSubArtifact extends StatefulItem implements IStatefulSubArtifact, IIStatefulSubArtifact {
    public isLoaded = false;
    private subject: Rx.BehaviorSubject<IStatefulSubArtifact>;

    public deleted: boolean;

    constructor(private parentArtifact: IStatefulArtifact, private subArtifact: Models.ISubArtifact, services: IStatefulArtifactServices) {
        super(subArtifact, services);
        this.metadata = new MetaData(this);
        this.subject = new Rx.BehaviorSubject<IStatefulSubArtifact>(null);
    }

    public get artifactState() {
        return this.parentArtifact.artifactState;
    }

    public get projectId(): number {
        return this.parentArtifact.projectId;
    }

    protected load(): ng.IPromise<IStatefulSubArtifact> {
        const deferred = this.services.getDeferred<IStatefulSubArtifact>();
        this.services.artifactService.getSubArtifact(this.parentArtifact.id, this.id, this.getEffectiveVersion()).then((artifact: Models.ISubArtifact) => {
            this.initialize(artifact);
            deferred.resolve(this);
        }).catch((err) => {
            deferred.reject(err);
        });
        return deferred.promise;
    }

    public getObservable(): Rx.Observable<IStatefulSubArtifact> {
        if (!this.isFullArtifactLoadedOrLoading()) {
            this.loadPromise = this.load();

            this.loadPromise.then(() => {
                this.subject.onNext(this);
            }).catch((error) => {
                this.artifactState.readonly = true;
                this.subject.onError(error);
            }).finally(() => {
                this.loadPromise = null;
            });
        }
        return this.subject.filter(it => !!it).asObservable();
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
        delta.traces = this.relationships.changes();
        delta.attachmentValues = this.attachments.changes();
        delta.docRefValues = this.docRefs.changes();
        return delta;
    }

    public discard() {
        super.discard();
        this.artifactState.dirty = false;

        this.attachments.discard();
        this.docRefs.discard();
    }

    public lock(): ng.IPromise<IStatefulArtifact> {
        return this.parentArtifact.lock();
    }

    public getEffectiveVersion(): number {
        return this.parentArtifact.deleted ? this.parentArtifact.version : undefined;
    }

    protected getAttachmentsDocRefsInternal(): ng.IPromise<IArtifactAttachmentsResultSet> {
        return this.services.attachmentService.getArtifactAttachments(this.parentArtifact.id, this.id, this.getEffectiveVersion());
    }

    protected getRelationshipsInternal() {
        return this.services.relationshipsService.getRelationships(this.parentArtifact.id, this.id, this.getEffectiveVersion());
    }

}
