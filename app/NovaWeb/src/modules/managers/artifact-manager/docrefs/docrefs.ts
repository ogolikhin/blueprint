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
    getObservable(): Rx.IObservable<IArtifactDocRef[]>;
    get(refresh?: boolean): ng.IPromise<IArtifactDocRef[]>;
    add(docrefs: IArtifactDocRef[]);
    remove(docrefs: IArtifactDocRef[]);
    update(docrefs: IArtifactDocRef[]);
    changes(): IArtifactDocRef[];
    refresh(): ng.IPromise<IArtifactDocRef[]>;
    discard();
}

export class DocumentRefs implements IDocumentRefs {
    private docrefs: IArtifactDocRef[];
    private subject: Rx.BehaviorSubject<IArtifactDocRef[]>;
    private changeset: IChangeCollector;
    private isLoaded: boolean;
    private loadPromise: ng.IPromise<any>;

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

    public getObservable(): Rx.IObservable<IArtifactDocRef[]> {
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
        return this.docrefs || this.loadPromise;
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
                this.changeset.add(changeset);
                
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
                    this.changeset.add(changeset);
                    
                    this.statefulItem.lock();
                }
            });
            this.subject.onNext(this.docrefs);
        }

        return this.docrefs;
    }

    public changes(): IArtifactDocRef[] {
        let docRefChanges = new Array<IArtifactDocRef>();
        let changes = this.changeset.get();
        let uniqueKeys = changes
            .map(change => change.key)
            .filter((elem, index, self) => index == self.indexOf(elem));
        let deltaChanges = new Array<IChangeSet>();
        // remove changesets that cancel eachother.
        uniqueKeys.forEach((key) => {
            let addChanges = changes.filter(a => a.key === key && a.type === ChangeTypeEnum.Add);
            let deleteChanges = changes.filter(a => a.key === key && a.type === ChangeTypeEnum.Delete);
            if (addChanges.length > deleteChanges.length) {
                deltaChanges.push(addChanges[0]);
            } else if (addChanges.length < deleteChanges.length) {
                deltaChanges.push(deleteChanges[0])
            }
        });
        deltaChanges.forEach(change => {
            const docRef = change.value as IArtifactDocRef;
            docRef.changeType = change.type;
            docRefChanges.push(docRef);
        });
        return docRefChanges;
    }

    public discard() {
        this.changeset.reset();
        this.subject.onNext(this.docrefs);
    }

    // TODO: stub, implement
    public refresh(): ng.IPromise<any> {

        return null;
    }
}
