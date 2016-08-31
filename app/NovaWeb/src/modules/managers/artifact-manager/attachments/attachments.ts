import { Models, Enums } from "../../../main/models";
import { IStatefulArtifact, IArtifactAttachments, IArtifactAttachment } from "../interfaces";

export class ArtifactAttachments implements IArtifactAttachments {
    private attachments: IArtifactAttachment[];
    private subject: Rx.BehaviorSubject<IArtifactAttachment[]>;
    private state: IStatefulArtifact;

    constructor(artifactState: IStatefulArtifact) {
        this.attachments = [];
        this.state = artifactState;
        this.subject = new Rx.BehaviorSubject<IArtifactAttachment[]>(this.attachments);
    }

    public get value(): ng.IPromise<IArtifactAttachment[]> {
        // try to get attachments through a service
        return {} as ng.IPromise<IArtifactAttachment[]>;
    }    

    public get observable(): Rx.IObservable<IArtifactAttachment[]> {
        return this.subject.asObservable();
    }

    public add(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        this.attachments.push(attachment);
        this.subject.onNext(this.attachments);

        return null;
    }

    public update(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        // this.state.manager.lockArtifact();
        return {} as ng.IPromise<IArtifactAttachment[]>;
    }

    public remove(attachment: IArtifactAttachment): ng.IPromise<IArtifactAttachment[]> {
        return {} as ng.IPromise<IArtifactAttachment[]>;
    }
}
