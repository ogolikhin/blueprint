import "angular";
import {ILocalizationService} from "../core/localization";
import {INotificationService, EventSubscriber} from "../core/notification";
import {IDialogService} from "../services/dialog.svc";
import * as Models from "./models/models";



export class MainViewComponent implements ng.IComponentOptions {
    public template: string = require("./main.view.html");
    public controller: Function = MainViewController;
    public transclude: boolean = true;
}

export interface IMainViewController {
    
}

export class MainViewController implements IMainViewController {
    private _currentArtifact: string;
    public get currentArtifact() {
        return this._currentArtifact;
    }

    public static $inject: [string] = ["notification", "dialogService"];
    constructor(private _notification: INotificationService, private dialogService: IDialogService) {
        this._notification.attach(EventSubscriber.Main, "exception", this.showError.bind(this));
        this._notification.attach(EventSubscriber.ProjectManager, "artifactchanged", this.displayArtifact.bind(this));
    }

    private displayArtifact(artifact: Models.IArtifact) {
        this._currentArtifact = `${artifact.prefix}${artifact.id}: ${artifact.name}`;
    }
    private showError(error: any) {
        this.dialogService.alert(`Error: ${error["message"] || ""}`);
    }
}
