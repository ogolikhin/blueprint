﻿import {
    BpArtifactEditor,
    PropertyContext,
    ILocalizationService,
    IProjectManager,
    IMessageService,
    ISelectionManager,
    IWindowManager,
    Models,
    Enums
} from "./bp-artifact-editor";

export class BpArtifactGeneralEditor implements ng.IComponentOptions {
    public template: string = require("./bp-general-editor.html");
    public controller: Function = BpGeneralArtifactEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };
}

export class BpGeneralArtifactEditorController extends BpArtifactEditor {
    public static $inject: [string] = ["messageService", "selectionManager2", "windowManager", "localization", "projectManager"];

    constructor(
        messageService: IMessageService,
        selectionManager: ISelectionManager,
        windowManager: IWindowManager,
        localization: ILocalizationService,
        projectManager: IProjectManager
    ) {
        super(messageService, selectionManager, windowManager, localization, projectManager);
    }

    public activeTab: number;
    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public noteFields: AngularFormly.IFieldConfigurationObject[]; 

    public $onDestroy() {
        delete this.systemFields;
        delete this.noteFields;
        super.$onDestroy();
    }

    public get isLoaded(): boolean {
        return !!(this.systemFields && this.systemFields.length || this.noteFields && this.noteFields.length);
    }

    public get isNoteFieldsAvailable(): boolean {
        return this.noteFields && this.noteFields.length > 0;
    }


    public onLoading(): boolean {
        this.systemFields = [];
        this.noteFields = [];
        return super.onLoading();
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        super.onFieldUpdate(field);
        let propertyContext = field.data as PropertyContext;
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
