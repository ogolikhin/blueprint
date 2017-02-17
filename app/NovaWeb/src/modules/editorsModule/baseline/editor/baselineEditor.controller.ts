import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {Models} from "../../../main";
import {IMessageService} from "../../../main/components/messages/message.svc";
import {Enums} from "../../../main/models";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {IWindowManager} from "../../../main/services/window-manager";
import {IMetaDataService} from "../../../managers/artifact-manager";
import {IItemChangeSet} from "../../../managers/artifact-manager/changeset";
import {IValidationService} from "../../../managers/artifact-manager/validation/validation.svc";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {Helper, IDialogService} from "../../../shared";
import {IBPTreeViewControllerApi, IColumn, IColumnRendererParams, IHeaderCellRendererParams, ITreeNode} from "../../../shared/widgets/bp-tree-view/";
import {BpArtifactDetailsEditorController} from "../../artifactEditor/details/artifactDetailsEditor.controller";
import {IBaselineArtifact, IStatefulBaselineArtifact} from "../../configuration/classes/baseline-artifact";
import {IPropertyDescriptorBuilder} from "../../services";
import {IBaselineService} from "../baseline.service";

export class BpArtifactBaselineEditorController extends BpArtifactDetailsEditorController {
    public static $inject: [string] = [
        "$state",
        "messageService",
        "selectionManager",
        "windowManager",
        "localization",
        "propertyDescriptorBuilder",
        "validationService",
        "dialogService",
        "baselineService",
        "metadataService",
        "$location",
        "$window",
        "$scope"
    ];

    public selectAll: boolean = false;
    public selectAllClass: string;
    public isSystemPropertiesCollapsed: boolean = true;
    private baselineSubscriber: Rx.IDisposable;
    public selectedVMs: any[] = [];
    public itemsSelected: string;
    public api: IBPTreeViewControllerApi;
    public columns: IColumn[];
    public isBaselineChanging: boolean = false;

    constructor(private $state: ng.ui.IStateService,
                messageService: IMessageService,
                selectionManager: ISelectionManager,
                windowManager: IWindowManager,
                localization: ILocalizationService,
                propertyDescriptorBuilder: IPropertyDescriptorBuilder,
                validationService: IValidationService,
                private dialogService: IDialogService,
                private baselineService: IBaselineService,
                private metadataService: IMetaDataService,
                private $location: ng.ILocationService,
                protected $window: ng.IWindowService,
                private $scope: ng.IScope) {
        super($window, messageService, selectionManager, windowManager, localization, propertyDescriptorBuilder, validationService);
    }

    public get reviewUrl(): string {
        if (this.artifact && (<IStatefulBaselineArtifact>this.artifact).rapidReviewCreated) {
            return this.$location.protocol() + "://" + this.$window.location.host + "/ArtifactMgmt/RapidReview/" + this.artifact.id;
        }

        return "";
    }

    private unsubscribe(): void {
        if (this.baselineSubscriber) {
            this.baselineSubscriber.dispose();
            this.baselineSubscriber = undefined;
        }
    }

    protected destroy(): void {
        this.unsubscribe();

        super.destroy();
    }

    private subscribeOnBaselineChanges(baselineArtifact: IStatefulBaselineArtifact) {
        this.unsubscribe();

        if (baselineArtifact) {
            this.baselineSubscriber = baselineArtifact.getPropertyObservable()
                .filter(changes => changes.change && changes.item &&
                changes.change.key === Models.PropertyTypePredefined.BaselineContent)
                .subscribeOnNext(this.onBaselineArtifactsChanged);
        }
    }

    protected onArtifactReady() {
        if (this.editor && this.artifact) {
            const baselineArtifact = this.artifact as IStatefulBaselineArtifact;
            // if baseline is deleted we do not need to load metadata and baseline content
            if (!baselineArtifact.artifacts) {
                super.onArtifactReady();
                return;
            }
            this.metadataService.get(baselineArtifact.projectId).then(() => {
                this.rowData = baselineArtifact.artifacts.map((a: IBaselineArtifact) => {
                    return new BaselineNodeVM(a, this.artifact.projectId, this.metadataService, !this.artifact.artifactState.readonly);
                });

                this.columns = this.getColumns();

                this.subscribeOnBaselineChanges(baselineArtifact);

            }).catch((error: any) => {
                //ignore authentication errors here
                if (error) {
                    this.messageService.addError(error["message"] || "Project_MetaDataNotFound");
                }
            }).finally(() => {
                super.onArtifactReady();
            });
        }
        else {
            super.onArtifactReady();
        }
    }

