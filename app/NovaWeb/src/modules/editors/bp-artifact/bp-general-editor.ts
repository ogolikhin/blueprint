import {
    Models, Enums,
    BpArtifactEditor,
    IArtifactManager,
    IWindowManager
} from "./bp-artifact-editor";
import {IPropertyDescriptor, IPropertyDescriptorBuilder} from "../configuration/property-descriptor-builder";
import {ILocalizationService} from "../../core/localization/localization.service";
import {IMessageService} from "../../main/components/messages/message.svc";

export class BpArtifactGeneralEditor implements ng.IComponentOptions {
    public template: string = require("./bp-general-editor.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpGeneralArtifactEditorController;
}

export class BpGeneralArtifactEditorController extends BpArtifactEditor {
    public static $inject: [string] = [
        "$window",
        "messageService",
        "artifactManager",
        "windowManager",
        "localization",
        "propertyDescriptorBuilder"
    ];

    constructor($window: ng.IWindowService,
                messageService: IMessageService,
                artifactManager: IArtifactManager,
                windowManager: IWindowManager,
                localization: ILocalizationService,
                propertyDescriptorBuilder: IPropertyDescriptorBuilder) {
        super($window, messageService, artifactManager, windowManager, localization, propertyDescriptorBuilder);
    }

    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public noteFields: AngularFormly.IFieldConfigurationObject[];

    protected destroy(): void {
        this.systemFields = undefined;
        this.noteFields = undefined;

        super.destroy();
    }

    public get isLoaded(): boolean {
        return !!(this.systemFields && this.systemFields.length || this.noteFields && this.noteFields.length);
    }

    public get isNoteFieldsAvailable(): boolean {
        return this.noteFields && this.noteFields.length > 0;
    }

    public clearFields() {
        this.systemFields = [];
        this.noteFields = [];
    }

    public hasFields(): boolean {
        return ((angular.isArray(this.systemFields) ? this.systemFields.length : 0) +
            (angular.isArray(this.noteFields) ? this.noteFields.length : 0)) > 0;

    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        const propertyContext = field.data as IPropertyDescriptor;
        if (!propertyContext) {
            return;
        }
        if ([Models.PropertyTypePredefined.Name, Models.PropertyTypePredefined.Description].indexOf(propertyContext.propertyTypePredefined) >= 0) {

            field.type = "bpFieldReadOnly";
            if (true === propertyContext.isRichText) {
                this.noteFields.push(field);
            } else if (Enums.PropertyLookupEnum.System === propertyContext.lookup) {
                this.systemFields.push(field);
            } else {
                field.hide = true;
            }
        }
    }
}
