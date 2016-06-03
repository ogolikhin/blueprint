﻿import "angular";
import { Helper } from "../../../core/utils/helper";
import { ILocalizationService } from "../../../core";
import { IBPTreeController } from "../../../core/widgets/bp-tree/bp-tree";
import { IDialogSettings, BaseDialogController, IDialogService } from "../../../services/dialog.svc";
import { IProjectManager, Models } from "../../managers/project-manager";

export interface IOpenProjectResult {
    id: number;
    type: number;
    name: string;
    description: string;
}

export class OpenProjectController extends BaseDialogController {
    public hasCloseButton: boolean = true;
    private _selectedItem: IOpenProjectResult;
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
    public get returnValue(): IOpenProjectResult {
        return this.selectedItem || null;
    };

    public get hasError(): boolean {
        return Boolean(this._errorMessage);
    }
    public get errorMessage(): string {
        return this._errorMessage;
    }

    public get isProjectSelected(): boolean {
        return this.returnValue && this.returnValue.type === 1;
    }

    public get selectedItem() {
        return this._selectedItem;
    }

    public set selectedItem(item: any) {
        this._selectedItem = <IOpenProjectResult>{
            id: (item && item["id"]) || -1,
            name: (item && item["name"]) || "",
            type: (item && item["type"]) || -1,
            description: (item && item["description"]) || ""
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
        this.manager.getFolders(id)
            .then((nodes: Models.IProjectNode[]) => { 
                self.tree.reload(nodes, id);
                if (self.tree.isEmpty) {
                    this._errorMessage = this.localization.get("Project_NoProjectsAvailable");
                }
            }, (error) => {
                this._errorMessage = error.message;
            });

        return null;
    };

    public doSelect = (item: any) => {
        //check passed in parameter
        let self = this;
        this.$scope.$applyAsync((s) => {
            self.selectedItem = item;
        });
    }

}
