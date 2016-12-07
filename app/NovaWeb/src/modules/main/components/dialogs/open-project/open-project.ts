import {ProjectSearchResultVM} from "./../../bp-artifact-picker/search-result-vm";
import {Helper} from "./../../../../shared/utils/helper";
import {IDialogController, IDialogSettings, BaseDialogController} from "./../../../../shared/widgets/bp-dialog/bp-dialog";
import {AdminStoreModels, TreeModels} from "../../../models";

type OpenProjectVM = TreeModels.InstanceItemNodeVM | ProjectSearchResultVM;

export interface IOpenProjectController extends IDialogController {
    isProjectSelected: boolean;
    selectedName?: string;
    selectedDescription: string;

    // BpArtifactPicker bindings
    onSelectionChanged(selectedVMs: OpenProjectVM[] ): void;
    onDoubleClick: (vm: OpenProjectVM) => any;
}

export class OpenProjectController extends BaseDialogController implements IOpenProjectController {
    public hasCloseButton: boolean = true;
    private _returnValue: number;
    private _selectedName: string;
    private _selectedDescription: string;

    static $inject = ["$scope", "$uibModalInstance", "dialogSettings", "$sce"];

    constructor(private $scope: ng.IScope,
                $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                private $sce: ng.ISCEService) {
        super($uibModalInstance, dialogSettings);
    };

    //Dialog return value
    public get returnValue(): number {
        return this._returnValue;
    };

    public get isProjectSelected(): boolean {
        return Boolean(this.returnValue);
    }

    public get selectedName(): string {
        return this._selectedName;
    }

    public get selectedDescription() {
        return this._selectedDescription;
    }

    private setSelectedItem(vm: OpenProjectVM) {
        this._selectedName = vm ? vm.model.name : undefined;

        let description = vm ? vm.model.description : undefined;
        if (description) {
            const virtualDiv = window.document.createElement("DIV");
            virtualDiv.innerHTML = description;

            const aTags = virtualDiv.querySelectorAll("a");
            for (let a = 0; a < aTags.length; a++) {
                aTags[a].setAttribute("target", "_blank");
            }
            description = this.$sce.trustAsHtml(Helper.stripWingdings(virtualDiv.innerHTML));
        }
        this._selectedDescription = description;

        if (vm instanceof TreeModels.InstanceItemNodeVM && vm.model.type === AdminStoreModels.InstanceItemType.Project) {
            this._returnValue = vm.model.id;
        } else if (vm instanceof ProjectSearchResultVM) {
            this._returnValue = vm.model.itemId;
        } else {
            this._returnValue = undefined;
        }
    }

    // BpArtifactPicker bindings

    public onSelectionChanged(selectedVMs: OpenProjectVM[]): void {
        this.$scope.$applyAsync(() => {
            this.setSelectedItem(selectedVMs.length ? selectedVMs[0] : undefined);
        });
    }

    public onDoubleClick = (vm: OpenProjectVM): void => {
        this.$scope.$applyAsync(() => {
            this.setSelectedItem(vm);
            this.ok();
        });
    }
}
