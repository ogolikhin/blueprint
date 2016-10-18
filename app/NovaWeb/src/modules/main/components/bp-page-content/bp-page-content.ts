import {IWindowManager} from "../../services";
import {IArtifactManager} from "../../../managers";
import {IMessageService, INavigationService, ILocalizationService} from "../../../core";
import {IDiagramService} from "../../../editors/bp-diagram/diagram.svc";
//import {IDialogSettings, IDialogService} from "../../../shared";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PageContentCtrl;
    public controllerAs = "$content";
}

class PageContentCtrl {
    private subscribers: Rx.IDisposable[];

    public static $inject: [string] = [
        "messageService",
        "artifactManager",
        "diagramService",
        "windowManager",
        "navigationService",
        "dialogService"
    ];

    constructor(private messageService: IMessageService,
                private artifactManager: IArtifactManager,
                private diagramService: IDiagramService,
                private windowManager: IWindowManager,
                private navigationService: INavigationService,
                private dialogService: IDialogService    ) {
    }

    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
    }

    public onContentSelected($event: MouseEvent) {
        // if ($event.target && $event.target["tagName"] !== "BUTTON") {
        //     if (this.context) {
        //         this.selectionManager.selection = { artifact: this.context.artifact, source: SelectionSource.Editor };
        //     } else {
        //         this.selectionManager.clearSelection();
        //     }
        // }
    }

    public openArtifactPicker() {
        const dialogSettings = <IDialogSettings>{
            okButton: "Open",
            template: require("../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: "Single project Artifact picker"
        };

        const dialogData: IArtifactPickerOptions = {
            showSubArtifacts: false,
            isOneProjectLevel: true
        };

        this.dialogService.open(dialogSettings, dialogData);
    }

}
