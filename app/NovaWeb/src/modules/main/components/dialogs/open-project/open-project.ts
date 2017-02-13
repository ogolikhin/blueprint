import {BaseDialogController, IDialogSettings} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {TreeModels} from "../../../models";
import {IInstanceItem, InstanceItemType} from "../../../models/admin-store-models";
import {IProjectSearchResult} from "../../../models/search-service-models";
import {ProjectSearchResultVM} from "../../bp-artifact-picker/search-result-vm";
type OpenProjectVM = TreeModels.InstanceItemNodeVM | ProjectSearchResultVM;

export class OpenProjectController extends BaseDialogController {
    public hasCloseButton: boolean = true;
    private _returnValue: IInstanceItem | IProjectSearchResult;
    private _descriptionMaxHeight: number;

    selectedName: string;
    selectedDescription: string;

    static $inject = [
        "$window",
        "$scope",
        "$uibModalInstance",
        "dialogSettings",
        "$sce",
        "$timeout"
    ];

    constructor(private $window: ng.IWindowService,
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
        const doc = this.$window.document;

        this._returnValue = undefined;

        let description = vm ? vm.model.description : undefined;
        if (description) {
            const virtualDiv = doc.createElement("DIV");
            virtualDiv.innerHTML = description.replace(/<\/p>/gi, "</p>\n\r");
            description = virtualDiv.innerText.replace(/(?:\r\n|\r|\n|\u00a0|\ufeff|\u200b)/g, " ").trim();
        }

        this.selectedName = vm ? vm.model.name : undefined;
        this.selectedDescription = description;
        if (vm instanceof TreeModels.InstanceItemNodeVM && vm.model.type === InstanceItemType.Project) {
            this._returnValue = vm.model;
        } else if (vm instanceof ProjectSearchResultVM) {
            this._returnValue = vm.model;
        } else {
            this._returnValue = undefined;
        }

        const descriptionElement = doc.querySelector(".open-project__description");
        if (descriptionElement) {
            const clampClasses = ["line-clamp", "line-clamp-3", "line-clamp--gray-lightest"];
            clampClasses.forEach((clampClass) => {
                descriptionElement.classList.remove(clampClass);
            });

            this.$timeout(() => {
                if (!this._descriptionMaxHeight) { // we calculate it only the first time
                    const styles = this.$window.getComputedStyle(descriptionElement);
                    // returns the resolved/computed value: https://developer.mozilla.org/en-US/docs/Web/CSS/resolved_value
                    const lineHeight = styles.getPropertyValue("line-height");
                    // if line-height is not in px, we use a backup value. we want up to 3 lines
                    this._descriptionMaxHeight = (lineHeight.indexOf("px") === lineHeight.length - 2 ? parseFloat(lineHeight) : 16) * 3;
                }
                if (descriptionElement.scrollHeight > this._descriptionMaxHeight) {
                    clampClasses.forEach((clampClass) => {
                        descriptionElement.classList.add(clampClass);
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
            if (this.returnValue) {
                this.ok();
            }
        });
    }
}
