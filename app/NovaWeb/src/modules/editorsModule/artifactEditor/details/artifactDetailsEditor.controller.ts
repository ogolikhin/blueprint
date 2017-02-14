import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {Enums, Models} from "../../../main";
import {IMessageService} from "../../../main/components/messages/message.svc";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {IWindowManager} from "../../../main/services/window-manager";
import {IValidationService} from "../../../managers/artifact-manager/validation/validation.svc";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {IPropertyDescriptor, IPropertyDescriptorBuilder} from "../../services";
import {BpArtifactEditor} from "../artifactEditor.controller";

export class BpArtifactDetailsEditorController extends BpArtifactEditor {
    public static $inject: [string] = [
        "$window",
        "messageService",
        "selectionManager",
        "windowManager",
        "localization",
        "propertyDescriptorBuilder",
        "validationService"
    ];

    constructor($window: ng.IWindowService,
                messageService: IMessageService,
                selectionManager: ISelectionManager,
                windowManager: IWindowManager,
                localization: ILocalizationService,
                propertyDescriptorBuilder: IPropertyDescriptorBuilder,
                validationService: IValidationService) {
        super($window, messageService, selectionManager, windowManager, localization, propertyDescriptorBuilder);
        this.validationService = validationService;
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

    private validationService: IValidationService;

    protected destroy(): void {
        this.systemFields = undefined;
        this.customFields = undefined;
        this.specificFields = undefined;
        this.richTextFields = undefined;

        super.destroy();
    }

    public clearFields() {
        this.systemFields = [];
        this.customFields = [];
        this.specificFields = [];
        this.richTextFields = [];
    }

    public hasFields(): boolean {
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
            this.isSpecificPropertyAvailable = this.artifact.predefinedType === ItemTypePredefined.Document ||
                this.artifact.predefinedType === ItemTypePredefined.Actor;
            if (this.artifact.predefinedType === ItemTypePredefined.Document) {
                this.specificPropertiesHeading = this.localization.get("Nova_Document_File", "File");
            } else if (this.artifact.predefinedType === ItemTypePredefined.Actor) {
                this.specificPropertiesHeading = this.localization.get("Property_Actor_Section_Name", "Actor Properties");
            } else {
                this.specificPropertiesHeading = this.artifact.name + this.localization.get("Nova_Properties", " Properties");
                //TODO:: return this.artifact.type.name + this.localization.get("Nova_Properties", " Properties");
            }
        }
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        const propertyContext = field.data as IPropertyDescriptor;
        if (!propertyContext) {
            return;
        }

        //re-group fields
        if (propertyContext.isRichText &&
            (propertyContext.isMultipleAllowed || Models.PropertyTypePredefined.Description === propertyContext.propertyTypePredefined)
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

    public isRtfFieldValid = (field: AngularFormly.IFieldConfigurationObject): boolean => {
        const propertyContext = field.data as IPropertyDescriptor;
        if (!propertyContext) {
            return true;
        }

        if (propertyContext.isRichText && propertyContext.isRequired) {
            const value = this.model[field.key];
            return this.validationService.textRtfValidation.hasValueIfRequired(true, value);
        } else {
            return true;
        }
    };
}
