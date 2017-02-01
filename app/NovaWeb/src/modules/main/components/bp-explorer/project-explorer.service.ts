import {ExplorerNodeVM, TreeNodeVMFactory} from "../../models/tree-node-vm-factory";
import {IMetaDataService} from "../../../managers/artifact-manager/metadata/metadata.svc";
import {IInstanceItem, InstanceItemType} from "../../models/admin-store-models";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {ItemTypePredefined} from "../../models/enums";
import {IMessageService} from "../messages/message.svc";
import {IProjectSearchResult} from "../../models/search-service-models";
import {IItemInfoResult} from "../../../commonModule/itemInfo/itemInfo.service";
import {ILoadingOverlayService} from "../../../commonModule/loadingOverlay/loadingOverlay.service";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {OpenProjectController} from "../dialogs/open-project/open-project";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IArtifact} from "../../models/models";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {INavigationService} from "../../../commonModule/navigation/navigation.service";
import {IChangeSet, ChangeTypeEnum} from "../../../managers/artifact-manager/changeset/changeset";

export interface IProjectExplorerService {
    projects: ExplorerNodeVM[];
    projectsChangeObservable: Rx.Observable<IChangeSet>;
    projectsObservable: Rx.Observable<ExplorerNodeVM[]>;
    // selectedId: number;

    setSelectionId(id: number);
    getSelectionId(): number;

    add(projectId: number): ng.IPromise<void>;

    remove(projectId: number): void;
    removeAll(): void;

    openProject(project: IInstanceItem | IProjectSearchResult | IItemInfoResult): ng.IPromise<void>;
    openProjectWithDialog(): void;
    openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void>;

    refresh(projectId: number, selectionId?: number, forceOpen?: boolean);
    refreshAll();

    getProject(id: number): ExplorerNodeVM;
}

export class ProjectExplorerService implements IProjectExplorerService {
    private factory: TreeNodeVMFactory;
    private _projects: ExplorerNodeVM[];
    private projectsChangeSubject: Rx.Subject<IChangeSet>;
    private projectsSubject: Rx.Subject<ExplorerNodeVM[]>;
    private _selectedId: number;

    public projectsChangeObservable: Rx.Observable<IChangeSet>;

    static $inject = [
        "$q",
        "$timeout",
        "$log",
        "localization",
        "navigationService",
        "selectionManager",
        "loadingOverlayService",
        "dialogService",
        "messageService",
        "projectService",
        "metadataService"
    ];

    constructor(private $q: ng.IQService,
                private $timeout: ng.ITimeoutService,
                private $log: ng.ILogService,
                private localization: ILocalizationService,
                private navigationService: INavigationService,
                private selectionManager: ISelectionManager,
                private loadingOverlayService: ILoadingOverlayService,
                private dialogService: IDialogService,
                private messageService: IMessageService,
                private projectService: IProjectService,
                private metadataService: IMetaDataService) {

        this.factory = new TreeNodeVMFactory(projectService);
        this.projectsSubject = new Rx.Subject<ExplorerNodeVM[]>();
        this.projectsChangeSubject = new Rx.Subject<IChangeSet>();
        this.projectsChangeObservable = this.projectsChangeSubject.asObservable();

        this.projects = [];
    }

    public get projects(): ExplorerNodeVM[] {
        return this._projects;
    }

    public set projects(val: ExplorerNodeVM[]) {
        this._projects = val;
        this.projectsSubject.onNext(val);
    }

    public get projectsObservable(): Rx.Observable<ExplorerNodeVM[]> {
        return this.projectsSubject.asObservable();
    }

    // FIXME: remove accessors/mutators
    private get selectedId(): number {
        return this._selectedId;
    }

    private set selectedId(val: number) {
        this._selectedId = val;
    }

    public setSelectionId(id: number) {
        this.selectedId = id;
        const change = {
            type: ChangeTypeEnum.Select
        } as IChangeSet;
        this.projectsChangeSubject.onNext(change);
    }

    public getSelectionId() {
        return this.selectedId;
    }

    public add(projectId: number): ng.IPromise<void> {
        let projectNode: ExplorerNodeVM = this.getProject(projectId);
        if (!projectNode) {
            return this.projectService.getProject(projectId).then((projectInfo: IInstanceItem) => {
                const project = {
                    id: projectInfo.id,
                    name: projectInfo.name,
                    description: projectInfo.description,
                    parentFolderId: undefined,
                    type: InstanceItemType.Project,
                    hasChildren: true,
                    permissions: projectInfo.permissions,
                    projectId: projectId,
                    itemTypeId: ItemTypePredefined.Project,
                    prefix: "PR",
                    itemTypeName: "Project",
                    predefinedType: ItemTypePredefined.Project
                } as IInstanceItem;

                return this.metadataService.get(projectId)
                    .then(() => {
                        projectNode = this.factory.createExplorerNodeVM(project, true);
                        this.selectedId = projectNode.model.id;
                        this.projects.unshift(projectNode);

                        const change = {
                            type: ChangeTypeEnum.Add,
                            value: projectNode
                        } as IChangeSet;
                        this.projectsChangeSubject.onNext(change);

                        this.navigationService.navigateTo({id: projectId});

                    }).catch((err: any) => {
                        if (err) {
                            this.messageService.addError(err);
                        }
                        return this.$q.reject(err);
                    });
            });
        }

        // the project has been loaded already
        return this.$q.resolve();
    }

