import {IProjectManager, Models} from "../..";
import {ILocalizationService } from "../../../core";
import {IMessageService, Message, MessageType} from "../../../shell";

import {ArtifactEditor} from "./editor-view"

export class BpArtifactDetails implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-details.html");
    public controller: Function = BpArtifactDetailsController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        currentArtifact: "<",
    };
    public transclude: boolean = true;
}

interface IFieldTab {
    title: string,
    index: number,
    fields: [AngularFormly.IFieldConfigurationObject],
    active?: boolean
};



export class BpArtifactDetailsController {
    private _subscribers: Rx.IDisposable[];
    static $inject: [string] = ["$scope", "localization", "messageService",  "projectManager"];
    private _artifact: Models.IArtifact;

    public currentArtifact: string;

    constructor(private $scope,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private projectManager: IProjectManager) {
    }
    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.projectManager.currentArtifact.subscribeOnNext(this.loadView, this),
        ];
    }
    public model = {};
    public tabs = [];
    public activeTab: number = 1;


    public fields: Models.IArtifactDetailFields = {
        systemFields: [],
        customFields: [],
        noteFields: []
    };

    public get isCustomPropertyAvailable(): boolean {
        return this.fields && this.fields.customFields && this.fields.customFields.length > 0;
    }
    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this._artifact = null;
        this.model = null;
        this.fields = null;
        this.tabs = null;
        this.editor = null;

    }

    private updateArtifact(artifact: Models.IArtifact): any { 
        return {};
    }

    private editor: ArtifactEditor;

//    private properties: Models.IPropertyType[];
    public loadView(artifact: Models.IArtifact) {
        try {

            if (!artifact) {
                return;
            }
            this.activeTab = -1;

            //this.editor = new ArtifactEditor(artifact, this.projectManager.getArtifctPropertyTypes(artifact));
            //this.fields = this.editor.getFields();
            //this.model = this.editor.getModel();
            //this.tabs = this.fields.noteFields.map((it: AngularFormly.IFieldConfigurationObject, index: number) => {
            //    let tab = <IFieldTab>{
            //        title: it.templateOptions.label,
            //        index: index,
            //        fields: [it],
            //    };
            //    delete it.templateOptions.label;
            //    return tab;
            //});

            this.activeTab = 0;
        } catch(ex) {
            this.messageService.addError(ex["message"]);
        }

    }

}
