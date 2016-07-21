import {Models} from "../../../";
import {IMessageService} from "../../../../shell/";
import {IArtifactService} from "../../../services/";
import {BpBaseEditor, PropertyEditor, FieldContext, IEditorContext} from "./bp-base-editor";



export class BpProjectEditor implements ng.IComponentOptions {
    public template: string = require("./bp-project-editor.html");
    public controller: Function = BpProjectEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };

}


export class BpProjectEditorController extends BpBaseEditor {
    public static $inject: [string] = ["messageService", "artifactService"];

    constructor(messageService: IMessageService, artifactService: IArtifactService) {
        super(messageService, artifactService);
    }

    public activeTab: number;
    public systemFields: AngularFormly.IFieldConfigurationObject[] = [] 
    public richTextFields: AngularFormly.IFieldConfigurationObject[] = [] 

    public get isSystemPropertyAvailable(): boolean {
        return this.systemFields && this.systemFields.length > 0;
    }
    public get isDescriptionPropertyAvailable(): boolean {
        return this.richTextFields && this.richTextFields.length > 0;
    }

    public onPropertyChange($viewValue, $modelValue, scope) {
    };

    public contextLoaded(context: IEditorContext) {
        try {
            super.contextLoaded(context);

            this.fields.forEach((it: AngularFormly.IFieldConfigurationObject) => {
                if (true === it.data["isSystem"]) {
                    this.systemFields.push(it);
                } else if (true === it.data["isRichText"]) {
                    this.richTextFields.push(it);
                }
            });

        } catch (ex) {
            this.messageService.addError(ex["message"]);
        }

    }
}
