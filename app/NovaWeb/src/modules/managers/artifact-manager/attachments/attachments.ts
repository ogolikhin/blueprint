import { IIStatefulItem } from "../item";
import { IDispose } from "../../models";
import { ChangeTypeEnum, IChangeCollector, IChangeSet, ChangeSetCollector  } from "../changeset";
import { IArtifactAttachmentsResultSet, IArtifactAttachment } from "./attachments.svc";


export interface IArtifactAttachments extends IDispose {
    isLoading: boolean;
    initialize(attachments: IArtifactAttachment[]);
    getObservable(): Rx.IObservable<IArtifactAttachment[]>;
    add(attachments: IArtifactAttachment[]);
    remove(attachments: IArtifactAttachment[]);
    changes(): IArtifactAttachment[];
    refresh(): ng.IPromise<IArtifactAttachment[]>;
    discard();
}

export class ArtifactAttachments implements IArtifactAttachments {
    private attachments: IArtifactAttachment[];
    private subject: Rx.BehaviorSubject<IArtifactAttachment[]>;
    private changeset: IChangeCollector;
    private isLoaded: boolean;
    private loadPromise: ng.IPromise<any>;

    constructor(private statefulItem: IIStatefulItem) {
        this.subject = new Rx.BehaviorSubject<IArtifactAttachment[]>(this.attachments);
        this.changeset = new ChangeSetCollector(statefulItem);
    }

    public get isLoading(): boolean {
        return !this.isLoaded || !!this.loadPromise;
    }

    public initialize(attachments: IArtifactAttachment[]) {
        this.isLoaded = true;
        this.attachments = attachments;
        this.subject.onNext(this.attachments);
    }

    // refresh = true: turn lazy loading off, always reload
    private get(refresh: boolean = true): ng.IPromise<IArtifactAttachment[]> {
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

    public getObservable(): Rx.IObservable<IArtifactAttachment[]> {
        if (!this.isLoadedOrLoading()) {
            this.loadPromise = this.statefulItem.getAttachmentsDocRefs()
                .catch(error => {
                    this.subject.onError(error);
                }).finally(() => {
                    this.loadPromise = null;
                });
        }
        
        return this.subject.filter(it => !!it).asObservable();
    }

    protected isLoadedOrLoading() {
        return this.attachments || this.loadPromise;
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
        let uniqueKeys = changes
            .map(change => change.key)
            .filter((elem, index, self) => index === self.indexOf(elem));
        let deltaChanges = new Array<IChangeSet>();
        // remove changesets that cancel eachother.
        uniqueKeys.forEach((key) => {
            let addChanges = changes.filter(a => a.key === key && a.type === ChangeTypeEnum.Add);
            let deleteChanges = changes.filter(a => a.key === key && a.type === ChangeTypeEnum.Delete);
            if (addChanges.length > deleteChanges.length) {
                deltaChanges.push(addChanges[0]);
            } else if (addChanges.length < deleteChanges.length) {
                deltaChanges.push(deleteChanges[0]);
            }
        });
        deltaChanges.forEach(change => {
            const attachment = change.value as IArtifactAttachment;
            attachment.changeType = change.type;
            attachmentChanges.push(attachment);
        });
        return attachmentChanges;
    }

    public dispose() {
        delete this.attachments;
        delete this.changeset;
        delete this.loadPromise;
    }

    public discard() {
        this.changeset.reset();
        this.subject.onNext(this.attachments);
    }

    public refresh(): ng.IPromise<IArtifactAttachment[]> {
        this.isLoaded = false;
        return this.get(true);
    }
}
