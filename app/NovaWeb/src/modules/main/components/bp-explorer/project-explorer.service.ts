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
import {IArtifact, IArtifactWithProject} from "../../models/models";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {INavigationService} from "../../../commonModule/navigation/navigation.service";
import {IChangeSet, ChangeTypeEnum} from "../../../managers/artifact-manager/changeset/changeset";
import {MoveCopyArtifactInsertMethod} from "../dialogs/move-copy-artifact/move-copy-artifact";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";

export interface IProjectExplorerService {
    projects: ExplorerNodeVM[];
    projectsChangeObservable: Rx.Observable<IChangeSet>;

    setSelectionId(id: number);
    getSelectionId(): number;

    add(projectId: number): ng.IPromise<void>;

    remove(projectId: number);
    removeAll();

    openProject(project: IInstanceItem | IProjectSearchResult | IItemInfoResult): ng.IPromise<void>;
    openProjectWithDialog(): void;
    openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void>;

    refresh(projectId: number, expandToArtifact?: IArtifact);
    refreshAll();

    getProject(id: number): ExplorerNodeVM;

    // misc
    getDescendantsToBeDeleted(artifact: IArtifact): ng.IPromise<IArtifactWithProject[]>;
    calculateOrderIndex(insertMethod: MoveCopyArtifactInsertMethod, selectedArtifact: IArtifact): ng.IPromise<number>;
}

export class ProjectExplorerService implements IProjectExplorerService {
    private factory: TreeNodeVMFactory;
    private projectsChangeSubject: Rx.Subject<IChangeSet>;
    private _selectedId: number;

    public projects: ExplorerNodeVM[];
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

        this.projects = [];
        this.factory = new TreeNodeVMFactory(projectService);
        this.projectsChangeSubject = new Rx.Subject<IChangeSet>();
        this.projectsChangeObservable = this.projectsChangeSubject.asObservable();