    public remove(projectId: number) {
        this.metadataService.remove(projectId);
        const removedProjects = _.remove(this.projects, project => project.model.projectId === projectId);
        if (removedProjects.length) {
            if (this.projects.length) {
                this.selectedId = this.projects[0].model.projectId;
            }
            const change = {
                type: ChangeTypeEnum.Delete,
                value: removedProjects[0]
            } as IChangeSet;
            this.projectsChangeSubject.onNext(change);
        }
    }

    public removeAll() {
        // FIXME: use remove
        _.forEach(this.projects, project => {
            this.metadataService.remove(project.model.id);
            const change = {
                type: ChangeTypeEnum.Delete,
                value: project
            } as IChangeSet;
            this.projectsChangeSubject.onNext(change);
        });

        this.projects = [];
    }

    // opens and selects project
    public openProject(project: IInstanceItem | IProjectSearchResult | IItemInfoResult): ng.IPromise<void> {
        let projectId: number;
        if (project.hasOwnProperty("id")) {
            projectId = (project as {id: number}).id;
        } else if (project.hasOwnProperty("itemId")) {
            projectId = (project as {itemId: number}).itemId;
        } else {
            throw new Error("project does not have id or itemId");
        }

        const openProjectLoadingId = this.loadingOverlayService.beginLoading();
        return this.add(projectId).finally(() => {
            this.loadingOverlayService.endLoading(openProjectLoadingId);
        });
    }

    private createProjectNode(projectId: number): ng.IPromise<ExplorerNodeVM> {
        return this.projectService.getProject(projectId).then((projectInfo: IInstanceItem) => {
            const project = {
                id: projectInfo.id,
                name: projectInfo.name,
                description: projectInfo.description,
                parentFolderId: undefined,
                type: InstanceItemType.Project,
                hasChildren: true,
                permissions: projectInfo.permissions,
                projectId: projectId,
                itemTypeId: ItemTypePredefined.Project,
                prefix: "PR",
                itemTypeName: "Project",
                predefinedType: ItemTypePredefined.Project
            };

            return this.factory.createExplorerNodeVM(project, true);
        });
    }

    // FIXME
    public openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void> {
        const openProjectIndex = _.findIndex(this.projects, project => project.model.id === projectId);

        return this.metadataService.get(projectId)
            .then(() => this.getProjectExpandedToNode(projectId, artifactIdToExpand))
            .then(project => {
                this.selectedId = artifactIdToExpand;

                // replace exising project
                if (_.isUndefined(openProjectIndex)) {
                    this.projects.unshift(project);
                } else {
                    this.projects.splice(openProjectIndex, 1, project);
                }

                const change = {
                    type: ChangeTypeEnum.Add,
                    value: project
                } as IChangeSet;
                this.projectsChangeSubject.onNext(change);
            });
    }

    private getProjectExpandedToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<ExplorerNodeVM> {
        return this.projectService.getProjectTree(projectId, artifactIdToExpand, true).then((artifacts: IArtifact[]) => {
            return this.createProjectNode(projectId)
                .then((project: ExplorerNodeVM) => {
                    //populate it
                    project.children = artifacts.map((artifact: IArtifact) => this.factory.createExplorerNodeVM(artifact));

                    //open any children that have children
                    this.openChildNodes(project.children, artifacts);

                    return project;
                });
        });
    }

    private openChildNodes(childrenNodes: ExplorerNodeVM[], childrenData: IArtifact[]) {
        _.forEach(childrenNodes, (node: ExplorerNodeVM) => {
            const childData = childrenData.filter(artifact => artifact.id === node.model.id);

            //if it has children - expand the node
            if (childData[0].hasChildren && childData[0].children) {
                node.children = childData[0].children.map(artifact => this.factory.createExplorerNodeVM(artifact));
                node.expanded = true;
                this.openChildNodes(node.children, childData[0].children);
            }
        });
    }

    public openProjectWithDialog(): void {
        this.dialogService.open(<IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../dialogs/open-project/open-project.html"),
            controller: OpenProjectController,
            css: "nova-open-project"
        }).then((project: IInstanceItem | IProjectSearchResult) => {
            this.openProject(project);
        });
    }

    public getProject(id: number): ExplorerNodeVM {
        return _.find(this.projects, project => project.model.id === id);
    }

    public refresh(projectId: number, selectionId?: number, forceOpen?: boolean) {
        this.$log.debug("refreshing project: " + projectId);

        const projectNode = this.getProject(projectId);
        if (!projectNode) {
            return;
        }

        const selectedArtifact = this.selectionManager.getArtifact();
        if (selectedArtifact && selectedArtifact.projectId === projectId) {
            this.selectionManager.autosave().then(() => {
                this.remove(projectId);
                this.openProjectAndExpandToNode(projectId, selectedArtifact.id);
            });

        } else {
            const change = {
                type: ChangeTypeEnum.Refresh,
                value: projectNode
            } as IChangeSet;
            this.projectsChangeSubject.onNext(change);
        }
    }

    public refreshAll() {
        return this.selectionManager.autosave().then(() => {
           this.refreshCurrentArtifact();
            _.forEach(this.projects, project => {
                this.refresh(project.model.id);
            });
        });
    }

    private refreshCurrentArtifact() {
        const selectedArtifact = this.selectionManager.getArtifact();
        if (selectedArtifact) {
            selectedArtifact.refresh();
        }
    }

}
