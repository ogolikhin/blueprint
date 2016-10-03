import * as angular from "angular";
import { IIStatefulItem } from "../item";
import { ChangeSetCollector, ChangeTypeEnum, IChangeCollector, IChangeSet } from "../changeset";
import { IRelationship, TraceDirection, IArtifactRelationshipsResultSet } from "../../../main/models/relationshipmodels";

export interface IArtifactRelationships {
    // getObservable(): Rx.IObservable<IRelationship[]>;
    observable: Rx.IObservable<IRelationship[]>;
    get(refresh?: boolean): ng.IPromise<IRelationship[]>;
    add(relationships: IRelationship[]);
    remove(relationships: IRelationship[]);
    update(relationships: IRelationship[]);
    changes(): IRelationship[];
    refresh(): ng.IPromise<IRelationship[]>;
    discard();
    canEdit: boolean;
}

export class ArtifactRelationships implements IArtifactRelationships {
    private relationships: IRelationship[];
    private originalRelationships: IRelationship[];
    private subject: Rx.BehaviorSubject<IRelationship[]>;
    
    private changeset: IChangeCollector;
    private isLoaded: boolean;
    private loadPromise: ng.IPromise<any>;
    public canEdit: boolean;

    constructor(private statefulItem: IIStatefulItem) {
        this.relationships = [];
        this.originalRelationships = [];
        this.subject = new Rx.BehaviorSubject<IRelationship[]>(this.relationships);
        this.changeset = new ChangeSetCollector(statefulItem);
    }

    // refresh = true: turn lazy loading off, always reload
    public get(refresh: boolean = true): ng.IPromise<IRelationship[]> {
        const deferred = this.statefulItem.getServices().getDeferred<IRelationship[]>();

        if (this.isLoaded && !refresh) {
            deferred.resolve(this.relationships);
            this.subject.onNext(this.relationships);
        } else {
            this.statefulItem.getRelationships().then((result: IArtifactRelationshipsResultSet) => {
                const manual = result.manualTraces || [];
                const other = result.otherTraces || [];
                let loadedRelationships = manual.concat(other);
                this.canEdit = result.canEdit;
                this.relationships = loadedRelationships;
                this.originalRelationships = angular.copy(loadedRelationships);
                deferred.resolve(loadedRelationships);
                this.subject.onNext(this.relationships);
                this.isLoaded = true;
            }, (error) => {
                deferred.reject(error);
            });
        }

        return deferred.promise;
    }

    // public getObservable(): Rx.IObservable<IRelationship[]> {
    //     if (!this.isLoadedOrLoading()) {

    //         this.loadPromise = this.statefulItem.getRelationships().then((result: IRelationship[]) => {
    //             this.subject.onNext(this.relationships);

    //         }, (error) => {
    //             this.subject.onError(error);

    //         }).finally(() => {
    //             this.isLoaded = true;
    //             this.loadPromise = null;
    //         });
    //     }
        
    //     return this.subject.filter(it => !!it).asObservable();
    // }

    protected isLoadedOrLoading() {
        return this.relationships || this.loadPromise;
    }

    public get observable(): Rx.IObservable<IRelationship[]> {
        return this.subject.asObservable();
    }

    public add(relationships: IRelationship[]): IRelationship[] {
        if (relationships) {
            relationships.map((relationship: IRelationship) => {
                this.relationships.push(relationship);
                
                const changeset = {
                    type: ChangeTypeEnum.Add,
                    key: this.getKey(relationship),
                    value: relationship
                } as IChangeSet;
                this.changeset.add(changeset);
                
                this.statefulItem.lock();
            });
            this.subject.onNext(this.relationships);
        }
        
        return this.relationships;
    }

    private getKey(relationship: IRelationship) {
        return `${relationship.itemId}-${relationship.traceType}`;
    }

    public update(docrefs: IRelationship[]): IRelationship[] {
        throw Error("operation not supported");
    }

    public remove(relationships: IRelationship[]): IRelationship[] {
        if (relationships) {
            relationships.map((relationship: IRelationship) => {
                const foundRelationshipIndex = this.relationships.indexOf(relationship);

                if (foundRelationshipIndex > -1) {
                    this.relationships.splice(foundRelationshipIndex, 1);
                    
                    const changeset = {
                        type: ChangeTypeEnum.Delete,
                        key: relationship.artifactId,
                        value: relationship
                    } as IChangeSet;
                    this.changeset.add(changeset);
                    
                    this.statefulItem.lock();
                }
            });
            this.subject.onNext(this.relationships);
        }

        return this.relationships;
    }

    private isChanged = (updated: IRelationship, original: IRelationship) => {
        return updated.traceDirection !== original.traceDirection || updated.suspect !== original.suspect;
    }

    private getMatchingRelationshipEntry = (toFind: IRelationship, relationshipList: IRelationship[]) => {
        let matches = relationshipList.filter(a => a.itemId === toFind.itemId && a.traceType === toFind.traceType);
        if (matches.length !== 1) {
            return null;
        } else {
            return matches[0];
        }
    };

    public changes() {
        let deltaRelationshipChanges = new Array<IRelationship>();
        this.relationships.forEach(updatedRelationship => {
            let oldRelationship = this.getMatchingRelationshipEntry(updatedRelationship, this.originalRelationships);
            if (oldRelationship && this.isChanged(updatedRelationship, oldRelationship)) {
                updatedRelationship.changeType = ChangeTypeEnum.Update;
                deltaRelationshipChanges.push(updatedRelationship);
            } else if (!oldRelationship) {
                updatedRelationship.changeType = ChangeTypeEnum.Add;
                deltaRelationshipChanges.push(updatedRelationship);
            }
        });
        this.originalRelationships.forEach(originalRelationship => {
            let updatedRelationship = this.getMatchingRelationshipEntry(originalRelationship, this.relationships);
            if (!updatedRelationship) {
                originalRelationship.changeType = ChangeTypeEnum.Delete;
                deltaRelationshipChanges.push(originalRelationship);
            }
        });
        return deltaRelationshipChanges;
    }

    public discard() {
        this.changeset.reset();
        this.subject.onNext(this.relationships);
    }

    // TODO: stub, implement
    public refresh(): ng.IPromise<any> {

        return null;
    }
}
