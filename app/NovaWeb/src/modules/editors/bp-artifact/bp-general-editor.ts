import { IMessageService, IStateManager, IWindowResize, ISidebarToggle, Models } from "./";
import { BpBaseEditor, PropertyContext, LookupEnum, IProjectManager } from "./bp-base-editor";

export class BpGeneralEditor implements ng.IComponentOptions {
    public template: string = require("./bp-general-editor.html");
    public controller: Function = BpGeneralEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };
}

export class BpGeneralEditorController extends BpBaseEditor {
    public static $inject: [string] = ["messageService", "stateManager", "windowResize", "sidebarToggle", "$timeout", "projectManager"];

    constructor(
        messageService: IMessageService,
        stateManager: IStateManager,
        windowResize: IWindowResize,
        sidebarToggle: ISidebarToggle,
        $timeout: ng.ITimeoutService,
        projectManager: IProjectManager
    ) {
        super(messageService, stateManager, windowResize, sidebarToggle, $timeout, projectManager);
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


    public onLoading(obj: any): boolean {
        this.systemFields = [];
        this.noteFields = [];
        return super.onLoading(obj);
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
            } else if (LookupEnum.System === propertyContext.lookup) {
                this.systemFields.push(field);
            } else {
                field.hide = true;
            }
        }
    }
}
