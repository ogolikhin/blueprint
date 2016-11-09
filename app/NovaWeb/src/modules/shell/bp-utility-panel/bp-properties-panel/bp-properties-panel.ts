import {Models} from "../../../main";
import {
    ISelectionManager,
    IStatefulArtifact,
    IStatefulSubArtifact,
    IStatefulItem
} from "../../../managers/artifact-manager";
import {IBpAccordionPanelController} from "../../../main/components/bp-accordion/bp-accordion";
import {BPBaseUtilityPanelController} from "../bp-base-utility-panel";
import {PropertyEditor} from "../../../editors/bp-artifact/bp-property-editor";
import {IPropertyDescriptorBuilder, IPropertyDescriptor} from "../../../editors/configuration/property-descriptor-builder";
import {PropertyLookupEnum} from "../../../main/models/enums";
import {Helper} from "../../../shared/utils/helper";
import {PropertyEditorFilters} from "./bp-properties-panel-filters";
import {IMessageService} from "../../../core/messages/message.svc";
import {ILocalizationService} from "../../../core/localization/localizationService";

export class BPPropertiesPanel implements ng.IComponentOptions {
    public template: string = require("./bp-properties-panel.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPPropertiesController;
    public controllerAs = "$ctrl";
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export class BPPropertiesController extends BPBaseUtilityPanelController {

    public static $inject: [string] = [
        "$q",
        "selectionManager",
        "messageService",
        "localization",
        "propertyDescriptorBuilder"
    ];

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: PropertyEditor;

    public isLoading: boolean = false;

    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public customFields: AngularFormly.IFieldConfigurationObject[];
    public specificFields: AngularFormly.IFieldConfigurationObject[];
    public richTextFields: AngularFormly.IFieldConfigurationObject[];

    public selectedArtifact: IStatefulArtifact;
    private selectedSubArtifact: IStatefulSubArtifact;

    protected artifactSubscriber: Rx.IDisposable;
    protected subArtifactSubscriber: Rx.IDisposable;

    constructor($q: ng.IQService,
                protected selectionManager: ISelectionManager,
                public messageService: IMessageService,
                public localization: ILocalizationService,
                protected propertyDescriptorBuilder: IPropertyDescriptorBuilder,
                public bpAccordionPanel: IBpAccordionPanelController) {
        super($q, selectionManager, bpAccordionPanel);
        this.editor = new PropertyEditor(this.localization);
    }

    public $onDestroy() {
        delete this.systemFields;
        delete this.specificFields;
        delete this.customFields;
        delete this.richTextFields;
        super.$onDestroy();
    }

    public destroySubscribers() {
        if (this.artifactSubscriber) {
            this.artifactSubscriber.dispose();
        }
        if (this.subArtifactSubscriber) {
            this.subArtifactSubscriber.dispose();
        }

    }

    public get isSystemPropertyAvailable(): boolean {
        return this.systemFields && this.systemFields.length > 0;
    }

    public get isSpecificPropertyAvailable(): boolean {
        return this.specificFields && this.specificFields.length > 0;
    }

    public get isCustomPropertyAvailable(): boolean {
        return this.customFields && this.customFields.length > 0;
    }

    public get isRichTextPropertyAvailable(): boolean {
        return this.richTextFields && this.richTextFields.length > 0;
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        if (subArtifact) {
            this.selectedArtifact = artifact;
            this.selectedSubArtifact = subArtifact;
            if (Helper.hasArtifactEverBeenSavedOrPublished(subArtifact)) {
                //TODO: implement .getObservable
                this.onUpdate();
                this.subArtifactSubscriber = this.selectedSubArtifact.getObservable().subscribe(this.onSubArtifactChanged);
            } else {
                this.reset();
            }
            // for new selection
        } else if (artifact) {
            this.selectedSubArtifact = null;
            this.selectedArtifact = artifact;
            this.artifactSubscriber = this.selectedArtifact.getObservable().subscribe(this.onArtifactChanged);

        } else {
            this.selectedArtifact = null;
            this.selectedSubArtifact = null;
            this.reset();
        }

        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    protected onArtifactChanged = (it) => {
        this.onUpdate();
        if (this.artifactSubscriber) {
            this.artifactSubscriber.dispose();
        }
    };

    protected onSubArtifactChanged = (it) => {
        this.onUpdate();
        if (this.subArtifactSubscriber) {
            this.subArtifactSubscriber.dispose();
        }
    };

    private hasFields(): boolean {
        return ((this.systemFields || []).length +
            (this.customFields || []).length +
            (this.richTextFields || []).length +
            (this.specificFields || []).length) > 0;
    }

    private shouldRenewFields(item: IStatefulItem): boolean {
        if (item.artifactState.readonly || !this.hasFields()) {
            return true;
        }
        return false;
    }

    private clearFields() {
        this.systemFields = [];
        this.customFields = [];
        this.specificFields = [];
        this.richTextFields = [];
    }

    public onUpdate() {
        this.reset();

        if (!this.editor || !this.selectedArtifact) {
            return;
        }

        let propertyDescriptorsPromise: ng.IPromise<IPropertyDescriptor[]>;
        let selectedItem: IStatefulItem;

        if (this.isSubartifactSelected()) {
            propertyDescriptorsPromise = this.propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(this.selectedSubArtifact);
            selectedItem = this.selectedSubArtifact;

        } else {
            propertyDescriptorsPromise = this.propertyDescriptorBuilder.createArtifactPropertyDescriptors(this.selectedArtifact);
            selectedItem = this.selectedArtifact;
        }

        propertyDescriptorsPromise.then((propertyDescriptors) => {
            this.displayContent(selectedItem, propertyDescriptors);
        });
    }

    private displayContent(selectedItem: IStatefulItem, propertyDescriptors: IPropertyDescriptor[]) {
        const propertyEditorFilter = new PropertyEditorFilters(this.localization);
        const propertyFilters = propertyEditorFilter.getPropertyEditorFilters(selectedItem.predefinedType);

        const shouldCreateFields = this.editor.create(selectedItem, propertyDescriptors, this.shouldRenewFields(selectedItem));

        if (shouldCreateFields) {
            this.clearFields();
            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                let propertyContext = field.data as IPropertyDescriptor;
                if (propertyContext && propertyFilters[propertyContext.propertyTypePredefined]) {
                    return;
                }

                //add property change handler to each field
                Object.assign(field.templateOptions, {
                    onChange: this.onValueChange.bind(this)
                });

                    const isReadOnly = selectedItem.artifactState.readonly;
                    if (isReadOnly) {
                        field.templateOptions.disabled = true;                    
                        if (field.key !== "documentFile" &&
                            field.type !== "bpFieldImage" &&
                            field.type !== "bpFieldInheritFrom") {
                            field.type = "bpFieldReadOnly";
                        }
                    }

                this.onFieldUpdate(field);

            });
        }
        this.model = this.editor.getModel();
        this.isLoading = false;
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        const propertyContext = field.data as IPropertyDescriptor;
        if (!propertyContext) {
            return;
        }

        //re-group fields
        if (true === propertyContext.isRichText && PropertyLookupEnum.Special === propertyContext.lookup) {
            this.systemFields.push(field);
        } else if (true === propertyContext.isRichText &&
            (true === propertyContext.isMultipleAllowed || Models.PropertyTypePredefined.Description === propertyContext.propertyTypePredefined)
        ) {
            this.richTextFields.push(field);
        } else if (PropertyLookupEnum.System === propertyContext.lookup) {
            this.systemFields.push(field);
        } else if (PropertyLookupEnum.Custom === propertyContext.lookup) {
            this.customFields.push(field);
        } else if (PropertyLookupEnum.Special === propertyContext.lookup) {
            this.specificFields.push(field);
        }

    }

    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: AngularFormly.ITemplateScope) {
        
        //here we need to update original model
        const context = $field.data as IPropertyDescriptor;
        if (!context) {
            return;
        }
        let value = this.editor.convertToModelValue($field, $value);
        switch (context.lookup) {
            case PropertyLookupEnum.Custom:
                this.getSelectedItem().customProperties.set(context.modelPropertyName as number, value);
                break;
            case PropertyLookupEnum.Special:
                this.getSelectedItem().specialProperties.set(context.modelPropertyName as number, value);
                break;
            default:
                this.getSelectedItem()[context.modelPropertyName] = value;
                break;
            }
        context.isFresh = false;
        
    };

    private getSelectedItem(): IStatefulItem {
        return this.isSubartifactSelected() ? this.selectedSubArtifact : this.selectedArtifact;
    }

    private isSubartifactSelected(): boolean {
        return !!this.selectedSubArtifact;
    }

    public get specificPropertiesHeading(): string {
        if (this.selectedArtifact.predefinedType === Models.ItemTypePredefined.Document) {
            return this.localization.get("Nova_Document_File", "File");
        } else if (this.selectedArtifact.predefinedType === Models.ItemTypePredefined.Actor) {
            return this.localization.get("Property_Actor_Section_Name", "Actor Properties");
        } else if (this.isSubartifactSelected()) {
            return this.localization.get("Property_SubArtifact_Section_Name", "Sub-Artifact Properties");
        } else {
            return this.localization.get("Property_Artifact_Section_Name", "Artifact Properties");
        }
    }

    private reset() {
        this.fields = [];
        this.model = {};
        this.systemFields = [];
        this.customFields = [];
        this.specificFields = [];
        this.richTextFields = [];
    }
}