    private visibleArtifact: BaselineNodeVM;

    public onGridReset(isExpanding: boolean): void {
        //Added this check for the case when grid reset performed because user added artifact to baseline
        //In this case we don't deselect ag-grid rows
        if (!this.isBaselineChanging) {
            this.selectedVMs = [];
            this.api.deselectAll();
            this.isBaselineChanging = false;
        }

        if (this.visibleArtifact) {
            this.api.ensureNodeVisible(this.visibleArtifact);
            this.visibleArtifact = undefined;
        }
    }

    private onBaselineArtifactsChanged = (changes: IItemChangeSet) => {
        this.isBaselineChanging = true;
        const baselineArtifact = this.artifact as IStatefulBaselineArtifact;
        this.rowData = baselineArtifact.artifacts.map((a: IBaselineArtifact) => {
            return new BaselineNodeVM(a, baselineArtifact.projectId, this.metadataService, !baselineArtifact.artifactState.readonly);
        });
    };

    private headerCellRendererSelectAll(params: IHeaderCellRendererParams, isArtifactReadOnly: boolean) {
        let cb = document.createElement("i");
        cb.setAttribute("class", "ag-checkbox-unchecked");

        if (isArtifactReadOnly) {
            cb.setAttribute("class", "disabled");
        }

        let sp = document.createElement("span");
        sp.setAttribute("class", "ag-group-checkbox");

        sp.appendChild(cb);

        let eHeader = document.createElement("span");
        eHeader.setAttribute("class", "ag-header-checkbox");
        eHeader.appendChild(sp);

        if (!isArtifactReadOnly) {
            cb.addEventListener("click", function (e) {
                let checked: boolean;

                if ((e.target)["data-checked"] && (e.target)["data-checked"] === true) {
                    checked = false;
                    cb.setAttribute("class", "ag-checkbox-unchecked");
                } else {
                    checked = true;
                    cb.setAttribute("class", "ag-checkbox-checked");
                }

                (<HTMLInputElement>e.target)["data-checked"] = checked;
                params.context.allSelected = checked;
                params.context.selectAllClass.selectAll(checked);
            });
        }
        return eHeader;
    }

    public getColumns(): IColumn[] {
        return [
            {
                isCheckboxSelection: !this.artifact.artifactState.readonly,
                width: 30,
                minColWidth: 20,
                maxWidth: 30,
                headerCellRenderer: (params) => {
                    return this.headerCellRendererSelectAll(params, this.artifact.artifactState.readonly);
                },
                field: "chck",
                cellRenderer: (params: IColumnRendererParams) => {
                    if (this.artifact.artifactState.readonly) {
                        return `<span class="ag-cell-wrapper"><span class="ag-selection-checkbox"><i class="ag-checkbox-unchecked disabled"></i></span></span>`;
                    }
                    return "";
                }
            },
            {
                width: 100,
                minColWidth: 100,
                headerName: `<span class="header-name">` + this.localization.get("Label_ID") + `</span>`,
                field: "model.id",
                isGroup: true,
                isCheckboxHidden: true,
                cellClass: (vm: BaselineNodeVM) => vm.getCellClass(),
                cellRenderer: (params: IColumnRendererParams) => {
                    const vm = params.data as BaselineNodeVM;
                    const prefix = Helper.escapeHTMLText(vm.model.prefix);
                    const icon = vm.getIcon();
                    return `${icon} <a ui-sref="main.item({id: ${vm.model.id}})" ui-sref-opts="{inherit: false}"
                                       class="baseline__link">${prefix}${vm.model.id}</a>`;
                }
            },
            {
                headerName: this.localization.get("Label_Name"),
                isGroup: true,
                isCheckboxHidden: true,
                cellRenderer: (params: IColumnRendererParams) => {
                    const vm = params.data as BaselineNodeVM;
                    const path = vm.model.artifactPath;
                    const name = Helper.escapeHTMLText(vm.model.name);
                    const tooltipName: string = Helper.escapeQuot(vm.model.name);

                    let pathName = "";
                    path.map((baselineArtifact: string, index: number) => {
                        if (index !== 0) {
                            pathName += " > ";
                        }

                        pathName = pathName + `${Helper.escapeHTMLText(baselineArtifact)}`;
                    });

                    return `<div bp-tooltip="${tooltipName}" bp-tooltip-truncated="true" class="baseline__name">
                                <a ui-sref="main.item({id: ${vm.model.id}})" ui-sref-opts="{inherit: false}">${name}</a>
                            </div>
                            <div bp-tooltip="${Helper.escapeQuot(pathName)}" bp-tooltip-truncated="true" class="path">${pathName}</div>`;
                }
            },
            {
                headerName: this.localization.get("Label_Description"),
                isGroup: true,
                isCheckboxHidden: true,
                cellRenderer: (params: IColumnRendererParams) => {
                    const vm = params.data as BaselineNodeVM;
                    const tooltip: string = Helper.escapeQuot(vm.model.description);
                    const desc = Helper.escapeHTMLText(vm.model.description);

                    if (vm.model.description) {
                        return `<div class="baseline__description" bp-tooltip="${tooltip}" bp-tooltip-truncated="true">${desc}</div>`;
                    }

                    return "";
                }
            },
            {
                headerName: this.localization.get("Label_Options"),
                width: 60,
                maxWidth: 60,
                minColWidth: 60,
                isCheckboxHidden: true,
                cellRenderer: (params: IColumnRendererParams) => {
                    params.$scope["removeArtifact"] = ($event) => {
                        $event.stopPropagation();

                        this.dialogService.confirm(this.localization.get("Artifact_Baseline_Confirmation_Delete_Item")).then(() => {

                            const vm = params.data as BaselineNodeVM;
                            const baselineArtifact = this.artifact as IStatefulBaselineArtifact;
                            baselineArtifact.removeArtifacts([vm.model]);

                            let index = _.findIndex(this.selectedVMs, (item) => item.model.id === vm.model.id);

                            if (index > -1) {
                                this.selectedVMs.splice(index, 1);
                                let item_selected = this.localization.get("Artifact_Baseline_Items_Selected");
                                this.itemsSelected = item_selected.replace("{0}", (this.selectedVMs.length).toString());
                            }
                        });


                    };

                    if (this.artifact.artifactState.readonly) {
                        return `<i class="icon icon__normal fonticon-delete-filled disabled"></i>`;
                    } else {
                        return `<i class="icon icon__action fonticon-delete-filled" ng-click="removeArtifact($event)"></i>`;
                    }

                }
            }];
    }

