import "angular";
//import {ILocalizationService} from "../core/localization";
import {IDialogService, IEventManager, EventSubscriber} from "../core/";
import {IProjectManager} from "./";
import * as Models from "./models/models";



export class MainViewComponent implements ng.IComponentOptions {
    public template: string = require("./main.view.html");
    public controller: Function = MainViewController;
    public transclude: boolean = true;
}

export interface IMainViewController {
    
}

export class MainViewController implements IMainViewController {
    private _listeners: string[];
    private _currentArtifact: string;
    public get currentArtifact() { 
        return this._currentArtifact;
    }

    static $inject: [string] = ["$state", "eventManager", "projectManager", "dialogService"];
    constructor(
        private $state: ng.ui.IState,
        private eventManager: IEventManager,
        private projectManager: IProjectManager,
        private dialogService: IDialogService) {
    }

    public $onInit() {
        this.projectManager.initialize();
        this._listeners = [
            this.eventManager.attach(EventSubscriber.Main, "exception", this.showError.bind(this)),
            this.eventManager.attach(EventSubscriber.ProjectManager, "artifactchanged", this.displayArtifact.bind(this))
        ];
    }
    public $onDestroy() {
        this._listeners.map(function (it) {
            this.eventManager.detachById(it);
        }.bind(this));
    }
    
    private displayArtifact(artifact: Models.IArtifact) {
        this._currentArtifact = `${artifact.prefix}${artifact.id}: ${artifact.name}`;
    }
    private showError(error: any) {
        this.dialogService.alert(`Error: ${error["message"] || ""}`);
    }
}
