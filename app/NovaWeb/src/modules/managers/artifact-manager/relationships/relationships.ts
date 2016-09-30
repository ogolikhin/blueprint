import { ChangeSetCollector } from "../changeset";
import { Models } from "../../../main/models";
import { IRelationship, TraceDirection } from "../../../main/models/relationshipmodels";
import {
    ChangeTypeEnum, 
    IChangeCollector, 
    IChangeSet
} from "../";

import { 
    IBlock,
    IIStatefulItem
} from "../../models";

export interface IArtifactRelationships extends IBlock<IRelationship[]> {
    observable: Rx.IObservable<IRelationship[]>;
    get(refresh?: boolean): ng.IPromise<Relationships.IRelationship[]>;
    add(relationships: IRelationship[]);
    remove(relationships: IRelationship[]);
    update(relationships: IRelationship[]);
    changes(): IRelationship[];
    discard();
}

export class ArtifactRelationships implements IArtifactRelationships {
    private relationships: IRelationship[];
    private originalRelationships: IRelationship[];
    private subject: Rx.BehaviorSubject<IRelationship[]>;
    
    private changeset: IChangeCollector;
    private isLoaded: boolean;

    constructor(private statefulItem: IIStatefulItem) {
        this.relationships = [];
        this.originalRelationships = [];
        this.subject = new Rx.BehaviorSubject<IRelationship[]>(this.relationships);
        this.changeset = new ChangeSetCollector(statefulItem);
    }

    // public initialize(relationships: Relationships.IRelationship[]) {
    //     this.isLoaded = true;
    //     this.relationships = relationships;
    //     this.subject.onNext(this.relationships);
    // }

    // refresh = true: turn lazy loading off, always reload
    public get(refresh: boolean = true): ng.IPromise<Relationships.IRelationship[]> {
        const deferred = this.statefulItem.getServices().getDeferred<IRelationship[]>();

        if (this.isLoaded && !refresh) {
            deferred.resolve(this.relationships);
            this.subject.onNext(this.relationships);
        } else {
            this.statefulItem.getRelationships().then((result: IRelationship[]) => {
                this.relationships = result;
                this.relationships.forEach(relationship => {
                    this.originalRelationships.push(this.cloneRelationship(relationship));
                });
                deferred.resolve(result);
                this.subject.onNext(this.relationships);
                this.isLoaded = true;
            }, (error) => {
                deferred.reject(error);
            });
        }

        return deferred.promise;
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
        return `${relationship.itemId}-${relationship.traceType}`
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
        if (matches.length !== 1){
            return null;
        } else {
            return matches[0];
        }
    };

    private cloneRelationship = (original: IRelationship) => {
        return {
            artifactId: original.artifactId,
            artifactTypePrefix: original.artifactTypePrefix,
            artifactName: original.artifactName,
            itemId: original.itemId,
            itemTypePrefix: original.itemTypePrefix,
            itemName: original.itemName,
            itemLabel: original.itemLabel,
            projectId: original.projectId,
            projectName: original.projectName,
            traceDirection: original.traceDirection,
            traceType: original.traceType,
            suspect: original.suspect,
            hasAccess: original.hasAccess,
            primitiveItemTypePredefined: original.primitiveItemTypePredefined,
            isSelected: original.isSelected
        }
    }

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
}
