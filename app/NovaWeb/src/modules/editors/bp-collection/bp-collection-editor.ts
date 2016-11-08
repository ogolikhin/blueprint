import * as _ from "lodash";
import {Models} from "../../main";
import {IColumn, ITreeViewNode, IColumnRendererParams} from "../../shared/widgets/bp-tree-view/";
import {BpArtifactDetailsEditorController} from "../bp-artifact/bp-details-editor";
import {ICollectionService} from "./collection.svc";
import {IStatefulCollectionArtifact, ICollectionArtifact} from "./collection-artifact";
import {Helper} from "../../shared";
import {IMetaDataService} from "../../managers/artifact-manager";
import {ChangeTypeEnum, IChangeSet} from "../../managers/artifact-manager/changeset";
import {IDialogService} from "../../shared";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IArtifactManager} from "../../managers/artifact-manager/artifact-manager";
import {IWindowManager} from "../../main/services/window-manager";

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
    private collectionSubscriber: Rx.IDisposable;

    constructor(private $state: ng.ui.IStateService,
                messageService: IMessageService,
                artifactManager: IArtifactManager,
                windowManager: IWindowManager,
                localization: ILocalizationService,
                dialogService: IDialogService,
                private collectionService: ICollectionService,
                private metadataService: IMetaDataService,
                private $location: ng.ILocationService,
                private $window: ng.IWindowService) {
        super(messageService, artifactManager, windowManager, localization, dialogService);
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
                let removingNodeIndex = collectionArtifacts.findIndex((nodeVM: CollectionNodeVM) => {
                    if (nodeVM.model.id === change.key) {
                        return true;
                    }
                    return false;
                });

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
                const url = this.$state.href("main.item", {id: collectionNodeVM.model.id});
                return `<span class="ag-group-value-wrapper">${icon} <a ng-href="${url}" target="_blank" 
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

                    tooltipText = tooltipText + `${Helper.escapeHTMLText(collectionArtifact)}`;
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
                params.$scope["removeArtifact"] = () => {
                    const node = <CollectionNodeVM>params.vm;
                    const collectionArtifact = this.artifact as IStatefulCollectionArtifact;
                    collectionArtifact.removeArtifacts([node.model]);
                };
                return `<i class="icon icon__normal fonticon-delete-filled" ng-click="removeArtifact()"></i>`;
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
