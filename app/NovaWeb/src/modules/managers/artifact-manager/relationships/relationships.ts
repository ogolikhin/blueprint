import * as angular from "angular";
import {IIStatefulItem} from "../item";
import {ChangeSetCollector, ChangeTypeEnum, IChangeCollector, IChangeSet} from "../changeset";
import {IRelationship, IArtifactRelationshipsResultSet, LinkType} from "../../../main/models/relationshipModels";

export interface IArtifactRelationships {
    getObservable(): Rx.IObservable<IRelationship[]>;
    get(refresh?: boolean): ng.IPromise<IRelationship[]>;
    add(relationships: IRelationship[]);
    remove(relationships: IRelationship[]);
    update(relationships: IRelationship[]): IRelationship[];
    changes(): IRelationship[];
    refresh(): ng.IPromise<IRelationship[]>;
    discard();
    updateManual(relationships: IRelationship[]);
    canEdit: boolean;
}

export interface IResult {
    found: boolean;
    index: number;
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

    public get(refresh: boolean = false): ng.IPromise<IRelationship[]> {
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

    protected isLoadedOrLoading() {
        return this.relationships || this.loadPromise;
    }

    public getObservable(): Rx.IObservable<IRelationship[]> {
        return this.subject.asObservable();
    }

    // we do not use changesets to calculate what to send back to the server
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

    public updateManual(relationships: IRelationship[]): IRelationship[] {
        this.relationships = this.relationships.filter((relationship: IRelationship) =>
        relationship.traceType !== LinkType.Manual);

        this.relationships = this.relationships.concat(relationships);

        this.relationships.map((relationship: IRelationship) => {
            const changeset = {
                type: ChangeTypeEnum.Add,
                key: this.getKey(relationship),
                value: relationship
            } as IChangeSet;
            this.changeset.add(changeset);
        });

        let originalManualTraces = this.originalRelationships.filter((relationship: IRelationship) =>
        relationship.traceType === LinkType.Manual);
        if (originalManualTraces.length > 0 && relationships.length === 0) {  //something was deleted
            //we dont calculate what exactly was deleted as we dont look at the changesets for relationships.
            const changeset = {
                type: ChangeTypeEnum.Delete,
                key: null,
                value: null
            } as IChangeSet;
            this.changeset.add(changeset);
        }

        this.statefulItem.lock();

        this.subject.onNext(this.relationships);

        return this.relationships;
    }

    // we do not use changesets to calculate what to send back to the server
    public remove(relationships: IRelationship[]): IRelationship[] {
        if (relationships) {
            relationships.map((relationship: IRelationship) => {
                const foundRelationshipIndex = this.inArray(this.relationships, relationship).index;

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

    private composeDeltaChangeObject (relationship: IRelationship, changeType: ChangeTypeEnum) {
        let result = {
            artifactId: relationship.artifactId,
            itemId: relationship.itemId,
            traceDirection: relationship.traceDirection,
            traceType: relationship.traceType,
            suspect: relationship.suspect
        } as IRelationship;
        result.changeType = changeType;
        return result;
    }

    public changes() {
        const deltaRelationshipChanges: IRelationship[] = [];
        this.relationships.forEach((updatedRelationship: IRelationship) => {
            const oldRelationship = this.getMatchingRelationshipEntry(updatedRelationship, this.originalRelationships);
            if (oldRelationship && this.isChanged(updatedRelationship, oldRelationship)) {
                deltaRelationshipChanges.push(this.composeDeltaChangeObject(updatedRelationship, ChangeTypeEnum.Update));
            } else if (!oldRelationship) {
                deltaRelationshipChanges.push(this.composeDeltaChangeObject(updatedRelationship, ChangeTypeEnum.Add));
            }
        });
        this.originalRelationships.forEach((originalRelationship: IRelationship) => {
            const updatedRelationship = this.getMatchingRelationshipEntry(originalRelationship, this.relationships);
            if (!updatedRelationship) {
                deltaRelationshipChanges.push(this.composeDeltaChangeObject(originalRelationship, ChangeTypeEnum.Delete));
            }
        });
        if (deltaRelationshipChanges && deltaRelationshipChanges.length > 0) {
            return deltaRelationshipChanges;
        }
        return undefined;
    }

    public discard() {
        this.changeset.reset();
        this.subject.onNext(this.relationships);
    }

    public refresh(): ng.IPromise<IRelationship[]> {
        this.isLoaded = false;
        return this.get(true);
    }

    public inArray(array, item) {
        let found = false,
            index = -1;
        if (array) {
            for (let i = 0; i < array.length; i++) {
                if (array[i].itemId === item.itemId) {
                    found = true;
                    index = i;
                    break;
                }
            }
        }

        return <IResult>{"found": found, "index": index};
    }
}
