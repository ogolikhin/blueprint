﻿import {
    BpArtifactEditor,
    PropertyContext,
    ILocalizationService,
    IProjectManager,
    IMessageService,
    IStateManager,
    IWindowManager,
    Models,
    Enums
} from "./bp-artifact-editor";
import { IArtifactService } from "../../main";


export class BpArtifactDetailsEditor implements ng.IComponentOptions {
    public template: string = require("./bp-details-editor.html");
    public controller: Function = BpArtifactDetailsEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };
}

export class BpArtifactDetailsEditorController extends BpArtifactEditor {
    public static $inject: [string] = [
        "messageService", "stateManager", "windowManager", "localization", "projectManager", "artifactService"];

    constructor(
        messageService: IMessageService,
        stateManager: IStateManager,
        windowManager: IWindowManager,
        localization: ILocalizationService,
        projectManager: IProjectManager,
        private artifactService: IArtifactService
    ) {
        super(messageService, stateManager, windowManager, localization, projectManager);
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
