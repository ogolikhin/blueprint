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
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<"
    };
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
        "metadataService"
    ];

    private collection: ICollection;

    constructor(private $state: ng.ui.IStateService,
        messageService: IMessageService,
        artifactManager: IArtifactManager,
        windowManager: IWindowManager,
        localization: ILocalizationService,
        dialogService: IDialogService,
        private collectionService: ICollectionService,
        private metadataService: IMetaDataService) {
        super(messageService, artifactManager, windowManager, localization, dialogService);

        //for (let i = 1; i <= 500; i++) {
        //    this.rootNode.push(new CollectionNodeVM({ id: i, name: `New Artifact ${i}`, description: "This is the description" } as Models.IArtifact));
        //}
    }
    public onArtifactReady() {
        super.onArtifactReady();
        if (this.editor && this.artifact) {            
            this.collectionService.getCollection(this.artifact.id).then((result: ICollection) => {
                this.metadataService.get(result.projectId).then(() => {
                    this.collection = result;
                    this.rootNode = result.artifacts.map((a: ICollectionArtifact) => new CollectionNodeVM(a, result.projectId, this.metadataService));
                }).catch((error: any) => {
                    //ignore authentication errors here
                    if (error) {
                        this.messageService.addError(error["message"] || "Project_MetaDataNotFound");
                    }
                }).finally(() => {
                    //this.isLoading = false;
                });                   
            }).catch((error: any) => {
                //ignore authentication errors here
                if (error) {
                    this.messageService.addError(error["message"] || "Artifact_NotFound");
                }
            }).finally(() => {
                //this.isLoading = false;
            });               
        }
    }
   
           

    public columns: IColumn[] = [
        {
            isCheckboxSelection: true,
            width: 20
        },
        {
            headerName: "ID",
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
            headerName: "Name",
            field: "model.name"         
        },
        {
            headerName: "Description",
            field: "model.description"
        },
        {
            headerName: "Options",            
            isGroup: true,
            width: 50,
            isCheckboxHidden: true,            
            innerRenderer: (vm: CollectionNodeVM, eGridCell: HTMLElement) => {               
                return `<i class="icon fonticon-delete-filled"></i>`;
            }          
        }];

    public rootNode: CollectionNodeVM[] = [];
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
            result.push("is-" + Helper.toDashCase(typeName));
        }
        return result;
    }

    public isSelectable(): boolean {
        return true;
    }   
}
