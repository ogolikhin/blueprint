﻿import { ILocalizationService } from "../../core";
import { Helper } from "../../core/utils/helper";
import { IProjectManager, Models } from "../../main";

export class BPUtilityPanel implements ng.IComponentOptions {
    public template: string = require("./bp-utility-panel.html");
    public controller: Function = BPUtilityPanelController;
}

export class BPUtilityPanelController {
    public static $inject: [string] = [
        "localization",
        "projectManager"
    ];

    private _subscribers: Rx.IDisposable[];
    private _currentArtifact: string;
    private _currentArtifactClass: string;

    public get currentArtifact() { 
        return this._currentArtifact;
    }

    public get currentArtifactClass() {
        return this._currentArtifactClass;
    }

    constructor(
        private localization: ILocalizationService,
        private projectManager: IProjectManager) {
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
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
        this._currentArtifact = artifact ? `${(artifact.prefix || "")}${artifact.id}: ${artifact.name}` : null;
        this._currentArtifactClass = artifact ?
        "icon-" + Helper.dashCase(Models.ItemTypePredefined[artifact.predefinedType] || "document") :
            "icon-document";
    }
}
