import { Models, Enums } from "../../../main/models";
import { IArtifactAttachmentsResultSet, IArtifactAttachment } from "../../../shell/bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";
import { 
    IIStatefulArtifact,
    IArtifactAttachments
} from "../../models";

export class ArtifactAttachments implements IArtifactAttachments {
    private attachments: IArtifactAttachment[];
    private subject: Rx.BehaviorSubject<IArtifactAttachment[]>;
    private statefulArtifact: IIStatefulArtifact;

    constructor(statefulArtifact: IIStatefulArtifact) {
        this.attachments = [];
        this.statefulArtifact = statefulArtifact;
        this.subject = new Rx.BehaviorSubject<IArtifactAttachment[]>(this.attachments);
    }

    public initialize(attachments: IArtifactAttachment[]) {
        this.attachments = attachments;
        this.subject.onNext(this.attachments);
    }

    // TODO: how would this work for subartifact attachments?
    public get value(): ng.IPromise<IArtifactAttachment[]> {
        return this.statefulArtifact.getAttachmentsDocRefs().then((result: IArtifactAttachmentsResultSet) => {
            return result.attachments;
        });
    }

    public get observable(): Rx.IObservable<IArtifactAttachment[]> {
        return this.subject.asObservable();
    }

    public add(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        const deferred = this.statefulArtifact.getDeferred<IArtifactAttachment[]>();

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
        const deferred = this.statefulArtifact.getDeferred<IArtifactAttachment[]>();
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
