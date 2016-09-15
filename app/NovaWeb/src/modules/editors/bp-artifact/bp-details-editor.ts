import { Models, Enums } from "../../main";

import {
    BpArtifactEditor,
    ILocalizationService, 
    IProjectManager, 
    IArtifactManager, 
//    IStatefulArtifact,
    IMessageService,  
    IWindowManager, 
    PropertyContext, 
} from "./bp-artifact-editor";

import { IArtifactService } from "../../main/services";
import { IDialogService } from "../../shared";


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
        "messageService", "artifactManager", "windowManager", "localization", "projectManager", "artifactService", "dialogService"];

    constructor(
        messageService: IMessageService,
        artifactManager: IArtifactManager,
        windowManager: IWindowManager,
        localization: ILocalizationService,
        projectManager: IProjectManager,
        private artifactService: IArtifactService,
        private dialogService: IDialogService
    ) {
        super(messageService, artifactManager, windowManager, localization, projectManager);
    }

    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public customFields: AngularFormly.IFieldConfigurationObject[];
    public specificFields: AngularFormly.IFieldConfigurationObject[];
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

    public get isSpecificPropertyAvailable(): boolean {
        return this.artifact.predefinedType === Models.ItemTypePredefined.Document ||
               this.artifact.predefinedType === Models.ItemTypePredefined.Actor;
    }

    public get specificPropertiesHeading(): string {
        if (this.artifact.predefinedType === Models.ItemTypePredefined.Document) {
            return this.localization.get("Nova_Document_File", "File");
        } else if (this.artifact.predefinedType === Models.ItemTypePredefined.Actor) {
            return this.localization.get("Property_Actor_Section_Name", "Actor Properties");
        } else {
            return this.artifact.name + this.localization.get("Nova_Properties", " Properties");
            //TODO:: return this.artifact.type.name + this.localization.get("Nova_Properties", " Properties");
        }
    }

    public $onDestroy() {
        delete this.systemFields;
        delete this.customFields;
        delete this.specificFields;
        delete this.richTextFields;
        super.$onDestroy();
    }

    public clearFields() {
        this.systemFields = [];
        this.customFields = [];
        this.specificFields = [];
        this.richTextFields = [];
    }
    

    public onLoad() {
        this.isLoading = true;
        this.artifact.load().then((it: Models.IArtifact) => {
            // delete context.artifact.lockedByUser;
            // delete context.artifact.lockedDateTime;
            // context.artifact = angular.extend({}, context.artifact, it);
            // this.stateManager.addChange(context.artifact);
            this.onUpdate();
            // if (state.moved) {
            //     this.dialogService.alert("Artifact_Lock_DoesNotExist");
            //}
        }).catch((error: any) => {
            //ignore authentication errors here
            if (error) {
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
            this.specificFields.push(field);
        }
    }
}
