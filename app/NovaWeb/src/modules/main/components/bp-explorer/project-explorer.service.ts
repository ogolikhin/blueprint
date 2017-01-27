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

export interface IProjectExplorerService {
    projects: ExplorerNodeVM[];
    projectsObservable: Rx.Observable<ExplorerNodeVM[]>;
    selectedId: number;

    add(projectId: number): ng.IPromise<void>;

    remove(projectId: number): void;
    removeAll(): void;

    openProject(project: IInstanceItem | IProjectSearchResult | IItemInfoResult): ng.IPromise<void>;
    openProjectWithDialog(): void;
    openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void>;

    refresh(projectId: number, selectionId?: number, forceOpen?: boolean): ng.IPromise<void>;
}

export class ProjectExplorerService implements IProjectExplorerService {
    private factory: TreeNodeVMFactory;
    private _projects: ExplorerNodeVM[];
    private projectsSubject: Rx.Subject<ExplorerNodeVM[]>;
    private _selectedId: number;

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

    public get selectedId(): number {
        return this._selectedId;
    }

    public set selectedId(val: number) {
        this._selectedId = val;
        this.triggerProjectsUpdate();
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
                        this.triggerProjectsUpdate();
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
            removedProjects[0].unloadChildren();
        }
        this.triggerProjectsUpdate();
    }

    public removeAll() {
        // FIXME: use remove
        _.forEach(this.projects, project => {
            this.metadataService.remove(project.model.projectId);
            project.unloadChildren();
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

    // FIXME
    public openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void> {
        // const artifactToExpand = {} as IArtifact;
        // artifactToExpand.id = artifactIdToExpand;
        // artifactToExpand.projectId = projectId;
        // return this.doRefresh(projectId, artifactToExpand, false);
        return null;
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

    public refresh(projectId: number, selectionId?: number, forceOpen?: boolean): ng.IPromise<void> {
        this.$log.debug("refreshing project: " + projectId);

        const projectNode = this.getProject(projectId);
        if (!projectNode) {
            return this.$q.reject();
        }

        return this.selectionManager.autosave().then(() => {
            let selectedArtifact = {} as IArtifact;
            if (selectionId) {
                selectedArtifact.id = selectionId;
                selectedArtifact.projectId = projectNode.model.id;
            } else {
                selectedArtifact = this.selectionManager.getArtifact();
            }
            this.triggerProjectsUpdate();

            // FIXME: refresh
            // return this.doRefresh(projectNode.model.id, selectedArtifact, forceOpen);
            return null;
        });
    }

    public refreshAll(): ng.IPromise<void> {
        // TODO: implement
        return null;
    }

    private triggerProjectsUpdate() {
        this.projects = this.projects.slice();
    }
}
