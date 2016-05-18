/// <reference path="../../../core/notification.ts" />
import {ILocalizationService} from "../../../core/localization";
import {IDialogSettings, IDialogService} from "../../../services/dialog.svc";
import {IProjectNotification, SubscriptionEnum} from "../../services/project-notification";
import {IOpenProjectResult, OpenProjectController} from "../dialogs/open-project.ctrl";

interface IBPToolbarController {
    execute(evt: ng.IAngularEvent): void;
    showSubLevel(evt: ng.IAngularEvent): void;
}

export class BPToolbarComponent implements ng.IComponentOptions {
    public template: string = require("./bp-toolbar.html");
    public controller: Function = BPToolbarController;
}

class BPToolbarController implements IBPToolbarController {

    static $inject = ["localization", "dialogService", "projectNotification" ];

    constructor(private localization: ILocalizationService, private dialogService: IDialogService, private notificator: IProjectNotification) {
    }

    execute(evt: any): void {
        if (!evt) {
            return;
        }
        evt.preventDefault();
        var element = evt.currentTarget;
        switch (element.id) {
            case `ProjectClose`:
                this.notificator.notify(SubscriptionEnum.ProjectClose);
                break;
            case `ProjectCloseAll`:
                this.notificator.notify(SubscriptionEnum.ProjectClose, true);
                break;
            default:
                this.dialogService.alert(`Selected Action is ${element.id || element.innerText}`);
                break;
        }
    }

    showSubLevel(evt: any): void {
        // this is needed to allow tablets to show submenu (as touch devices don't understand hover)
        if (!evt) {
            return;
        }
        evt.preventDefault();
        evt.stopImmediatePropagation();
    }


    public openProject() {
        this.dialogService.open(<IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../dialogs/open-project.template.html"),
            controller: OpenProjectController,
            css: "nova-open-project"
        }).then((selected: IOpenProjectResult) => {
            if (selected && selected.id) {
                this.notificator.notify(SubscriptionEnum.ProjectLoad, selected.id, selected.name);
            }
        });
    }

    //temporary
    public deleteArtifact() {
        this.dialogService.confirm("This is simple confirmation message.<br/><br/> Please confirm.", "Please confirm")
            .then((confirmed: boolean) => {
                if (confirmed) {
                    this.dialogService.alert("Delete is confirmed");
                }
            });
    }

}