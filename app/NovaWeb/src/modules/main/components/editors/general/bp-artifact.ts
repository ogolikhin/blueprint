import {Models} from "../../../";
import {IMessageService} from "../../../../shell/";
import {IArtifactService} from "../../../services/";
import {PropertyEditor, FieldContext} from "./bp-artifact-editor";


interface IEditorContext {
    artifact?: Models.IArtifact;
    project?: Models.IProject;
    propertyTypes?: Models.IPropertyType[];
}

interface IArtifactDetailFields {
    systemFields: AngularFormly.IFieldConfigurationObject[];
    customFields: AngularFormly.IFieldConfigurationObject[];
    richTextFields: AngularFormly.IFieldConfigurationObject[];
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
        "messageService",
        "artifactService"
    ];

    private editor: PropertyEditor;

    constructor(
        private messageService: IMessageService,
        private artifactService: IArtifactService) {
    }


    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
    }

    public $onDestroy() {
    }

    public $onChanges(changesObj) {
        if (changesObj.context) {
            this._context = changesObj.context.currentValue;

//            this.activeTab = -1;
            this.fields = null;
            this.model = null;
            if (this._context && this._context.artifact && this._context.propertyTypes) {
                //is it a project?
                if (this._context.artifact.id === this._context.artifact.projectId) {
                    this.load(this._context.artifact, this._context.propertyTypes);
                } else {
                    this.artifactService.getArtifact(this._context.artifact.id).then((artifactDetails) => {
                        //TODO: change
                        angular.extend(this._context.artifact, { propertyValues: artifactDetails.propertyValues });
                        this.load(this._context.artifact, this._context.propertyTypes);
                    });
                }
            }
        }
    }
    public form: angular.IFormController;
    public model = {};
    public activeTab: number;
    public fields: IArtifactDetailFields = {
        systemFields: [],
        customFields: [],
        richTextFields: []
    };

    private _fields: AngularFormly.IFieldConfigurationObject[];

    private _context: IEditorContext;

    public get isCustomPropertyAvailable(): boolean {
        return this.fields && this.fields.customFields && this.fields.customFields.length > 0;
    }
    public get isSystemPropertyAvailable(): boolean {
        return this.fields && this.fields.systemFields && this.fields.systemFields.length > 0;
    }
    public get isTabPropertyAvailable(): boolean {
        return this.fields && this.fields.richTextFields && this.fields.richTextFields.length > 0;
    }
    public onPropertyChange($viewValue, $modelValue, scope) {
    };


    private load(artifact: Models.IArtifact, propertyTypes: Models.IPropertyType[]) {
        try {

            if (!artifact) {
                throw new Error("#Project_NotFound");
            }
            let fieldContexts = propertyTypes.map((it: Models.IPropertyType) => {
                switch (it.propertyTypePredefined) {
                    case Models.PropertyTypePredefined.Name:
                        return new FieldContext(it, "name");
                    case Models.PropertyTypePredefined.ItemType:
                        return new FieldContext(it, "itemTypeId");
                    default:
                        return new FieldContext(it);
                }
            });


            this.editor = new PropertyEditor(artifact, fieldContexts);
            this.model = this.editor.getModel();

            this.fields = <IArtifactDetailFields>{
                systemFields: [],
                customFields: [],
                richTextFields: []
            };
            this._fields = this.editor.getFields();
            
            this._fields.forEach((it: AngularFormly.IFieldConfigurationObject) => {
                //add property change handler to each field
                angular.extend(it.templateOptions, {
                    onChange: this.onPropertyChange
                });

                if (true === it.data["isSystem"]) {
                    this.fields.systemFields.push(it);
                } else if (true === it.data["isRichText"]) {
                    this.fields.richTextFields.push(it);
                } else {
                    this.fields.customFields.push(it);
                } 
            });
            this.form && this.form.$setPristine();

        } catch (ex) {
            this.messageService.addError(ex["message"]);
        }

    }
}
