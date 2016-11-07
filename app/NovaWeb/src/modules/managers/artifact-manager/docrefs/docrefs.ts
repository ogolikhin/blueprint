import {IIStatefulItem} from "../item";
import {IDispose} from "../../models";
import {ChangeTypeEnum, IChangeCollector, IChangeSet, ChangeSetCollector} from "../changeset";
import {IArtifactAttachmentsResultSet, IArtifactDocRef} from "../attachments";
import {IApplicationError} from "../../../core/error/applicationError";

export interface IDocumentRefs extends IDispose {
    isLoading: boolean;
    initialize(docrefs: IArtifactDocRef[]);
    getObservable(): Rx.IObservable<IArtifactDocRef[]>;
    // get(refresh?: boolean): ng.IPromise<IArtifactDocRef[]>;
    add(docrefs: IArtifactDocRef[]);
    remove(docrefs: IArtifactDocRef[]);
    update(docrefs: IArtifactDocRef[]);
    changes(): IArtifactDocRef[];
    refresh(): ng.IPromise<IArtifactDocRef[]>;
    discard();
    errorObservable(): Rx.Observable<IApplicationError>;
}

export class DocumentRefs implements IDocumentRefs {
    private docrefs: IArtifactDocRef[];
    private subject: Rx.BehaviorSubject<IArtifactDocRef[]>;
    private changeset: IChangeCollector;
    private isLoaded: boolean;
    private loadPromise: ng.IPromise<any>;
    private _error: Rx.BehaviorSubject<IApplicationError>;

    constructor(private statefulItem: IIStatefulItem) {
        this.docrefs = [];
        this.subject = new Rx.BehaviorSubject<IArtifactDocRef[]>(this.docrefs);
        this.changeset = new ChangeSetCollector(statefulItem);
        this.isLoaded = true;
    }

    public get isLoading(): boolean {
        return !this.isLoaded || !!this.loadPromise;
    }

    public initialize(docrefs: IArtifactDocRef[]) {
        this.isLoaded = true;
        this.docrefs = docrefs;
        this.subject.onNext(this.docrefs);
    }

    protected get error(): Rx.BehaviorSubject<IApplicationError> {
        if (!this._error) {
            this._error = new Rx.BehaviorSubject<IApplicationError>(null);
        }
        return this._error;
    }

    public errorObservable(): Rx.Observable<IApplicationError> {
        return this.error.filter(it => !!it).distinctUntilChanged().asObservable();
    }

    // refresh = true: turn lazy loading off, always reload
    private get(refresh: boolean = true): ng.IPromise<IArtifactDocRef[]> {
        const deferred = this.statefulItem.getServices().getDeferred<IArtifactDocRef[]>();

        if (this.isLoaded && !refresh) {
            deferred.resolve(this.docrefs);
            this.subject.onNext(this.docrefs);
        } else {
            this.statefulItem.getAttachmentsDocRefs().then((result: IArtifactAttachmentsResultSet) => {
                deferred.resolve(result.documentReferences);
                this.isLoaded = true;
            }, (error) => {
                deferred.reject(error);
            });
        }

        return deferred.promise;
    }

    public getObservable(): Rx.IObservable<IArtifactDocRef[]> {
        if (!this.isLoadedOrLoading()) {
            this.loadPromise = this.statefulItem.getAttachmentsDocRefs()
                .catch(error => {
                    this.error.onNext(error);
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
        const changes = this.changeset.get();
        const uniqueKeys = changes
            .map(change => change.key)
            .filter((elem, index, self) => index === self.indexOf(elem));
        const deltaChanges = new Array<IChangeSet>();
        // remove changesets that cancel eachother.
        uniqueKeys.forEach((key) => {
            const addChanges = changes.filter(a => a.key === key && a.type === ChangeTypeEnum.Add);
            const deleteChanges = changes.filter(a => a.key === key && a.type === ChangeTypeEnum.Delete);
            if (addChanges.length > deleteChanges.length) {
                deltaChanges.push(addChanges[0]);
            } else if (addChanges.length < deleteChanges.length) {
                deltaChanges.push(deleteChanges[0]);
            }
        });
        if (deltaChanges.length > 0) {
            const docRefChanges: IArtifactDocRef[] = [];
            deltaChanges.forEach(change => {
                const docRef = change.value as IArtifactDocRef;
                docRef.changeType = change.type;
                docRefChanges.push(docRef);
            });
            return docRefChanges;
        }
        return undefined;
    }

    public dispose() {
        delete this.docrefs;
        delete this.changeset;
        delete this.loadPromise;
    }

    public discard() {
        this.changeset.reset();
        this.subject.onNext(this.docrefs);
    }

    public refresh(): ng.IPromise<any> {
        this.isLoaded = false;
        return this.get(true);
    }
}
