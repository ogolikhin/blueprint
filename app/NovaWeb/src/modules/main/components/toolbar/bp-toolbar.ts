import {ILocalizationService} from "../../../core/localization";
import {IDialogSettings, IDialogService} from "../../../services/dialog.svc";
import {IOpenProjectResult, OpenProjectController} from "../dialogs/openproject.ctrl";
import {IMainViewController} from "../../main.view";

interface IBPToolbarController {
    execute(evt: ng.IAngularEvent): void;
    showSubLevel(evt: ng.IAngularEvent): void;
}

export class BPToolbar implements ng.IComponentOptions {
    public template: string;
    public controller: Function;
    public require : any;

    constructor() {
        this.template = require("./bp-toolbar.html");
        this.controller = BPToolbarController;
        this.require = {
            parent: "^bpMainView"
        } ;
    }
}

class BPToolbarController implements IBPToolbarController {

    static $inject = ["localization", "dialogService" ];
    public parent: IMainViewController;
    constructor(private localization: ILocalizationService, private dialogService: IDialogService) {
    }

    execute(evt: any): void {
        if (!evt) {
            return;
        }
        evt.preventDefault();
        var element = evt.currentTarget;
        this.dialogService.alert(`Selected Action is ${element.id || element.innerText}`);
    }

    showSubLevel(evt: any): void {
        // this is needed to allow tablets to show submenu (as touch devices don't understand hover)
        if (!evt) {
            return;
        }
        evt.preventDefault();
        evt.stopImmediatePropagation();
    }

    // not used yet and maybe not needed
    /*toggleFullScreenOnMobile(): void {
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
    }*/

    public openProject() {
        
        this.dialogService.open(<IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../dialogs/openproject.template.html"),
            controller: OpenProjectController,
            css: "nova-open-project"
        }).then((selected: IOpenProjectResult) => {
            if (selected && selected.id) {
                this.dialogService.alert(`Project \"${selected.name} [ID:${selected.id}]\" is selected.`);
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