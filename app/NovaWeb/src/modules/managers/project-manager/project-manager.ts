import * as angular from "angular";
import { ILocalizationService, IMessageService } from "../../core";
import { IStatefulArtifactFactory } from "../artifact-manager/artifact";
import { Project, ArtifactNode } from "./project";
import { IArtifactNode, IStatefulArtifact, IDispose} from "../models";
//import { StatefulArtifact } from "../artifact-manager/artifact";

import { Models, Enums } from "../../main/models";
import { IProjectService } from "./project-service";
import { SelectionSource } from "../selection-manager";

import { IArtifactManager } from "../../managers";
import { IMetaDataService } from "../artifact-manager/metadata";
export interface IProjectManager extends IDispose {
    projectCollection: Rx.BehaviorSubject<Project[]>;

    // eventManager
    initialize();
    add(data: Models.IProject);
    remove(all?: boolean): void;
    loadArtifact(id: number): void;
    loadFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;
    getProject(id: number);
    getArtifactNode(id: number): IArtifactNode;
    getArtifact(id: number): IStatefulArtifact;
    getSelectedProject(): Project; 

}


export class ProjectManager  implements IProjectManager { 

    private _projectCollection: Rx.BehaviorSubject<Project[]>;
    private subscribers: Rx.IDisposable[];
    private statechangesubscriber: Rx.IDisposable;
    static $inject: [string] = [
        "localization", 
        "messageService", 
        "projectService", 
        "artifactManager", 
        "metadataService",
        "statefulArtifactFactory"
    ];

    constructor(
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private projectService: IProjectService,
        private artifactManager: IArtifactManager,
        private metadataService: IMetaDataService,
        private statefulArtifactFactory: IStatefulArtifactFactory) {
        
        this.subscribers = [];
    }

    private onChange(artifact: IStatefulArtifact) {
        this.projectCollection.onNext(this._projectCollection.getValue());
    }

    // private onArtifactSelect(artifact: IStatefulArtifact) {
    //     if (this.statechangesubscriber) {
    //         this.statechangesubscriber.dispose();
    //         delete this.statechangesubscriber;
    //     }
    //     if (artifact) {
    //         this.statechangesubscriber = artifact.observable().subscribeOnNext(this.onChange, this);
    //     }
    // }
    
    private onChangeInArtifactManagerCollection(artifact: IStatefulArtifact){
         //Projects will null parentId have been removed from ArtifactManager
         if (artifact.parentId === null) {
             this.removeArtifact(artifact);
         }
     }

     private disposeSubscribers() {
         this.subscribers.forEach((s) => s.dispose());
         this.subscribers = [];
     }

    public dispose() {
        this.remove(true);
        this.disposeSubscribers();

        if (this._projectCollection) {
            this._projectCollection.dispose();
            delete this._projectCollection ;
        }
    }

    public initialize() {
        this.disposeSubscribers();

        if (this._projectCollection) {
            this._projectCollection.dispose();
            delete this._projectCollection ;
        }

        this.subscribers.push(this.artifactManager.collectionChangeObservable.subscribeOnNext(this.onChangeInArtifactManagerCollection, this));
    }

