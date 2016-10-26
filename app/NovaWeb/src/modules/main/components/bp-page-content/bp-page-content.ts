import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PageContentCtrl;
}

class PageContentCtrl {
    private subscribers: Rx.IDisposable[];

    public static $inject: [string] = [
        "dialogService"
    ];

    constructor(private dialogService: IDialogService) {
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
