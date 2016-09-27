import {
    IArtifactAttachmentsResultSet,
    IArtifactAttachment,
    ChangeTypeEnum, 
    IChangeCollector, 
    IChangeSet, 
    ChangeSetCollector 
} from "../";

import { 
    IBlock,
    IIStatefulItem
} from "../../models";

export interface IArtifactAttachments extends IBlock<IArtifactAttachment[]> {
    initialize(attachments: IArtifactAttachment[]);
    observable: Rx.IObservable<IArtifactAttachment[]>;
    get(refresh?: boolean): ng.IPromise<IArtifactAttachment[]>;
    add(attachments: IArtifactAttachment[]);
    remove(attachments: IArtifactAttachment[]);
    update(attachments: IArtifactAttachment[]);
    changes(): IArtifactAttachment[];
    discard();
}

export class ArtifactAttachments implements IArtifactAttachments {
    private attachments: IArtifactAttachment[];
    private subject: Rx.BehaviorSubject<IArtifactAttachment[]>;
    private changeset: IChangeCollector;
    private isLoaded: boolean;

    constructor(private statefulItem: IIStatefulItem) {
        this.attachments = [];
        this.subject = new Rx.BehaviorSubject<IArtifactAttachment[]>(this.attachments);
        this.changeset = new ChangeSetCollector(statefulItem);
    }

    public initialize(attachments: IArtifactAttachment[]) {
        this.isLoaded = true;
        this.attachments = attachments;
        this.subject.onNext(this.attachments);
    }

    // refresh = true: turn lazy loading off, always reload
    public get(refresh: boolean = true): ng.IPromise<IArtifactAttachment[]> {
        const deferred = this.statefulItem.getServices().getDeferred<IArtifactAttachment[]>();

        if (this.isLoaded && !refresh) {
            deferred.resolve(this.attachments);
            this.subject.onNext(this.attachments);
        } else {
            this.statefulItem.getAttachmentsDocRefs().then((result: IArtifactAttachmentsResultSet) => {
                deferred.resolve(result.attachments);
                this.subject.onNext(this.attachments);
                this.isLoaded = true;
            }, (error) => {
                deferred.reject(error);
            });
        }

        return deferred.promise;
    }

    public get observable(): Rx.IObservable<IArtifactAttachment[]> {
        return this.subject.asObservable();
    }

    public add(attachments: IArtifactAttachment[]): IArtifactAttachment[] {
        if (attachments) {
            attachments.map((attachment: IArtifactAttachment) => {
                this.attachments.push(attachment); 
                const changeset = {
                    type: ChangeTypeEnum.Add,
                    key: attachment.guid,
                    value: attachment
                } as IChangeSet;
                this.changeset.add(changeset);
                this.statefulItem.lock();
            });
            this.subject.onNext(this.attachments);
        }
        
        return this.attachments;
    }

    public update(attachments: IArtifactAttachment[]): IArtifactAttachment[] {
        throw Error("operation not supported");
    }

    public remove(attachments: IArtifactAttachment[]): IArtifactAttachment[] {
        if (attachments) {
            attachments.map((attachment: IArtifactAttachment) => {
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
                    this.statefulItem.lock();
                }
            });
            this.subject.onNext(this.attachments);
        }

        return this.attachments;
    }

    public changes(): IArtifactAttachment[] {
        let attachmentChanges = new Array<IArtifactAttachment>();
        let changes = this.changeset.get();
        changes.forEach(change => {
            const attachment = change.value as IArtifactAttachment;
            attachment.changeType = change.type;
            attachmentChanges.push(attachment);
        });
        return attachmentChanges;
    }

    public discard() {
        this.changeset.reset();
        this.subject.onNext(this.attachments);
    }
}
