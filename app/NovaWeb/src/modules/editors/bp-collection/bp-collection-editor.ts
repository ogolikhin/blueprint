import * as angular from "angular";
import * as _ from "lodash";
import {Models, Enums} from "../../main";
import {IColumn, ITreeViewNodeVM} from "../../shared/widgets/bp-tree-view/";
import {BpArtifactDetailsEditorController} from "../bp-artifact/bp-details-editor";
import {ICollectionService} from "./collection.svc";
import {ICollection, ICollectionArtifact} from "./models";
import {Helper} from "../../shared";
import {IMetaDataService} from "../../managers/artifact-manager";


import {
    BpArtifactEditor,
    ILocalizationService,
    IArtifactManager,
    IMessageService,
    IWindowManager,
    PropertyContext
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

    public collection: ICollection;
    public selectAll: boolean = false;
    public selectAllClass: string;
    public isSystemPropertiesCollapsed: boolean = true;
    //public reviewUrl: string;

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
        if (this.collection && this.collection.isCreated) {
            return this.$location.protocol() + "://" + this.$window.location.host + "/ArtifactMgmt/RapidReview/" + this.collection.id;
        }

        return "";
    }   

    public onArtifactReady() {
        if (this.editor && this.artifact) {
            this.collectionService.getCollection(this.artifact.id).then((result: ICollection) => {
                this.metadataService.get(result.projectId).then(() => {
                    this.collection = result;
                    this.rootNode = result.artifacts.map((a: ICollectionArtifact) => {
                        return new CollectionNodeVM(a, result.projectId, this.metadataService);
                    });
                }).catch((error: any) => {
                    //ignore authentication errors here
                    if (error) {
                        this.messageService.addError(error["message"] || "Project_MetaDataNotFound");
                    }
                }).finally(() => {
                    super.onArtifactReady();
                });
            }).catch((error: any) => {
                if (error) {
                    this.messageService.addError(error["message"] || "Artifact_NotFound");
                }
                super.onArtifactReady();
            });
        }
        else {
            super.onArtifactReady();
        }
    }

    private headerCellRendererSelectAll(params) {
        let cb = document.createElement("input");
        cb.setAttribute("type", "checkbox");
        cb.setAttribute("id", "selectAllCheckbox");
        cb.setAttribute("type", "checkbox");
        cb.setAttribute("selected", params.context.allSelected);       

        let eHeader = document.createElement("label");
        let eTitle = document.createTextNode(params.colDef.headerName);
        eHeader.appendChild(cb);
        eHeader.appendChild(eTitle);

        cb.addEventListener("change", function (e) {
            let checked: boolean = (<HTMLInputElement>e.target).checked;
            params.context.allSelected = checked;
            console.log(params.context.allSelected);
            console.log(typeof e.target);
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
                return `<span class="ag-group-value-wrapper">${icon} <a ng-href="${url}" target="_blank">${prefix}${vm.model.id}</a></span>`;
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

class CollectionNodeVM implements ITreeViewNodeVM {
    public key: string;

    constructor(public model: ICollectionArtifact, private projectId: number, private metadataService: IMetaDataService) {
        this.key = String(model.id);
    }



    public getIcon(): string {
        let artifactType = this.metadataService.getArtifactItemTypeTemp(this.projectId, this.model.itemTypeId);
        if (artifactType && artifactType.iconImageId && angular.isNumber(artifactType.iconImageId)) {
            return `<bp-item-type-icon item-type-id="${artifactType.id}" item-type-icon="${artifactType.iconImageId}"></bp-item-type-icon>`;
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
