import { IArtifactAttachmentsResultSet, IArtifactDocRef } from "./";
import {
    ChangeTypeEnum, 
    IChangeCollector, 
    IChangeSet,
    ChangeSetCollector
} from "../";
import { 
    IBlock,
    IIStatefulItem
} from "../../models";


export interface IDocumentRefs extends IBlock<IArtifactDocRef[]> {
    initialize(docrefs: IArtifactDocRef[]);
    observable: Rx.IObservable<IArtifactDocRef[]>;
    get(refresh?: boolean): ng.IPromise<IArtifactDocRef[]>;
    add(docrefs: IArtifactDocRef[]);
    remove(docrefs: IArtifactDocRef[]);
    update(docrefs: IArtifactDocRef[]);
    discard();
}

export class DocumentRefs implements IDocumentRefs {
    private docrefs: IArtifactDocRef[];
    private subject: Rx.BehaviorSubject<IArtifactDocRef[]>;
    private changeset: IChangeCollector;
    private isLoaded: boolean;

    constructor(private statefulItem: IIStatefulItem) {
        this.docrefs = [];
        this.subject = new Rx.BehaviorSubject<IArtifactDocRef[]>(this.docrefs);
        this.changeset = new ChangeSetCollector(statefulItem);
    }

    public initialize(docrefs: IArtifactDocRef[]) {
        this.isLoaded = true;
        this.docrefs = docrefs;
        this.subject.onNext(this.docrefs);
    }

    // refresh = true: turn lazy loading off, always reload
    public get(refresh: boolean = true): ng.IPromise<IArtifactDocRef[]> {
        const deferred = this.statefulItem.getServices().getDeferred<IArtifactDocRef[]>();

        if (this.isLoaded && !refresh) {
            deferred.resolve(this.docrefs);
            this.subject.onNext(this.docrefs);
        } else {
            this.statefulItem.getAttachmentsDocRefs().then((result: IArtifactAttachmentsResultSet) => {
                deferred.resolve(result.documentReferences);
                this.isLoaded = true;
            });
        }

        return deferred.promise;
    }

    public get observable(): Rx.IObservable<IArtifactDocRef[]> {
        return this.subject.asObservable();
    }

    public add(docrefs: IArtifactDocRef[]): IArtifactDocRef[] {
        if (docrefs) {
            docrefs.map((docref: IArtifactDocRef) => {
                this.docrefs.push(docref);
                
                const changeset = {
                    type: ChangeTypeEnum.Add,
                    key: docref.artifactId,
                    value: docref
                } as IChangeSet;
                this.changeset.add(changeset, docref);
                
                this.statefulItem.lock();
            });
            this.subject.onNext(this.docrefs);
        }
        
        return this.docrefs;
    }

    public update(docrefs: IArtifactDocRef[]): IArtifactDocRef[] {
        throw Error("operation not supported");
    }

    public remove(docrefs: IArtifactDocRef[]): IArtifactDocRef[] {
        if (docrefs) {
            docrefs.map((docref: IArtifactDocRef) => {
                const foundDocRefIndex = this.docrefs.indexOf(docref);

                if (foundDocRefIndex > -1) {
                    this.docrefs.splice(foundDocRefIndex, 1);
                    
                    const changeset = {
                        type: ChangeTypeEnum.Delete,
                        key: docref.artifactId,
                        value: docref
                    } as IChangeSet;
                    this.changeset.add(changeset, docref);
                    
                    this.statefulItem.lock();
                }
            });
            this.subject.onNext(this.docrefs);
        }

        return this.docrefs;
    }

    public discard() {
        this.docrefs = this.changeset.reset().map((changeset: IChangeSet) => changeset.value);
        this.subject.onNext(this.docrefs);
    }
}
