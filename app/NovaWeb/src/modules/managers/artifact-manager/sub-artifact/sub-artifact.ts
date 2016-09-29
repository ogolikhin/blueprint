import { Models, Relationships } from "../../../main/models";
// import { ArtifactState} from "../state";
import { IStatefulArtifactServices } from "../services";
import { IMetaData } from "../metadata";
import { StatefulItem } from "../item";
import {
    IStatefulArtifact,
    IIStatefulSubArtifact,
    IStatefulSubArtifact,
    IArtifactAttachmentsResultSet
} from "../../models";

export class StatefulSubArtifact extends StatefulItem implements IStatefulSubArtifact, IIStatefulSubArtifact {
    private isLoaded = false;
    private subject: Rx.BehaviorSubject<IStatefulSubArtifact>;

    public deleted: boolean;

    constructor(private parentArtifact: IStatefulArtifact, private subArtifact: Models.ISubArtifact, services: IStatefulArtifactServices) {
        super(subArtifact, services);
        // this.changesets = new ChangeSetCollector(this.artifact);
    }

    //TODO.
    //Needs implementation of other object like
    //attachments, traces and etc.

    public get artifactState() {
        return this.parentArtifact.artifactState;
    }

    public get metadata(): IMetaData {
        return this.parentArtifact.metadata;
    }


    public get projectId(): number {
        return this.parentArtifact.projectId;
    }

    public getObservable(): Rx.Observable<IStatefulSubArtifact> {
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
                if (error && error.statusCode === 404) {
                    this.deleted = true;
                }
                deferred.reject(error);
            });
        return deferred.promise;
    }

    public getRelationships(): ng.IPromise<Relationships.IRelationship[]> {
        const deferred = this.services.getDeferred();
        this.services.relationshipsService.getRelationships(this.parentArtifact.id, this.id)
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

}
