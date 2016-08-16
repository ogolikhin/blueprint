﻿import { ILocalizationService } from "../../../core";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IMessageService } from "../../../core";
import { IProjectManager, Models, ISelectionManager } from "../../";
import { OpenProjectController } from "../dialogs/open-project";
import { BPTourController } from "../dialogs/bp-tour/bp-tour";

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
    static $inject = ["localization", "dialogService", "projectManager", "selectionManager", "messageService", "$rootScope"];

    constructor(
        private localization: ILocalizationService,
        private dialogService: IDialogService,
        private projectManager: IProjectManager,
        private selectionManager: ISelectionManager,
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
                this.dialogService.open(<IDialogSettings>{
                    okButton: this.localization.get("App_Button_Ok"),
                    template: require("../../../shared/widgets/bp-dialog/bp-dialog.html"),
                    header: this.localization.get("App_DialogTitle_Alert"),
                    message: "Are you sure you would like to delete the artifact"
                }).then((confirm: boolean) => {
                    if (confirm) {
                        this.dialogService.alert("you clicked confirm!");
                    }
                })
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
    }

    public goToImpactAnalysis() {
        let url = `Web/#/ImpactAnalysis/${this._currentArtifact}`;
        window.open(url);
    }

    public openTour() {
        this.dialogService.open(<IDialogSettings>{
            template: require("../dialogs/bp-tour/bp-tour.html"),
            controller: BPTourController,
            backdrop: true,
            css: "nova-tour"
        });
    }

    public $onInit(o) {
        this._subscribers = [
            this.selectionManager.selectedArtifactObservable.subscribe(this.displayArtifact)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private displayArtifact = (artifact: Models.IArtifact) => {
        this._currentArtifact = 
            artifact && artifact.prefix && artifact.prefix !== "ACO" && artifact.prefix !== "_CFL" && 
            artifact.version !== 0 ? artifact.id : null;
    }

}