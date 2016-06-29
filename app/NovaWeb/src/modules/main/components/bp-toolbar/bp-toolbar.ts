import { ILocalizationService, IDialogSettings, IDialogService } from "../../../core";
import { IMessageService, Message } from "../../../shell";
import { IProjectManager, Models } from "../../";
import { OpenProjectController } from "../dialogs/open-project";

interface IBPToolbarController {
    execute(evt: ng.IAngularEvent): void;
    showSubLevel(evt: ng.IAngularEvent): void;
}

export class BPToolbar implements ng.IComponentOptions {
    public template: string = require("./bp-toolbar.html");
    public controller: Function = BPToolbarController;
}

class BPToolbarController implements IBPToolbarController {

    private _subscribers: Rx.IDisposable[];
    private _currentArtifact: number;

    public get currentArtifact() {
        return this._currentArtifact;
    }
    static $inject = ["localization", "dialogService", "projectManager", "messageService", "$rootScope"];

    constructor(
        private localization: ILocalizationService,
        private dialogService: IDialogService,
        private projectManager: IProjectManager,
        private messageService: IMessageService,
        private $rootScope: ng.IRootScopeService) {
    }

    execute(evt: any): void {
        if (!evt) {
            return;
        }
        evt.preventDefault();
        var element = evt.currentTarget;
        switch (element.id.toLowerCase()) {
            case `projectclose`:
                this.projectManager.closeProject();
                break;
            case `projectcloseall`:
                this.projectManager.closeProject(true);
                break;
            case `deleteartifact`:
//                this.deleteArtifact();
                //NOTE: this is temporary solution to show differetnt type of messages. 
                //TODO:: Will be removed
                /* tslint:disable:max-line-length */
                this.messageService.addMessage(new Message(1, "<b>Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.</b>"));
                this.messageService.addMessage(new Message(2, "2"));
                this.messageService.addMessage(new Message(3, "3"));
                this.messageService.addMessage(new Message(1, "Section 1.10.32 of de Finibus Bonorum et Malorum, written by Cicero in 45 BC"));
                this.messageService.addMessage(new Message(2, "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."));
                this.messageService.addMessage(new Message(3, "It is a long established fact that a reader will be distracted by the "));
        /* tslint:enable:max-line-length */

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
            css: "nova-open-project" // removed modal-resize-both as resizing the modal causes too many artifacts with ag-grid
        }).then((project: Models.IProject) => {
            if (project) {
                this.projectManager.loadProject(project);
            }
        });
    }

    //temporary
    private deleteArtifact() {
        this.dialogService.confirm("This is simple confirmation message.<br/><br/> Please confirm.", "Please confirm")
            .then((confirmed: boolean) => {
                if (confirmed) {
                    this.dialogService.alert("Delete is confirmed");
                }
            });
    }

    public goToImpactAnalysis() {
        let url = `Web/#/ImpactAnalysis/${this._currentArtifact}`;
        window.open(url);
    }

    public $onInit(o) {
        let selectedArtifactSubscriber: Rx.IDisposable = this.projectManager.currentArtifact
            .distinctUntilChanged()
            .subscribe(this.displayArtifact);

        this._subscribers = [
            selectedArtifactSubscriber
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private displayArtifact = (artifact: Models.IArtifact) => {
        this._currentArtifact = artifact && artifact.prefix && artifact.prefix !== "ACO" && artifact.prefix !== "_CFL" ? artifact.id : null;
    }

}