import { IArtifactAttachmentsResultSet, IArtifactAttachment } from "./";
// import { Models, Enums } from "../../../main/models";
import { ChangeSetCollector } from "../changeset";
import { 
    // IStatefulArtifact, 
    ChangeTypeEnum, 
    IChangeCollector, 
    IChangeSet,
    // IStatefulSubArtifact, 
    IIStatefulArtifact,
    IIStatefulSubArtifact,
    IArtifactAttachments
} from "../../models";

export class ArtifactAttachments implements IArtifactAttachments {
    private attachments: IArtifactAttachment[];
    private subject: Rx.BehaviorSubject<IArtifactAttachment[]>;
    private statefulItem: IIStatefulArtifact | IIStatefulSubArtifact;
    private changeset: IChangeCollector;

    constructor(statefulArtifact: IIStatefulArtifact | IIStatefulSubArtifact) {
        this.attachments = [];
        this.statefulItem = statefulArtifact;
        this.subject = new Rx.BehaviorSubject<IArtifactAttachment[]>(this.attachments);
        this.changeset = new ChangeSetCollector();
    }

    public initialize(attachments: IArtifactAttachment[]) {
        this.attachments = attachments;
        this.subject.onNext(this.attachments);
    }

    // TODO: how would this work for subartifact attachments?
    public get value(): ng.IPromise<IArtifactAttachment[]> {
        return this.statefulItem.getAttachmentsDocRefs().then((result: IArtifactAttachmentsResultSet) => {
            return result.attachments;
        });
    }

    public get observable(): Rx.IObservable<IArtifactAttachment[]> {
        return this.subject.asObservable();
    }

    public add(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        const deferred = this.statefulItem.getServices().getDeferred<IArtifactAttachment[]>();

        this.attachments.push(attachment);

        const changeset = {
            type: ChangeTypeEnum.Add,
            key: attachment.guid,
            value: attachment
        } as IChangeSet;
        this.changeset.add(changeset);

        // TODO: can locking be done implicitly?
        // TODO: must propagate locking as a return value. if not locked, revert value.
        this.statefulItem.lock();

        deferred.resolve(this.attachments);
        this.subject.onNext(this.attachments);

        return deferred.promise;
    }

    public update(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        throw Error("operation not supported");
    }

    public remove(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        const deferred = this.statefulItem.getServices().getDeferred<IArtifactAttachment[]>();
        const foundAttachmentIndex = this.attachments.indexOf(attachment);
        let deletedAttachment: IArtifactAttachment;

        if (foundAttachmentIndex > -1) {
            deletedAttachment = this.attachments.splice(foundAttachmentIndex, 1)[0];
            
            const changeset = {
                type: ChangeTypeEnum.Delete,
                key: attachment.guid || attachment.attachmentId,
                value: attachment
            } as IChangeSet;
            this.changeset.add(changeset);

            deferred.resolve(this.attachments);
            this.subject.onNext(this.attachments);
        } else {
            deferred.reject("Attachment not found");
        }

        return deferred.promise;
    }

    public discard() {
        // TODO: implement discard.

        // this.changeset.reset().forEach((it: IChangeSet) => {
        //     this.get(it.key as number).value = it.value;
        // });
    }
}
