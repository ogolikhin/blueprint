import { Models, Relationships } from "../../../main/models";
// import { ArtifactState} from "../state";
import { IStatefulArtifactServices } from "../services";
import { IStatefulArtifact } from "../artifact";
import { StatefulItem, IStatefulItem, IIStatefulItem } from "../item";
import { IArtifactAttachmentsResultSet } from "../attachments";
import { MetaData } from "../metadata";
import { HttpStatusCode } from "../../../core/http";

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
        // this.changesets = new ChangeSetCollector(this.artifact);
    }

    public get artifactState() {
        return this.parentArtifact.artifactState;
    }

    public get projectId(): number {
        return this.parentArtifact.projectId;
    }

    protected load():  ng.IPromise<IStatefulSubArtifact> {
        const deferred = this.services.getDeferred<IStatefulSubArtifact>();
            this.services.artifactService.getSubArtifact(this.parentArtifact.id, this.id).then((artifact: Models.ISubArtifact) => {
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
        } else {
//            this.subject.onNext(this);
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
        delta.attachmentValues = this.attachments.changes();
        delta.docRefValues = this.docRefs.changes();
        return delta;
    }
    public discard() {
        super.discard();
        this.artifactState.dirty = false;


        // this.changesets.reset().forEach((it: IChangeSet) => {
        //     this[it.key as string].value = it.value;
        // });

        this.attachments.discard();
        this.docRefs.discard();

        // deferred.resolve(this);
    }

    public lock(): ng.IPromise<IStatefulArtifact> {
        return this.parentArtifact.lock();
    }

    public getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet> {
        const deferred = this.services.getDeferred();
        this.services.attachmentService.getArtifactAttachments(this.parentArtifact.id, this.id, true)
            .then( (result: IArtifactAttachmentsResultSet) => {
                this.attachments.initialize(result.attachments);
                this.docRefs.initialize(result.documentReferences);
                
                deferred.resolve(result);
            }, (error) => {
                if (error && error.statusCode === HttpStatusCode.NotFound) {
                    this.deleted = true;
                }
                deferred.reject(error);
            });
        return deferred.promise;
    }

    public getRelationships(): ng.IPromise<Relationships.IArtifactRelationshipsResultSet> {
        const deferred = this.services.getDeferred();
        this.services.relationshipsService.getRelationships(this.parentArtifact.id, this.id)
            .then( (result: Relationships.IArtifactRelationshipsResultSet) => {
                deferred.resolve(result);
            }, (error) => {
                if (error && error.statusCode === HttpStatusCode.NotFound) {
                    this.deleted = true;
                }
                deferred.reject(error);
            });
        return deferred.promise;
    }

}
