import { IMessageService } from "../../core";
import { ISelectionManager } from "../selection-manager/selection-manager";
import { IMetaDataService } from "./metadata";
import { IStatefulArtifactFactory, } from "./artifact";
import { IStatefulArtifact, IDispose } from "../models";

export interface IArtifactManager extends IDispose {
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
    private artifactList: IStatefulArtifact[];
    public static $inject = [ 
        "$q", 
        "messageService",
        "selectionManager", 
        "metadataService", 
        "statefulArtifactFactory" ];
        
    constructor(
        private $q: ng.IQService, 
        private messageService: IMessageService, 
        private selectionService: ISelectionManager, 
        private metadataService: IMetaDataService,
        private artifactFactory: IStatefulArtifactFactory) {
        this.artifactList = [];
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
        return this.artifactList;
    }

    public get(id: number): ng.IPromise<IStatefulArtifact> {
        let deferred = this.$q.defer<IStatefulArtifact>();
        let artifact = this.artifactList.filter((it: IStatefulArtifact) => it.id === id)[0];
        if (artifact) {
            deferred.resolve(artifact);
        } else {
            this.artifactFactory.createStatefulArtifact({id: id}).load().then((it: IStatefulArtifact) => {
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
        this.artifactList.push(artifact);
    }

    public remove(id: number): IStatefulArtifact {
        let stateArtifact: IStatefulArtifact;
        this.artifactList = this.artifactList.filter((artifact: IStatefulArtifact) => {
            if (artifact.id === id) {
                artifact.dispose();
                stateArtifact = artifact;
                return false;
            }
            return true;
        });
        return stateArtifact;
    }

    public removeAll(projectId?: number) {
        
        this.artifactList = this.artifactList.filter((it: IStatefulArtifact) => {
            if (projectId || it.projectId === projectId) {
                it.dispose();
                this.metadataService.remove(it.projectId);
                return false;
            }
            return true;
        });
        
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
