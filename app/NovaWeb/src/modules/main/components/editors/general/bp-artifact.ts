import { Models } from "../../../";
import {IMessageService,} from "../../../../shell/";
import {IArtifactService} from "./artifact.svc";
import {ArtifactEditor} from "./bp-artifact-editor"


interface IFieldTab {
    title: string,
    index: number,
    fields: [AngularFormly.IFieldConfigurationObject],
    active?: boolean
};

interface IEditorContext {
    artifact?: Models.IArtifact;
    project?: Models.IProject;
}

export class BpArtifact implements ng.IComponentOptions {
    public template: string = require("./bp-artifact.html");
    public controller: Function = BpArtifactController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };

}

export class BpArtifactController {
    public static $inject: [string] = [
        "messageService", "artifactService"
    ];

    
    private _subscribers: Rx.IDisposable[];
    private editor: ArtifactEditor;


    constructor(
        private messageService: IMessageService,
        private artifactService: IArtifactService) {
    }


    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
    }

    public $onDestroy() {
    }

    public model = {};
    public tabs = [];
    public activeTab: number = 1;
    public fields: Models.IArtifactDetailFields = {
        systemFields: [],
        customFields: [],
        noteFields: []
    };

    private _context: IEditorContext;

    public set context(value: IEditorContext) {
        this._context = value;

        if (this._context && this._context.artifact && this._context.project) {
            this.artifactService.getArtifact(this._context.artifact.id).then((artifactDetails) => {
                //TODO: change
                angular.extend(this._context.artifact, artifactDetails);
                this.load(this._context.artifact, this._context.project);
            });
        }
    }

    public get isCustomPropertyAvailable(): boolean {
        return this.fields && this.fields.customFields && this.fields.customFields.length > 0;
    }

    private load(artifact: Models.IArtifact, project: Models.IProject) {
        try {

            if (!artifact || !project) {
                if (!artifact || !project) {
                    throw new Error("#Project_NotFound");
                }

            }
            this.activeTab = -1;

            this.editor = new ArtifactEditor(artifact, this._context.project);
            this.fields = this.editor.getFields();
            this.model = this.editor.getModel();
            this.tabs = this.fields.noteFields.map((it: AngularFormly.IFieldConfigurationObject, index: number) => {
                let tab = <IFieldTab>{
                    title: it.templateOptions.label,
                    index: index,
                    fields: [it],
                };
                delete it.templateOptions.label;
                return tab;
            });

            this.activeTab = 0;

        } catch (ex) {
            this.messageService.addError(ex["message"]);
        }

    }


}
