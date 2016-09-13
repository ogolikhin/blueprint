import "angular";
import { Helper } from "../../../../shared/";
import { ILocalizationService } from "../../../../core";
import { IBPTreeController, ITreeNode } from "../../../../shared/widgets/bp-tree/bp-tree";
import { IDialogSettings, BaseDialogController, IDialogService } from "../../../../shared/";
import { IProjectManager, Models, IProjectRepository, ISelectionManager } from "../../../";


export interface IArtifactPickerController {
    propertyMap: any;  
    selectedItem?: any;
    getProjects: any;
}

export interface IArtifactPickerFilter {
    ItemTypePredefines: Models.ItemTypePredefined[];
}

export class ArtifactPickerController extends BaseDialogController implements IArtifactPickerController {
    public hasCloseButton: boolean = true;
    private _selectedItem: Models.IArtifact;

    public tree: IBPTreeController;
    public projectId: number;
    public projectView: boolean = false;
    public projectName: string;


    static $inject = [
        "$scope", 
        "localization", 
        "$uibModalInstance", 
        "projectManager", 
        "selectionManager", 
        "projectRepository", 
        "dialogService", 
        "dialogSettings",
        "dialogData"];
        
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private manager: IProjectManager,
        private selectionManager: ISelectionManager,
        private projectRepository: IProjectRepository,
        private dialogService: IDialogService,
        dialogSettings: IDialogSettings,
        private dialogData: IArtifactPickerFilter
    ) {
        super($uibModalInstance, dialogSettings);
        dialogService.dialogSettings.okButton = "OK";
        let _project = this.manager.getProject(this.selectionManager.selection.artifact.projectId);
        if (_project) {
            this.projectId = _project.id;
            this.projectName = _project.name;
        }
        $scope.$on("$destroy", () => {
            this.columns[0].cellClass = null;
            this.columns[0].cellRendererParams = null;
            this.columns = null;
            this.doSelect = null;
            this.doSync = null;
            this.doLoad = null;
        });
    };

    public propertyMap = {
        id: "id",
        itemTypeId: "itemTypeId",
        type: "type",
        name: "name",
        hasChildren: "hasChildren"
    };

    //Dialog return value
    public get returnValue(): any {
        return this.selectedItem || null;
    };

    public get selectedItem(): Models.IArtifact {
        return this._selectedItem;
    }

    private setSelectedItem(item: Models.IArtifact) {
        this._selectedItem = this.isItemSelectable(item) ? item : undefined;
    }

    private isItemSelectable(item: Models.IArtifact): boolean {
        if (this.projectView) {
            return false;
        }

        if (this.dialogData && this.dialogData.ItemTypePredefines && this.dialogData.ItemTypePredefines.length > 0) {
            return this.dialogData.ItemTypePredefines.indexOf(item.predefinedType) >= 0;
        } else {
            return true;
        }
    }

    private onEnterKeyPressed = (e: any) => {
        const key = e.which || e.keyCode;
        if (key === 13) {
            this.ok();
        }
    };

    public columns = [{
        headerName: "",
        field: "name",
        cellClass: function (params) {
            let css: string[] = [];

            if (params.data.hasChildren) {
                css.push("has-children");
            }

            if (params.data.predefinedType) {
                if (params.data.predefinedType === Models.ItemTypePredefined.PrimitiveFolder) {
                    css.push("is-folder");
                } else if (params.data.predefinedType === Models.ItemTypePredefined.Project) {
                    css.push("is-project");
                } else {               
                    css.push("is-" + Helper.toDashCase(Models.ItemTypePredefined[params.data.predefinedType]));
                }
            } else {
               if (params.data.type === 0) {
                   css.push("is-folder");
               } else if (params.data.type === 1) {
                   css.push("is-project");
               }
            }

            return css;
        },
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                let icon = "<i></i>";
                let name = Helper.escapeHTMLText(params.data.name);

                if (!this.projectView) {
                    if (params.data.itemTypeId === 1) {
                        const cell = params.eGridCell;
                        cell.addEventListener("keydown", this.onEnterKeyPressed);
                    }

                    //TODO: for now it display custom icons just for already loaded projects
                    let projectID = params.data.projectId;
                    let loadedProjects = this.manager.projectCollection.getValue();
                    let isProjectLoaded = loadedProjects.map((p) => { return p.id; }).indexOf(projectID);
                    if (projectID && isProjectLoaded !== -1) {
                        let artifactType = this.manager.getArtifactType(params.data as Models.IArtifact);
                        if (artifactType && artifactType.iconImageId && angular.isNumber(artifactType.iconImageId)) {
                            icon = `<bp-item-type-icon
                                item-type-id="${artifactType.id}"
                                item-type-icon="${artifactType.iconImageId}"></bp-item-type-icon>`;
                        }
                    }
                }
                return `${icon}<span>${name}</span>`;
            },
            padding: 20
        },
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering: true
    }];

    public doLoad = (prms: any): any[] => {
        let self = this;
        if (!this.projectView) {
            let artifactId = null;
            if (prms) {
                artifactId = prms.id;
            }
            this.projectRepository.getArtifacts(this.projectId, artifactId)
                .then((nodes: Models.IArtifact[]) => {
                    const filtered = nodes.filter(this.filterCollections);
                    self.tree.reload(filtered, artifactId);
                }, (error) => {
                });
            return null;
        } else {
            this.projectName = this.localization.get("App_Header_Name");
            let id = (prms && angular.isNumber(prms.id)) ? prms.id : null;
            this.projectRepository.getFolders(id)
                .then((nodes: Models.IProjectNode[]) => {                  
                    self.tree.reload(nodes, id);
                }, (error) => {
                });

            return null;
        }
    };

    public doSelect = (item: Models.IProjectNode | Models.IItem | any) => {
        let self = this;
        if (!this.projectView) {
            this.$scope.$applyAsync((s) => {
                self.setSelectedItem(item);
            });
        } else {
            if (item && item.type === Models.ProjectNodeType.Project) {
                this.projectId = item.id;
                this.projectRepository.getProject(this.projectId).then(
                    (project: Models.IProject) => {
                        this.projectName = project.name;
                        this.projectRepository.getArtifacts(this.projectId)
                            .then((nodes: Models.IArtifact[]) => {
                                const filtered = nodes.filter(this.filterCollections);
                                this.projectView = false;
                                self.tree.reload(filtered);
                            }, (error) => {

                            });
                    }
                );
            }
        }
    }

    private filterCollections(node: Models.IItem) {
        if (node.predefinedType !== Models.ItemTypePredefined.CollectionFolder) {
            return true;
        }
    }

    public doSync = (node: ITreeNode): Models.IArtifact => {
        if (!this.projectView) {
            let artifact = this.manager.getArtifact(node.id);
            if (artifact && artifact.hasChildren) {
                angular.extend(artifact, {
                    loaded: node.loaded,
                    open: node.open
                });
            };
            return artifact;
        }
    };

    public getProjects() {
        this.projectView = true;
        this._selectedItem = undefined;
        this.doLoad(null);
    }
}
