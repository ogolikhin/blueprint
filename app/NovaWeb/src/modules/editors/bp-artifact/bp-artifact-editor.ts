import { ILocalizationService, IStateManager, IMessageService, IArtifactService, Models } from "./";
import { BpBaseEditor, PropertyContext, LookupEnum, IEditorContext } from "./bp-base-editor";

export class BpArtifactEditor implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-editor.html");
    public controller: Function = BpArtifactEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };
}

export class BpArtifactEditorController extends BpBaseEditor {
    public static $inject: [string] = ["messageService", "stateManager", "artifactService", "localization", "$timeout"];

    public scrollOptions = {
        minScrollbarLength: 20,
        scrollXMarginOffset: 4,
        scrollYMarginOffset: 4
    };

    constructor(
        messageService: IMessageService,
        stateManager: IStateManager,
        private artifactService: IArtifactService,
        private localization: ILocalizationService,
        $timeout: ng.ITimeoutService
    ) {
        super(messageService, stateManager, $timeout);
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

    public onLoad(context: IEditorContext) {
        this.isLoading = true;
        this.artifactService.getArtifact(context.artifact.id).then((it: Models.IArtifact) => {
            angular.extend(context.artifact, it);
            this.onUpdate(context);
        }).catch((error: any) => {
            //ignore authentication errors here
            if (error.statusCode !== 1401) {
                this.messageService.addError(error["message"] || this.localization.get("Artifact_NotFound"));
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
        } else if (LookupEnum.System === propertyContext.lookup) {
            this.systemFields.push(field);
        } else if (LookupEnum.Custom === propertyContext.lookup) {
            this.customFields.push(field);
        } else if (LookupEnum.Special === propertyContext.lookup) {
            
        }

    }
}
