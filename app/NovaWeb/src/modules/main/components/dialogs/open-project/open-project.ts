import {ProjectSearchResultVM} from "../../bp-artifact-picker/search-result-vm";
import {Helper} from "../../../../shared/utils/helper";
import {IDialogSettings, BaseDialogController} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {TreeModels} from "../../../models";
import {IInstanceItem} from "../../../models/admin-store-models";
import {IProjectSearchResult} from "../../../models/search-service-models";

type OpenProjectVM = TreeModels.InstanceItemNodeVM | ProjectSearchResultVM;

export class OpenProjectController extends BaseDialogController {
    public hasCloseButton: boolean = true;
    private _returnValue: IInstanceItem | IProjectSearchResult;
    selectedName: string;
    selectedDescription: string;

    static $inject = [
        "$scope",
        "$uibModalInstance",
        "dialogSettings",
        "$sce"
    ];

    constructor(private $scope: ng.IScope,
                $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                private $sce: ng.ISCEService) {
        super($uibModalInstance, dialogSettings);
    };

    //Dialog return value
    public get returnValue(): IInstanceItem | IProjectSearchResult {
        return this._returnValue;
    };

    public get isProjectSelected(): boolean {
        return !!this.returnValue;
    }

    private setSelectedItem(vm: OpenProjectVM) {
        this._returnValue = undefined;
        let description = vm ? vm.model.description : undefined;
        if (description) {

            const virtualDiv = window.document.createElement("DIV");
            virtualDiv.innerHTML = description;

            description = Helper.stripWingdings(virtualDiv.innerHTML);
            description = String(description).replace(/<[^>]+>/gm, "");
        }

        this.selectedName = vm ? vm.model.name : undefined;
        this.selectedDescription = description;
        if (vm && vm.model) {
            this._returnValue = vm.model;
        }
    }

    // BpArtifactPicker bindings

    public onSelectionChanged(selectedVMs: OpenProjectVM[]): void {
        if (selectedVMs && selectedVMs.length) {
            if (selectedVMs[0].model.hasOwnProperty("id") && this.returnValue && this.returnValue.hasOwnProperty("id")) {
                if ((selectedVMs[0] as TreeModels.InstanceItemNodeVM).model.id === (this.returnValue as any).id) {
                    return undefined;
                }
            }
            this.setSelectedItem(selectedVMs.length ? selectedVMs[0] : undefined);
        }
    }
}
