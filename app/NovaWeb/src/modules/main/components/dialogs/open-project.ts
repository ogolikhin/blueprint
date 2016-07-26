import "angular";
import { Helper } from "../../../core/utils/helper";
import { ILocalizationService } from "../../../core";
import { IBPTreeController } from "../../../core/widgets/bp-tree/bp-tree";
import { IDialogSettings, BaseDialogController, IDialogService} from "../../../core";
import { IProjectManager, Models } from "../../";

export interface IOpenProjectController {
    propertyMap: any;
    errorMessage: string;
    hasError: boolean;
    isProjectSelected: boolean;
    selectedItem?: Models.IProject;

}

export class OpenProjectController extends BaseDialogController implements IOpenProjectController {
    public hasCloseButton: boolean = true;
    private _selectedItem: Models.IProject;
    private _errorMessage: string;
    private tree: IBPTreeController;

    static $inject = ["$scope", "localization", "$uibModalInstance", "projectManager", "dialogService", "params", "$sce"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private manager: IProjectManager,
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
    public get returnValue(): Models.IProject {
        return this.selectedItem || null;
    };

    public get hasError(): boolean {
        return Boolean(this._errorMessage);
    }
    public get errorMessage(): string {
        return this._errorMessage;
    }

    public get isProjectSelected(): boolean {
        return this.returnValue && this.returnValue.itemTypeId === 1;
    }

    public get selectedItem() {
        return this._selectedItem;
    }

    private setSelectedItem(item: any) {
        this._selectedItem = <Models.IProject>{
            id: (item && item["id"]) || -1,
            name: (item && item["name"]) || "",
            description: (item && item["description"]) || "",
            itemTypeId: (item && item["type"]) || -1
            
        };

        if (this._selectedItem.description) {
            var description = this._selectedItem.description;
            var virtualDiv = window.document.createElement("DIV");
            virtualDiv.innerHTML = description;
            var aTags = virtualDiv.querySelectorAll("a");
            for (var a = 0; a < aTags.length; a++) {
                aTags[a].setAttribute("target", "_blank");
            }
            description = virtualDiv.innerHTML;
            this._selectedItem.description = this.$sce.trustAsHtml(description);
        }
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
            "has-children": function(params) { return params.data.hasChildren; },
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
        suppressFiltering : true
    }];

    public doLoad = (prms: any): any[] => { 
        //check passed in parameter
        let self = this;
        let id = (prms && angular.isNumber(prms.id)) ? prms.id : null;
        this.manager.loadFolders(id)
            .then((nodes: Models.IProjectNode[]) => { 
                self.tree.reload(nodes, id);
                if (self.tree.isEmpty) {
                    this._errorMessage = this.localization.get("Project_NoProjectsAvailable");
                }
            }, (error) => {
                //close dialog on authentication error
                if (error.statusCode === 1401) {
                    this.cancel();
                } else {
                    this._errorMessage = error.message;
                }
            });

        return null;
    };

    public doSelect = (item: any) => {
        //check passed in parameter
        let self = this;
        this.$scope.$applyAsync((s) => {
            self.setSelectedItem(item);
        });
    }

}
