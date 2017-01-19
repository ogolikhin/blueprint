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
        "$document",
        "$scope",
        "$uibModalInstance",
        "dialogSettings",
        "$sce",
        "$timeout"
    ];

    constructor(private $document: ng.IDocumentService,
                private $scope: ng.IScope,
                $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                private $sce: ng.ISCEService,
                private $timeout: ng.ITimeoutService) {
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
        const doc = this.$document[0];

        this._returnValue = undefined;

        let description = vm ? vm.model.description : undefined;
        if (description) {
            const virtualDiv = doc.createElement("DIV");
            virtualDiv.innerHTML = description.replace(/<\/p>/gi, "</p>\n\r");
            description = virtualDiv.innerText.replace(/(?:\r\n|\r|\n|\u00a0|\ufeff|\u200b)/g, " ").trim();
        }

        this.selectedName = vm ? vm.model.name : undefined;
        this.selectedDescription = description;
        if (vm && vm.model) {
            this._returnValue = vm.model;
        }

        const descriptionDiv = doc.querySelector(".open-project__description");
        if (descriptionDiv) {
            const clampClasses = ["line-clamp", "line-clamp-3", "line-clamp--gray-lightest"];
            clampClasses.forEach((clampClass) => {
                descriptionDiv.classList.remove(clampClass);
            });

            this.$timeout(() => {
                if (descriptionDiv.scrollHeight > 47) {
                    clampClasses.forEach((clampClass) => {
                        descriptionDiv.classList.add(clampClass);
                    });
                }
            });
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

    public onDoubleClick(vm: OpenProjectVM): void {
        this.$scope.$applyAsync(() => {
            this.setSelectedItem(vm);
            this.ok();
        });
    }
}
