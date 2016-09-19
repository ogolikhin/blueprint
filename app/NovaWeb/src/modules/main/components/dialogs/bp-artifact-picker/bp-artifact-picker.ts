import "angular";
import { ColDef } from "ag-grid/main";
import { Helper } from "../../../../shared/";
import { ILocalizationService } from "../../../../core";
import { ArtifactPickerNodeVM, InstanceItemNodeVM, ArtifactNodeVM, SubArtifactNodeVM } from "./bp-artifact-picker-node-vm";
import { IDialogSettings, BaseDialogController } from "../../../../shared/";
import { Models } from "../../../models";
import { IProjectManager } from "../../../../managers";
import { IProjectService } from "../../../../managers/project-manager/project-service";

export interface IArtifactPickerController {
    project: Models.IProject;
    rootNode: ArtifactPickerNodeVM<any>;
    columnDefs: any[];
    onSelect: (vm: ArtifactPickerNodeVM<any>) => void;
}

export interface IArtifactPickerFilter {
    ItemTypePredefines: Models.ItemTypePredefined[];
}

export class ArtifactPickerController extends BaseDialogController implements IArtifactPickerController {

    public hasCloseButton: boolean = true;
    private _selectedItem: Models.IItem;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "$scope",
        "localization",
        "projectManager",
        "projectService",
        "dialogData"];
        
    constructor(
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private projectManager: IProjectManager,
        private projectService: IProjectService,
        private dialogData: IArtifactPickerFilter
    ) {
        super($instance, dialogSettings);
        this.project = this.projectManager.getSelectedProject();

        $scope.$on("$destroy", () => {
            this.columnDefs[0].cellClass = null;
            this.columnDefs[0].cellRendererParams["innerRenderer"] = null;
            this.columnDefs = null;
            this.onSelect = null;
        });
    };

    //Dialog return value
    public get returnValue(): any {
        return this._selectedItem || null;
    };

    private setSelectedItem(item: Models.IItem) {
        this.$scope.$applyAsync((s) => {
            this._selectedItem = this.isItemSelectable(item) ? item : undefined;
        });
    }

    private isItemSelectable(item: Models.IItem): boolean {
        return !(item &&
            this.dialogData &&
            this.dialogData.ItemTypePredefines &&
            this.dialogData.ItemTypePredefines.length > 0 &&
            this.dialogData.ItemTypePredefines.indexOf(item.predefinedType) === -1);
    }

    private onEnterKeyPressed = (e: any) => {
        const key = e.which || e.keyCode;
        if (key === 13) {
            this.ok();
        }
    };

    public columnDefs: ColDef[] = [{
        headerName: "",
        field: "name",
        cellClass: function (params) {
            const vm = params.data as ArtifactPickerNodeVM<any>;
            let css: string[] = [];

            if (vm.isExpandable) {
                css.push("has-children");
            }

            var typeClass = vm.getTypeClass();
            if (typeClass) {
                css.push(typeClass);
            }

            return css;
        },
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: (params) => {
                const vm = params.data as ArtifactPickerNodeVM<any>;
                let icon = "<i></i>";
                const name = Helper.escapeHTMLText(vm.name);

                if (vm instanceof InstanceItemNodeVM && vm.model.type === Models.ProjectNodeType.Folder) {
                    params.eGridCell.addEventListener("keydown", this.onEnterKeyPressed);
                } else if (vm instanceof ArtifactNodeVM) {
                    //TODO: for now it display custom icons just for already loaded projects
                    let statefulArtifact = this.projectManager.getArtifact(vm.model.id);
                    if (statefulArtifact) {
                        let artifactType = statefulArtifact.metadata.getItemType();
                        if (artifactType && artifactType.iconImageId && angular.isNumber(artifactType.iconImageId)) {
                            icon = `<bp-item-type-icon
                                item-type-id="${artifactType.id}"
                                item-type-icon="${artifactType.iconImageId}"></bp-item-type-icon>`;
                        }
                    }
                }
                return `<span class="ag-group-value-wrapper">${icon}<span>${name}</span></span>`;
            },
            padding: 20
        },
        suppressMenu: true,
        suppressSorting: true,
    }];

    public rootNode: InstanceItemNodeVM;

    public onSelect = (vm: ArtifactPickerNodeVM<any>) => {
        if (vm instanceof ArtifactNodeVM || vm instanceof SubArtifactNodeVM) {
            this.setSelectedItem(vm.model);
        } else {
            this.setSelectedItem(undefined);
            if (vm instanceof InstanceItemNodeVM) {
                if (vm.model.type === Models.ProjectNodeType.Project) {
                    this.projectService.getProject(vm.model.id).then(project => this.project = project);
                }
            }
        }
    };

    private _project: Models.IProject;

    public get project(): Models.IProject {
        return this._project;
    }

    public set project(project: Models.IProject) {
        this.setSelectedItem(undefined);
        this._project = project;
        if (project) {
            this.rootNode = new InstanceItemNodeVM(this.projectService, {
                id: project.id,
                type: Models.ProjectNodeType.Project,
                name: project.name,
                hasChildren: project.hasChildren,
            } as Models.IProjectNode, true);
        } else {
            this.rootNode = new InstanceItemNodeVM(this.projectService, {
                id: 0,
                type: Models.ProjectNodeType.Folder,
                name: "",
                hasChildren: true
            } as Models.IProjectNode, true);
        }
    }
}
