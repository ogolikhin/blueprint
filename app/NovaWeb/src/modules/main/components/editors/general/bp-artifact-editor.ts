import {Models} from "../../../";
import {IMessageService} from "../../../../shell/";
import {IArtifactService} from "../../../services/";
import {BpBaseEditor, PropertyEditor, FieldContext, IEditorContext} from "./bp-base-editor";



export class BpArtifactEditor implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-editor.html");
    public controller: Function = BpArtifactEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };

}





export class BpArtifactEditorController extends BpBaseEditor {
    public static $inject: [string] = ["messageService", "artifactService"];

    constructor(messageService: IMessageService, artifactService: IArtifactService) {
        super(messageService, artifactService);
    }



    public activeTab: number;

    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public customFields: AngularFormly.IFieldConfigurationObject[];
    public richTextFields: AngularFormly.IFieldConfigurationObject[];


    public get isCustomPropertyAvailable(): boolean {
        return this.systemFields && this.systemFields.length > 0;
    }
    public get isSystemPropertyAvailable(): boolean {
        return this.customFields && this.customFields.length > 0;
    }
    public get isTabPropertyAvailable(): boolean {
        return this.richTextFields && this.richTextFields.length > 0;
    }


    public onPropertyChange($viewValue, $modelValue, scope) {
    };


    public contextLoading(context: IEditorContext) {
        this.artifactService.getArtifact(context.artifact.id).then((it: Models.IArtifact) => {
            //TODO: change
            angular.extend(context.artifact, { propertyValues: it.propertyValues });
            this.contextLoaded(context);
        });
    }

    public contextLoaded(context: IEditorContext) {
        try {
            super.contextLoaded(context);

            this.systemFields = [];
            this.customFields = [];
            this.richTextFields = [];
            
            this.fields.forEach((it: AngularFormly.IFieldConfigurationObject) => {
                if (true === it.data["isSystem"]) {
                    this.systemFields.push(it);
                } else if (true === it.data["isRichText"]) {
                    this.richTextFields.push(it);
                } else {
                    this.customFields.push(it);
                } 
            });
            if (this.form) {
                this.form.$setPristine();
            } 

        } catch (ex) {
            this.messageService.addError(ex["message"]);
        }

    }
}
