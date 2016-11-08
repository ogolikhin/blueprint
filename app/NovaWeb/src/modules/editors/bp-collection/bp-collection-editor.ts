import * as angular from "angular";
import * as _ from "lodash";
import {Models, Enums} from "../../main";
import {IColumn, ITreeViewNode, IColumnRendererParams} from "../../shared/widgets/bp-tree-view/";
import {BpArtifactDetailsEditorController} from "../bp-artifact/bp-details-editor";
import {ICollectionService} from "./collection.svc";
import {IStatefulCollectionArtifact, ICollection, ICollectionArtifact} from "./collection-artifact";
import {Helper} from "../../shared";
import {IMetaDataService} from "../../managers/artifact-manager";
import {ChangeSetCollector, ChangeTypeEnum, IChangeCollector, IChangeSet} from "../../managers/artifact-manager/changeset";
import {
    ILocalizationService,
    IArtifactManager,
    IMessageService,
    IWindowManager
} from "../bp-artifact/bp-artifact-editor";

import {IDialogService} from "../../shared";
import {IPropertyDescriptorBuilder} from "./../configuration/property-descriptor-builder";

export class BpArtifactCollectionEditor implements ng.IComponentOptions {
    public template: string = require("./bp-collection-editor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactCollectionEditorController;
}

export class BpArtifactCollectionEditorController extends BpArtifactDetailsEditorController {
    public static $inject: [string] = [
        "$state",
        "messageService",
        "artifactManager",
        "windowManager",
        "localization",
        "propertyDescriptorBuilder",
        "dialogService",
        "collectionService",
        "metadataService",
        "$location",
        "$window",
        "$scope"
    ];

    public selectAll: boolean = false;
    public selectAllClass: string;
    public isSystemPropertiesCollapsed: boolean = true;
    private collectionSubscriber: Rx.IDisposable;
    public selectedVMs: any[] = [];
    public itemsSelected: string;
    //public showBulkActions: boolean;
    //public selectedVMsLength: number;

    constructor(private $state: ng.ui.IStateService,
        messageService: IMessageService,
        artifactManager: IArtifactManager,
        windowManager: IWindowManager,
        localization: ILocalizationService,
        propertyDescriptorBuilder: IPropertyDescriptorBuilder,
        private dialogService: IDialogService,
        private collectionService: ICollectionService,
        private metadataService: IMetaDataService,
        private $location: ng.ILocationService,
        private $window: ng.IWindowService,
        private $scope: ng.IScope
    ) {
        super(messageService, artifactManager, windowManager, localization, propertyDescriptorBuilder);
    }

    public get reviewUrl(): string {
        if (this.artifact && (<IStatefulCollectionArtifact>this.artifact).rapidReviewCreated) {
            return this.$location.protocol() + "://" + this.$window.location.host + "/ArtifactMgmt/RapidReview/" + this.artifact.id;
        }

        return "";
    }

    private subscribeOnCollectionChanges(collectionArtifact: IStatefulCollectionArtifact) {
        if (this.collectionSubscriber) {
            this.collectionSubscriber.dispose();
            this.collectionSubscriber = null;
        }
        this.collectionSubscriber = collectionArtifact.collectionObservable().subscribe(this.onCollectionArtifactsChanged);
    }

