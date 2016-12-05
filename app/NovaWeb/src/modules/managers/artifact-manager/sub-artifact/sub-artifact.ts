import {ISubArtifact} from "../../../main/models/models";
import {ReuseSettings} from "../../../main/models/enums";
import {IStatefulArtifactServices} from "../services/services";
import {IStatefulArtifact} from "../artifact/artifact";
import {StatefulItem, IStatefulItem, IIStatefulItem} from "../item/item";
import {IArtifactAttachmentsResultSet} from "../attachments/attachments.svc";
import {MetaData} from "../metadata/metadata";
import {IChangeSet} from "../changeset/changeset";

export interface IIStatefulSubArtifact extends IIStatefulItem {
}

export interface IStatefulSubArtifact extends IStatefulItem, ISubArtifact {
    getObservable(): Rx.Observable<IStatefulSubArtifact>;
    validate(): ng.IPromise<boolean>;
}

export class StatefulSubArtifact extends StatefulItem implements IStatefulSubArtifact, IIStatefulSubArtifact {
    public isLoaded = false;
    private _subject: Rx.BehaviorSubject<IStatefulSubArtifact>;

    constructor(private parentArtifact: IStatefulArtifact, private subArtifact: ISubArtifact, services: IStatefulArtifactServices) {
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
        this.services.artifactService.getSubArtifact(this.parentArtifact.id, this.id, this.getEffectiveVersion()).then((artifact: ISubArtifact) => {
            this.initialize(artifact);
            deferred.resolve(this);
        }).catch((error) => {
            this.artifactState.readonly = true;
            deferred.reject(error);
        });
        return deferred.promise;
    }

    protected loadWithNotify(): ng.IPromise<IStatefulSubArtifact> {
        this.loadPromise = this.load();
        return this.loadPromise.then(() => {
            this.subject.onNext(this);
            return this.services.$q.resolve(this);
        }).catch((error) => {
            this.error.onNext(error);
            return this.services.$q.reject(error);
        }).finally(() => {
            this.loadPromise = null;
        });
    }

    public getObservable(): Rx.Observable<IStatefulSubArtifact> {
        if (!this.isFullArtifactLoadedOrLoading()) {
            this.loadWithNotify();
        }
        return this.subject.filter(it => !!it).asObservable();
    }

    public get readOnlyReuseSettings(): ReuseSettings {
        return this.parentArtifact.readOnlyReuseSettings;
    }

    public isReuseSettingSRO(reuseSetting: ReuseSettings): boolean {
        return (this.parentArtifact.readOnlyReuseSettings & ReuseSettings.Subartifacts) === ReuseSettings.Subartifacts;
    }

    public changes(): ISubArtifact {
        const traces = this.relationships.changes();
        const attachmentValues = this.attachments.changes();
        const docRefValues = this.docRefs.changes();
        const customPropertyChangedValues = this.customProperties.changes();
        const specificPropertyChangedValues = this.specialProperties.changes();

        let hasChanges = false;

        const delta = <ISubArtifact>{};
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

    public validate(): ng.IPromise<boolean> {
        return this.services.propertyDescriptor.createSubArtifactPropertyDescriptors(this).then((propertyTypes) => {
            const result = this.validateItem(propertyTypes);

            if (result) {
                return this.services.$q.resolve(result);
            }

            const  message: string = `The Sub Artifact ${this.prefix + this.id.toString() + ":" + this.name} has validation errors.`;
            return this.services.$q.reject(new Error(message));
        });


    }
}
