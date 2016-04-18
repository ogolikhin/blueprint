import {ILocalizationService} from "../../../core/localization";
import {IDialogOptions, IDialogService} from "../dialogs/dialog.svc";
import {OpenProjectController} from "../dialogs/openprojectcontroller";


interface IToolbarController {
    add(): void;
    clear(): void;
    execute(evt: ng.IAngularEvent): void;
}


export class Toolbar implements ng.IComponentOptions {
    public template: string;
    public controller: Function;
    public require: string;

    constructor() {
        this.template = require("./toolbar.html");
        this.controller = ToolbarCtrl;
        this.require = "^parent";
    }
}

class ToolbarCtrl implements IToolbarController {

    static $inject = ["localization", "dialogService" ];

    constructor(private localization: ILocalizationService, private dialogService: IDialogService) {
    }

    add(): void {
    }

    clear(): void {
    }

    execute(evt: any): void {
        if (!evt) {
            return;
        }
        evt.preventDefault();
        var element = evt.currentTarget;
        this.dialogService.alert("Selected Action is " + (element.id || element.innerText));
    }

    public openProject() {
        this.dialogService.open(<IDialogOptions>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../dialogs/openprojectdialog.html"),
            controller: OpenProjectController,
        }).then((id: number) => {
            this.dialogService.alert("Project is selected: " + id);
        });
    }

    public deleteArtifact() {
        this.dialogService.confirm("This is simple confirmation message.<br/><br/> Please confirm.")
            .then((confirmed: boolean) => {
                if (confirmed) {
                    this.dialogService.alert("Delete is confirmed");
                }
            });
    }

}