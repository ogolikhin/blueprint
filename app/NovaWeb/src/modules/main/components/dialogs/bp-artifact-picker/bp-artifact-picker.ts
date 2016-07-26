import "angular";
import { Helper } from "../../../../core/utils/helper";
import { ILocalizationService } from "../../../../core";
import { IBPTreeController, ITreeNode } from "../../../../core/widgets/bp-tree/bp-tree";
import { IDialogSettings, BaseDialogController, IDialogService } from "../../../../core/services/dialog";
import { IProjectManager, Models, ProjectRepository, IProjectRepository } from "../../../";


export interface IArtifactPickerController {
    propertyMap: any;
    errorMessage: string;
    hasError: boolean;
    isItemSelected: boolean;
    selectedItem?: any;
    setHeader: Function;

}

export class ArtifactPickerController extends BaseDialogController implements IArtifactPickerController {
    public hasCloseButton: boolean = true;
    private _selectedItem: Models.IProject;
    private _errorMessage: string;
    private tree: IBPTreeController;
    public projectId: number;

    static $inject = ["$scope", "localization", "$uibModalInstance", "projectManager", "projectRepository", "dialogService", "params", "$sce"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private manager: IProjectManager,
        private projectRepository: IProjectRepository,
        private dialogService: IDialogService,
        params: IDialogSettings,
        private $sce: ng.ISCEService
    ) {
        super($uibModalInstance, params);
        this.projectId = this.manager.currentProject.getValue().id;
      
    };


    public setHeader(params): any {
       
        ////var eCell = document.createElement('span');
        ////eCell.innerHTML =
        ////    '<div style="text-align: left;">' +
        ////    '  <div id="agResizeBar" style="width: 4px; height: 100%; float: right; cursor: col-resize;"></div>' +
        ////    '  <div style="padding: 4px; overflow: hidden; text-overflow: ellipsis;">' +
        ////    '    <span id="agMenu"><i class="fa fa-bars"></i></span>' +        
        ////    '    <span id="agHeaderCellLabel">' +
        ////    '      <span id="agText"></span>' +
        ////    '      <span id="agSortAsc"><i class="fa fa-long-arrow-down"></i></span>' +
        ////    '      <span id="agSortDesc"><i class="fa fa-long-arrow-up"></i></span>' +
        ////    '      <span id="agNoSort"></span>' +
        ////    '      <span id="agFilter"><i class="fa fa-filter"></i></span>' +
        ////    '    </span>' +
        ////    '    <span id="myCalendarIcon"><i class="fa fa-calendar">>></i></span>' + params.value
        ////'  </div>' +
        ////    '</div>';
    
        ////return eCell;


         //return params.value;
       
        return "custom header";
      
    }

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

    public get hasError(): boolean {
        return Boolean(this._errorMessage);
    }
    public get errorMessage(): string {
        return this._errorMessage;
    }

    public get isItemSelected(): boolean {
        return this.returnValue;
    }

    public get selectedItem() {
        return this._selectedItem;
    }

    private setSelectedItem(item: any) {
        this._selectedItem = item;
    }

    private onEnterKeyPressed = (e: any) => {
        var key = e.which || e.keyCode;
        if (key === 13) {
            //user pressed Enter key on project
            this.ok();
        }
    };

    public columns = [{
        headerName: this.localization.get("App_Header_Name"),
        field: "name",
        cellClassRules: {
            "has-children": function (params) { return params.data.hasChildren; },
            "is-folder": function (params) { return params.data.type === 0; },
            "is-project": function (params) { return params.data.type === 1; }
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
        //check passed in parameter
        let self = this;
        let artifactId = null;
        if (prms) {
           
            artifactId = prms.id;
        }
        this.projectRepository.getArtifacts(this.projectId, artifactId)
            .then((nodes: Models.IArtifact[]) => {
                self.tree.reload(nodes, artifactId);
                //if (self.tree.isEmpty) {
                //    this._errorMessage = this.localization.get("Project_NoProjectsAvailable");
                //}
            }, (error) => {
                this._errorMessage = error.message;
            });
        
        return null;
    };

    public doSelect = (item: any) => {
        let self = this;
        this.$scope.$applyAsync((s) => {
            self.setSelectedItem(item);
        });

      //  this.manager.setCurrentArtifact(this.doSync(item));
    }

    public doSync = (node: ITreeNode): Models.IArtifact => {
        let artifact = this.manager.getArtifact(node.id);
        if (artifact.hasChildren) {
            angular.extend(artifact, {
                loaded: node.loaded,
                open: node.open
            });
        };
        return artifact;
    };

}
