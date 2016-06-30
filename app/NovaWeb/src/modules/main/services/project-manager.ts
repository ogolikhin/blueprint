import "angular";
import {ILocalizationService } from "../../core";
import {IMessageService, Message, MessageType} from "../../shell";
import {IProjectRepository, Models} from "./project-repository";

export {Models}

export interface IProjectManager {
    // eventManager
    initialize();
    dispose();

    projectCollection: Rx.BehaviorSubject<Models.IProject[]>;
    currentProject: Rx.BehaviorSubject<Models.IProject>;
    currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;
    isProjectSelected: boolean;
    isArtifactSelected: boolean;

    setCurrentProject(project: Models.IProject): void;
    setCurrentArtifact(artifact: Models.IArtifact): void;

    loadProject(project: Models.IProject): void;
    loadArtifact(project: Models.IArtifact): void;
    loadArtifactDetails(artifact: Models.IArtifact): void;
    loadFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;

    closeProject(all?: boolean): void;

    getArtifact(artifactId: number, project?: Models.IArtifact): Models.IArtifact;
    getArtifactPropertyFileds(project: Models.IArtifact): Models.IArtifactDetailFields;
}


export class ProjectManager implements IProjectManager {

    private _projectCollection: Rx.BehaviorSubject<Models.IProject[]>;
    private _currentProject: Rx.BehaviorSubject<Models.IProject>;
    private _currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    static $inject: [string] = ["localization", "messageService", "projectRepository"];
    constructor(
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private _repository: IProjectRepository) {
    }

    public dispose() {
        //clear all Project Manager event subscription
        if (this.projectCollection) {
            this.projectCollection.dispose();
        }
        if (this.currentProject) {
            this.currentProject.dispose();
        }
        if (this.currentArtifact) {
            this.currentArtifact.dispose();
        }
    }

    public initialize = () => {
        //subscribe to event
        this.dispose();
        this._projectCollection = new Rx.BehaviorSubject<Models.IProject[]>([]);
        this._currentProject = new Rx.BehaviorSubject<Models.IProject>(null);
        this._currentArtifact = new Rx.BehaviorSubject<Models.IArtifact>(null);
        
        this.currentArtifact.subscribeOnNext(this.loadArtifactDetails, this);
    }

