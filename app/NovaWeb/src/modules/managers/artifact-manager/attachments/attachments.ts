import { Models, Enums } from "../../../main/models";
import { IArtifactAttachmentsResultSet } from "../../../shell/bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";
import { 
    IStatefulArtifact, 
    IArtifactAttachments, 
    IArtifactAttachment, 
    IArtifactManager 
} from "../../models";

export class ArtifactAttachments implements IArtifactAttachments {
    private attachments: IArtifactAttachment[];
    private subject: Rx.BehaviorSubject<IArtifactAttachment[]>;
    private state: IStatefulArtifact;
    private manager: IArtifactManager;

    constructor(artifactState: IStatefulArtifact) {
        this.attachments = [];
        this.manager = artifactState.manager;
        this.state = artifactState;
        this.subject = new Rx.BehaviorSubject<IArtifactAttachment[]>(this.attachments);
    }

    // TODO: how would this work for subartifact attachments?
    public get value(): ng.IPromise<IArtifactAttachment[]> {
        let subArtifactId = null;

        const deferred = this.manager.$q.defer<IArtifactAttachment[]>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${this.state.id}/attachment`, 
            method: "GET",
            params: {
                subartifactId: subArtifactId,
                addDrafts: true
            },
            timeout: null
        };

        this.manager.request<Models.IArtifact>(requestObj).then((result: ng.IHttpPromiseCallbackArg<IArtifactAttachmentsResultSet>) => {
            this.attachments = result.data.attachments;
            deferred.resolve(this.attachments);
        
        }).catch((err) => {
            deferred.reject(err);
        });

        return deferred.promise;
    }

    public get observable(): Rx.IObservable<IArtifactAttachment[]> {
        return this.subject.asObservable();
    }

    public add(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        const deferred = this.manager.$q.defer<IArtifactAttachment[]>();

        this.attachments.push(attachment);

        // TODO: add changeset tracking

        deferred.resolve(this.attachments);
        this.subject.onNext(this.attachments);

        return deferred.promise;
    }

    public update(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        throw Error("operation not supported");
    }

    public remove(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        const deferred = this.manager.$q.defer<IArtifactAttachment[]>();
        const foundAttachmentIndex = this.attachments.indexOf(attachment);
        let deletedAttachment: IArtifactAttachment;

        if (foundAttachmentIndex > -1) {
            deletedAttachment = this.attachments.splice(foundAttachmentIndex, 1)[0];
            
            // TODO: add changeset tracking

            deferred.resolve(this.attachments);
            this.subject.onNext(this.attachments);
        } else {
            deferred.reject("Attachment not found");
        }

        return deferred.promise;
    }
}
