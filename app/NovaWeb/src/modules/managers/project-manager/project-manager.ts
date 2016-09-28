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
    refresh(data: Models.IProject);
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
        "$q",
        "localization", 
        "messageService", 
        "projectService", 
        "artifactManager", 
        "metadataService",
        "statefulArtifactFactory"
    ];

    constructor(
        private $q: ng.IQService,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private projectService: IProjectService,
        private artifactManager: IArtifactManager,
        private metadataService: IMetaDataService,
        private statefulArtifactFactory: IStatefulArtifactFactory) {
        
        this.subscribers = [];

        const stateSub = this.artifactManager.selection.artifactObservable
            // cannot always skip 1 and rely on the artifact observable having 2 values (initial and new)
            // this is true when navigating to artifact X from artifact X via breadcrumb (loop)
            //.skip(1) // skip the first (initial) value
            .filter((artifact: IStatefulArtifact) => artifact != null && (artifact.artifactState.deleted || artifact.artifactState.misplaced))
            .flatMap((artifact: IStatefulArtifact) => {
                return artifact.getObservable();
            }).subscribeOnNext(this.onDeleteOrMove);
        
    }


    private onDeleteOrMove = (artifact: IStatefulArtifact) => {
        const project = this.getProject(artifact.projectId);
//        this.refresh(project);
    }

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

    public refresh(currentProject: Models.IProject): ng.IPromise<any>{
        let defer = this.$q.defer<any>();
        
        let project: Project;
        if (!currentProject) {
            throw new Error("Project_NotFound");
        }
        project = this.getProject(currentProject.id);
        if (!project) {
            throw new Error("Project_NotFound");
        }
        
        let selectedArtifact = this.artifactManager.selection.getArtifact();
        
        if (selectedArtifact.artifactState.dirty) {
            selectedArtifact.autosave().then(() => {
                this.doRefresh(project, selectedArtifact, defer, currentProject);
            }).catch(() => {
                defer.reject();
            });
        } else {
            this.doRefresh(project, selectedArtifact, defer, currentProject);
        }
        
        return defer.promise; 
    }
    
    private doRefresh(project: Project, selectedArtifact: IStatefulArtifact, defer: any, currentProject: Models.IProject){
        let selectedArtifactNode = this.getArtifactNode(selectedArtifact.id);
        //try with selected artifact
        this.projectService.getProjectTree(project.id, selectedArtifact.id, selectedArtifactNode.open)
        .then((data: Models.IArtifact[]) => {
            this.onGetProjectTree(project, data, selectedArtifact.id);
            defer.resolve();
        }).catch((error: any) => {
            if(error.statusCode === 404 && error.errorCode === 3000){
                //try with selected artifact's parent
                this.projectService.getProjectTree(project.id, selectedArtifact.parentId, selectedArtifactNode.open)
                .then((data: Models.IArtifact[]) => {
                    this.onGetProjectTree(project, data, selectedArtifact.parentId);
                    defer.resolve();
                }).catch((error: any) => {
                    if(error.statusCode === 404 && error.errorCode === 3000){
                        //try it with project
                        this.projectService.getArtifacts(project.id).then((data: Models.IArtifact[]) => {
                            this.onGetProjectTree(project, data, -1);
                            defer.resolve();
                        }).catch((err: any) => {
                            this.onGetProjectTreeError(project, error);
                            defer.reject();
                        });
                    }else{
                        this.onGetProjectTreeError(project, error);
                        defer.reject();
                    }
                });
            }else{
                this.onGetProjectTreeError(project, error);
                defer.reject();
            }
        });

        this.metadataService.remove(currentProject.id);    
        this.metadataService.load(currentProject.id);  
    }
    
    private onGetProjectTree(project: Project, data: Models.IArtifact[], selectedArtifactId?: number){
        this.artifactManager.removeAll(project.id);
        
        project.children = data.map((it: Models.IArtifact) => {
            const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
            this.artifactManager.add(statefulArtifact);
            return new ArtifactNode(statefulArtifact, project);
        });
        project.loaded = true;
        project.open = true;

        this.openChildNodes(project.children, data);

        if(selectedArtifactId > 0){
            this.artifactManager.selection.setArtifact(this.getArtifact(selectedArtifactId));
        }

        //this.projectCollection.onNext(this.projectCollection.getValue());
    }
    
    private onGetProjectTreeError(project: Project, error: any){
        //ignore authentication errors here
        if (error) {
            this.messageService.addError(error["message"] || "Artifact_NotFound");
        } else {
            project.children = [];
            project.loaded = false;
            project.open = false;
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    private openChildNodes(childrenNodes: IArtifactNode[], childrenData: Models.IArtifact[]){
        angular.forEach(childrenNodes, (node) => {
            let childData = childrenData.filter(function (it) {
                return it.id === node.id;
            });
            if (childData[0].hasChildren && childData[0].children){
                node.children = childData[0].children.map((it: Models.IArtifact) => {
                    const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(it);
                    this.artifactManager.add(statefulArtifact);
                    return new ArtifactNode(statefulArtifact,node);
                });
                node.loaded = true;
                node.open = true;
                
                this.openChildNodes(node.children, childData[0].children);
            }
        });
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
                    it.dispose();
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