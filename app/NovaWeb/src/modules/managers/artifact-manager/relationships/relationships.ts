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
    add(docrefs: Relationships.IRelationship[]);
    remove(docrefs: Relationships.IRelationship[]);
    update(docrefs: Relationships.IRelationship[]);
    discard();
}

export class ArtifactRelationships implements IArtifactRelationships {
    private relationships: Relationships.IRelationship[];
    private subject: Rx.BehaviorSubject<Relationships.IRelationship[]>;
    private statefulItem: IIStatefulItem;
    private changeset: IChangeCollector;
    private isLoaded: boolean;

    constructor(statefulItem: IIStatefulItem) {
        this.relationships = [];
        this.statefulItem = statefulItem;
        this.subject = new Rx.BehaviorSubject<Relationships.IRelationship[]>(this.relationships);
        this.changeset = new ChangeSetCollector();
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
            // this.statefulItem.getAttachmentsDocRefs().then((result: IArtifactRelationshipsResultSet) => {
            //     deferred.resolve(result.documentReferences);
            //     this.isLoaded = true;
            // });
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
                this.changeset.add(changeset, relationship);
                
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
                    this.changeset.add(changeset, relationship);
                    
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
