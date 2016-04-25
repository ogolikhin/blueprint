import {ILocalizationService} from "../../../core/localization";
import {IDialogSettings, IDialogService} from "../../../services/dialog.svc";
import {OpenProjectController} from "../dialogs/openprojectcontroller";


interface IToolbarController {
    add(): void;
    clear(): void;
    execute(evt: ng.IAngularEvent): void;
    showSubLevel(evt: ng.IAngularEvent): void;
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

    showSubLevel(evt: any): void {
        // this is needed to allow tablets to show submenu (as touch devices don't understand hover)
        if (!evt) {
            return;
        }
        evt.preventDefault();
        evt.stopImmediatePropagation();
    }

    toggleFullScreenOnMobile(): void {
        // requestFullScreen can only be initiated by a user gesture and works for mobile devices only
        var doc: any = window.document;
        var docEl = doc.documentElement;

        var requestFullScreen = docEl.requestFullscreen || docEl.mozRequestFullScreen || docEl.webkitRequestFullScreen || docEl.msRequestFullscreen;
        var cancelFullScreen = doc.exitFullscreen || doc.mozCancelFullScreen || doc.webkitExitFullscreen || doc.msExitFullscreen;

        if (!doc.fullscreenElement && !doc.mozFullScreenElement && !doc.webkitFullscreenElement && !doc.msFullscreenElement) {
            requestFullScreen.call(docEl);
        } else {
            cancelFullScreen.call(doc);
        }
    }

    public openProject() {
        this.dialogService.open(<IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../dialogs/openprojectdialog.html"),
            controller: OpenProjectController,
            css: "nova-open-project"
        }).then((selected: any) => {
            if (selected && selected.Id) {
                this.dialogService.alert("Project \"" + selected.Name + "\" is selected. Id:[" + selected.Id + "]");
            }
        });
    }

    public deleteArtifact() {
        this.dialogService.confirm("This is simple confirmation message.<br/><br/> Please confirm.", "Please confirm")
            .then((confirmed: boolean) => {
                if (confirmed) {
                    this.dialogService.alert("Delete is confirmed");
                }
            });
    }

}