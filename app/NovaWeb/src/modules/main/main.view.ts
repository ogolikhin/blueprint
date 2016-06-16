﻿import "angular";
//import {ILocalizationService} from "../core/localization";
import {IDialogService, IEventManager, EventSubscriber} from "../core/";
import {IProjectManager, Models} from "./";



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
//        this.projectManager.initialize();
        this.projectManager.currentArtifact.asObservable().subscribe(this.displayArtifact);
        this._listeners = [
            this.eventManager.attach(EventSubscriber.Main, "exception", this.showError),
            this.eventManager.attach(EventSubscriber.ProjectManager, "artifactchanged", this.displayArtifact),
        ];
    }
    public $onDestroy() {
        this._listeners.map(function (it) {
            this.eventManager.detachById(it);
        }.bind(this));
    }

    private alert(obj, property, value) {
        this.dialogService.alert(`Object changed: Property:[${property} Value:[${value}]`);
    }
    
    private displayArtifact = (artifact: Models.IArtifact) => {
        if (artifact) {
            this._currentArtifact = `${artifact.prefix}${artifact.id}: ${artifact.name}`;
        } else {
            this._currentArtifact = null;
        }
    }
    private showError = (error: any) => {
        this.dialogService.alert(`Error: ${error["message"] || ""}`);
    }
}
