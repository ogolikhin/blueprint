import * as angular from "angular";
import {ILocalizationService} from "../../../../core";
import {Helper, IBPTreeController, IDialogSettings, BaseDialogController} from "../../../../shared";
import {Models} from "../../../models";
import {IProjectManager} from "../../../../managers";

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

    static $inject = ["$scope", "localization", "$uibModalInstance", "projectManager", "dialogSettings", "$sce"];

    constructor(private $scope: ng.IScope,
                private localization: ILocalizationService,
                $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                private manager: IProjectManager,
                dialogSettings: IDialogSettings,
                private $sce: ng.ISCEService) {
        super($uibModalInstance, dialogSettings);

    };

    public propertyMap = {
        id: "id",
        type: "type",
        name: "name",
        hasChildren: "hasChildren",
        children: "children",
        loaded: "loaded",
        open: "open"
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

    private _selectedDescription: string;

    public get selectedDescription() {
        return this._selectedDescription;
    }

    private setSelectedItem(item: any) {
        this._selectedItem = <Models.IProject>{
            id: (item && item["id"]) || -1,
            name: (item && item["name"]) || "",
            description: (item && item["description"]) || "",
            itemTypeId: (item && item["type"]) || -1
        };

        if (this._selectedItem.description) {
            const description = this._selectedItem.description;
            const virtualDiv = window.document.createElement("DIV");
            virtualDiv.innerHTML = description;

            const aTags = virtualDiv.querySelectorAll("a");
            for (let a = 0; a < aTags.length; a++) {
                aTags[a].setAttribute("target", "_blank");
            }
            this._selectedDescription = this.$sce.trustAsHtml(Helper.stripWingdings(virtualDiv.innerHTML));
            this._selectedItem.description = this._selectedDescription.toString();
        } else {
            this._selectedDescription = null;
        }
    }

    private onEnterKeyPressed = (e: any) => {
        const key = e.which || e.keyCode;
        if (key === 13) {
            //user pressed Enter key on project
            this.ok();
        }
    };

    public columns = [{
        headerName: this.localization.get("App_Header_Name"),
        field: "name",
        cellClassRules: {
            "has-children": function (params) {
                return params.data.hasChildren;
            },
            "is-folder": function (params) {
                return params.data.type === 0;
            },
            "is-project": function (params) {
                return params.data.type === 1;
            }
        },
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                if (params.data.type === 1) {
                    let cell = params.eGridCell;
                    cell.addEventListener("keydown", this.onEnterKeyPressed);
                }
                return `<i></i><span>${Helper.escapeHTMLText(params.data.name)}</span>`;
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
        let id = (prms && angular.isNumber(prms.id)) ? prms.id : null;
        this.manager.loadFolders(id)
            .then((nodes: Models.IProjectNode[]) => {
                self.tree.reload(nodes, id);
                if (self.tree.isEmpty) {
                    this._errorMessage = this.localization.get("Project_NoProjectsAvailable");
                }
            }, (error) => {
                //close dialog on authentication error
                this._errorMessage = this.localization.get("Project_NoProjectsAvailable");
            });

        return null;
    };

    public doSelect = (item: any) => {
        //check passed in parameter
        this.$scope.$applyAsync((s) => {
            this.setSelectedItem(item);
        });
    }

}
