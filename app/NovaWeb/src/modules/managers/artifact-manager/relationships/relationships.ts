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
    getObservable(): Rx.IObservable<Relationships.IRelationship[]>;
    add(relationships: Relationships.IRelationship[]);
    remove(relationships: Relationships.IRelationship[]);
    update(relationships: Relationships.IRelationship[]);
    refresh(): ng.IPromise<Relationships.IRelationship[]>;
    discard();
}

export class ArtifactRelationships implements IArtifactRelationships {
    private relationships: Relationships.IRelationship[];
    private subject: Rx.BehaviorSubject<Relationships.IRelationship[]>;
    
    private changeset: IChangeCollector;
    private isLoaded: boolean;
    private loadPromise: ng.IPromise<any>;

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
    // public get(refresh: boolean = true): ng.IPromise<Relationships.IRelationship[]> {
    //     const deferred = this.statefulItem.getServices().getDeferred<Relationships.IRelationship[]>();

    //     if (this.isLoaded && !refresh) {
    //         deferred.resolve(this.relationships);
    //         this.subject.onNext(this.relationships);
    //     } else {
    //         this.statefulItem.getRelationships().then((result: Relationships.IRelationship[]) => {
    //             deferred.resolve(result);
    //             this.subject.onNext(this.relationships);
    //             this.isLoaded = true;
    //         }, (error) => {
    //             deferred.reject(error);
    //         });
    //     }

    //     return deferred.promise;
    // }

    public getObservable(): Rx.IObservable<Relationships.IRelationship[]> {
        if (!this.isLoadedOrLoading()) {

            this.loadPromise = this.statefulItem.getRelationships().then((result: Relationships.IRelationship[]) => {
                this.subject.onNext(this.relationships);

            }, (error) => {
                this.subject.onError(error);

            }).finally(() => {
                this.isLoaded = true;
                this.loadPromise = null;
            });
        }
        
        return this.subject.filter(it => !!it).asObservable();
    }

    protected isLoadedOrLoading() {
        return this.relationships || this.loadPromise;
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
        this.changeset.reset();
        this.subject.onNext(this.relationships);
    }

    // TODO: stub, implement
    public refresh(): ng.IPromise<any> {

        return null;
    }
}