    public get projectCollection(): Rx.BehaviorSubject<Project[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<Project[]>([]));
    }

    public add(data: Models.IProject) {
        let project: Project;
        try {    
            if (!data) {
                throw new Error("Project_NotFound");
            }
            project = this.getProject(data.id);
            if (!project) {
                this.metadataService.load(data.id).then(() => {
                    angular.extend(data, {
                        projectId: data.id,
                        itemTypeId: Enums.ItemTypePredefined.Project,
                        prefix: "PR",
                        permissions: 4095,
                        predefinedType: Enums.ItemTypePredefined.Project,
                        hasChildren: true
                    });

                    const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(data);
                    this.artifactManager.add(statefulArtifact);
                    project = new Project(statefulArtifact);
                    this.projectCollection.getValue().unshift(project);
                    this.loadArtifact(project.id);

                });                
            }

        } catch (ex) {
            this.messageService.addError(ex);
            throw ex;
        }
    }

    public removeArtifact(artifact: IStatefulArtifact) {
         let node: IArtifactNode = this.getArtifactNode(artifact.id);
         node.parentNode.children = node.parentNode.children.filter((child) => child.id !== artifact.id);
 
         this.projectCollection.onNext(this.projectCollection.getValue());
     }

    public remove(all: boolean = false) {
        try {
            let projectId: number = 0;
            if (!all) {
                let artifact = this.artifactManager.selection.getArtifact();
                if (artifact) {
                    projectId = artifact.projectId;    
                } 
            }
            let _projectCollection = this.projectCollection.getValue().filter((it: Project) => {
                let result = true;
                if (all || it.id === projectId) {
                    this.artifactManager.removeAll(it.projectId);
                    result = false;
                }
                return result;
            });

            this.projectCollection.onNext(_projectCollection);
        } catch (ex) {
            this.messageService.addError(ex);
            throw ex;
        }
 
    }

    public loadArtifact(id: number) {
        let node: IArtifactNode;

        try {
            node = this.getArtifactNode(id);
            if (!node) {
                throw new Error("Artifact_NotFound");
            }

            this.projectService.getArtifacts(node.projectId, node.artifact.id)
                .then((data: Models.IArtifact[]) => {
                    node.children = data.map((it: Models.IArtifact) => {
                        const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                        this.artifactManager.add(statefulArtifact);
                        return new ArtifactNode(statefulArtifact, node);
                    });
                    node.loaded = true;
                    node.open = true;

                    this.projectCollection.onNext(this.projectCollection.getValue());
//                    this.artifactManager.selection.setArtifact(node.artifact, SelectionSource.Explorer);

                }).catch((error: any) => {
                    //ignore authentication errors here
                    if (error) {
                        this.messageService.addError(error["message"] || "Artifact_NotFound");
                    } else {
                        node.children = [];
                        node.loaded = false;
                        node.open = false;
                        //node.hasChildren = false;                        
                        this.projectCollection.onNext(this.projectCollection.getValue());
                    }
                });

        } catch (ex) {
            this.messageService.addError(ex["message"] || "Artifact_NotFound");
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    // public updateArtifactName(artifact: Models.IArtifact) {
    //     let project = this.projectCollection.getValue().filter(function(it) {
    //         return it.id === artifact.projectId;
    //     })[0];
    //     if (project) {
    //         let art = project.artifacts.filter(function(it) {
    //             return it.id === artifact.id;
    //         })[0];
    //         if (art) {
    //             art.name = artifact.name;
    //         }
    //         this.projectCollection.onNext(this.projectCollection.getValue());
    //     }
    // }


    public loadFolders(id?: number): ng.IPromise<Models.IProjectNode[]> {
        return this.projectService.getFolders(id);
    }

    public getProject(id: number): Project {
        let project = this.projectCollection.getValue().filter(function (it) {
            return it.id === id;
        })[0];
        return project;
    }

    public getSelectedProject(): Project {
        let artifact = this.artifactManager.selection.getArtifact();
        let project = this.getProject(artifact.projectId);
        return project;
    }


    public getArtifactNode(id: number): IArtifactNode {
        let found: IArtifactNode;
        let projects  = this.projectCollection.getValue();
        for (let i = 0, it: Project; !found && (it = projects[i++]); ) {
            found = it.getNode(id);
        }
        return found;
    };

    public getArtifact(id: number): IStatefulArtifact {
        let found = this.getArtifactNode(id);
        return found ? found.artifact : null;
    };


    public getSubArtifact(artifact: number | Models.IArtifact, subArtifactId: number): Models.ISubArtifact {
        let foundArtifact: Models.ISubArtifact;
        //TODO: Needs to be implemented


        return foundArtifact;
    };

}