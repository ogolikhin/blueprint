import {Models, Enums} from "../../main";
import {IColumn, ITreeViewNodeVM} from "../../shared/widgets/bp-tree-view/";
import {BpArtifactDetailsEditorController} from "../bp-artifact/bp-details-editor";

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
        "messageService",
        "artifactManager",
        "windowManager",
        "localization",
        "dialogService"
    ];

    constructor(messageService: IMessageService,
        artifactManager: IArtifactManager,
        windowManager: IWindowManager,
        localization: ILocalizationService,
        dialogService: IDialogService) {
        super(messageService, artifactManager, windowManager, localization, dialogService);

        for (let i = 1; i <= 500; i++) {
            this.rootNode.push(new CollectionNodeVM({ id: i, name: `New Artifact ${i}`, description: "This is the description" } as Models.IArtifact));
        }
    }
   
    public $onDestroy() {      
        super.$onDestroy();
    }
           

    public columns: IColumn[] = [{
        isCheckboxSelection: true
    }, {
            headerName: "ID",
            field: "model.id",
            isSortable: true,
            filter: "number"
        }, {
            headerName: "Name",
            field: "model.name",
            isSortable: true,
            filter: "text"
        }, {
            headerName: "Description",
            field: "model.description"
        }, {
            headerName: "Options"
        }];

    public rootNode: CollectionNodeVM[] = [];
}

class CollectionNodeVM implements ITreeViewNodeVM {
    public key: string;

    constructor(public model: Models.IArtifact) {
        this.key = String(model.id);
    }

    public isSelectable(): boolean {
        return true;
    }
}
