import {Models, Enums} from "../../main";

import {
    BpArtifactEditor,
    ILocalizationService,
    IArtifactManager,
    IMessageService,
    IWindowManager,
    PropertyContext
} from "./bp-artifact-editor";

import {IDialogService} from "../../shared";


export class BpArtifactDetailsEditor implements ng.IComponentOptions {
    public template: string = require("./bp-details-editor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactDetailsEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<"
    };
}

export class BpArtifactDetailsEditorController extends BpArtifactEditor {
    public static $inject: [string] = [
        "messageService",
        "artifactManager",
        "windowManager",
        "localization",
        "dialogService"
    ];

    constructor(messageService: IMessageService,
                artifactManager: IArtifactManager,
                windowManager: IWindowManager,
                localization: ILocalizationService,
                private dialogService: IDialogService) {
        super(messageService, artifactManager, windowManager, localization);
    }

    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public customFields: AngularFormly.IFieldConfigurationObject[];
    public specificFields: AngularFormly.IFieldConfigurationObject[];
    public richTextFields: AngularFormly.IFieldConfigurationObject[];
    public isSystemPropertyAvailable: boolean;
    public isCustomPropertyAvailable: boolean;
    public isRichTextPropertyAvailable: boolean;
    public isSpecificPropertyAvailable: boolean;
    public specificPropertiesHeading: string;


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

    public hasFields(): boolean  {
        return ((this.systemFields || []).length +
               (this.customFields || []).length +
               (this.richTextFields || []).length +
               (this.specificFields || []).length) > 0;
    }

    protected onFieldUpdateFinished() {
        if (this.artifact) {
            this.isSystemPropertyAvailable = this.systemFields && this.systemFields.length > 0;
            this.isCustomPropertyAvailable = this.customFields && this.customFields.length > 0;
            this.isRichTextPropertyAvailable = this.richTextFields && this.richTextFields.length > 0;
            this.isSpecificPropertyAvailable = this.artifact.predefinedType === Models.ItemTypePredefined.Document ||
                this.artifact.predefinedType === Models.ItemTypePredefined.Actor;
            if (this.artifact.predefinedType === Models.ItemTypePredefined.Document) {
                this.specificPropertiesHeading = this.localization.get("Nova_Document_File", "File");
            } else if (this.artifact.predefinedType === Models.ItemTypePredefined.Actor) {
                this.specificPropertiesHeading = this.localization.get("Property_Actor_Section_Name", "Actor Properties");
            } else {
                this.specificPropertiesHeading = this.artifact.name + this.localization.get("Nova_Properties", " Properties");
                //TODO:: return this.artifact.type.name + this.localization.get("Nova_Properties", " Properties");
            }
        }
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        let propertyContext = field.data as PropertyContext;
        if (!propertyContext) {
            return;
        }

        //re-group fields
        if (true === propertyContext.isRichText &&
            (true === propertyContext.isMultipleAllowed || Models.PropertyTypePredefined.Description === propertyContext.propertyTypePredefined)
        ) {
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
