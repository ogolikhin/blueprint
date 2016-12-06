import {IProjectSearchResult} from "./../../../models/search-service-models";
import {ProjectSearchResultVM} from "./../../bp-artifact-picker/search-result-vm";
import {Helper, IDialogSettings, BaseDialogController} from "../../../../shared";
import {IColumn, IColumnRendererParams} from "../../../../shared/widgets/bp-tree-view/";
import {AdminStoreModels, TreeModels} from "../../../models";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export interface IOpenProjectController {
    isProjectSelected: boolean;
    selectedItem?: TreeModels.InstanceItemNodeVM | ProjectSearchResultVM;
    selectedDescription: string;

    // BpArtifactPicker bindings
    onSelectionChanged(selectedVMs: TreeModels.InstanceItemNodeVM[] | ProjectSearchResultVM[] ): void;
    onDoubleClick: (vm: TreeModels.InstanceItemNodeVM | ProjectSearchResultVM) => any;
}

export class OpenProjectController extends BaseDialogController implements IOpenProjectController {
    public hasCloseButton: boolean = true;
    private _selectedItem: TreeModels.InstanceItemNodeVM | ProjectSearchResultVM;
    private _errorMessage: string;

    static $inject = ["$scope", "localization", "$uibModalInstance", "dialogSettings", "$sce"];

    constructor(private $scope: ng.IScope,
                private localization: ILocalizationService,
                $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                private $sce: ng.ISCEService) {
        super($uibModalInstance, dialogSettings);
    };

    //Dialog return value
    public get returnValue(): AdminStoreModels.IInstanceItem | IProjectSearchResult {
        return this.isProjectSelected ? this.selectedItem.model : undefined;
    };

    public get isProjectSelected(): boolean {
        return (this.selectedItem instanceof TreeModels.InstanceItemNodeVM && this.selectedItem.model.type === AdminStoreModels.InstanceItemType.Project) ||
            this.selectedItem instanceof ProjectSearchResultVM;
    }

    public get selectedItem(): TreeModels.InstanceItemNodeVM | ProjectSearchResultVM {
        return this._selectedItem;
    }

    private _selectedDescription: string;

    public get selectedDescription() {
        return this._selectedDescription;
    }

    private setSelectedItem(item: TreeModels.InstanceItemNodeVM | ProjectSearchResultVM) {
        this._selectedItem = item;

        const description = this.selectedItem ? this.selectedItem.model.description : undefined;
        if (description) {
            //TODO Why do we need this? Project descriptions are plain text and Instance Folders can't have descriptions.
            const virtualDiv = window.document.createElement("DIV");
            virtualDiv.innerHTML = description;

            const aTags = virtualDiv.querySelectorAll("a");
            for (let a = 0; a < aTags.length; a++) {
                aTags[a].setAttribute("target", "_blank");
            }
            this._selectedDescription = this.$sce.trustAsHtml(Helper.stripWingdings(virtualDiv.innerHTML));
            this.selectedItem.model.description = this.selectedDescription.toString();
        } else {
            this._selectedDescription = undefined;
        }
    }

    // BpArtifactPicker bindings

    public onSelectionChanged(selectedVMs: TreeModels.InstanceItemNodeVM[] | ProjectSearchResultVM[]): void {
        this.$scope.$applyAsync(() => {
            this.setSelectedItem(selectedVMs.length ? selectedVMs[0] : undefined);
        });
    }

    public onDoubleClick = (vm: TreeModels.InstanceItemNodeVM | ProjectSearchResultVM): void => {
        this.$scope.$applyAsync(() => {
            this.setSelectedItem(vm);
            this.ok();
        });
    }
}
