import {
    BpBaseEditor,
    PropertyContext,
    ILocalizationService,
    IProjectManager,
    IMessageService,
    IStateManager,
    ISidebarToggle,
    Models,
    Enums
} from "./bp-base-editor";
import { IArtifactService } from "../../main";


export class BpArtifactEditor implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-editor.html");
    public controller: Function = BpArtifactEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };
}

export class BpArtifactEditorController extends BpBaseEditor {
    public static $inject: [string] = [
        "localization", "messageService", "stateManager", "sidebarToggle", "artifactService", "projectManager"];

    constructor(
        localization: ILocalizationService,
        messageService: IMessageService,
        stateManager: IStateManager,
        sidebarToggle: ISidebarToggle,
        private artifactService: IArtifactService,
        projectManager: IProjectManager
    ) {
        super(localization, messageService, stateManager, sidebarToggle, projectManager);
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
    public get isRichTextPropertyAvailable(): boolean {
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

    public onLoad(context: Models.IEditorContext) {
        this.isLoading = true;
        this.artifactService.getArtifact(context.artifact.id).then((it: Models.IArtifact) => {
            angular.extend(context.artifact, it);
            this.stateManager.addChange(context.artifact);
            this.onUpdate(context);
        }).catch((error: any) => {
            //ignore authentication errors here
            if (error.statusCode !== 1401) {
                this.messageService.addError(error["message"] || "Artifact_NotFound");
                }
        }).finally(() => {
            this.isLoading = false;
        });
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        let propertyContext = field.data as PropertyContext;
        if (!propertyContext) {
            return;
        }
        
        //re-group fields
        if (true === propertyContext.isRichText) {
            this.richTextFields.push(field);
        } else if (Enums.PropertyLookupEnum.System === propertyContext.lookup) {
            this.systemFields.push(field);
        } else if (Enums.PropertyLookupEnum.Custom === propertyContext.lookup) {
            this.customFields.push(field);
        } else if (Enums.PropertyLookupEnum.Special === propertyContext.lookup) {
            
        }

    }
}