    public onArtifactReady() {
        if (this.editor && this.artifact) {
            const collectionArtifact = this.artifact as IStatefulCollectionArtifact;
            this.metadataService.get(collectionArtifact.projectId).then(() => {
                this.rootNode = collectionArtifact.artifacts.map((a: ICollectionArtifact) => {
                    return new CollectionNodeVM(a, this.artifact.projectId, this.metadataService);
                });
                this.subscribeOnCollectionChanges(collectionArtifact);

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

    private onCollectionArtifactsChanged = (changes: IChangeSet[]) => {
        if (!changes || changes.length === 0) {
            return;
        }

        let collectionArtifacts = this.rootNode.slice();

        changes.map((change: IChangeSet) => {
            if (change.type === ChangeTypeEnum.Add) {
                let addedTreeVM = new CollectionNodeVM(change.value, this.artifact.projectId, this.metadataService);
                collectionArtifacts.push(addedTreeVM);
            }
            else if (change.type === ChangeTypeEnum.Delete) {
                const removingNodeIndex = collectionArtifacts.findIndex((nodeVM: CollectionNodeVM) => nodeVM.model.id === change.key);

                if (removingNodeIndex > -1) {
                    collectionArtifacts.splice(removingNodeIndex, 1);
                }
            }
        });

        this.rootNode = collectionArtifacts;
    };

    private headerCellRendererSelectAll(params) {
        let cb = document.createElement("i");
        cb.setAttribute("class", "ag-checkbox-unchecked");

        let sp = document.createElement("span");
        sp.setAttribute("class", "ag-group-checkbox");

        sp.appendChild(cb);

        let eHeader = document.createElement("span");
        eHeader.setAttribute("class", "ag-header-checkbox");
        eHeader.appendChild(sp);

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
        return eHeader;
    }

    public columns: IColumn[] = [
        {
            isCheckboxSelection: true,
            width: 30,
            headerName: "",
            headerCellRenderer: this.headerCellRendererSelectAll,
            field: "chck"
        },
        {
            width: 100,
            colWidth: 100,
            headerName: `<span class="header-name">` + this.localization.get("Label_ID") + `</span>`,
            field: "model.id",
            isGroup: true,
            isCheckboxHidden: true,
            cellClass: (vm: CollectionNodeVM) => vm.getCellClass(),
            innerRenderer: (params: IColumnRendererParams) => {
                const collectionNodeVM = <CollectionNodeVM>params.vm;
                const prefix = Helper.escapeHTMLText(collectionNodeVM.model.prefix);
                const icon = collectionNodeVM.getIcon();
                const url = this.$state.href("main.item", { id: collectionNodeVM.model.id });
                return `<span class="ag-group-value-wrapper">${icon} <a ng-href="${url}" target="_blank" class="collection__link"
                            ng-click="$event.stopPropagation();">${prefix}${collectionNodeVM.model.id}</a></span>`;
            }
        },
        {
            headerName: this.localization.get("Label_Name"),
            isGroup: true,
            isCheckboxHidden: true,
            innerRenderer: (params: IColumnRendererParams) => {
                const collectionNodeVM = <CollectionNodeVM>params.vm;
                const path = collectionNodeVM.model.artifactPath;

                let tooltipText = "";
                path.map((collectionArtifact: string, index: number) => {
                    if (index !== 0) {
                        tooltipText += " > ";
                    }

                    tooltipText = tooltipText + `${Helper.escapeHTMLText(collectionArtifact)}` ;
                });

                return `<div bp-tooltip="${collectionNodeVM.model.name}" bp-tooltip-truncated="true" class="collection__name">` +
                            `${collectionNodeVM.model.name}</div>` +
                            `<div bp-tooltip="${tooltipText}" bp-tooltip-truncated="true" class="path">` + tooltipText + `</div>`;
            }
        },
        {
            headerName: this.localization.get("Label_Description"),
            isGroup: true,
            isCheckboxHidden: true,
            innerRenderer: (params: IColumnRendererParams) => {
                const collectionNodeVM = <CollectionNodeVM>params.vm;
                if (collectionNodeVM.model.description) {
                    return `<div class="collection__description" bp-tooltip="${collectionNodeVM.model.description}" ` +
                        `bp-tooltip-truncated="true">${collectionNodeVM.model.description}</div>`;
                }

                return "";
            }
        },
        {
            headerName: this.localization.get("Label_Options"),
            isGroup: true,
            width: 60,
            colWidth: 60,
            isCheckboxHidden: true,
            innerRenderer: (params: IColumnRendererParams) => {
                params.$scope["removeArtifact"] = ($event) => {
                    $event.stopPropagation();

                    this.dialogService.confirm(this.localization.get("Artifact_Collection_Confirmation_Delete_Item")).then(() => {

                        const node = <CollectionNodeVM>params.vm;
                        const collectionArtifact = this.artifact as IStatefulCollectionArtifact;
                        collectionArtifact.removeArtifacts([node.model]);

                        let index = _.findIndex(this.selectedVMs, (item) => item.model.id === node.model.id);

                        if (index > -1) {
                            this.selectedVMs.splice(index, 1);
                            let item_selected = this.localization.get("Artifact_Collection_Items_Selected");
                            this.itemsSelected = item_selected.replace("{0}", (this.selectedVMs.length).toString());
                        }
                    });


                };
                return `<i class="icon icon__action fonticon-delete-filled" ng-click="removeArtifact($event)"></i>`;
            }
        }];

    public rootNode: CollectionNodeVM[] = [];

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

        let item_selected = this.localization.get("Artifact_Collection_Items_Selected");
        this.itemsSelected = item_selected.replace("{0}", (this.selectedVMs.length).toString());
        this.$scope.$applyAsync();
    }

    public bulkDelete() {
        let confirmation = this.localization.get("Artifact_Collection_Confirmation_Delete_Items")
            .replace("{0}", this.selectedVMs.length.toString());

        this.dialogService.confirm(confirmation).then(() => {
            const collectionArtifact = this.artifact as IStatefulCollectionArtifact;
            const artifactsToBeDeleted = _.map(this.selectedVMs, (node) => node.model);
            collectionArtifact.removeArtifacts(artifactsToBeDeleted);
            this.selectedVMs.length = 0;
        });
    }
}

class CollectionNodeVM implements ITreeViewNode {
    public key: string;

    constructor(public model: ICollectionArtifact, private projectId: number, private metadataService: IMetaDataService) {
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
        const typeName = Models.ItemTypePredefined[this.model.itemTypePredefined];
        if (typeName) {
            result.push("is-" + _.kebabCase(typeName));
        }
        return result;
    }

    public isSelectable(): boolean {
        return true;
    }
}