    public rowData: BaselineNodeVM[] = [];

    public toggleAll(): void {
        this.selectAll = !this.selectAll;
        this.selectAllClass = this.selectAll ? "ag-checkbox-checked" : "ag-checkbox-unchecked";
    }

    public onSelect = (vm, isSelected: boolean) => {
        if (isSelected) {
            this.selectedVMs.push(vm);
        } else {
            let index = _.findIndex(this.selectedVMs, {key: vm.key});

            if (index > -1) {
                this.selectedVMs.splice(index, 1);
            }
        }

        let item_selected = this.localization.get("Artifact_Baseline_Items_Selected");
        this.itemsSelected = item_selected.replace("{0}", (this.selectedVMs.length).toString());
        this.$scope.$applyAsync();
    }

    public bulkDelete() {
        let confirmation = this.localization.get("Artifact_Baseline_Confirmation_Delete_Items")
            .replace("{0}", this.selectedVMs.length.toString());

        this.dialogService.confirm(confirmation).then(() => {
            const baselineArtifact = this.artifact as IStatefulBaselineArtifact;
            const artifactsToBeDeleted = _.map(this.selectedVMs, (node) => node.model);
            baselineArtifact.removeArtifacts(artifactsToBeDeleted);
            this.selectedVMs.length = 0;
        });
    }

    public hasRequiredPermissions(): boolean {
        return Helper.hasDesiredPermissions(this.artifact, Enums.RolePermissions.CreateRapidReview);
    }
}

class BaselineNodeVM implements ITreeNode {
    public key: string;

    constructor(public model: IBaselineArtifact, private projectId: number, private metadataService: IMetaDataService,
                public selectable: boolean = true) {
        this.key = String(model.id);
    }

    public getIcon(): string {
        let artifactType = this.metadataService.getArtifactItemTypeTemp(this.projectId, this.model.itemTypeId);
        if (artifactType && artifactType.iconImageId && angular.isNumber(artifactType.iconImageId)) {
            return `<bp-item-type-icon item-type-id="${artifactType.id}" item-type-icon-id="${artifactType.iconImageId}"></bp-item-type-icon>`;
        }
        return `<i class="icon__normal"></i>`;
    }

    public getCellClass(): string[] {
        const result = [] as string[];
        const typeName = ItemTypePredefined[this.model.itemTypePredefined];
        if (typeName) {
            result.push("is-" + _.kebabCase(typeName));
        }
        return result;
    }
}
