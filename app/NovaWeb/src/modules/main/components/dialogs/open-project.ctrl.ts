﻿import "angular";
import {ILocalizationService} from "../../../core/localization";
import {IBPTreeController} from "../../../core/widgets/bp-tree/bp-tree";

import {IDialogSettings, BaseDialogController, IDialogService} from "../../../services/dialog.svc";
import * as pSvc from "../../services/project.svc";

export interface IOpenProjectResult {
    id: number;
    name: string;
    description: string;
}

export class OpenProjectController extends BaseDialogController {
    public hasCloseButton: boolean = true;
    private selectedItem: any;
    private tree: IBPTreeController;


    static $inject = ["$scope", "localization", "$uibModalInstance", "projectService", "dialogService", "params", "$sce"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private service: pSvc.IProjectService,
        private dialogService: IDialogService,
        params: IDialogSettings,
        private $sce: ng.ISCEService
    ) {
        super($uibModalInstance, params);
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
        return this.selectedItem && this.selectedItem.type === `Project`;
    }

    public escapeHTMLText = (stringToEscape: string): string =>  {
        var stringEscaper = window.document.createElement("TEXTAREA");
        stringEscaper.textContent = stringToEscape;
        return stringEscaper.innerHTML;
    };

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
            "is-folder": function (params) { return params.data.type === "Folder"; },
            "is-project": function (params) { return params.data.type === "Project"; }
        },
        cellRenderer: "group",  
        cellRendererParams: {
            innerRenderer: (params) => {
                var sanitizedName = this.escapeHTMLText(params.data.name);

                if (params.data.type === "Project") {
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
        this.service.getFolders(id)
            .then((data: any[]) => { //pSvc.IProjectNode[]
                self.tree.setDataSource(data, id);
            }, (error) => {
                //self.showError(error);
            });

        return null;
    };

    public doSelect = (item: any) => {
        //check passed in parameter
        this.$scope.$applyAsync((s) => {
            this.selectedItem = item;
            if (item.Description) {
                var description = item.description;
                var virtualDiv = window.document.createElement("DIV");
                virtualDiv.innerHTML = description;
                var aTags = virtualDiv.querySelectorAll("a");
                for (var a = 0; a < aTags.length; a++) {
                    aTags[a].setAttribute("target", "_blank");
                }
                description = virtualDiv.innerHTML;
                this.selectedItem.Description = this.$sce.trustAsHtml(description);
            }
        });
    }
}
