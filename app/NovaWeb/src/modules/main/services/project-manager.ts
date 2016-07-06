import "angular";
import {ILocalizationService } from "../../core";
import {IMessageService, Message, MessageType} from "../../shell";
import {IProjectRepository, Models} from "./project-repository";
//import {IArtifactService} from "./artifact-service";
import {tinymceMentionsData} from "../../util/tinymce-mentions.mock.ts";

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
        private _repository: IProjectRepository
//        private artifactService: IArtifactService
    ) {
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
                self.setCurrentArtifact(_project);

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
            //let self = this;
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

        return fields;

    }


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
            let project = this.getProject(artifact.projectId);
            if (!project || !project.meta) {
                throw new Error(this.localization.get("Project_NotFound"));
            }

            let artifactType = project.meta.artifactTypes.filter((it: Models.IItemType) => {
                return it.id === artifact.typeId;
            })[0];


            fields.systemFields = this.createSystemPropertyFileds(artifactType, project.meta);
            fields.customFields = this.createCustomPropertyFileds(artifact, artifactType, project.meta);
            fields.noteFields = this.createNotePropertyFileds(artifactType, project.meta);

            return fields;


        } catch (ex) {
            this.messageService.addError(ex["message"] || this.localization.get("Project_NotFound"));
        }

    }

    private createField(modelName: string, type: Models.IPropertyType): AngularFormly.IFieldConfigurationObject {
        if (!modelName) {
            throw new Error(this.localization.get("Artifact_Details_FieldNameError"));
        }
        if (!type) {
            throw new Error(this.localization.get("ArtifactType_NotFound"));
        }
        let field: AngularFormly.IFieldConfigurationObject = {
            key: modelName,
            templateOptions: {
                label: type.name,
                required: type.isRequired,
                disabled: type.disabled
            },
            data: type,
            expressionProperties: {},
        };


        switch (type.primitiveType) {
            case Models.IPrimitiveType.Text:
                field.type = type.isRichText ? "tinymceInline" : (type.isMultipleAllowed ? "textarea" : "input");
                field.defaultValue = type.stringDefaultValue;
                //field.templateOptions.minlength;
                //field.templateOptions.maxlength;
                break;
            case Models.IPrimitiveType.Date:
                field.type = "input";
                field.templateOptions.type = "date";
                field.defaultValue = type.dateDefaultValue || new Date();
                //field.templateOptions.min = type.minDate;
                //field.templateOptions.max = type.maxDate;
                break;
            case Models.IPrimitiveType.Number:
                field.type = "input";
                field.templateOptions.type = "number";
                field.defaultValue = type.decimalDefaultValue || 0;
                field.templateOptions.min = type.minNumber;
                field.templateOptions.max = type.maxNumber;
                break;
            case Models.IPrimitiveType.Choice:
                field.type = "select";
                field.defaultValue = (type.defaultValidValueIndex || 0).toString();
                if (type.validValues) {
                    field.templateOptions.options = type.validValues.map(function (it, index) {
                        return <AngularFormly.ISelectOption>{ value: index.toString(), name: it };
                    });
                }
                break;
            default:
                return undefined;
        }
        return field;
    }

    private createSystemPropertyFileds(artifactType: Models.IItemType, metaData: Models.IProjectMeta): AngularFormly.IFieldConfigurationObject[] {
        let fields: AngularFormly.IFieldConfigurationObject[] = [];
        let field: AngularFormly.IFieldConfigurationObject;

        fields.push(this.createField("name", <Models.IPropertyType>{
            id: -1,
            name: "Name",
            primitiveType: Models.IPrimitiveType.Text,
            isRequired: true
        }));
        fields.push(field = this.createField("type", <Models.IPropertyType>{
            id: -1,
            name: "Type",
            primitiveType: Models.IPrimitiveType.Choice,
            isRequired: true
        }));

        field.templateOptions.options = metaData.artifactTypes.filter((it: Models.IItemType) => {
            return (artifactType && artifactType.baseType === it.baseType);
        }).map(function (it) {
            return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.name };
        });
        field.expressionProperties = {
            "templateOptions.disabled": "to.options.length < 2",
        };

        fields.push(this.createField("createBy", <Models.IPropertyType>{
            id: -1,
            name: "Created by",
            primitiveType: Models.IPrimitiveType.Text,
            disabled: true
        }));
        fields.push(this.createField("createdOn", <Models.IPropertyType>{
            id: -1,
            name: "Created on",
            primitiveType: Models.IPrimitiveType.Date,
            disabled: true
        }));
        fields.push(this.createField("lastEditBy", <Models.IPropertyType>{
            id: -1,
            name: "Last edited by",
            primitiveType: Models.IPrimitiveType.Text,
            disabled: true
        }));
        fields.push(this.createField("lastEditOn", <Models.IPropertyType>{
            id: -1,
            name: "Last edited on",
            primitiveType: Models.IPrimitiveType.Date,
            disabled: true
        }));

        return fields;

    }

    private createCustomPropertyFileds(model: any, artifactType: Models.IItemType, metaData: Models.IProjectMeta): AngularFormly.IFieldConfigurationObject[] {
        let fields: AngularFormly.IFieldConfigurationObject[] = [];
        let field: AngularFormly.IFieldConfigurationObject;

        if (artifactType) {
            metaData.propertyTypes.map((it: Models.IPropertyType) => {
                if (artifactType.customPropertyTypeIds.indexOf(it.id) >= 0) {
                    field = this.createField(`property_${it.id}`, it);
                    if (field) {
                        fields.push(field);
                    }
                }
            });
        }
        return fields;
    }

    private createNotePropertyFileds(artifactType: Models.IItemType, metaData: Models.IProjectMeta): AngularFormly.IFieldConfigurationObject[] {
        let fields: AngularFormly.IFieldConfigurationObject[] = [];
        fields.push({
            key: "tinymceControl",
            type: "tinymce",
            data: { // using data property
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standard tinymce option
                    plugins: "advlist autolink link image paste lists charmap print noneditable mention",
                    mentions: {
                        source: tinymceMentionsData,
                        delay: 100,
                        items: 5,
                        queryBy: "fullname",
                        insert: function (item) {
                            return `<a class="mceNonEditable" href="mailto:` + item.emailaddress + `" title="ID# ` + item.id + `">` + item.fullname + `</a>`;
                        }
                    }
                }
            },
            templateOptions: {
                label: "TinyMCE control"
            }
        });
        fields.push({
            key: "tinymceInlineControl",
            type: "tinymceInline",
            data: { // using data property
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standard tinymce option
                    inline: true,
                    plugins: "advlist autolink link image paste lists charmap print noneditable mention",
                    mentions: {
                        source: tinymceMentionsData,
                        delay: 100,
                        items: 5,
                        queryBy: "fullname",
                        insert: function (item) {
                            return `<a class="mceNonEditable" href="mailto:` + item.emailaddress + `" title="ID# ` + item.id + `">` + item.fullname + `</a>`;
                        }
                    },
                    fixed_toolbar_container: ".form-tinymce-toolbar"
                }
            },
            templateOptions: {
                label: "TinyMCE Inline control"
            }
        });

        return fields;
    }


}