import {Models} from "../../../";
import {IMessageService} from "../../../../shell/";
import {IArtifactService} from "../../../services/";
import {BpBaseEditor, PropertyContext, LookupEnum, IEditorContext } from "./bp-base-editor";

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

    constructor(messageService: IMessageService, private artifactService: IArtifactService) {
        super(messageService);
    }

    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public customFields: AngularFormly.IFieldConfigurationObject[];
    public richTextFields: AngularFormly.IFieldConfigurationObject[];

    public get isSystemPropertyAvailable(): boolean {
        return this.systemFields && this.systemFields.length > 0;
    }
    public get isCustomPropertyAvailable(): boolean {
        return this.customFields && this.customFields.length > 0;
    }
    public get isTabPropertyAvailable(): boolean {
        return this.richTextFields && this.richTextFields.length > 0;
    }

    public $onDestroy() {
        delete this.systemFields;
        delete this.customFields;
        delete this.richTextFields;
        super.$onDestroy();
    }

    public onLoading(obj: any): boolean {
        this.systemFields = [];
        this.customFields = [];
        this.richTextFields = [];
        return super.onLoading(obj);
    }

    public onLoad(context: IEditorContext) {
        this.artifactService.getArtifact(context.artifact.id).then((it: Models.IArtifact) => {
            //TODO: change
            angular.extend(context.artifact, it);
            this.onUpdate(context);
        });
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        let propertyContext = field.data as PropertyContext;
        if (!propertyContext) {
            return;
        }
        //re=group fields
        if (true === propertyContext.isRichText) {
            this.richTextFields.push(field);
        } else if (LookupEnum.System === propertyContext.lookup) {
            this.systemFields.push(field);
        } else if (LookupEnum.Custom === propertyContext.lookup) {
            this.customFields.push(field);
        } else if (LookupEnum.Special === propertyContext.lookup) {
            
        }

    }
}
