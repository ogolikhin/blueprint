import * as angular from "angular";
import * as _ from "lodash";
import {Models, Enums} from "../../main";
import {IColumn, ITreeViewNode} from "../../shared/widgets/bp-tree-view/";
import {BpArtifactDetailsEditorController} from "../bp-artifact/bp-details-editor";
import {ICollectionService} from "./collection.svc";
import {IStatefulCollectionArtifact, ICollection, ICollectionArtifact} from "./collection-artifact";
import {Helper} from "../../shared";
import {IMetaDataService} from "../../managers/artifact-manager";
import {
    ILocalizationService,
    IArtifactManager,
    IMessageService,
    IWindowManager
} from "../bp-artifact/bp-artifact-editor";

import {IDialogService} from "../../shared";

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
        "dialogService",
        "collectionService",
        "metadataService",
        "$location",
        "$window"
    ];

    public selectAll: boolean = false;
    public selectAllClass: string;
    public isSystemPropertiesCollapsed: boolean = true;

    constructor(private $state: ng.ui.IStateService,
        messageService: IMessageService,
        artifactManager: IArtifactManager,
        windowManager: IWindowManager,
        localization: ILocalizationService,
        dialogService: IDialogService,
        private collectionService: ICollectionService,
        private metadataService: IMetaDataService,
        private $location: ng.ILocationService,
        private $window: ng.IWindowService
    ) {
        super(messageService, artifactManager, windowManager, localization, dialogService);
    }

    public get reviewUrl(): string {
        if (this.artifact && (<IStatefulCollectionArtifact>this.artifact).rapidReviewCreated) {
            return this.$location.protocol() + "://" + this.$window.location.host + "/ArtifactMgmt/RapidReview/" + this.artifact.id;
        }

        return "";
    }

    public onArtifactReady() {
        if (this.editor && this.artifact) {
            const collectionArtifact = this.artifact as IStatefulCollectionArtifact;
            this.metadataService.get(collectionArtifact.projectId).then(() => {
                this.rootNode = collectionArtifact.artifacts.map((a: ICollectionArtifact) => {
                    return new CollectionNodeVM(a, this.artifact.projectId, this.metadataService);
                });

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
            innerRenderer: (vm: CollectionNodeVM, eGridCell: HTMLElement) => {
                const prefix = Helper.escapeHTMLText(vm.model.prefix);
                const icon = vm.getIcon();
                const url = this.$state.href("main.item", { id: vm.model.id });
                return `<span class="ag-group-value-wrapper">${icon} <a ng-href="${url}" target="_blank" 
                            ng-click="$event.stopPropagation();">${prefix}${vm.model.id}</a></span>`;
            }
        },
        {
            headerName: this.localization.get("Label_Name"),
            isGroup: true,
            isCheckboxHidden: true,
            innerRenderer: (vm: CollectionNodeVM, eGridCell: HTMLElement) => {
                const path = vm.model.artifactPath;

                let tooltipText = "";
                path.map((collectionArtifact: string, index: number) => {
                    if (index !== 0) {
                        tooltipText += " > ";
                    }

                    tooltipText = tooltipText + `${Helper.escapeHTMLText(collectionArtifact)}` ;
                });

                return `<div bp-tooltip="${vm.model.name}" bp-tooltip-truncated="true" class="collection__name">${vm.model.name}</div>` +
                            `<div bp-tooltip="${tooltipText}" bp-tooltip-truncated="true" class="path">` + tooltipText + `</div>`;
            }
        },
        {
            headerName: this.localization.get("Label_Description"),
            isGroup: true,
            isCheckboxHidden: true,
            innerRenderer: (vm: CollectionNodeVM, eGridCell: HTMLElement) => {
                if (vm.model.description) {
                    return `<div class="collection__description" bp-tooltip="${vm.model.description}" ` +
                        `bp-tooltip-truncated="true">${vm.model.description}</div>`;
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
            innerRenderer: (vm: CollectionNodeVM, eGridCell: HTMLElement) => {
                return `<i class="icon icon__normal fonticon-delete-filled"></i>`;
            }
        }];

    public rootNode: CollectionNodeVM[] = [];

    public toggleAll(): void {
        this.selectAll = !this.selectAll;
        this.selectAllClass = this.selectAll ? "ag-checkbox-checked" : "ag-checkbox-unchecked";
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
        return `<i></i>`;
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
