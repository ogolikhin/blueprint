import {Models} from "../../../main/models";
import {IStatefulArtifactServices} from "../services";
import {IStatefulArtifact} from "../artifact";
import {StatefulItem, IStatefulItem, IIStatefulItem} from "../item";
import {IArtifactAttachmentsResultSet} from "../attachments";
import {MetaData} from "../metadata";
import {HttpStatusCode} from "../../../core/http";
import {IChangeSet} from "../changeset";

export interface IIStatefulSubArtifact extends IIStatefulItem {
}

export interface IStatefulSubArtifact extends IStatefulItem, Models.ISubArtifact {
    getObservable(): Rx.Observable<IStatefulSubArtifact>;
}

export class StatefulSubArtifact extends StatefulItem implements IStatefulSubArtifact, IIStatefulSubArtifact {
    public isLoaded = false;
    private _subject: Rx.BehaviorSubject<IStatefulSubArtifact>;

    constructor(private parentArtifact: IStatefulArtifact, private subArtifact: Models.ISubArtifact, services: IStatefulArtifactServices) {
        super(subArtifact, services);
        this.metadata = new MetaData(this);
    }

    public unsubscribe() {
        super.unsubscribe();
        this.subject.onCompleted();
        delete this._subject;
    }

    protected get subject(): Rx.BehaviorSubject<IStatefulSubArtifact> {
        if (!this._subject) {
            this._subject = new Rx.BehaviorSubject<IStatefulSubArtifact>(null);
        }
        return this._subject;
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
                this.error.onNext(error);
            }).finally(() => {
                this.loadPromise = null;
            });
        }
        return this.subject.filter(it => !!it).asObservable();
    }

    public changes(): Models.ISubArtifact {
        const traces = this.relationships.changes();
        const attachmentValues = this.attachments.changes();
        const docRefValues = this.docRefs.changes();
        const customPropertyChangedValues = this.customProperties.changes();
        const specificPropertyChangedValues = this.specialProperties.changes();

        let hasChanges = false;

        const delta = <Models.ISubArtifact>{};
        delta.id = this.id;

        this.changesets.get().forEach((it: IChangeSet) => {
                hasChanges = true;
                delta[it.key as string] = it.value;
        });

        if (traces || 
        attachmentValues || 
        docRefValues || 
        (customPropertyChangedValues && customPropertyChangedValues.length > 0) || 
        (specificPropertyChangedValues && specificPropertyChangedValues.length > 0)) {

            delta.customPropertyValues = customPropertyChangedValues;
            delta.specificPropertyValues = specificPropertyChangedValues;

            delta.traces = traces;
            delta.attachmentValues = attachmentValues;
            delta.docRefValues = docRefValues;

            return delta;
        }

        if (hasChanges) {
            return delta;
        }

        return undefined;
    }

    public discard() {
        super.discard();
        this.artifactState.dirty = false;
        
        this.customProperties.discard();
        this.specialProperties.discard();

        this.attachments.discard();
        this.docRefs.discard();
    }

    public lock(): ng.IPromise<IStatefulArtifact> {
        return this.parentArtifact.lock();
    }

    public getEffectiveVersion(): number {
        return this.artifactState.historical ? this.parentArtifact.version : undefined;
    }

    protected getAttachmentsDocRefsInternal(): ng.IPromise<IArtifactAttachmentsResultSet> {
        return this.services.attachmentService.getArtifactAttachments(this.parentArtifact.id, this.id, this.getEffectiveVersion());
    }

    protected getRelationshipsInternal() {
        return this.services.relationshipsService.getRelationships(this.parentArtifact.id, this.id, this.getEffectiveVersion());
    }

}
