import "angular";
import {Helper} from "../../../core/utils/helper";
import {ILocalizationService} from "../../../core/localization";
import {IBPTreeController} from "../../../core/widgets/bp-tree/bp-tree";
import {IDialogSettings, BaseDialogController, IDialogService} from "../../../services/dialog.svc";
import {IProjectManager, Models} from "../../managers/project-manager";

export interface IOpenProjectResult {
    id: number;
    name: string;
    description: string;
}

export class OpenProjectController extends BaseDialogController {
    public hasCloseButton: boolean = true;
    private selectedItem: any;
    private tree: IBPTreeController;

    static $inject = ["$scope", "localization", "$uibModalInstance", "projectManager", "dialogService", "params", "$sce"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private projectManager: IProjectManager,
        private dialogService: IDialogService,
        params: IDialogSettings,
        private $sce: ng.ISCEService
    ) {
        super($uibModalInstance, params);

    };

    public propertyMap = {
        id: "id",
        type: "type",
        name: "name",
        hasChildren: "hasChildren"
    };

    //Dialog return value
    public get returnvalue(): IOpenProjectResult {
        return <IOpenProjectResult>{
            id: (this.selectedItem && this.selectedItem["id"]) || -1,
            name: (this.selectedItem && this.selectedItem["name"]) || "",
            description: (this.selectedItem && this.selectedItem["description"]) || ""
        };
    };
    public get isProjectSelected(): boolean {
        return this.selectedItem && this.selectedItem.type === Models.ArtifactTypeEnum.Folder;
    }

    private onEnterKeyOnProject = (e: any) => {
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
            "has-children": function(params) { return params.data.hasChildren; },
            "is-folder": function (params) { return params.data.type === Models.ArtifactTypeEnum.Folder; },
            "is-project": function (params) { return params.data.type === Models.ArtifactTypeEnum.Project; }
        },
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                var sanitizedName = Helper.escapeHTMLText(params.data.name);

                if (params.data.type === 1) {
                    var cell = params.eGridCell;
                    cell.addEventListener("keydown", this.onEnterKeyOnProject);
                }
                return sanitizedName;
            }
        },
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering : true
    }];

    public doLoad = (prms: any): any[] => {
        //check passed in parameter
        let self = this;
        let id = (prms && angular.isNumber(prms.id)) ? prms.id : null;
        this.projectManager.getFolders(id)
            .then((data: Models.IProjectNode[]) => { //pSvc.IProjectNode[]
                if (angular.isNumber(id)) {
                    self.tree.addNodeChildren(id, data);
                } else {
                    self.tree.addNode(data);
                }
                self.tree.refresh();
            }, (error) => {
                //self.showError(error);
            });

        return null;
    };

    public doSelect = (item: any) => {
        //check passed in parameter
        let self = this;
        this.$scope.$applyAsync((s) => {
            self.selectedItem = item;
            if (self.selectedItem.description) {
                var description = self.selectedItem.description;
                var virtualDiv = window.document.createElement("DIV");
                virtualDiv.innerHTML = description;
                var aTags = virtualDiv.querySelectorAll("a");
                for (var a = 0; a < aTags.length; a++) {
                    aTags[a].setAttribute("target", "_blank");
                }
                description = virtualDiv.innerHTML;
                self.selectedItem.description = this.$sce.trustAsHtml(description);
            }
        });
    }
}
