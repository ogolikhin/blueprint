import { IArtifactHistory, 
    // IArtifactHistoryResultSet, 
    // IArtifactHistoryService, 
    IArtifactHistoryVersion
} from "./";

import { 
    IBlock,
    IIStatefulArtifact
} from "../../models";


export interface IArtifactHistory extends IBlock<IArtifactHistoryVersion[]> {
    observable: Rx.IObservable<IArtifactHistoryVersion[]>;
    get(refresh?: boolean): ng.IPromise<IArtifactHistoryVersion[]>;
    add(docrefs: IArtifactHistoryVersion[]);
    remove(docrefs: IArtifactHistoryVersion[]);
    update(docrefs: IArtifactHistoryVersion[]);
    discard();
}

export class ArtifactHistory implements IArtifactHistory {
    private history: IArtifactHistoryVersion[];
    private subject: Rx.BehaviorSubject<IArtifactHistoryVersion[]>;
    private statefulItem: IIStatefulArtifact;
    private isLoaded: boolean;

    constructor(statefulItem: IIStatefulArtifact) {
        this.history = [];
        this.statefulItem = statefulItem;
        this.subject = new Rx.BehaviorSubject<IArtifactHistoryVersion[]>(this.history);
    }

    // refresh = true: turn lazy loading off, always reload
    public get(refresh: boolean = true): ng.IPromise<IArtifactHistoryVersion[]> {
        const deferred = this.statefulItem.getServices().getDeferred<IArtifactHistoryVersion[]>();

        if (this.isLoaded && !refresh) {
            deferred.resolve(this.history);
            this.subject.onNext(this.history);
        } else {
            this.statefulItem.getArtifactHistory(this.statefulItem.id).then((result: IArtifactHistoryVersion[]) => {
                deferred.resolve(result);
                this.isLoaded = true;
            });
        }

        return deferred.promise;
    }

    public get observable(): Rx.IObservable<IArtifactHistoryVersion[]> {
        return this.subject.asObservable();
    }

    public add(docrefs: IArtifactHistoryVersion[]): void {
        throw Error("operation not supported");
    }

    public update(docrefs: IArtifactHistoryVersion[]): void {
        throw Error("operation not supported");
    }

    public remove(docrefs: IArtifactHistoryVersion[]): void {
        throw Error("operation not supported");
    }

    public discard() {
        throw Error("operation not supported");
    }
}
