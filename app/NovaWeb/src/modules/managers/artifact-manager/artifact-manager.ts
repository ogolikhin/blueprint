import { IMessageService } from "../../core";
import { ISelectionManager } from "../selection-manager/selection-manager";
import { IMetaDataService } from "./metadata";
import { IStatefulArtifactFactory, } from "./artifact";
import { IStatefulArtifact, IDispose } from "../models";

export interface IArtifactManager extends IDispose {
    collectionChangeObservable: Rx.Observable<IStatefulArtifact>;
    selection: ISelectionManager;
    list(): IStatefulArtifact[];
    add(artifact: IStatefulArtifact);
    get(id: number): ng.IPromise<IStatefulArtifact>;
    remove(id: number): IStatefulArtifact;
    removeAll(projectId?: number);
    saveAll(): void;
    publishAll(): void;
    refreshAll(): void;
}

export class ArtifactManager  implements IArtifactManager {
    private artifactDictionary: { [id: number]: IStatefulArtifact };
    private collectionChangeSubject: Rx.BehaviorSubject<IStatefulArtifact>;
    public static $inject = [
        "$log",
        "$q", 
        "messageService",
        "selectionManager", 
        "metadataService", 
        "statefulArtifactFactory"
    ];
        
    constructor(
        private $log: ng.ILogService,
        private $q: ng.IQService, 
        private messageService: IMessageService, 
        private selectionService: ISelectionManager, 
        private metadataService: IMetaDataService,
        private artifactFactory: IStatefulArtifactFactory) {
        this.artifactDictionary = {};
        this.collectionChangeSubject = new Rx.BehaviorSubject<IStatefulArtifact>(null);
//        this.selectionService.selectionObservable.subscribeOnNext(this.onArtifactSelect, this);
    }
    
    public dispose() {
        this.removeAll();
        this.selection.dispose();
    }

    public get selection(): ISelectionManager {
        return this.selectionService;
    }

    public list(): IStatefulArtifact[] {
        let artifactList: IStatefulArtifact[] = [];
        
        for (let artifactKey in this.artifactDictionary) {
            let artifact = this.artifactDictionary[artifactKey];
            
            if (this.artifactDictionary.hasOwnProperty(artifactKey)) {
                artifactList.push(artifact);
            }
        }

        return artifactList;
    }

    public get collectionChangeObservable(): Rx.Observable<IStatefulArtifact> {
         return this.collectionChangeSubject.filter((it: IStatefulArtifact) => it != null).asObservable();
    }

    public get(id: number): ng.IPromise<IStatefulArtifact> {
        const deferred = this.$q.defer<IStatefulArtifact>();
        const artifact = this.artifactDictionary[id];
        if (artifact) {
            deferred.resolve(artifact);
        } else {
            this.artifactFactory.createStatefulArtifact({id: id}).refresh().then((it: IStatefulArtifact) => {
                this.add(it);
                deferred.resolve(it);
            }).catch((err) => {
                if (err) {
                    this.messageService.addError(err);
                }
                
                deferred.reject(err);
            });
        }
        return deferred.promise;
    }
    
    public add(artifact: IStatefulArtifact) {
        if (this.artifactDictionary[artifact.id]) {
            this.$log.info(`Overwriting an already added artifact with id: ${artifact.id}`);
        }
        this.artifactDictionary[artifact.id] = artifact;
        this.collectionChangeSubject.onNext(artifact);
    }

    public remove(id: number): IStatefulArtifact {
        let artifact: IStatefulArtifact = this.artifactDictionary[id];

        if (artifact) {
            delete this.artifactDictionary[id];
            this.collectionChangeSubject.onNext(artifact);
        }

        return artifact;
    }

    public removeAll(projectId?: number) {
        for (const artifactKey in this.artifactDictionary) {
            const artifact = this.artifactDictionary[artifactKey];
            
            if (this.artifactDictionary.hasOwnProperty(artifactKey)) {
                if (!projectId || artifact.projectId === projectId) {
                    artifact.dispose();
                    delete this.artifactDictionary[artifactKey];
                }
            }
        }
        if (projectId) {
            this.metadataService.remove(projectId);
        }
    }

    // TODO: 
    public saveAll() {
        throw new Error("Not implemented yet");
    }

    // TODO: 
    public publishAll() {
        throw new Error("Not implemented yet");
    }

    // TODO: 
    public refreshAll() {
        throw new Error("Not implemented yet");
    }

    // private changeSubscriber: Rx.IDisposable[];

    // private setSubject(selection: ISelection) {
    //     let old = this.selectionSubject.getValue();
    //     if (old.artifact.id !== selection.artifact.id) {
    //         this.changeSubscriber = selection.artifact.artifactState.observable.subscribeOnNext()
    //     }
    //     this.selectionSubject.onNext(selection);
    // }
    
}
