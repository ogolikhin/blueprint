import "angular";
import { Helper } from "../../../../shared/";
import { ILocalizationService } from "../../../../core";
import { IBPTreeController, ITreeNode } from "../../../../shared/widgets/bp-tree/bp-tree";
import { IDialogSettings, BaseDialogController, IDialogService } from "../../../../shared/";
import { IProjectManager, Models, IProjectRepository } from "../../../";
import { HttpHandledErrorStatusCodes } from "../../../../shell/error/http-error-interceptor";

export interface IArtifactPickerController {
    propertyMap: any;  
    selectedItem?: any;
    getProjects: any;
}

export class ArtifactPickerController extends BaseDialogController implements IArtifactPickerController {
    public hasCloseButton: boolean = true;
    private _selectedItem: Models.IProject;

    public tree: IBPTreeController;
    public projectId: number;
    public projectView: boolean = false;
    public projectName: string;


    static $inject = ["$scope", "localization", "$uibModalInstance", "projectManager", "projectRepository", "dialogService", "params"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private manager: IProjectManager,
        private projectRepository: IProjectRepository,
        private dialogService: IDialogService,
        params: IDialogSettings
    ) {
        super($uibModalInstance, params);
        dialogService.params.okButton = "OK";
        this.projectId = this.manager.currentProject.getValue().id;
        this.projectName = this.manager.currentProject.getValue().name;
    };

    public propertyMap = {
        id: "id",
        type: "type",
        name: "name",
        hasChildren: "hasChildren"
    };

    //Dialog return value
    public get returnValue(): any {
        return this.selectedItem || null;
    };

    public get selectedItem() {
        return this._selectedItem;
    }

    private setSelectedItem(item: any) {
        this._selectedItem = item;
    }

    private onEnterKeyPressed = (e: any) => {
        var key = e.which || e.keyCode;
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
                var sanitizedName = Helper.escapeHTMLText(params.data.name);

                if (params.data.type === 1) {
                    var cell = params.eGridCell;
                    cell.addEventListener("keydown", this.onEnterKeyPressed);
                }
                return sanitizedName;
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
                     var arr = nodes.filter((node: Models.IItemType) => {
                          return this.filterCollections(node);
                        });                         
                    self.tree.reload(arr, artifactId);
                }, (error) => {
                     if (error.statusCode === HttpHandledErrorStatusCodes.handledUnauthorizedStatus) {
                    this.cancel();
                } 
                });

            return null;
        } else {
            this.projectName = this.localization.get("App_Header_Name");
            let id = (prms && angular.isNumber(prms.id)) ? prms.id : null;
            this.projectRepository.getFolders(id)
                .then((nodes: Models.IProjectNode[]) => {                  
                    self.tree.reload(nodes, id);
                }, (error) => {
                    if (error.statusCode === HttpHandledErrorStatusCodes.handledUnauthorizedStatus) {
                        this.cancel();
                    }
                });

            return null;
        }
    };

    public doSelect = (item: any) => {
        let self = this;
        if (!this.projectView) {
            this.$scope.$applyAsync((s) => {
                self.setSelectedItem(item);
            });
        } else {
            if (item) {
                this.projectId = item.id;
                this.projectRepository.getProject(this.projectId).then(
                    (project: Models.IProject) => {
                        this.projectName = project.name;
                        this.projectRepository.getArtifacts(this.projectId)
                            .then((nodes: Models.IArtifact[]) => {
                                var arr = nodes.filter((node: Models.IItemType) => {
                                return this.filterCollections(node);
                             });   
                                this.projectView = false;
                                self.tree.reload(arr);
                            }, (error) => {

                            });
                    }
                );

            }
        }
    }

    private filterCollections(node: Models.IItemType) {       
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
        this.doLoad(null);
    }
}