        this.selectionManager.currentlySelectedArtifactObservable
            .subscribeOnNext(this.onChangeInCurrentlySelectedArtifact, this);
    }

    private onChangeInCurrentlySelectedArtifact(artifact: IStatefulArtifact) {
        if (artifact.artifactState.misplaced) {
            this.refresh(artifact.projectId, artifact);
        }
    }

    public setSelectionId(id: number) {
        this._selectedId = id;
        const change = {
            type: ChangeTypeEnum.Select
        } as IChangeSet;
        this.projectsChangeSubject.onNext(change);
    }

    public getSelectionId() {
        return this._selectedId;
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
                        this._selectedId = projectNode.model.id;
                        this.projects.unshift(projectNode);

                        // FIXME: maybe return a promise here
                        projectNode.loadChildrenAsync().then((children: ExplorerNodeVM[]) => {
                            projectNode.children = children;
                            this.notifyProjectsUpdate();
                            this.navigationService.navigateTo({id: projectId});
                        });

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

    private notifyProjectsUpdate() {
        const change = {
            type: ChangeTypeEnum.Update,
            value: this.projects
        } as IChangeSet;
        this.projectsChangeSubject.onNext(change);
    }

    public remove(projectId: number) {
        this.metadataService.remove(projectId);
        const removedProjects = _.remove(this.projects, project => project.model.projectId === projectId);
        if (removedProjects.length) {
            if (this.projects.length) {
                this._selectedId = this.projects[0].model.projectId;
            }

            removedProjects[0].unloadChildren();
            this.notifyProjectsUpdate();
        }
    }

    public removeAll() {
        _.forEachRight(this.projects, project => {
            this.remove(project.model.id);
        });
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

    // FIXME: add project error checking
    public openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void> {
        const openProjectIndex = _.findIndex(this.projects, project => project.model.id === projectId);

        return this.metadataService.get(projectId)
            .then(() => this.getProjectExpandedToNode(projectId, artifactIdToExpand))
            .then(project => {
                this._selectedId = artifactIdToExpand;

                // replace exising project
                if (_.isUndefined(openProjectIndex)) {
                    this.projects.unshift(project);
                } else {
                    this.projects.splice(openProjectIndex, 1, project);
                }

                this.notifyProjectsUpdate();
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

    public refresh(projectId: number, expandToArtifact?: IArtifact) {
        this.$log.debug("refreshing project: " + projectId);

        const projectNode = this.getProject(projectId);
        if (!projectNode) {
            return;
        }

        this.selectionManager.autosave().then(() => {
            if (expandToArtifact && expandToArtifact.projectId === projectId) {
                this.getProjectExpandedToNode(projectId, expandToArtifact.id)
                    .then(project => {
                        const index = _.findIndex(this.projects, p => p.model.id === p.model.id);
                        this.projects[index].unloadChildren();
                        this.projects.splice(index, 1, project);

                        project.loadChildrenAsync().then((children: ExplorerNodeVM[]) => {
                            this.notifyProjectsUpdate();
                        });
                    });
            } else {
                projectNode.unloadChildren();
                projectNode.loadChildrenAsync().then((children: ExplorerNodeVM[]) => {
                    projectNode.children = children;
                    this.notifyProjectsUpdate();
                });
            }
        });
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

    private getArtifactNode(id: number): ExplorerNodeVM {
        let found: ExplorerNodeVM;
        _.find(this.projects, project => {
            found = project.getNode(model => model.id === id);
            return found;
        });
        return found;
    };

    // FIXME: clean up method
    public calculateOrderIndex(insertMethod: MoveCopyArtifactInsertMethod, selectedArtifact: IArtifact): ng.IPromise<number> {
        let promise: ng.IPromise<void>;
        let orderIndex: number;
        let index: number;

        let siblings: IArtifact[];
        const parentArtifactNode: ExplorerNodeVM = this.getArtifactNode(selectedArtifact.parentId);

        //if parent isn't found, or if its children aren't loaded
        if (!parentArtifactNode || (!parentArtifactNode.children || parentArtifactNode.children.length === 0)) {
            //get children from server
            promise = this.projectService.getArtifacts(selectedArtifact.projectId, selectedArtifact.parentId)
                .then((data: IArtifact[]) => {
                    siblings = data;
                });
        } else {
            //otherwise, get children from cache
            siblings = _.map(parentArtifactNode.children, (node) => node.model);
            promise = this.$q.resolve();
        }

        return promise.then(() => {
            //sort by order index
            siblings = _.sortBy(siblings, (a) => a.orderIndex);
            index = _.findIndex(siblings, (a) => a.id === selectedArtifact.id);

            //compute new order index
            if (index === 0 && insertMethod === MoveCopyArtifactInsertMethod.Above) { //first
                orderIndex = selectedArtifact.orderIndex / 2;
            } else if (index === siblings.length - 1 && insertMethod === MoveCopyArtifactInsertMethod.Below) { //last
                orderIndex = selectedArtifact.orderIndex + 10;
            } else {    //in between
                if (insertMethod === MoveCopyArtifactInsertMethod.Above) {
                    orderIndex = (siblings[index - 1].orderIndex + selectedArtifact.orderIndex) / 2;
                } else if (insertMethod === MoveCopyArtifactInsertMethod.Below) {
                    orderIndex = (siblings[index + 1].orderIndex + selectedArtifact.orderIndex) / 2;
                }
            }
            return orderIndex;
        });
    }

    public getDescendantsToBeDeleted(artifact: IArtifact): ng.IPromise<IArtifactWithProject[]> {
        let projectName: string;
        return this.projectService.getProject(artifact.projectId).then((project: IInstanceItem) => {
            projectName = project.name;
            return this.projectService.getArtifacts(project.id, artifact.id);
        }).then((data: IArtifact[]) => {
            return data.map(a => _.assign({projectName: projectName}, a) as IArtifactWithProject);
        });
    }
}
