import { ChangeSetCollector } from "../changeset";
import { Relationships } from "../../../main";
import {
    ChangeTypeEnum, 
    IChangeCollector, 
    IChangeSet
} from "../";

import { 
    IBlock,
    IIStatefulItem
} from "../../models";

export interface IArtifactRelationships extends IBlock<Relationships.IRelationship[]> {
    observable: Rx.IObservable<Relationships.IRelationship[]>;
    get(refresh?: boolean): ng.IPromise<Relationships.IRelationship[]>;
    add(relationships: Relationships.IRelationship[]);
    remove(relationships: Relationships.IRelationship[]);
    update(relationships: Relationships.IRelationship[]);
    discard();
}

export class ArtifactRelationships implements IArtifactRelationships {
    private relationships: Relationships.IRelationship[];
    private subject: Rx.BehaviorSubject<Relationships.IRelationship[]>;
    
    private changeset: IChangeCollector;
    private isLoaded: boolean;

    constructor(private statefulItem: IIStatefulItem) {
        this.relationships = [];
        this.subject = new Rx.BehaviorSubject<Relationships.IRelationship[]>(this.relationships);
        this.changeset = new ChangeSetCollector(statefulItem);
    }

    // public initialize(relationships: Relationships.IRelationship[]) {
    //     this.isLoaded = true;
    //     this.relationships = relationships;
    //     this.subject.onNext(this.relationships);
    // }

    // refresh = true: turn lazy loading off, always reload
    public get(refresh: boolean = true): ng.IPromise<Relationships.IRelationship[]> {
        const deferred = this.statefulItem.getServices().getDeferred<Relationships.IRelationship[]>();

        if (this.isLoaded && !refresh) {
            deferred.resolve(this.relationships);
            this.subject.onNext(this.relationships);
        } else {
            this.statefulItem.getRelationships().then((result: Relationships.IRelationship[]) => {
                deferred.resolve(result);
                this.subject.onNext(this.relationships);
                this.isLoaded = true;
            }, (error) => {
                deferred.reject(error);
            });
        }

        return deferred.promise;
    }

    public get observable(): Rx.IObservable<Relationships.IRelationship[]> {
        return this.subject.asObservable();
    }

    public add(relationships: Relationships.IRelationship[]): Relationships.IRelationship[] {
        if (relationships) {
            relationships.map((relationship: Relationships.IRelationship) => {
                this.relationships.push(relationship);
                
                const changeset = {
                    type: ChangeTypeEnum.Add,
                    key: relationship.artifactId,
                    value: relationship
                } as IChangeSet;
                this.changeset.add(changeset);
                
                this.statefulItem.lock();
            });
            this.subject.onNext(this.relationships);
        }
        
        return this.relationships;
    }

    public update(docrefs: Relationships.IRelationship[]): Relationships.IRelationship[] {
        throw Error("operation not supported");
    }

    public remove(relationships: Relationships.IRelationship[]): Relationships.IRelationship[] {
        if (relationships) {
            relationships.map((relationship: Relationships.IRelationship) => {
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

    public discard() {
        this.relationships = this.changeset.reset().map((changeset: IChangeSet) => changeset.value);
        this.subject.onNext(this.relationships);
    }
}