    public get projectCollection(): Rx.BehaviorSubject<Models.IProject[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<Models.IProject[]>([]));
    }
    public get currentProject(): Rx.BehaviorSubject<Models.IProject> {
        return this._currentProject || (this._currentProject = new Rx.BehaviorSubject<Models.IProject>(null));
    }

    public get currentArtifact(): Rx.BehaviorSubject<Models.IArtifact> {
        return this._currentArtifact || (this._currentArtifact = new Rx.BehaviorSubject<Models.IArtifact>(null));
    }

    public setCurrentProject(project: Models.IProject) {
        if (project) {
            let _currentproject = this.currentProject.getValue();
            if (_currentproject && _currentproject.id === project.id) {
                return;
            }
        }
        this.currentProject.onNext(project);
    }

    public setCurrentArtifact(artifact: Models.IArtifact) {
        if (artifact) {
            let _currentartifact = this.currentArtifact.getValue();
            if (_currentartifact && _currentartifact.id === artifact.id) {
                return;
            } 

            let project = this.getProject(artifact.projectId);
            if (project) {
                this.setCurrentProject(project);
            }
        }
        this.currentArtifact.onNext(artifact);
    }

    public loadProject = (project: Models.IProject) => {
        try {
            if (!project) {
                throw new Error(this.localization.get("Project_NotFound"));
            }
            let self = this;
            var _projectCollection: Models.IProject[] = this.projectCollection.getValue();
            var _project = this.getProject(project.id);

            if (_project) {
                _projectCollection = _projectCollection.filter(function (it) {
                    return it !== _project;
                });
                _projectCollection.unshift(_project);
                self.projectCollection.onNext(_projectCollection);
                self.setCurrentArtifact(project);

            } else {
                this._repository.getArtifacts(project.id)
                    .then((result: Models.IArtifact[]) => {
                        _project = new Models.Project(project, {
                            artifacts: result,
                            loaded: true,
                            open: true
                        });
                        _projectCollection.unshift(_project);
                        self.projectCollection.onNext(_projectCollection);
                        self.loadProjectMeta(_project);
                    }).catch((error: any) => {
                        this.messageService.addError(error["message"] || this.localization.get("Project_NotFound"));
                    });
            } 
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
        }
    }

    public loadArtifact = (artifact: Models.IArtifact) => {
        try {
            if (artifact === null) {
                return;
            }
            if (!artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }

            let self = this;
            let _artifact = this.getArtifact(artifact.id);
            if (!_artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }
            this._repository.getArtifacts(artifact.projectId, artifact.id)
                .then((result: Models.IArtifact[]) => {
                    angular.extend(_artifact, {
                        artifacts: result,
                        hasChildren: true,
                        loaded: true,
                        open: true
                    });
                    self.projectCollection.onNext(self.projectCollection.getValue());
                    self.setCurrentArtifact(_artifact);
                }).catch((error: any) => {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                });
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Artifact_NotFound"));
            this.projectCollection.onNext(this.projectCollection.getValue());
        }
    }

    public loadArtifactDetails = (artifact: Models.IArtifact) => {
        try {
            if (artifact === null) {
                return;
            }
            if (!artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }
            let self = this;
            let _artifact = this.getArtifact(artifact.id);
            if (!_artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }
            this._repository.getArtifactDetails(artifact.projectId, artifact.id)
                .then((result: Models.IArtifactDetails) => {
//                    angular.extend(_artifact, result);
//                    self.setCurrentArtifact(_artifact);
                }).catch((error: any) => {
                    this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
                });
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Artifact_NotFound"));
        }
    }

    private loadProjectMeta = (project: Models.IProject) => {
        try {
            if (!project) {
                throw new Error(this.localization.get("Project_NotFound"));
            }

            let self = this;
            this._repository.getProjectMeta(project.id)
                .then((result: Models.IProjectMeta) => {
                    project.meta = result;
                    self.setCurrentProject(project);
                    self.setCurrentArtifact(project);
                }).catch((error: any) => {
                    this.messageService.addError(error["message"] || this.localization.get("Project_NotFound"));
                });
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
        }
    }

    public closeProject = (all: boolean = false) => {
        try {
            if (!this.currentProject.getValue()) {
                this.messageService.addMessage(new Message(MessageType.Warning, "Not selected projects"));
                return;
            }
            let projectsToRemove: Models.IProject[] = [];
            let _projectCollection = this.projectCollection.getValue().filter(function (it: Models.IProject) {
                let result = true;
                if (all || it.id === this.currentProject.getValue().id) {
                    projectsToRemove.push(it);
                    result = false;
                }
                return result;
            }.bind(this));

            this.projectCollection.onNext(_projectCollection);
            this.setCurrentArtifact(this.projectCollection.getValue()[0] || null);
            this.setCurrentProject(this.projectCollection.getValue()[0] || null);
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
        }

    }

    public loadFolders(id?: number) {
        try {
            return this._repository.getFolders(id);
        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
        }
    }

    public getProject(id: number) {
        let project = this.projectCollection.getValue().filter(function (it) {
            return it.id === id;
        })[0];
        return project;
    }

    public getArtifact(id: number, project?: Models.IArtifact): Models.IArtifact {
        let foundArtifact: Models.IArtifact;
        if (project) {
            if (project.id === id) {
                foundArtifact = project;
            }
            for (let i = 0, it: Models.IArtifact; !foundArtifact && (it = project.artifacts[i++]); ) {
                if (it.id === id) {
                    foundArtifact = it;
                } else if (it.artifacts) {
                    foundArtifact = this.getArtifact(id, it);
                }
            }
        } else {
            for (let i = 0, it: Models.IArtifact; !foundArtifact && (it = this.projectCollection.getValue()[i++]); ) {
                foundArtifact = this.getArtifact(id, it);
            }
        }
        return foundArtifact;
    };

    public get isProjectSelected(): boolean {
        //NOTE: current Project must have a refference if project collection has any items
        return !!this.currentProject.getValue();
    }


    public get isArtifactSelected(): boolean {
        return !!this.currentArtifact.getValue();
    }


    public getArtifactSystemPropertyFileds(itemType: Models.IItemType, metaData: Models.IProjectMeta): AngularFormly.IFieldConfigurationObject[] {
        let fields: AngularFormly.IFieldConfigurationObject[] = [];
        

        fields.push({
            key: "name",
            type: "input",
            templateOptions: {
                label: "Name",
                required: true
            }
        });
        
        if (itemType) {
            fields.push({
                key: "type",
                type: "select",
                defaultValue: itemType.id.toString(),
                templateOptions: {
                    label: "Type",
                    required: true,
                    options: metaData.artifactTypes.filter((it: Models.IItemType) => {
                        return (itemType && itemType.baseType === it.baseType);
                    }).map(function (it) {
                        return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.name };
                    })
                },
                expressionProperties: {
                    "templateOptions.disabled": "to.options.length < 2",
                }
            });
        }
        fields.push({
            key: "createdBy",
            type: "input",
            templateOptions: {
                label: "Created by",
                disabled: true
            }
        });
        fields.push({
            key: "createdOn",
            type: "input",
            templateOptions: {
                type: "date",
                label: "Created on",
                disabled: true,
            }
        });
        fields.push({
            key: "lastEditBy",
            type: "input",
            templateOptions: {
                label: "Last edited by",
                disabled: true
            }
        });
        fields.push({
            key: "lastEditOn",
            type: "input",
            templateOptions: {
                type: "date",
                label: "Last edited on",
                disabled: true
            }
        });
        fields.push({
            key: "tinymceControl",
            type: "tinymce",
            data: { // using data property
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standard tinymce option
                    inline: false,
                    plugins: [
                        "advlist autolink lists link image charmap print preview hr anchor pagebreak",
                        "searchreplace wordcount visualblocks visualchars code fullscreen",
                        "insertdatetime media nonbreaking save table contextmenu directionality",
                        "emoticons template paste textcolor colorpicker textpattern imagetools"
                    ],

                    image_advtab: true,
                    toolbar1: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image",
                    toolbar2: "print preview media | forecolor backcolor emoticons",

                }
            },
            templateOptions: {
                label: "TinyMCE control",
            }
        });

        return fields;

    }


    //private getFieldType(type: Models.IPrimitiveType): string {
    //    switch (type) {
    //        case Models.IPrimitiveType.Choice:
    //            return "select";
    //        default:
    //            return "input"
    //    }
    //}

    public getArtifactPropertyFileds(artifact: Models.IArtifact): Models.IArtifactDetailFields {
        try {
            let fields: Models.IArtifactDetailFields = <Models.IArtifactDetailFields>{
                systemFields: [],
                customFields: [],
                noteFields: []
            };
            if (!artifact) {
                throw new Error(this.localization.get("Artifact_NotFound"));
            }
            let _project = this.getProject(artifact.projectId);
            if (!_project) {
                throw new Error(this.localization.get("Project_NotFound"));
            }
            if (!_project.meta) { 
                return;
            }
            var artifactType = _project.meta.artifactTypes.filter((it: Models.IItemType) => {
                return it.id === artifact.typeId;
            })[0];


            fields.systemFields = this.getArtifactSystemPropertyFileds(artifactType, _project.meta);

            if (artifactType) {


                _project.meta.propertyTypes.map((it: Models.IPropertyType) => {
                    if (artifactType.customPropertyTypeIds.indexOf(it.id) >= 0) {
                        fields.customFields.push({
                            key: it.id,
                            type: "input",
                            templateOptions: {
                                label: it.name,
                                required: it.isRequired,
                            }
                        });
                    }
                    
                });

            }
            fields.noteFields = [
                {
                    key: "description",
                    type: "textarea",
                    templateOptions: {
                        label: "Description",
                    }
                }];
            return fields;


        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
        }

    }
}